// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.Buffers;
using Sci.NET.Mathematics.Backends.Managed.Iterators;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels.LinearAlgebra;
using Sci.NET.Mathematics.Intrinsics;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Performance;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed;

internal class ManagedLinearAlgebraKernels : ILinearAlgebraKernels
{
    private const int MatrixMultiplyMrFp32 = 8;
    private const int MatrixMultiplyNrFp32 = 8;
    private const int MatrixMultiplyMrFp64 = 4;
    private const int MatrixMultiplyNrFp64 = 4;
    private const int MatrixMultiplyMcFp32 = 256;
    private const int MatrixMultiplyKcFp32 = 256;
    private const int MatrixMultiplyNcFp32 = 256;
    private const int MatrixMultiplyMcFp64 = 128;
    private const int MatrixMultiplyKcFp64 = 128;
    private const int MatrixMultiplyNcFp64 = 128;

    public unsafe void Hypot<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right, ITensor<TNumber> result)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        ManagedStreamingBinaryOperationIterator.Apply<HypotMicroKernel<TNumber>, TNumber>(
            left.Memory.ToPointer(),
            right.Memory.ToPointer(),
            result.Memory.ToPointer(),
            left.Shape.ElementCount,
            (ICpuComputeDevice)left.Device);
    }

    public unsafe void MatrixMultiply<TNumber>(Matrix<TNumber> left, Matrix<TNumber> right, Matrix<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftMemoryBlock = (SystemMemoryBlock<TNumber>)left.Memory;
        var rightMemoryBlock = (SystemMemoryBlock<TNumber>)right.Memory;
        var resultMemoryBlock = (SystemMemoryBlock<TNumber>)result.Memory;
        var leftMemoryBlockPtr = leftMemoryBlock.ToPointer();
        var rightMemoryBlockPtr = rightMemoryBlock.ToPointer();
        var resultMemoryBlockPtr = resultMemoryBlock.ToPointer();
        var leftRows = left.Rows;
        var rightColumns = right.Columns;
        var leftColumns = left.Columns;
        const int iBlock = 128;
        const int jBlock = 16;

        var cpuDevice = left.Device as ICpuComputeDevice;

        if ((cpuDevice?.IsAvx2Supported() ?? false) &&
            IntrinsicsHelper.IsAvx2Supported() &&
            typeof(TNumber) == typeof(float))
        {
            MatrixMultiplyFmaAvxFp32(
                (float*)leftMemoryBlockPtr,
                (float*)rightMemoryBlockPtr,
                (float*)resultMemoryBlockPtr,
                leftRows,
                rightColumns,
                leftColumns);
            return;
        }

        if ((cpuDevice?.IsAvx2Supported() ?? false) &&
            IntrinsicsHelper.IsAvx2Supported() &&
            typeof(TNumber) == typeof(double))
        {
            MatrixMultiplyFmaAvxFp64(
                (double*)leftMemoryBlockPtr,
                (double*)rightMemoryBlockPtr,
                (double*)resultMemoryBlockPtr,
                leftRows,
                rightColumns,
                leftColumns);
            return;
        }

        var iTiles = (leftRows + iBlock - 1) / iBlock;
        var jTiles = (rightColumns + jBlock - 1) / jBlock;
        var tileCount = iTiles * jTiles;

        _ = Parallel.For(
            0,
            tileCount,
            new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
            flat =>
            {
                long ti = flat / jTiles;
                long tj = flat - (ti * jTiles);
                long i0 = ti * iBlock;
                long j0 = tj * jBlock;
                long iMax = Math.Min(i0 + iBlock, leftRows);
                long jMax = Math.Min(j0 + jBlock, rightColumns);

                for (var i = i0; i < iMax; i++)
                {
                    for (var j = j0; j < jMax; ++j)
                    {
                        var sum = TNumber.Zero;
                        for (var k = 0; k < leftColumns; ++k)
                        {
                            sum += leftMemoryBlockPtr[(i * leftColumns) + k] * rightMemoryBlockPtr[(k * rightColumns) + j];
                        }

                        resultMemoryBlockPtr[(i * rightColumns) + j] = sum;
                    }
                }
            });
    }

    public unsafe void InnerProduct<TNumber>(Tensors.Vector<TNumber> left, Tensors.Vector<TNumber> right, Scalar<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var leftMemoryBlock = (SystemMemoryBlock<TNumber>)left.Memory;
        var rightMemoryBlock = (SystemMemoryBlock<TNumber>)right.Memory;
        var resultMemoryBlock = (SystemMemoryBlock<TNumber>)result.Memory;
        var cpuDevice = left.Device as ICpuComputeDevice;

        if ((cpuDevice?.IsAvx2Supported() ?? false) &&
            IntrinsicsHelper.IsAvx2Supported() &&
            typeof(TNumber) == typeof(float))
        {
            InnerProductFp32FmaAvx(
                (float*)leftMemoryBlock.Pointer,
                (float*)rightMemoryBlock.Pointer,
                (float*)resultMemoryBlock.Pointer,
                left.Length);

            return;
        }

        if ((cpuDevice?.IsAvx2Supported() ?? false) &&
            IntrinsicsHelper.IsAvx2Supported() &&
            typeof(TNumber) == typeof(double))
        {
            InnerProductFp64FmaAvx(
                (double*)leftMemoryBlock.Pointer,
                (double*)rightMemoryBlock.Pointer,
                (double*)resultMemoryBlock.Pointer,
                left.Length);

            return;
        }

        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<TNumber>(left.Length);

        if (processes == 1)
        {
            var sum = TNumber.Zero;

            for (var i = 0; i < left.Length; i++)
            {
                sum += leftMemoryBlock[i] * rightMemoryBlock[i];
            }

            return;
        }

        var results = (TNumber*)NativeMemory.AllocZeroed((UIntPtr)processes, (UIntPtr)Unsafe.SizeOf<TNumber>());

        try
        {
            _ = Parallel.For(
                0,
                processes,
                new ParallelOptions { MaxDegreeOfParallelism = processes },
                tid =>
                {
                    var start = tid * leftMemoryBlock.Length / processes;
                    var end = (tid + 1) * leftMemoryBlock.Length / processes;
                    var count = end - start;
                    var accumulatedSum = TNumber.Zero;

                    for (var i = 0; i < count; i++)
                    {
                        accumulatedSum += leftMemoryBlock[i] * rightMemoryBlock[i];
                    }

                    results[tid] = accumulatedSum;
                });

            for (var i = 0; i < processes; i++)
            {
                resultMemoryBlock[0] += results[i];
            }
        }
        finally
        {
            NativeMemory.Free(results);
        }
    }

    private static unsafe void MatrixMultiplyFmaAvxFp32(
        float* a,
        float* b,
        float* c,
        int m,
        int n,
        int k)
    {
        var numTiles = (m + MatrixMultiplyMcFp32 - 1) / MatrixMultiplyMcFp32;

        if (ManagedTensorBackend.ShouldParallelizeForTiles(numTiles))
        {
            using var allPanels = new ThreadLocal<Panel2dFp32>(
                () =>
                {
                    var aBuffer = (float*)NativeMemory.AlignedAlloc(MatrixMultiplyMcFp32 * MatrixMultiplyKcFp32 * sizeof(float), 32);
                    var bBuffer = (float*)NativeMemory.AlignedAlloc(MatrixMultiplyKcFp32 * MatrixMultiplyNcFp32 * sizeof(float), 32);

                    return new Panel2dFp32(aBuffer, bBuffer);
                },
                true);

            _ = Parallel.For(
                0,
                numTiles,
                new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
                tileIdx => InnerLoop(tileIdx, allPanels.Value));

            foreach (var panel in allPanels.Values)
            {
                NativeMemory.AlignedFree(panel.A);
                NativeMemory.AlignedFree(panel.B);
            }
        }
        else
        {
            var aBuffer = (float*)NativeMemory.AlignedAlloc(MatrixMultiplyMcFp32 * MatrixMultiplyKcFp32 * sizeof(float), 32);
            var bBuffer = (float*)NativeMemory.AlignedAlloc(MatrixMultiplyKcFp32 * MatrixMultiplyNcFp32 * sizeof(float), 32);
            var panels = new Panel2dFp32(aBuffer, bBuffer);

            for (var tileIdx = 0; tileIdx < numTiles; tileIdx++)
            {
                InnerLoop(tileIdx, panels);
            }

            NativeMemory.AlignedFree(panels.A);
            NativeMemory.AlignedFree(panels.B);
        }

        void InnerLoop(int tileIdx, Panel2dFp32 panels)
        {
            var mBase = tileIdx * MatrixMultiplyMcFp32;
            var mTile = Math.Min(MatrixMultiplyMcFp32, m - mBase);
            var aPanel = panels.A;
            var bPanel = panels.B;

            for (var kBase = 0; kBase < k; kBase += MatrixMultiplyKcFp32)
            {
                var kTile = Math.Min(MatrixMultiplyKcFp32, k - kBase);
                PackAFp32(a + (mBase * k) + kBase, aPanel, k, mTile, kTile);

                for (var nBase = 0; nBase < n; nBase += MatrixMultiplyNcFp32)
                {
                    var nc = Math.Min(MatrixMultiplyNcFp32, n - nBase);
                    PackBFp32(b + (kBase * n) + nBase, bPanel, n, kTile, nc);

                    for (var mReg = 0; mReg < mTile; mReg += MatrixMultiplyMrFp32)
                    {
                        var mr = Math.Min(MatrixMultiplyMrFp32, mTile - mReg);

                        for (var nReg = 0; nReg < nc; nReg += MatrixMultiplyNrFp32)
                        {
                            var nr = Math.Min(MatrixMultiplyNrFp32, nc - nReg);
                            var leftPtr = aPanel + (mReg * MatrixMultiplyKcFp32);
                            var rightPtr = bPanel + nReg;
                            var resultPtr = c + ((mBase + mReg) * n) + nBase + nReg;

                            if (mr == MatrixMultiplyMrFp32 && nr == MatrixMultiplyNrFp32)
                            {
                                MicroKernel8x8FmaAvxFp32(
                                    aPanel + (mReg * MatrixMultiplyKcFp32),
                                    bPanel + nReg,
                                    c + ((mBase + mReg) * n) + nBase + nReg,
                                    kTile,
                                    n);
                            }
                            else
                            {
                                MicroKernelScalarFp32(
                                    leftPtr,
                                    rightPtr,
                                    resultPtr,
                                    kTile,
                                    n,
                                    mr,
                                    nr);
                            }
                        }
                    }
                }
            }
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void MatrixMultiplyFmaAvxFp64(
        double* a,
        double* b,
        double* c,
        int m,
        int n,
        int k)
    {
        var numTiles = (m + MatrixMultiplyMcFp64 - 1) / MatrixMultiplyMcFp64;

        if (ManagedTensorBackend.ShouldParallelizeForTiles(numTiles))
        {
            using var allPanels = new ThreadLocal<Panel2dFp64>(
                () =>
                {
                    var aBuffer = (double*)NativeMemory.AlignedAlloc(MatrixMultiplyMcFp64 * MatrixMultiplyKcFp64 * sizeof(double), 32);
                    var bBuffer = (double*)NativeMemory.AlignedAlloc(MatrixMultiplyKcFp64 * MatrixMultiplyNcFp64 * sizeof(double), 32);

                    return new Panel2dFp64(aBuffer, bBuffer);
                },
                true);

            _ = Parallel.For(
                0,
                numTiles,
                new ParallelOptions { MaxDegreeOfParallelism = ManagedTensorBackend.MaxDegreeOfParallelism },
                tileIdx => InnerLoop(tileIdx, allPanels.Value));

            foreach (var panel in allPanels.Values)
            {
                NativeMemory.AlignedFree(panel.A);
                NativeMemory.AlignedFree(panel.B);
            }
        }
        else
        {
            var aBuffer = (double*)NativeMemory.AlignedAlloc(MatrixMultiplyMcFp64 * MatrixMultiplyKcFp64 * sizeof(double), 32);
            var bBuffer = (double*)NativeMemory.AlignedAlloc(MatrixMultiplyKcFp64 * MatrixMultiplyNcFp64 * sizeof(double), 32);
            var panels = new Panel2dFp64(aBuffer, bBuffer);

            for (var tileIdx = 0; tileIdx < numTiles; tileIdx++)
            {
                InnerLoop(tileIdx, panels);
            }

            NativeMemory.AlignedFree(panels.A);
            NativeMemory.AlignedFree(panels.B);
        }

        [MethodImpl(ImplementationOptions.HotPath)]
        void InnerLoop(int tileIdx, Panel2dFp64 panels)
        {
            var mBase = tileIdx * MatrixMultiplyMcFp64;
            var mTile = Math.Min(MatrixMultiplyMcFp64, m - mBase);
            var aPanel = panels.A;
            var bPanel = panels.B;

            for (var kBase = 0; kBase < k; kBase += MatrixMultiplyKcFp64)
            {
                var kTile = Math.Min(MatrixMultiplyKcFp64, k - kBase);
                PackAFp64(a + (mBase * k) + kBase, aPanel, k, mTile, kTile);

                for (var nBase = 0; nBase < n; nBase += MatrixMultiplyNcFp64)
                {
                    var nc = Math.Min(MatrixMultiplyNcFp64, n - nBase);
                    PackBFp64(b + (kBase * n) + nBase, bPanel, n, kTile, nc);

                    for (var mReg = 0; mReg < mTile; mReg += MatrixMultiplyMrFp64)
                    {
                        var mr = Math.Min(MatrixMultiplyMrFp64, mTile - mReg);

                        for (var nReg = 0; nReg < nc; nReg += MatrixMultiplyNrFp64)
                        {
                            var nr = Math.Min(MatrixMultiplyNrFp64, nc - nReg);
                            var leftPtr = aPanel + (mReg * MatrixMultiplyKcFp64);
                            var rightPtr = bPanel + nReg;
                            var resultPtr = c + ((mBase + mReg) * n) + nBase + nReg;

                            if (mr == MatrixMultiplyMrFp64 && nr == MatrixMultiplyNrFp64)
                            {
                                MicroKernel4x4FmaAvxFp64(
                                    aPanel + (mReg * MatrixMultiplyKcFp64),
                                    bPanel + nReg,
                                    c + ((mBase + mReg) * n) + nBase + nReg,
                                    kTile,
                                    n);
                            }
                            else
                            {
                                MicroKernelScalarFp64(
                                    leftPtr,
                                    rightPtr,
                                    resultPtr,
                                    kTile,
                                    n,
                                    mr,
                                    nr);
                            }
                        }
                    }
                }
            }
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void MicroKernel8x8FmaAvxFp32(float* aPanel, float* bPanel, float* cTile, int kTile, int ldc)
    {
        var c0 = Avx.LoadVector256(cTile + (0 * ldc));
        var c1 = Avx.LoadVector256(cTile + (1 * ldc));
        var c2 = Avx.LoadVector256(cTile + (2 * ldc));
        var c3 = Avx.LoadVector256(cTile + (3 * ldc));
        var c4 = Avx.LoadVector256(cTile + (4 * ldc));
        var c5 = Avx.LoadVector256(cTile + (5 * ldc));
        var c6 = Avx.LoadVector256(cTile + (6 * ldc));
        var c7 = Avx.LoadVector256(cTile + (7 * ldc));

        int p = 0;

        for (; p + 3 < kTile; p += 4)
        {
            var b0 = Avx.LoadVector256(bPanel + ((p + 0) * MatrixMultiplyNcFp32));
            var a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp32) + p + 0);
            var a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp32) + p + 0);
            var a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp32) + p + 0);
            var a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp32) + p + 0);
            var a4 = Avx.BroadcastScalarToVector256(aPanel + (4 * MatrixMultiplyKcFp32) + p + 0);
            var a5 = Avx.BroadcastScalarToVector256(aPanel + (5 * MatrixMultiplyKcFp32) + p + 0);
            var a6 = Avx.BroadcastScalarToVector256(aPanel + (6 * MatrixMultiplyKcFp32) + p + 0);
            var a7 = Avx.BroadcastScalarToVector256(aPanel + (7 * MatrixMultiplyKcFp32) + p + 0);

            c0 = Fma.MultiplyAdd(a0, b0, c0);
            c1 = Fma.MultiplyAdd(a1, b0, c1);
            c2 = Fma.MultiplyAdd(a2, b0, c2);
            c3 = Fma.MultiplyAdd(a3, b0, c3);
            c4 = Fma.MultiplyAdd(a4, b0, c4);
            c5 = Fma.MultiplyAdd(a5, b0, c5);
            c6 = Fma.MultiplyAdd(a6, b0, c6);
            c7 = Fma.MultiplyAdd(a7, b0, c7);

            var b1 = Avx.LoadVector256(bPanel + ((p + 1) * MatrixMultiplyNcFp32));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp32) + p + 1);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp32) + p + 1);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp32) + p + 1);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp32) + p + 1);
            a4 = Avx.BroadcastScalarToVector256(aPanel + (4 * MatrixMultiplyKcFp32) + p + 1);
            a5 = Avx.BroadcastScalarToVector256(aPanel + (5 * MatrixMultiplyKcFp32) + p + 1);
            a6 = Avx.BroadcastScalarToVector256(aPanel + (6 * MatrixMultiplyKcFp32) + p + 1);
            a7 = Avx.BroadcastScalarToVector256(aPanel + (7 * MatrixMultiplyKcFp32) + p + 1);

            c0 = Fma.MultiplyAdd(a0, b1, c0);
            c1 = Fma.MultiplyAdd(a1, b1, c1);
            c2 = Fma.MultiplyAdd(a2, b1, c2);
            c3 = Fma.MultiplyAdd(a3, b1, c3);
            c4 = Fma.MultiplyAdd(a4, b1, c4);
            c5 = Fma.MultiplyAdd(a5, b1, c5);
            c6 = Fma.MultiplyAdd(a6, b1, c6);
            c7 = Fma.MultiplyAdd(a7, b1, c7);

            var b2 = Avx.LoadVector256(bPanel + ((p + 2) * MatrixMultiplyNcFp32));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp32) + p + 2);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp32) + p + 2);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp32) + p + 2);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp32) + p + 2);
            a4 = Avx.BroadcastScalarToVector256(aPanel + (4 * MatrixMultiplyKcFp32) + p + 2);
            a5 = Avx.BroadcastScalarToVector256(aPanel + (5 * MatrixMultiplyKcFp32) + p + 2);
            a6 = Avx.BroadcastScalarToVector256(aPanel + (6 * MatrixMultiplyKcFp32) + p + 2);
            a7 = Avx.BroadcastScalarToVector256(aPanel + (7 * MatrixMultiplyKcFp32) + p + 2);

            c0 = Fma.MultiplyAdd(a0, b2, c0);
            c1 = Fma.MultiplyAdd(a1, b2, c1);
            c2 = Fma.MultiplyAdd(a2, b2, c2);
            c3 = Fma.MultiplyAdd(a3, b2, c3);
            c4 = Fma.MultiplyAdd(a4, b2, c4);
            c5 = Fma.MultiplyAdd(a5, b2, c5);
            c6 = Fma.MultiplyAdd(a6, b2, c6);
            c7 = Fma.MultiplyAdd(a7, b2, c7);

            var b3 = Avx.LoadVector256(bPanel + ((p + 3) * MatrixMultiplyNcFp32));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp32) + p + 3);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp32) + p + 3);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp32) + p + 3);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp32) + p + 3);
            a4 = Avx.BroadcastScalarToVector256(aPanel + (4 * MatrixMultiplyKcFp32) + p + 3);
            a5 = Avx.BroadcastScalarToVector256(aPanel + (5 * MatrixMultiplyKcFp32) + p + 3);
            a6 = Avx.BroadcastScalarToVector256(aPanel + (6 * MatrixMultiplyKcFp32) + p + 3);
            a7 = Avx.BroadcastScalarToVector256(aPanel + (7 * MatrixMultiplyKcFp32) + p + 3);

            c0 = Fma.MultiplyAdd(a0, b3, c0);
            c1 = Fma.MultiplyAdd(a1, b3, c1);
            c2 = Fma.MultiplyAdd(a2, b3, c2);
            c3 = Fma.MultiplyAdd(a3, b3, c3);
            c4 = Fma.MultiplyAdd(a4, b3, c4);
            c5 = Fma.MultiplyAdd(a5, b3, c5);
            c6 = Fma.MultiplyAdd(a6, b3, c6);
            c7 = Fma.MultiplyAdd(a7, b3, c7);
        }

        for (; p < kTile; p++)
        {
            var bCol = Avx.LoadVector256(bPanel + (p * MatrixMultiplyNcFp32));
            var a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp32) + p);
            var a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp32) + p);
            var a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp32) + p);
            var a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp32) + p);
            var a4 = Avx.BroadcastScalarToVector256(aPanel + (4 * MatrixMultiplyKcFp32) + p);
            var a5 = Avx.BroadcastScalarToVector256(aPanel + (5 * MatrixMultiplyKcFp32) + p);
            var a6 = Avx.BroadcastScalarToVector256(aPanel + (6 * MatrixMultiplyKcFp32) + p);
            var a7 = Avx.BroadcastScalarToVector256(aPanel + (7 * MatrixMultiplyKcFp32) + p);

            c0 = Fma.MultiplyAdd(a0, bCol, c0);
            c1 = Fma.MultiplyAdd(a1, bCol, c1);
            c2 = Fma.MultiplyAdd(a2, bCol, c2);
            c3 = Fma.MultiplyAdd(a3, bCol, c3);
            c4 = Fma.MultiplyAdd(a4, bCol, c4);
            c5 = Fma.MultiplyAdd(a5, bCol, c5);
            c6 = Fma.MultiplyAdd(a6, bCol, c6);
            c7 = Fma.MultiplyAdd(a7, bCol, c7);
        }

        Avx.Store(cTile + (0 * ldc), c0);
        Avx.Store(cTile + (1 * ldc), c1);
        Avx.Store(cTile + (2 * ldc), c2);
        Avx.Store(cTile + (3 * ldc), c3);
        Avx.Store(cTile + (4 * ldc), c4);
        Avx.Store(cTile + (5 * ldc), c5);
        Avx.Store(cTile + (6 * ldc), c6);
        Avx.Store(cTile + (7 * ldc), c7);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void MicroKernel4x4FmaAvxFp64(
        double* aPanel,
        double* bPanel,
        double* cTile,
        int kTile,
        int ldc)
    {
        var c0 = Avx.LoadVector256(cTile + (0 * ldc));
        var c1 = Avx.LoadVector256(cTile + (1 * ldc));
        var c2 = Avx.LoadVector256(cTile + (2 * ldc));
        var c3 = Avx.LoadVector256(cTile + (3 * ldc));

        int p = 0;

        for (; p + 3 < kTile; p += 4)
        {
            var b0 = Avx.LoadVector256(bPanel + ((p + 0) * MatrixMultiplyNcFp64));
            var a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp64) + p + 0);
            var a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp64) + p + 0);
            var a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp64) + p + 0);
            var a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp64) + p + 0);

            c0 = Fma.MultiplyAdd(a0, b0, c0);
            c1 = Fma.MultiplyAdd(a1, b0, c1);
            c2 = Fma.MultiplyAdd(a2, b0, c2);
            c3 = Fma.MultiplyAdd(a3, b0, c3);

            var b1 = Avx.LoadVector256(bPanel + ((p + 1) * MatrixMultiplyNcFp64));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp64) + p + 1);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp64) + p + 1);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp64) + p + 1);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp64) + p + 1);

            c0 = Fma.MultiplyAdd(a0, b1, c0);
            c1 = Fma.MultiplyAdd(a1, b1, c1);
            c2 = Fma.MultiplyAdd(a2, b1, c2);
            c3 = Fma.MultiplyAdd(a3, b1, c3);

            var b2 = Avx.LoadVector256(bPanel + ((p + 2) * MatrixMultiplyNcFp64));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp64) + p + 2);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp64) + p + 2);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp64) + p + 2);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp64) + p + 2);

            c0 = Fma.MultiplyAdd(a0, b2, c0);
            c1 = Fma.MultiplyAdd(a1, b2, c1);
            c2 = Fma.MultiplyAdd(a2, b2, c2);
            c3 = Fma.MultiplyAdd(a3, b2, c3);

            var b3 = Avx.LoadVector256(bPanel + ((p + 3) * MatrixMultiplyNcFp64));
            a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp64) + p + 3);
            a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp64) + p + 3);
            a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp64) + p + 3);
            a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp64) + p + 3);

            c0 = Fma.MultiplyAdd(a0, b3, c0);
            c1 = Fma.MultiplyAdd(a1, b3, c1);
            c2 = Fma.MultiplyAdd(a2, b3, c2);
            c3 = Fma.MultiplyAdd(a3, b3, c3);
        }

        for (; p < kTile; p++)
        {
            var bCol = Avx.LoadVector256(bPanel + (p * MatrixMultiplyNcFp64));
            var a0 = Avx.BroadcastScalarToVector256(aPanel + (0 * MatrixMultiplyKcFp64) + p);
            var a1 = Avx.BroadcastScalarToVector256(aPanel + (1 * MatrixMultiplyKcFp64) + p);
            var a2 = Avx.BroadcastScalarToVector256(aPanel + (2 * MatrixMultiplyKcFp64) + p);
            var a3 = Avx.BroadcastScalarToVector256(aPanel + (3 * MatrixMultiplyKcFp64) + p);

            c0 = Fma.MultiplyAdd(a0, bCol, c0);
            c1 = Fma.MultiplyAdd(a1, bCol, c1);
            c2 = Fma.MultiplyAdd(a2, bCol, c2);
            c3 = Fma.MultiplyAdd(a3, bCol, c3);
        }

        Avx.Store(cTile + (0 * ldc), c0);
        Avx.Store(cTile + (1 * ldc), c1);
        Avx.Store(cTile + (2 * ldc), c2);
        Avx.Store(cTile + (3 * ldc), c3);
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void MicroKernelScalarFp32(
        float* aPanel,
        float* bPanel,
        float* cTile,
        int kTile,
        int ldc,
        int mr,
        int nr)
    {
        const int fp32VectorLength = 8;
        int nrSimd = nr / fp32VectorLength;

        for (var i = 0; i < mr; i++)
        {
            for (var jSimd = 0; jSimd < nrSimd; jSimd++)
            {
                int j = jSimd * fp32VectorLength;
                var acc = Avx.LoadVector256(cTile + (i * ldc) + j);

                for (var k = 0; k < kTile; k++)
                {
                    var aVec = Vector256.Create(*(aPanel + (i * MatrixMultiplyKcFp32) + k));
                    var bVec = Avx.LoadVector256(bPanel + (k * MatrixMultiplyNcFp32) + j);

                    acc = Fma.MultiplyAdd(aVec, bVec, acc);
                }

                Avx.Store(cTile + (i * ldc) + j, acc);
            }

            for (var j = nrSimd * fp32VectorLength; j < nr; j++)
            {
                var acc = cTile[(i * ldc) + j];

                for (var k = 0; k < kTile; k++)
                {
                    var aVal = aPanel[(i * MatrixMultiplyKcFp32) + k];
                    var bVal = bPanel[(k * MatrixMultiplyNcFp32) + j];
                    acc += aVal * bVal;
                }

                cTile[(i * ldc) + j] = acc;
            }
        }
    }

    private static unsafe void MicroKernelScalarFp64(
        double* aPanel,
        double* bPanel,
        double* cTile,
        int kTile,
        int ldc,
        int mr,
        int nr)
    {
        const int fp64VectorLength = 4;
        int nrSimd = nr / fp64VectorLength;

        for (var i = 0; i < mr; i++)
        {
            for (var jSimd = 0; jSimd < nrSimd; jSimd++)
            {
                int j = jSimd * fp64VectorLength;
                var acc = Avx.LoadVector256(cTile + (i * ldc) + j);

                for (var k = 0; k < kTile; k++)
                {
                    var aVec = Vector256.Create(*(aPanel + (i * MatrixMultiplyKcFp64) + k));
                    var bVec = Avx.LoadVector256(bPanel + (k * MatrixMultiplyNcFp64) + j);

                    acc = Fma.MultiplyAdd(aVec, bVec, acc);
                }

                Avx.Store(cTile + (i * ldc) + j, acc);
            }

            for (var j = nrSimd * fp64VectorLength; j < nr; j++)
            {
                var acc = cTile[(i * ldc) + j];

                for (var k = 0; k < kTile; k++)
                {
                    var aVal = aPanel[(i * MatrixMultiplyKcFp64) + k];
                    var bVal = bPanel[(k * MatrixMultiplyNcFp64) + j];
                    acc += aVal * bVal;
                }

                cTile[(i * ldc) + j] = acc;
            }
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void PackAFp32(float* src, float* dstPanel, int lda, int mTile, int kTile)
    {
        const int vectorSize = 8;
        var i = 0;

        for (; i < mTile; i++)
        {
            var k = 0;
            for (; k + vectorSize <= kTile; k += vectorSize)
            {
                Avx.Store(dstPanel + (i * MatrixMultiplyKcFp32) + k, Avx.LoadVector256(src + (i * lda) + k));
            }

            for (; k < kTile; k++)
            {
                dstPanel[(i * MatrixMultiplyKcFp32) + k] = src[(i * lda) + k];
            }

            for (; k < MatrixMultiplyKcFp32 - vectorSize; k += vectorSize)
            {
                Avx.Store(dstPanel + (i * MatrixMultiplyKcFp32) + k, Vector256<float>.Zero);
            }

            for (; k < MatrixMultiplyKcFp32; k++)
            {
                dstPanel[(i * MatrixMultiplyKcFp32) + k] = 0f;
            }
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void PackBFp32(float* src, float* dstPanel, int ldb, int kTile, int nTile)
    {
        const int vectorSize = 8;
        const int vectorizedWidth = MatrixMultiplyNcFp32 / vectorSize * vectorSize;

        for (var k = 0; k < MatrixMultiplyNcFp32; k++)
        {
            var srcRow = src + (k * ldb);
            var dstRow = dstPanel + (k * MatrixMultiplyNcFp32);
            var j = 0;

            if (k < kTile)
            {
                for (; j < vectorizedWidth && j + vectorSize <= nTile; j += vectorSize)
                {
                    Avx.Store(dstRow + j, Avx.LoadVector256(srcRow + j));
                }

                if (j < vectorizedWidth && j < nTile)
                {
                    var remaining = nTile - j;
                    for (var r = 0; r < remaining; r++)
                    {
                        dstRow[j + r] = srcRow[j + r];
                    }

                    j += vectorSize;
                }
            }

            for (; j < vectorizedWidth - 1; j += vectorSize)
            {
                Avx.Store(dstRow + j, Vector256<float>.Zero);
            }

            for (; j < MatrixMultiplyNcFp32; j++)
            {
                dstRow[j] = k < kTile && j < nTile ? srcRow[j] : 0;
            }
        }
    }

    private static unsafe void PackAFp64(
        double* src,
        double* dstPanel,
        int lda,
        int mTile,
        int kTile)
    {
        const int vectorSize = 4;
        var i = 0;

        for (; i < mTile; i++)
        {
            var k = 0;
            for (; k + vectorSize <= kTile; k += vectorSize)
            {
                Avx.Store(dstPanel + (i * MatrixMultiplyKcFp64) + k, Avx.LoadVector256(src + (i * lda) + k));
            }

            for (; k < kTile; k++)
            {
                dstPanel[(i * MatrixMultiplyKcFp64) + k] = src[(i * lda) + k];
            }

            for (; k < MatrixMultiplyKcFp64 - vectorSize; k += vectorSize)
            {
                Avx.Store(dstPanel + (i * MatrixMultiplyKcFp64) + k, Vector256<double>.Zero);
            }

            for (; k < MatrixMultiplyKcFp64; k++)
            {
                dstPanel[(i * MatrixMultiplyKcFp64) + k] = 0f;
            }
        }
    }

    private static unsafe void PackBFp64(
        double* src,
        double* dstPanel,
        int ldb,
        int kTile,
        int nTile)
    {
        const int vectorSize = 4;
        const int vectorizedWidth = MatrixMultiplyNcFp64 / vectorSize * vectorSize;

        for (var k = 0; k < MatrixMultiplyNcFp64; k++)
        {
            var srcRow = src + (k * ldb);
            var dstRow = dstPanel + (k * MatrixMultiplyNcFp64);
            var j = 0;

            if (k < kTile)
            {
                for (; j < vectorizedWidth && j + vectorSize <= nTile; j += vectorSize)
                {
                    Avx.Store(dstRow + j, Avx.LoadVector256(srcRow + j));
                }

                if (j < vectorizedWidth && j < nTile)
                {
                    var remaining = nTile - j;
                    for (var r = 0; r < remaining; r++)
                    {
                        dstRow[j + r] = srcRow[j + r];
                    }

                    j += vectorSize;
                }
            }

            for (; j < vectorizedWidth - 1; j += vectorSize)
            {
                Avx.Store(dstRow + j, Vector256<double>.Zero);
            }

            for (; j < MatrixMultiplyNcFp64; j++)
            {
                dstRow[j] = k < kTile && j < nTile ? srcRow[j] : 0;
            }
        }
    }

    private static unsafe void InnerProductFp32FmaAvx(float* leftMemoryPtr, float* rightMemoryPtr, float* resultPtr, long n)
    {
        if (n <= 0)
        {
            resultPtr[0] = 0.0f;
            return;
        }

        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<float>(n);
        var partials = new float[processes];

        if (processes == 1)
        {
            InnerLoop(0);
        }
        else
        {
            _ = Parallel.For(
                0,
                processes,
                new ParallelOptions { MaxDegreeOfParallelism = processes },
                InnerLoop);
        }

        // Neumaier Sum for accuracy
        float s = 0, c = 0;
        for (var i = 0; i < partials.Length; i++)
        {
            var t = s + partials[i];
            c += float.Abs(s) >= float.Abs(partials[i]) ? s - t + partials[i] : partials[i] - t + s;
            s = t;
        }

        resultPtr[0] = s + c;

        void InnerLoop(int tid)
        {
            var start = tid * n / processes;
            var end = (tid + 1) * n / processes;

            var i = start;
            var acc0 = Vector256<float>.Zero;
            var acc1 = Vector256<float>.Zero;
            var acc2 = Vector256<float>.Zero;
            var acc3 = Vector256<float>.Zero;

            for (; i + 32 <= end; i += 32)
            {
                var leftVector0 = Avx.LoadVector256(leftMemoryPtr + i + 0);
                var leftVector1 = Avx.LoadVector256(leftMemoryPtr + i + 8);
                var leftVector2 = Avx.LoadVector256(leftMemoryPtr + i + 16);
                var leftVector3 = Avx.LoadVector256(leftMemoryPtr + i + 24);

                var rightVector0 = Avx.LoadVector256(rightMemoryPtr + i + 0);
                var rightVector1 = Avx.LoadVector256(rightMemoryPtr + i + 8);
                var rightVector2 = Avx.LoadVector256(rightMemoryPtr + i + 16);
                var rightVector3 = Avx.LoadVector256(rightMemoryPtr + i + 24);

                acc0 = Fma.MultiplyAdd(leftVector0, rightVector0, acc0);
                acc1 = Fma.MultiplyAdd(leftVector1, rightVector1, acc1);
                acc2 = Fma.MultiplyAdd(leftVector2, rightVector2, acc2);
                acc3 = Fma.MultiplyAdd(leftVector3, rightVector3, acc3);
            }

            for (; i + 8 <= end; i += 8)
            {
                var va = Avx.LoadVector256(leftMemoryPtr + i);
                var vb = Avx.LoadVector256(rightMemoryPtr + i);
                acc0 = Fma.MultiplyAdd(va, vb, acc0);
            }

            var acc = Avx.Add(Avx.Add(acc0, acc1), Avx.Add(acc2, acc3));
            var tmp = Avx.HorizontalAdd(acc, acc);
            tmp = Avx.HorizontalAdd(tmp, tmp);

            var sumVec = Avx.Add(tmp, Avx.Permute2x128(tmp, tmp, 0x01));
            var sum = sumVec.GetElement(0);

            for (; i < end; i++)
            {
                sum += leftMemoryPtr[i] * rightMemoryPtr[i];
            }

            partials[tid] = sum;
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static unsafe void InnerProductFp64FmaAvx(double* leftMemoryPtr, double* rightMemoryPtr, double* resultPtr, long n)
    {
        if (n <= 0)
        {
            resultPtr[0] = 0.0f;
            return;
        }

        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<double>(n);
        var partials = new double[processes];

        if (processes == 1)
        {
            InnerLoop(0);
        }
        else
        {
            _ = Parallel.For(
                0,
                processes,
                new ParallelOptions { MaxDegreeOfParallelism = processes },
                InnerLoop);
        }

        double s = 0, c = 0;
        for (var i = 0; i < partials.Length; i++)
        {
            var t = s + partials[i];
            c += double.Abs(s) >= double.Abs(partials[i]) ? s - t + partials[i] : partials[i] - t + s;
            s = t;
        }

        resultPtr[0] = s + c;

        void InnerLoop(int tid)
        {
            var start = tid * n / processes;
            var end = (tid + 1) * n / processes;

            var i = start;
            var acc0 = Vector256<double>.Zero;
            var acc1 = Vector256<double>.Zero;
            for (; i + 8 <= end; i += 8)
            {
                var leftVector0 = Avx.LoadVector256(leftMemoryPtr + i + 0);
                var leftVector1 = Avx.LoadVector256(leftMemoryPtr + i + 4);

                var rightVector0 = Avx.LoadVector256(rightMemoryPtr + i + 0);
                var rightVector1 = Avx.LoadVector256(rightMemoryPtr + i + 4);

                acc0 = Fma.MultiplyAdd(leftVector0, rightVector0, acc0);
                acc1 = Fma.MultiplyAdd(leftVector1, rightVector1, acc1);
            }

            for (; i + 4 <= end; i += 4)
            {
                var leftVector = Avx.LoadVector256(leftMemoryPtr + i);
                var rightVector = Avx.LoadVector256(rightMemoryPtr + i);
                acc0 = Fma.MultiplyAdd(leftVector, rightVector, acc0);
            }

            var acc = Avx.Add(acc0, acc1);

            var sum = acc.GetElement(0) + acc.GetElement(1) + acc.GetElement(2) + acc.GetElement(3);

            for (; i < end; i++)
            {
                sum += leftMemoryPtr[i] * rightMemoryPtr[i];
            }

            partials[tid] = sum;
        }
    }
}
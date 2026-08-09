// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Iterators;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Concurrency;
using Sci.NET.Mathematics.Intrinsics;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

[SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "All cases are covered.")]
internal static class ManagedReductionIterator<TNumber, TReduction>
    where TNumber : unmanaged, INumber<TNumber>
    where TReduction : IReductionOperation<TNumber>, IReductionOperationAvx2
{
    public static unsafe void Apply(
        ITensor<TNumber> input,
        ITensor<TNumber> output,
        int[] axes)
    {
        var inputMemory = (SystemMemoryBlock<TNumber>)input.Memory;
        var outputMemory = (SystemMemoryBlock<TNumber>)output.Memory;
        var geometry = ReductionGeometry.Compute(input.Shape, output.Shape, axes);

        switch (geometry.Pattern)
        {
            case ReductionPattern.FullReduction:
                ApplyFullReduction(inputMemory, outputMemory, geometry, (ICpuComputeDevice)input.Device);
                break;

            case ReductionPattern.ContiguousInner:
                ApplyContiguousInner(inputMemory, outputMemory, geometry, (ICpuComputeDevice)input.Device);
                break;

            case ReductionPattern.ContiguousOuter:
                ApplyContiguousOuter(inputMemory.ToPointer(), outputMemory.ToPointer(), geometry);
                break;
            case ReductionPattern.Strided:
                ApplyStrided(inputMemory.ToPointer(), outputMemory.ToPointer(), geometry);
                break;
            default:
                throw new InvalidOperationException($"Unsupported reduction pattern: {geometry.Pattern}");
        }
    }

    private static unsafe void ApplyFullReduction(
        SystemMemoryBlock<TNumber> input,
        SystemMemoryBlock<TNumber> output,
        in ReductionGeometry geometry,
        ICpuComputeDevice device)
    {
        var count = geometry.TotalElements;

        if (device.IsAvx2Supported() && TReduction.HasAvx2Implementation())
        {
            switch (TNumber.Zero)
            {
                case float:
                    ApplyFullReductionAvx256Fp32((float*)input.ToPointer(), (float*)output.ToPointer(), count);
                    return;
                case double:
                    ApplyFullReductionAvx256Fp64((double*)input.ToPointer(), (double*)output.ToPointer(), count);
                    return;
            }
        }

        ApplyFullReductionScalar(input.ToPointer(), output.ToPointer(), count);
    }

    private static unsafe void ApplyFullReductionAvx256Fp32(
        float* input,
        float* output,
        long n)
    {
        var numThreads = ManagedTensorBackend.GetNumThreadsByElementCount<float>(n);
        var partials = new float[numThreads];

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numThreads,
            tid => ApplyFullReductionAvx256Fp32InnerLoop(tid, numThreads, n, input, partials));

        ManagedTensorBackend.ParallelExecutor.Run(tasks);

        var result = TReduction.Identity;
        foreach (var partial in partials)
        {
            var p = partial;
            result = TReduction.Accumulate(result, Unsafe.As<float, TNumber>(ref p));
        }

        var final = TReduction.Finalize(result, n);
        output[0] = Unsafe.As<TNumber, float>(ref final);
    }

    private static unsafe void ApplyFullReductionAvx256Fp32InnerLoop(int tid, int numThreads, long n, float* input, float[] partials)
    {
        const int unrollFactor = 4;
        const int elementsPerIteration = IntrinsicsHelper.AvxVectorSizeFp32 * unrollFactor;
        const int prefetchDistance = 64;

        var start = tid * n / numThreads;
        var end = (tid + 1) * n / numThreads;
        var count = end - start;
        var basePtr = input + start;

        var acc0 = TReduction.Avx256Fp32Identity;
        var acc1 = TReduction.Avx256Fp32Identity;
        var acc2 = TReduction.Avx256Fp32Identity;
        var acc3 = TReduction.Avx256Fp32Identity;

        long i = 0;
        var unrollLimit = count - elementsPerIteration;

        for (; i <= unrollLimit; i += elementsPerIteration)
        {
            Sse.Prefetch0(basePtr + i + prefetchDistance);

            acc0 = TReduction.AccumulateAvxFp32(acc0, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp32 * 0)));
            acc1 = TReduction.AccumulateAvxFp32(acc1, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp32 * 1)));
            acc2 = TReduction.AccumulateAvxFp32(acc2, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp32 * 2)));
            acc3 = TReduction.AccumulateAvxFp32(acc3, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp32 * 3)));
        }

        acc0 = TReduction.AccumulateAvxFp32(acc0, acc1);
        acc2 = TReduction.AccumulateAvxFp32(acc2, acc3);
        acc0 = TReduction.AccumulateAvxFp32(acc0, acc2);

        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp32; i += IntrinsicsHelper.AvxVectorSizeFp32)
        {
            acc0 = TReduction.AccumulateAvxFp32(acc0, Avx.LoadVector256(basePtr + i));
        }

        var scalar = TReduction.HorizontalReduceAvxFp32(acc0);

        for (; i < count; i++)
        {
            var scalarGeneric = TReduction.Accumulate(
                Unsafe.As<float, TNumber>(ref scalar),
                Unsafe.As<float, TNumber>(ref basePtr[i]));
            scalar = Unsafe.As<TNumber, float>(ref scalarGeneric);
        }

        partials[tid] = scalar;
    }

    private static unsafe void ApplyFullReductionAvx256Fp64(
        double* input,
        double* output,
        long n)
    {
        var numThreads = ManagedTensorBackend.GetNumThreadsByElementCount<double>(n);
        var partials = new double[numThreads];

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numThreads,
            tid => ApplyFullReductionAvx256Fp64InnerLoop(tid, numThreads, n, input, partials));

        ManagedTensorBackend.ParallelExecutor.Run(tasks);

        var result = TReduction.Identity;
        foreach (var partial in partials)
        {
            var p = partial;
            result = TReduction.Accumulate(result, Unsafe.As<double, TNumber>(ref p));
        }

        var final = TReduction.Finalize(result, n);
        output[0] = Unsafe.As<TNumber, double>(ref final);
    }

    private static unsafe void ApplyFullReductionAvx256Fp64InnerLoop(int tid, int numThreads, long n, double* input, double[] partials)
    {
        const int unrollFactor = 4;
        const int elementsPerIteration = IntrinsicsHelper.AvxVectorSizeFp64 * unrollFactor;
        const int prefetchDistance = 64;

        var start = tid * n / numThreads;
        var end = (tid + 1) * n / numThreads;
        var count = end - start;
        var basePtr = input + start;

        var acc0 = TReduction.Avx256Fp64Identity;
        var acc1 = TReduction.Avx256Fp64Identity;
        var acc2 = TReduction.Avx256Fp64Identity;
        var acc3 = TReduction.Avx256Fp64Identity;

        long i = 0;
        var unrollLimit = count - elementsPerIteration;

        for (; i <= unrollLimit; i += elementsPerIteration)
        {
            Sse.Prefetch0(basePtr + i + prefetchDistance);

            acc0 = TReduction.AccumulateAvxFp64(acc0, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp64 * 0)));
            acc1 = TReduction.AccumulateAvxFp64(acc1, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp64 * 1)));
            acc2 = TReduction.AccumulateAvxFp64(acc2, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp64 * 2)));
            acc3 = TReduction.AccumulateAvxFp64(acc3, Avx.LoadVector256(basePtr + i + (IntrinsicsHelper.AvxVectorSizeFp64 * 3)));
        }

        acc0 = TReduction.AccumulateAvxFp64(acc0, acc1);
        acc2 = TReduction.AccumulateAvxFp64(acc2, acc3);
        acc0 = TReduction.AccumulateAvxFp64(acc0, acc2);

        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp64; i += IntrinsicsHelper.AvxVectorSizeFp64)
        {
            acc0 = TReduction.AccumulateAvxFp64(acc0, Avx.LoadVector256(basePtr + i));
        }

        var scalar = TReduction.HorizontalReduceAvxFp64(acc0);

        for (; i < count; i++)
        {
            var scalarGeneric = TReduction.Accumulate(
                Unsafe.As<double, TNumber>(ref scalar),
                Unsafe.As<double, TNumber>(ref basePtr[i]));
            scalar = Unsafe.As<TNumber, double>(ref scalarGeneric);
        }

        partials[tid] = scalar;
    }

    private static unsafe void ApplyFullReductionScalar(
        TNumber* input,
        TNumber* output,
        long n)
    {
        var numThreads = ManagedTensorBackend.GetNumThreadsByElementCount<double>(n);
        var partials = new TNumber[numThreads];

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numThreads,
            tid => ApplyFullReductionScalarInnerLoop(tid, numThreads, n, input, partials));

        ManagedTensorBackend.ParallelExecutor.Run(tasks);

        var result = TReduction.Identity;
        foreach (var partial in partials)
        {
            result = TReduction.Accumulate(result, partial);
        }

        output[0] = TReduction.Finalize(result, n);
    }

    private static unsafe void ApplyFullReductionScalarInnerLoop(int tid, int numThreads, long n, TNumber* input, TNumber[] partials)
    {
        var start = tid * n / numThreads;
        var end = (tid + 1) * n / numThreads;
        var count = end - start;
        var basePtr = input + start;
        var partial = TReduction.Identity;

        for (var i = 0; i < count; i++)
        {
            partial = TReduction.Accumulate(partial, *(basePtr + i));
        }

        partials[tid] = partial;
    }

    private static unsafe void ApplyContiguousInner(
        SystemMemoryBlock<TNumber> input,
        SystemMemoryBlock<TNumber> output,
        in ReductionGeometry geometry,
        ICpuComputeDevice device)
    {
        var outerCount = geometry.OuterCount;
        var innerCount = geometry.InnerCount;

        if (device.IsAvx2Supported() && TReduction.HasAvx2Implementation() &&
            innerCount >= Vector256<TNumber>.Count * 4)
        {
            switch (TNumber.Zero)
            {
                case float:
                    ApplyContiguousInnerAvx256Fp32((float*)input.ToPointer(), (float*)output.ToPointer(), outerCount, innerCount);
                    return;
                case double:
                    ApplyContiguousInnerAvx256Fp64((double*)input.ToPointer(), (double*)output.ToPointer(), outerCount, innerCount);
                    return;
            }
        }

        ApplyContiguousInnerScalar(input.ToPointer(), output.ToPointer(), outerCount, innerCount);
    }

    private static unsafe void ApplyContiguousInnerAvx256Fp32(
        float* input,
        float* output,
        long outerCount,
        long innerCount)
    {
        ManagedTensorBackend.ParallelExecutor.For(
            0,
            outerCount,
            ManagedTensorBackend.MaxDegreeOfParallelism,
            outer => ApplyContiguousInnerAvx256Fp32InnerLoop(outer, innerCount, input, output));
    }

    private static unsafe void ApplyContiguousInnerAvx256Fp32InnerLoop(long outer, long innerCount, float* input, float* output)
    {
        var basePtr = input + (outer * innerCount);

        var acc0 = TReduction.Avx256Fp32Identity;
        var acc1 = TReduction.Avx256Fp32Identity;
        var acc2 = TReduction.Avx256Fp32Identity;
        var acc3 = TReduction.Avx256Fp32Identity;

        var j = 0;
        var unrollLimit = innerCount - (IntrinsicsHelper.AvxVectorSizeFp32 * 4);

        for (; j <= unrollLimit; j += IntrinsicsHelper.AvxVectorSizeFp32 * 4)
        {
            acc0 = TReduction.AccumulateAvxFp32(acc0, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp32 * 0)));
            acc1 = TReduction.AccumulateAvxFp32(acc1, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp32 * 1)));
            acc2 = TReduction.AccumulateAvxFp32(acc2, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp32 * 2)));
            acc3 = TReduction.AccumulateAvxFp32(acc3, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp32 * 3)));
        }

        acc0 = TReduction.AccumulateAvxFp32(acc0, acc1);
        acc2 = TReduction.AccumulateAvxFp32(acc2, acc3);
        acc0 = TReduction.AccumulateAvxFp32(acc0, acc2);

        for (; j <= innerCount - IntrinsicsHelper.AvxVectorSizeFp32; j += IntrinsicsHelper.AvxVectorSizeFp32)
        {
            acc0 = TReduction.AccumulateAvxFp32(acc0, Vector256.Load(basePtr + j));
        }

        var scalarAcc = TReduction.HorizontalReduceAvxFp32(acc0);

        for (; j < innerCount; j++)
        {
            var untypedAcc = TReduction.Accumulate(Unsafe.As<float, TNumber>(ref scalarAcc), Unsafe.As<float, TNumber>(ref basePtr[j]));
            scalarAcc = Unsafe.As<TNumber, float>(ref untypedAcc);
        }

        TNumber finalize = TReduction.Finalize(Unsafe.As<float, TNumber>(ref scalarAcc), innerCount);
        output[outer] = Unsafe.As<TNumber, float>(ref finalize);
    }

    private static unsafe void ApplyContiguousInnerAvx256Fp64(
        double* input,
        double* output,
        long outerCount,
        long innerCount)
    {
        ManagedTensorBackend.ParallelExecutor.For(
            0,
            outerCount,
            ManagedTensorBackend.MaxDegreeOfParallelism,
            outer => ApplyContiguousInnerAvx256Fp64InnerLoop(outer, innerCount, input, output));
    }

    private static unsafe void ApplyContiguousInnerAvx256Fp64InnerLoop(long outer, long innerCount, double* input, double* output)
    {
        var basePtr = input + (outer * innerCount);

        var acc0 = TReduction.Avx256Fp64Identity;
        var acc1 = TReduction.Avx256Fp64Identity;
        var acc2 = TReduction.Avx256Fp64Identity;
        var acc3 = TReduction.Avx256Fp64Identity;

        var j = 0;
        var unrollLimit = innerCount - (IntrinsicsHelper.AvxVectorSizeFp64 * 4);

        for (; j <= unrollLimit; j += IntrinsicsHelper.AvxVectorSizeFp64 * 4)
        {
            acc0 = TReduction.AccumulateAvxFp64(acc0, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp64 * 0)));
            acc1 = TReduction.AccumulateAvxFp64(acc1, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp64 * 1)));
            acc2 = TReduction.AccumulateAvxFp64(acc2, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp64 * 2)));
            acc3 = TReduction.AccumulateAvxFp64(acc3, Vector256.Load(basePtr + j + (IntrinsicsHelper.AvxVectorSizeFp64 * 3)));
        }

        acc0 = TReduction.AccumulateAvxFp64(acc0, acc1);
        acc2 = TReduction.AccumulateAvxFp64(acc2, acc3);
        acc0 = TReduction.AccumulateAvxFp64(acc0, acc2);

        for (; j <= innerCount - IntrinsicsHelper.AvxVectorSizeFp64; j += IntrinsicsHelper.AvxVectorSizeFp64)
        {
            acc0 = TReduction.AccumulateAvxFp64(acc0, Vector256.Load(basePtr + j));
        }

        var scalarAcc = TReduction.HorizontalReduceAvxFp64(acc0);

        for (; j < innerCount; j++)
        {
            var untypedAcc = TReduction.Accumulate(Unsafe.As<double, TNumber>(ref scalarAcc), Unsafe.As<double, TNumber>(ref basePtr[j]));
            scalarAcc = Unsafe.As<TNumber, double>(ref untypedAcc);
        }

        TNumber finalize = TReduction.Finalize(Unsafe.As<double, TNumber>(ref scalarAcc), innerCount);
        output[outer] = Unsafe.As<TNumber, double>(ref finalize);
    }

    private static unsafe void ApplyContiguousInnerScalar(
        TNumber* input,
        TNumber* output,
        long outerCount,
        long innerCount)
    {
        ManagedTensorBackend.ParallelExecutor.For(
            0,
            outerCount,
            ManagedTensorBackend.MaxDegreeOfParallelism,
            outer => ApplyContiguousInnerScalarInnerLoop(outer, innerCount, input, output));
    }

    private static unsafe void ApplyContiguousInnerScalarInnerLoop(long outer, long innerCount, TNumber* input, TNumber* output)
    {
        var baseIdx = outer * innerCount;
        var acc = TReduction.Identity;

        for (var j = 0; j < innerCount; j++)
        {
            acc = TReduction.Accumulate(acc, input[baseIdx + j]);
        }

        output[outer] = TReduction.Finalize(acc, innerCount);
    }

    private static unsafe void ApplyContiguousOuter(
        TNumber* input,
        TNumber* output,
        in ReductionGeometry geometry)
    {
        var outerCount = geometry.OuterCount;
        var innerCount = geometry.InnerCount;
        var stride = geometry.OuterStride;

        // No AVX implementation for this pattern yet
        ManagedTensorBackend
            .ParallelExecutor
            .For(
                0,
                outerCount,
                ManagedTensorBackend.MaxDegreeOfParallelism,
                outIdx =>
                {
                    var acc = TReduction.Identity;

                    for (var reduceIdx = 0; reduceIdx < innerCount; reduceIdx++)
                    {
                        var inputIdx = (reduceIdx * stride) + outIdx;
                        acc = TReduction.Accumulate(acc, input[inputIdx]);
                    }

                    output[outIdx] = TReduction.Finalize(acc, innerCount);
                });
    }

    private static unsafe void ApplyStrided(
        TNumber* input,
        TNumber* output,
        in ReductionGeometry geometry)
    {
        // No AVX implementation for this pattern yet
        var outerCount = geometry.OuterCount;
        var innerCount = geometry.InnerCount;
        var tensorStrides = geometry.TensorStrides;
        var resultToTensorDim = geometry.ResultToTensorDim;
        var reduceAxisDims = geometry.ReduceAxisDims;
        var reduceAxisStrides = geometry.ReduceAxisStrides;
        var resultRank = resultToTensorDim.Length;
        var reduceRank = reduceAxisDims.Length;

        var resultShape = new long[resultRank];
        for (var d = 0; d < resultRank; d++)
        {
            resultShape[d] = geometry.InputShape[resultToTensorDim[d]];
        }

        ManagedTensorBackend
            .ParallelExecutor
            .For(
                0,
                outerCount,
                ManagedTensorBackend.MaxDegreeOfParallelism,
                resultIdx =>
                {
                    var baseOffset = 0L;
                    var remaining = resultIdx;

                    for (var d = resultRank - 1; d >= 0; d--)
                    {
                        var dimSize = resultShape[d];
                        var coord = remaining % dimSize;
                        remaining /= dimSize;
                        baseOffset += coord * tensorStrides[resultToTensorDim[d]];
                    }

                    var acc = TReduction.Identity;

                    for (var reduceIdx = 0; reduceIdx < innerCount; reduceIdx++)
                    {
                        var offset = baseOffset;
                        var rem = reduceIdx;

                        for (var a = reduceRank - 1; a >= 0; a--)
                        {
                            var coord = rem % reduceAxisDims[a];
                            rem /= reduceAxisDims[a];
                            offset += coord * reduceAxisStrides[a];
                        }

                        acc = TReduction.Accumulate(acc, input[offset]);
                    }

                    output[resultIdx] = TReduction.Finalize(acc, innerCount);
                });
    }
}
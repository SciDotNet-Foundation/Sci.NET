// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedStreamingUnaryOperationIterator
{
    [SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "Reviewed")]
    public static unsafe void Apply<TOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<TNumber>(n);

        if (device.IsAvx2Supported() && TOp.HasAvx2Implementation())
        {
            switch (TNumber.Zero)
            {
                case float when processes == 1:
                    InnerLoopAvx2<TOp>(
                        0,
                        n,
                        processes,
                        (float*)inputPtr,
                        (float*)resultPtr);
                    return;
                case float when processes > 1:
                    _ = Parallel.For(
                        0,
                        processes,
                        new ParallelOptions { MaxDegreeOfParallelism = processes },
                        tid => InnerLoopAvx2<TOp>(
                            tid,
                            n,
                            processes,
                            (float*)inputPtr,
                            (float*)resultPtr));
                    return;
                case double when processes == 1:
                    InnerLoopAvx2<TOp>(
                        0,
                        n,
                        processes,
                        (double*)inputPtr,
                        (double*)resultPtr);
                    return;
                case double when processes > 1:
                    _ = Parallel.For(
                        0,
                        processes,
                        new ParallelOptions { MaxDegreeOfParallelism = processes },
                        tid => InnerLoopAvx2<TOp>(
                            tid,
                            n,
                            processes,
                            (double*)inputPtr,
                            (double*)resultPtr));
                    return;
            }
        }

        if (processes == 1)
        {
            switch (TNumber.Zero)
            {
                case float:
                    InnerLoopScalarFp32<TOp>(
                        0,
                        n,
                        processes,
                        (float*)inputPtr,
                        (float*)resultPtr);
                    return;
                case double:
                    InnerLoopScalarFp64<TOp>(
                        0,
                        n,
                        processes,
                        (double*)inputPtr,
                        (double*)resultPtr);
                    return;
                default:
                    InnerLoopScalar<TOp, TNumber>(
                        0,
                        n,
                        processes,
                        inputPtr,
                        resultPtr);
                    return;
            }
        }
        else
        {
            switch (TNumber.Zero)
            {
                case float:
                    _ = Parallel.For(
                        0,
                        processes,
                        new ParallelOptions { MaxDegreeOfParallelism = processes },
                        tid => InnerLoopScalarFp32<TOp>(
                            tid,
                            n,
                            processes,
                            (float*)inputPtr,
                            (float*)resultPtr));
                    return;
                case double:
                    _ = Parallel.For(
                        0,
                        processes,
                        new ParallelOptions { MaxDegreeOfParallelism = processes },
                        tid => InnerLoopScalarFp64<TOp>(
                            tid,
                            n,
                            processes,
                            (double*)inputPtr,
                            (double*)resultPtr));
                    return;
                default:
                    _ = Parallel.For(
                        0,
                        processes,
                        new ParallelOptions { MaxDegreeOfParallelism = processes },
                        tid => InnerLoopScalar<TOp, TNumber>(
                            tid,
                            n,
                            processes,
                            inputPtr,
                            resultPtr));
                    return;
            }
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        float* inputPtr,
        float* resultPtr)
        where TOp : IUnaryOperationAvx2, IUnaryOperationTail
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(float);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp32; i += IntrinsicsHelper.AvxVectorSizeFp32)
        {
            Sse.Prefetch0(inputPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var inputVector = Avx.LoadVector256(inputPtr + start + i);
            var result = TOp.ApplyAvx2Fp32(inputVector);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var input = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp32(input);
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        double* inputPtr,
        double* resultPtr)
        where TOp : IUnaryOperationAvx2
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp64; i += IntrinsicsHelper.AvxVectorSizeFp64)
        {
            Sse.Prefetch0(inputPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var inputVector = Avx.LoadVector256(inputPtr + start + i);
            var result = TOp.ApplyAvx2Fp64(inputVector);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp64(inputValue);
        }
    }

    private static unsafe void InnerLoopScalar<TOp, TNumber>(
        long tid,
        long n,
        long processes,
        TNumber* inputPtr,
        TNumber* resultPtr)
        where TOp : IUnaryOperation<TNumber>
        where TNumber : unmanaged, INumber<TNumber>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalar(inputValue);
        }
    }

    private static unsafe void InnerLoopScalarFp32<TOp>(
        long tid,
        long n,
        long processes,
        float* inputPtr,
        float* resultPtr)
        where TOp : IUnaryOperationTail
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp32(inputValue);
        }
    }

    private static unsafe void InnerLoopScalarFp64<TOp>(
        long tid,
        long n,
        long processes,
        double* inputPtr,
        double* resultPtr)
        where TOp : IUnaryOperationTail
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp64(inputValue);
        }
    }
}
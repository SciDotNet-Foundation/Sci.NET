// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedStreamingBinaryOperationIterator
{
    [SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "Reviewed")]
    public static unsafe void Apply<TOp, TNumber>(TNumber* leftPtr, TNumber* rightPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TOp : IBinaryOperation<TNumber>, IBinaryOperationAvx2
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
                        (float*)leftPtr,
                        (float*)rightPtr,
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
                            (float*)leftPtr,
                            (float*)rightPtr,
                            (float*)resultPtr));
                    return;
                case double when processes == 1:
                    InnerLoopAvx2<TOp>(
                        0,
                        n,
                        processes,
                        (double*)leftPtr,
                        (double*)rightPtr,
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
                            (double*)leftPtr,
                            (double*)rightPtr,
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
                        (float*)leftPtr,
                        (float*)rightPtr,
                        (float*)resultPtr);
                    return;
                case double:
                    InnerLoopScalarFp64<TOp>(
                        0,
                        n,
                        processes,
                        (double*)leftPtr,
                        (double*)rightPtr,
                        (double*)resultPtr);
                    return;
                default:
                    InnerLoopScalar<TOp, TNumber>(
                        0,
                        n,
                        processes,
                        leftPtr,
                        rightPtr,
                        resultPtr);
                    return;
            }
        }

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
                        (float*)leftPtr,
                        (float*)rightPtr,
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
                        (double*)leftPtr,
                        (double*)rightPtr,
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
                        leftPtr,
                        rightPtr,
                        resultPtr));
                return;
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        float* leftPtr,
        float* rightPtr,
        float* resultPtr)
        where TOp : IBinaryOperationAvx2
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(float);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp32; i += IntrinsicsHelper.AvxVectorSizeFp32)
        {
            Sse.Prefetch0(leftPtr + start + i + prefetchVectorCount);
            Sse.Prefetch0(rightPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var leftVector = Avx.LoadVector256(leftPtr + start + i);
            var rightVector = Avx.LoadVector256(rightPtr + start + i);
            var result = TOp.ApplyAvxFp32(leftVector, rightVector);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var leftValue = leftPtr[start + i];
            var rightValue = rightPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp32(leftValue, rightValue);
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        double* leftPtr,
        double* rightPtr,
        double* resultPtr)
        where TOp : IBinaryOperationAvx2
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorSizeFp64; i += IntrinsicsHelper.AvxVectorSizeFp64)
        {
            Sse.Prefetch0(leftPtr + start + i + prefetchVectorCount);
            Sse.Prefetch0(rightPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var leftVector = Avx.LoadVector256(leftPtr + start + i);
            var rightVector = Avx.LoadVector256(rightPtr + start + i);
            var result = TOp.ApplyAvxFp64(leftVector, rightVector);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var leftValue = leftPtr[start + i];
            var rightValue = rightPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp64(leftValue, rightValue);
        }
    }

    private static unsafe void InnerLoopScalar<TOp, TNumber>(
        long tid,
        long n,
        long processes,
        TNumber* leftPtr,
        TNumber* rightPtr,
        TNumber* resultPtr)
        where TOp : IBinaryOperation<TNumber>
        where TNumber : unmanaged, INumber<TNumber>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var leftValue = leftPtr[start + i];
            var rightValue = rightPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalar(leftValue, rightValue);
        }
    }

    private static unsafe void InnerLoopScalarFp32<TOp>(
        long tid,
        long n,
        long processes,
        float* leftPtr,
        float* rightPtr,
        float* resultPtr)
        where TOp : IBinaryOperationTail
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var leftValue = leftPtr[start + i];
            var rightValue = rightPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp32(leftValue, rightValue);
        }
    }

    private static unsafe void InnerLoopScalarFp64<TOp>(
        long tid,
        long n,
        long processes,
        double* leftPtr,
        double* rightPtr,
        double* resultPtr)
        where TOp : IBinaryOperationTail
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var leftValue = leftPtr[start + i];
            var rightValue = rightPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp64(leftValue, rightValue);
        }
    }
}
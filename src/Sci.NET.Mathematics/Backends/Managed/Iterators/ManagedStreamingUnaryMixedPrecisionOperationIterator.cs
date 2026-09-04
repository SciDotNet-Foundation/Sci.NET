// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Concurrency;
using Sci.NET.Mathematics.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedStreamingUnaryMixedPrecisionOperationIterator
{
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Readability")]
    public static unsafe void Apply<TOp, TIn, TOut>(TIn* inputPtr, TOut* resultPtr, long n)
        where TOp : IMixedPrecisionUnaryOperation<TIn, TOut>, IMixedPrecisionUnaryOperationAvx2
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<TIn, TOut>(n);
        ParallelExecutorTaskCollection<long> tasks;

        if (TOp.HasAvx2Implementation() && typeof(TIn) == typeof(float) && typeof(TOut) == typeof(double))
        {
            tasks = ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(
                    processes,
                    tid =>
                        InnerLoopAvx2<TOp>(
                            tid,
                            n,
                            processes,
                            (float*)inputPtr,
                            (double*)resultPtr));
        }
        else if (TOp.HasAvx2Implementation() && typeof(TIn) == typeof(double) && typeof(TOut) == typeof(float))
        {
            tasks = ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(
                    processes,
                    tid =>
                        InnerLoopAvx2<TOp>(
                            tid,
                            n,
                            processes,
                            (double*)inputPtr,
                            (float*)resultPtr));
        }
        else
        {
            tasks = ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(
                    processes,
                    tid =>
                        InnerLoopScalar<TOp, TIn, TOut>(
                            tid,
                            n,
                            processes,
                            inputPtr,
                            resultPtr));
        }

        try
        {
            ManagedTensorBackend.ParallelExecutor.Run(tasks);
        }
        finally
        {
            tasks.Dispose();
        }
    }

    private static unsafe void InnerLoopScalar<TOp, TIn, TOut>(
        long tid,
        long n,
        long processes,
        TIn* inputPtr,
        TOut* resultPtr)
        where TOp : IMixedPrecisionUnaryOperation<TIn, TOut>
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            resultPtr[start + i] = TOp.ApplyScalar(inputPtr[start + i]);
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        float* inputPtr,
        double* resultPtr)
        where TOp : IMixedPrecisionUnaryOperationAvx2
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorCountFp64; i += IntrinsicsHelper.AvxVectorCountFp64)
        {
            Sse.Prefetch0(inputPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var inputVector = Sse.LoadVector128(inputPtr + start + i);
            var result = TOp.ApplyAvxFp32Fp64(inputVector);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var input = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp32Fp64(input);
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        double* inputPtr,
        float* resultPtr)
        where TOp : IMixedPrecisionUnaryOperationAvx2
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        const int prefetchDistance = 256;
        const int prefetchVectorCount = prefetchDistance / sizeof(double);

        var i = 0;
        for (; i <= count - IntrinsicsHelper.AvxVectorCountFp64; i += IntrinsicsHelper.AvxVectorCountFp64)
        {
            Sse.Prefetch0(inputPtr + start + i + prefetchVectorCount);
            Sse.PrefetchNonTemporal(resultPtr + start + i + prefetchVectorCount);

            var inputVector = Avx.LoadVector256(inputPtr + start + i);
            var result = TOp.ApplyAvxFp64Fp32(inputVector);

            Sse.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var input = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalarFp64Fp32(input);
        }
    }
}
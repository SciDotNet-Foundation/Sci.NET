// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Concurrency;
using Sci.NET.Mathematics.Intrinsics;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedStreamingUnaryParameterizedIterator
{
    [SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "Reviewed")]
    public static unsafe void Apply<TOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, TOp instance, long n, ICpuComputeDevice device)
        where TOp : IUnaryParameterizedOperation<TOp, TNumber>, IUnaryParameterizedOperationAvx2<TOp>
        where TNumber : unmanaged, INumber<TNumber>
    {
        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<TNumber>(n);

        using var tasks = TNumber.Zero switch
        {
            float when device.IsAvx2Supported() && TOp.IsAvx2Supported() => ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(processes, tid =>
                    InnerLoopAvx2(
                        tid,
                        n,
                        processes,
                        (float*)inputPtr,
                        (float*)resultPtr,
                        instance)),
            double when device.IsAvx2Supported() && TOp.IsAvx2Supported() => ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(processes, tid =>
                    InnerLoopAvx2(
                        tid,
                        n,
                        processes,
                        (double*)inputPtr,
                        (double*)resultPtr,
                        instance)),
            float => ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(processes, tid =>
                    InnerLoopScalar(
                        tid,
                        n,
                        processes,
                        (float*)inputPtr,
                        (float*)resultPtr,
                        instance)),
            double => ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(processes, tid =>
                    InnerLoopScalar(
                        tid,
                        n,
                        processes,
                        (double*)inputPtr,
                        (double*)resultPtr,
                        instance)),
            _ => ParallelExecutorTaskFactory
                .RepeatedConstantOffset<long>(processes, tid =>
                    InnerLoopScalar(
                        tid,
                        n,
                        processes,
                        inputPtr,
                        resultPtr,
                        instance)),
        };

        ManagedTensorBackend.ParallelExecutor.Run(tasks);
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        float* inputPtr,
        float* resultPtr,
        TOp instance)
        where TOp : IUnaryParameterizedOperationAvx2<TOp>
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
            var result = TOp.ApplyAvx2Fp32(inputVector, instance);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var input = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyTailFp32(input, instance);
        }
    }

    private static unsafe void InnerLoopAvx2<TOp>(
        long tid,
        long n,
        long processes,
        double* inputPtr,
        double* resultPtr,
        TOp instance)
        where TOp : IUnaryParameterizedOperationAvx2<TOp>
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
            var result = TOp.ApplyAvx2Fp64(inputVector, instance);

            Avx.Store(resultPtr + start + i, result);
        }

        for (; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyTailFp64(inputValue, instance);
        }
    }

    private static unsafe void InnerLoopScalar<TOp, TNumber>(
        long tid,
        long n,
        long processes,
        TNumber* inputPtr,
        TNumber* resultPtr,
        TOp instance)
        where TOp : IUnaryParameterizedOperation<TOp, TNumber>
        where TNumber : unmanaged, INumber<TNumber>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyScalar(inputValue, instance);
        }
    }

    private static unsafe void InnerLoopScalar<TOp>(
        long tid,
        long n,
        long processes,
        float* inputPtr,
        float* resultPtr,
        TOp instance)
        where TOp : IUnaryParameterizedOperationTail<TOp>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyTailFp32(inputValue, instance);
        }
    }

    private static unsafe void InnerLoopScalar<TOp>(
        long tid,
        long n,
        long processes,
        double* inputPtr,
        double* resultPtr,
        TOp instance)
        where TOp : IUnaryParameterizedOperationTail<TOp>
    {
        var start = tid * n / processes;
        var end = (tid + 1) * n / processes;
        var count = end - start;

        for (var i = 0; i < count; i++)
        {
            var inputValue = inputPtr[start + i];
            resultPtr[start + i] = TOp.ApplyTailFp64(inputValue, instance);
        }
    }
}
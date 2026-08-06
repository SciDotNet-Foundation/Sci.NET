// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedStreamingUnaryMixedPrecisionOperationIterator
{
    public static unsafe void Apply<TOp, TIn, TOut>(TIn* inputPtr, TOut* resultPtr, long n)
        where TOp : IMixedPrecisionUnaryOperation<TIn, TOut>
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        var processes = ManagedTensorBackend.GetNumThreadsByElementCount<TIn, TOut>(n);

        if (processes == 1)
        {
            InnerLoopScalar<TOp, TIn, TOut>(0, n, processes, inputPtr, resultPtr);
        }
        else
        {
            _ = Parallel.For(
                0,
                processes,
                new ParallelOptions { MaxDegreeOfParallelism = processes },
                tid => InnerLoopScalar<TOp, TIn, TOut>(
                    tid,
                    n,
                    processes,
                    inputPtr,
                    resultPtr));
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
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels.Fused;

namespace Sci.NET.Mathematics.Backends.Managed.Iterators;

internal static class ManagedUnaryOperationIterator
{
    public static unsafe void Apply<TOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryOperationIterator.Apply<TOp, TNumber>(inputPtr, resultPtr, n, device);
    }

    public static unsafe void Apply<TFirstOp, TSecondOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TFirstOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TSecondOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryOperationIterator.Apply<FusedUnaryOperation<TFirstOp, TSecondOp, TNumber>, TNumber>(inputPtr, resultPtr, n, device);
    }

    public static unsafe void Apply<TFirstOp, TSecondOp, TThirdOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TFirstOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TSecondOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TThirdOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryOperationIterator.Apply<FusedUnaryOperation<TFirstOp, TSecondOp, TThirdOp, TNumber>, TNumber>(inputPtr, resultPtr, n, device);
    }

    public static unsafe void Apply<TFirstOp, TSecondOp, TThirdOp, TFourthOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, long n, ICpuComputeDevice device)
        where TFirstOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TSecondOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TThirdOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TFourthOp : IUnaryOperation<TNumber>, IUnaryOperationAvx2
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryOperationIterator.Apply<FusedUnaryOperation<TFirstOp, TSecondOp, TThirdOp, TFourthOp, TNumber>, TNumber>(inputPtr, resultPtr, n, device);
    }

    public static unsafe void Apply<TOp, TNumber>(TNumber* inputPtr, TNumber* resultPtr, TOp instance, long n, ICpuComputeDevice device)
        where TOp : IUnaryParameterizedOperation<TOp, TNumber>, IUnaryParameterizedOperationAvx2<TOp>
        where TNumber : unmanaged, INumber<TNumber>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryParameterizedIterator.Apply(inputPtr, resultPtr, instance, n, device);
    }

    public static unsafe void ApplyMixedPrecision<TOp, TIn, TOut>(TIn* inputPtr, TOut* resultPtr, long n, ICpuComputeDevice device)
        where TOp : IMixedPrecisionUnaryOperation<TIn, TOut>
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        // Keeping this abstracted for possible future use of blocked iterator
        ManagedStreamingUnaryMixedPrecisionOperationIterator.Apply<TOp, TIn, TOut>(inputPtr, resultPtr, n, device);
    }
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Devices;
using Sci.NET.Mathematics.Backends.Managed.Iterators;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels.Casting;
using Sci.NET.Mathematics.Memory;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed;

internal class ManagedCastingKernels : ICastingKernels
{
    public unsafe void Cast<TIn, TOut>(ITensor<TIn> input, ITensor<TOut> output)
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        var inputMemoryBlock = (SystemMemoryBlock<TIn>)input.Memory;
        var resultMemoryBlock = (SystemMemoryBlock<TOut>)output.Memory;

        ManagedUnaryOperationIterator.ApplyMixedPrecision<CastTruncatingMicroKernel<TIn, TOut>, TIn, TOut>(
            inputMemoryBlock.ToPointer(),
            resultMemoryBlock.ToPointer(),
            inputMemoryBlock.Length,
            (ICpuComputeDevice)input.Device);
    }
}
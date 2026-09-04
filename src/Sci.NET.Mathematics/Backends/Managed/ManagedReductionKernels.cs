// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Backends.Managed.Iterators;
using Sci.NET.Mathematics.Backends.Managed.MicroKernels.Reduction;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends.Managed;

internal class ManagedReductionKernels : IReductionKernels
{
    public void ReduceAdd<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        ManagedReductionIterator<TNumber, ReduceSumMicroKernel<TNumber>>.Apply(tensor, result, axes);
    }

    public void ReduceMean<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        ManagedReductionIterator<TNumber, ReduceMeanMicroKernel<TNumber>>.Apply(tensor, result, axes);
    }

    public void ReduceMax<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        ManagedReductionIterator<TNumber, ReduceMaxMicroKernel<TNumber>>.Apply(tensor, result, axes);
    }

    public void ReduceMin<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>
    {
        ManagedReductionIterator<TNumber, ReduceMinMicroKernel<TNumber>>.Apply(tensor, result, axes);
    }
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;

namespace Sci.NET.Mathematics.Tensors.Manipulation.Implementations;

internal class CastingService : ICastingService
{
    public ITensor<TOut> Cast<TIn, TOut>(ITensor<TIn> input)
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>
    {
        var result = new Tensor<TOut>(input.Shape, input.Backend);

        input.Backend.Casting.Cast(input, result);

        if (input.RequiresGradient)
        {
            throw new AutoDiffNotSupportedException(nameof(Cast));
        }

        return result;
    }
}
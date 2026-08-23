// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends;

/// <summary>
/// An interface for casting kernels.
/// </summary>
[PublicAPI]
public interface ICastingKernels
{
    /// <summary>
    /// Casts a <see cref="Tensor{TNumber}"/> to a different type.
    /// </summary>
    /// <param name="input">The <see cref="Tensor{TNumber}"/> to cast.</param>
    /// <param name="output">The result <see cref="Tensor{TNumber}"/>.</param>
    /// <typeparam name="TIn">The number type of the input <see cref="Tensor{TNumber}"/>.</typeparam>
    /// <typeparam name="TOut">The number type of the output <see cref="Tensor{TNumber}"/>.</typeparam>
    public void Cast<TIn, TOut>(ITensor<TIn> input, ITensor<TOut> output)
        where TIn : unmanaged, System.Numerics.INumber<TIn>
        where TOut : unmanaged, System.Numerics.INumber<TOut>;
}
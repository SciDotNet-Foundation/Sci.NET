// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Tensors.Manipulation;

/// <summary>
/// Provides tensor casting functionality.
/// </summary>
[PublicAPI]
public interface ICastingService
{
    /// <summary>
    /// Casts a <see cref="ITensor{TNumber}"/> to a different type.
    /// </summary>
    /// <param name="input">The <see cref="ITensor{TNumber}"/> to cast.</param>
    /// <typeparam name="TIn">The number type of the input <see cref="ITensor{TNumber}"/>.</typeparam>
    /// <typeparam name="TOut">The number type of the output <see cref="ITensor{TNumber}"/>.</typeparam>
    /// <returns>The input <see cref="ITensor{TNumber}"/> cast to <typeparamref name="TOut"/>.</returns>
    public ITensor<TOut> Cast<TIn, TOut>(ITensor<TIn> input)
        where TIn : unmanaged, INumber<TIn>
        where TOut : unmanaged, INumber<TOut>;
}
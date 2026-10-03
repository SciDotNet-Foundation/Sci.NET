// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Tensors.LinearAlgebra;

/// <summary>
/// An interface for norm functions.
/// </summary>
public interface INormService
{
    /// <summary>
    /// Computes the norm of a <see cref="Vector{TNumber}"/> for a given order.
    /// </summary>
    /// <param name="vector">The <see cref="Vector{TNumber}"/> to compute the norm of.</param>
    /// <param name="order">The order of the norm. If null, the default (2-norm) is used.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="Vector{TNumber}"/>.</typeparam>
    /// <returns>The norm of the <see cref="Vector{TNumber}"/>.</returns>
    public Scalar<TNumber> VectorNorm<TNumber>(Vector<TNumber> vector, Scalar<TNumber>? order = null)
        where TNumber : unmanaged, IPowerFunctions<TNumber>, ILogarithmicFunctions<TNumber>, IFloatingPointIeee754<TNumber>, INumber<TNumber>;
}

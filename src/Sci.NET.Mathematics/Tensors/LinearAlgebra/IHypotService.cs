// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Exceptions;

namespace Sci.NET.Mathematics.Tensors.LinearAlgebra;

/// <summary>
/// An interface for linear algebra operations.
/// </summary>
[PublicAPI]
public interface IHypotService
{
    /// <summary>
    /// Computes the element-wise hypotenuse of a <see cref="ITensor{TNumber}"/> and a <see cref="ITensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">>The right operand.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>s.</typeparam>
    /// <returns>>The result of the element-wise hypotenuse.</returns>
    /// <remarks>The shapes of <paramref name="left"/> and <paramref name="right"/> must be compatible for broadcasting.</remarks>
    /// <exception cref="InvalidShapeException">Thrown when the two tensors cannot be broadcast to compatible shapes.</exception>
    public ITensor<TNumber> Hypot<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>;
}
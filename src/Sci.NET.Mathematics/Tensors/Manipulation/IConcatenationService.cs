// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Tensors.Manipulation;

/// <summary>
/// A service for concatenating tensors.
/// </summary>
[PublicAPI]
public interface IConcatenationService
{
    /// <summary>
    /// Concatenates a collection of <see cref="ITensor{TNumber}"/> into a <see cref="ITensor{TNumber}"/>.
    /// </summary>
    /// <param name="tensors">The tensors to concatenate.</param>
    /// <typeparam name="TTensor">The type of the <see cref="ITensor{TNumber}"/>.</typeparam>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>.</typeparam>
    /// <returns>The concatenated <see cref="ITensor{TNumber}"/> collection.</returns>
    public ITensor<TNumber> Concatenate<TTensor, TNumber>(ICollection<TTensor> tensors)
        where TTensor : ITensor<TNumber>
        where TNumber : unmanaged, INumber<TNumber>;
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Sci.NET.Mathematics.Tensors;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for <see cref="Vector{TNumber}"/> operations.
/// </summary>
public static class VectorOperationExtensions
{
    /// <summary>
    /// Computes the norm of a <see cref="Vector{TNumber}"/>.
    /// </summary>
    /// <param name="vector">The <see cref="Vector{TNumber}"/> to compute the norm of.</param>
    /// <param name="order">The order of the norm. If null, 2 is used.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="Vector{TNumber}"/>.</typeparam>
    /// <returns>The norm of the <see cref="Vector{TNumber}"/>.</returns>
    [DebuggerStepThrough]
    public static Scalar<TNumber> VectorNorm<TNumber>(this Vector<TNumber> vector, Scalar<TNumber>? order = null)
        where TNumber : unmanaged, IPowerFunctions<TNumber>, ILogarithmicFunctions<TNumber>, IFloatingPointIeee754<TNumber>, INumber<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetNormService()
            .VectorNorm(vector, order);
    }
}

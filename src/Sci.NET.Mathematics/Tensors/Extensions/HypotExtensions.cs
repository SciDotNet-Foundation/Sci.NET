// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Sci.NET.Mathematics.Tensors;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for calculating the hypotenuse of tensors.
/// </summary>
public static class HypotExtensions
{
    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="Scalar{TNumber}"/>s.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <param name="left">The left <see cref="Scalar{TNumber}"/>.</param>
    /// <param name="right">The right <see cref="Matrix{TNumber}"/>.</param>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    [DebuggerStepThrough]
    public static Scalar<TNumber> Hypot<TNumber>(this Scalar<TNumber> left, Scalar<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToScalar();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Scalar{TNumber}"/> and <paramref name="right"/> <see cref="Vector{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Scalar{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Vector{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    [DebuggerStepThrough]
    public static Vector<TNumber> Hypot<TNumber>(this Scalar<TNumber> left, Vector<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToVector();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Scalar{TNumber}"/> and <paramref name="right"/> <see cref="Matrix{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Scalar{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Matrix{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    [DebuggerStepThrough]
    public static Matrix<TNumber> Hypot<TNumber>(this Scalar<TNumber> left, Matrix<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToMatrix();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Scalar{TNumber}"/> and <paramref name="right"/> <see cref="Tensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Scalar{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Tensor{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    [DebuggerStepThrough]
    public static Tensor<TNumber> Hypot<TNumber>(this Scalar<TNumber> left, Tensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Vector{TNumber}"/> and <paramref name="right"/> <see cref="Scalar{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Vector{TNumber}"/>.</param>
    /// <param name="right">The right <see cref="Scalar{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Vector<TNumber> Hypot<TNumber>(this Vector<TNumber> left, Scalar<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToVector();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="Vector{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left <see cref="Vector{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Vector{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Vector<TNumber> Hypot<TNumber>(this Vector<TNumber> left, Vector<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToVector();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Vector{TNumber}"/> and <paramref name="right"/> <see cref="Matrix{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Vector{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Matrix{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Matrix<TNumber> Hypot<TNumber>(this Vector<TNumber> left, Matrix<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToMatrix();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Vector{TNumber}"/> and <paramref name="right"/> <see cref="Tensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Vector{TNumber}"/>.</param>
    /// <param name="right">>The right <see cref="Tensor{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Vector<TNumber> left, Tensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Matrix{TNumber}"/> and <paramref name="right"/> <see cref="Scalar{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Matrix{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Scalar{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Matrix<TNumber> Hypot<TNumber>(this Matrix<TNumber> left, Scalar<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToMatrix();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Matrix{TNumber}"/> and <paramref name="right"/> <see cref="Vector{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Matrix{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Vector{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Matrix<TNumber> Hypot<TNumber>(this Matrix<TNumber> left, Vector<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToMatrix();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="Matrix{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left <see cref="Matrix{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Matrix{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Matrix<TNumber> Hypot<TNumber>(this Matrix<TNumber> left, Matrix<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToMatrix();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Matrix{TNumber}"/> and <paramref name="right"/> <see cref="Tensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Matrix{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Tensor{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Matrix<TNumber> left, Tensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Tensor{TNumber}"/> and <paramref name="right"/> <see cref="Scalar{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Tensor{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Scalar{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Tensor<TNumber> left, Scalar<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> <see cref="Tensor{TNumber}"/> and <paramref name="right"/> <see cref="Vector{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left <see cref="Tensor{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Vector{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Tensor<TNumber> left, Vector<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="Matrix{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left <see cref="Tensor{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Matrix{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Tensor<TNumber> left, Matrix<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="Tensor{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left <see cref="Tensor{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="Tensor{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static Tensor<TNumber> Hypot<TNumber>(this Tensor<TNumber> left, Tensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right)
            .ToTensor();
    }

    /// <summary>
    /// Calculates the hypotenuse of the <paramref name="left"/> and <paramref name="right"/> <see cref="ITensor{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left <see cref="ITensor{TNumber}"/>.</param>
    /// <param name="right"> The right <see cref="ITensor{TNumber}"/>.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>The hypotenuse of the <paramref name="left"/> and <paramref name="right"/> values.</returns>
    public static ITensor<TNumber> Hypot<TNumber>(this ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, IFloatingPointIeee754<TNumber>, IRootFunctions<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetHypotService()
            .Hypot(left, right);
    }
}

// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Sci.NET.Mathematics.Tensors;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for statistics operations.
/// </summary>
public static class StatisticsExtensions
{
    /// <summary>
    /// Finds the variance of the input tensor.
    /// </summary>
    /// <param name="value">The input tensor.</param>
    /// <param name="axis">The axis to calculate the variance along.</param>
    /// <typeparam name="TNumber">The type of the tensor.</typeparam>
    /// <returns>The variance tensor.</returns>
    public static ITensor<TNumber> Variance<TNumber>(this ITensor<TNumber> value, int? axis = null)
        where TNumber : unmanaged, IRootFunctions<TNumber>, INumber<TNumber>
    {
        return TensorServiceProvider
            .GetTensorOperationServiceProvider()
            .GetVarianceService()
            .Variance(value, axis);
    }
}

// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Tensors;

namespace Sci.NET.Mathematics.Backends;

/// <summary>
/// An interface providing tensor reduction operations.
/// </summary>
[PublicAPI]
public interface IReductionKernels
{
    /// <summary>
    /// Finds the sum of a <see cref="ITensor{TNumber}"/> over the specified axes.
    /// </summary>
    /// <param name="tensor">The <see cref="ITensor{TNumber}"/> to sum over.</param>
    /// <param name="axes">The axes to sum over.</param>
    /// <param name="result">The <see cref="ITensor{TNumber}"/> to store the result.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>s.</typeparam>
    public void ReduceAdd<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the mean of a <see cref="ITensor{TNumber}"/> over the specified axes.
    /// </summary>
    /// <param name="tensor">The <see cref="ITensor{TNumber}"/> to find the mean of.</param>
    /// <param name="axes">The axes to find the mean over.</param>
    /// <param name="result">The <see cref="ITensor{TNumber}"/> to store the result.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>s.</typeparam>
    public void ReduceMean<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the maximum value of a <see cref="ITensor{TNumber}"/> over the specified axes.
    /// </summary>
    /// <param name="tensor">The <see cref="ITensor{TNumber}"/> to find the maximum value of.</param>
    /// <param name="axes">The axes to find the maximum value over.</param>
    /// <param name="result">The <see cref="ITensor{TNumber}"/> to store the result.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>s.</typeparam>
    public void ReduceMax<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the minimum value of a <see cref="ITensor{TNumber}"/> over the specified axes.
    /// </summary>
    /// <param name="tensor">The <see cref="ITensor{TNumber}"/> to find the minimum value of.</param>
    /// <param name="axes">The axes to find the minimum value over.</param>
    /// <param name="result">The <see cref="ITensor{TNumber}"/> to store the result.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>s.</typeparam>
    public void ReduceMin<TNumber>(ITensor<TNumber> tensor, int[] axes, ITensor<TNumber> result)
        where TNumber : unmanaged, INumber<TNumber>;
}
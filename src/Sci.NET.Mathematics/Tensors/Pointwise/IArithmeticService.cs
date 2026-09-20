// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Tensors.Pointwise;

/// <summary>
/// An interface providing methods for <see cref="ITensor{TNumber}"/> arithmetic operations.
/// </summary>
public interface IArithmeticService
{
    /// <summary>
    /// Finds the sum of the <paramref name="left"/> <see cref="ITensor{TNumber}"/> and <paramref name="right"/> <see cref="ITensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <typeparam name="TNumber">The number type of the operands and result.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> containing the sum of the <paramref name="left"/> and <paramref name="right"/> operands.</returns>
    public ITensor<TNumber> Add<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the difference between the <paramref name="right"/> <see cref="ITensor{TNumber}"/> and <paramref name="left"/> <see cref="ITensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <typeparam name="TNumber">The number type of the operands and result.</typeparam>
    /// <returns>The difference between the <paramref name="right"/> and <paramref name="left"/> operands.</returns>
    public ITensor<TNumber> Subtract<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the element-wise product of the <paramref name="left"/> and <paramref name="right"/> <see cref="ITensor{TNumber}"/>s.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <typeparam name="TNumber">The number type of the operands.</typeparam>
    /// <returns>A new <see cref="ITensor{TNumber}"/> containing the element-wise product of the <paramref name="left"/> and <paramref name="right"/> operands.</returns>
    public ITensor<TNumber> Multiply<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Multiplies the <paramref name="left"/> by the <paramref name="right"/> and stores the result in the <paramref name="left"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <typeparam name="TNumber">The number type of the operands.</typeparam>
    public void MultiplyInplace<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the quotient of the <paramref name="left"/> <see cref="Tensor{TNumber}"/> and <paramref name="right"/> <see cref="Tensor{TNumber}"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <typeparam name="TNumber">The type of the operands and result.</typeparam>
    /// <returns>The quotient of the <paramref name="left"/> and <paramref name="right"/> operands.</returns>
    public ITensor<TNumber> Divide<TNumber>(ITensor<TNumber> left, ITensor<TNumber> right)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Negates the values of the <paramref name="value"/> tensor element-wise.
    /// </summary>
    /// <param name="value">The tensor whose elements are to be negated.</param>
    /// <typeparam name="TNumber">The numeric type of the tensor elements.</typeparam>
    /// <returns>A new tensor with each element being the negation of the corresponding element in the input tensor.</returns>
    public ITensor<TNumber> Negate<TNumber>(ITensor<TNumber> value)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Finds the absolute value of the <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to find the absolute value of.</param>
    /// <typeparam name="TNumber">The number type of the <paramref name="value"/>.</typeparam>
    /// <returns>The absolute value of the <paramref name="value"/>.</returns>
    public ITensor<TNumber> Abs<TNumber>(ITensor<TNumber> value)
        where TNumber : unmanaged, INumber<TNumber>;

    /// <summary>
    /// Computes the square root of a <see cref="ITensor{TNumber}"/>.
    /// </summary>
    /// <param name="tensor">The <see cref="ITensor{TNumber}"/> to compute the square root of.</param>
    /// <typeparam name="TNumber">The number type of the <see cref="ITensor{TNumber}"/>.</typeparam>
    /// <returns>The square root of the <see cref="ITensor{TNumber}"/>.</returns>
    public ITensor<TNumber> Sqrt<TNumber>(ITensor<TNumber> tensor)
        where TNumber : unmanaged, INumber<TNumber>;
}

// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Sci.NET.Mathematics.Intrinsics;

/// <summary>
/// Represents methods used to produce tensor kernels.
/// </summary>
[SuppressMessage("Roslynator", "RCS1079:Throwing of new NotImplementedException", Justification = "These methods should not be called directly.")]
[SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "These methods should not be called directly.")]
[SuppressMessage("Style", "IDE0022:Use block body for method", Justification = "No method will have any body.")]
public static class SdnMath
{
    /// <summary>
    /// Adds two values together to compute their sum.
    /// </summary>
    /// <param name="left">The value to which right is added.</param>
    /// <param name="right">The value that is added to left.</param>
    /// <returns>The sum of left and right.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Add(GenericNumber left, GenericNumber right) => throw new NotImplementedException();

    /// <summary>
    /// Subtracts two values to compute their difference.
    /// </summary>
    /// <param name="left">The value from which right is subtracted.</param>
    /// <param name="right">The value that is subtracted from left.</param>
    /// <returns>The value of right subtracted from left.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Subtract(GenericNumber left, GenericNumber right) => throw new NotImplementedException();

    /// <summary>
    /// Multiplies two values together to compute their product.
    /// </summary>
    /// <param name="left">The value that right multiplies.</param>
    /// <param name="right">The value that multiplies left.</param>
    /// <returns>The product of left multiplied by right.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Multiply(GenericNumber left, GenericNumber right) => throw new NotImplementedException();

    /// <summary>
    /// Divides one value by another to compute their quotient.
    /// </summary>
    /// <param name="left">The value that right divides.</param>
    /// <param name="right">The value that divides left.</param>
    /// <returns>The quotient of left divided by right.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Divide(GenericNumber left, GenericNumber right) => throw new NotImplementedException();

    /// <summary>
    /// Increments a value.
    /// </summary>
    /// <param name="value">The value to increment.</param>
    /// <returns>The result of incrementing the value.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Increment(GenericNumber value) => throw new NotImplementedException();

    /// <summary>
    /// Decrements a value.
    /// </summary>
    /// <param name="value">The value to decrement.</param>
    /// <returns>The result of decrementing the value.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented and will always throw a <see cref="NotImplementedException"/>.</exception>
    public static GenericNumber Decrement(GenericNumber value) => throw new NotImplementedException();
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Linq;

/// <summary>
/// Extension methods for numeric arrays.
/// </summary>
public static class NumericArrayExtensions
{
    /// <summary>
    /// Computes the product of a sequence.
    /// </summary>
    /// <param name="source">The sequence to find the product of.</param>
    /// <typeparam name="TNumber">The number type of the sequence.</typeparam>
    /// <returns>The inner product of the sequence.</returns>
    public static TNumber Product<TNumber>(this IEnumerable<TNumber> source)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var enumerable = source.ToList();

        return enumerable.Count == 0 ? TNumber.One : enumerable.Aggregate(static (a, b) => a * b);
    }

    /// <summary>
    /// Computes the mean of a sequence.
    /// </summary>
    /// <param name="source">The sequence to find the mean of.</param>
    /// <typeparam name="TNumber">The number type of the sequence.</typeparam>
    /// <returns>The mean of the sequence.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the sequence contains no elements.</exception>
    public static TNumber Mean<TNumber>(this IEnumerable<TNumber> source)
        where TNumber : unmanaged, INumber<TNumber>
    {
        var enumerable = source.ToList();

        if (enumerable.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var sum = enumerable.Aggregate(static (a, b) => a + b);
        return sum / TNumber.CreateChecked(enumerable.Count);
    }

    /// <summary>
    /// Computes the sum of the <paramref name="array"/>.
    /// </summary>
    /// <typeparam name="TNumber">The number type of the array.</typeparam>
    /// <param name="array">The array to compute the sum of.</param>
    /// <returns>The sum of the elements in the <paramref name="array"/>.</returns>
    public static TNumber Sum<TNumber>(this IEnumerable<TNumber> array)
        where TNumber : INumber<TNumber>
    {
        return array.Aggregate(TNumber.Zero, static (current, half) => current + half);
    }
}
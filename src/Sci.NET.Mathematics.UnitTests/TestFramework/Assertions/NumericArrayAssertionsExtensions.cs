// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using AwesomeAssertions.Execution;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

/// <summary>
/// Extension methods for numeric array assertions.
/// </summary>
public static class NumericArrayAssertionsExtensions
{
    /// <summary>
    /// Extension method to create a <see cref="NumericArrayAssertions{TNumber}" /> object for the given <see cref="ICollection{T}" />.
    /// </summary>
    /// <typeparam name="TNumber">The numeric type of the collection.</typeparam>
    /// <param name="numericCollection">The bfloat16 to create assertions for.</param>
    /// <returns>A <see cref="NumericArrayAssertions{TNumber}" /> object for the given <see cref="ICollection{T}" />.</returns>
    public static NumericArrayAssertions<TNumber> Should<TNumber>(this ICollection<TNumber> numericCollection)
        where TNumber : unmanaged, INumber<TNumber>
    {
        return new(numericCollection, AssertionChain.GetOrCreate());
    }
}

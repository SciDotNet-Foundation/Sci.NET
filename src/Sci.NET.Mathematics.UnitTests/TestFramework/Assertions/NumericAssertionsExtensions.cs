// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using AwesomeAssertions.Numeric;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

/// <summary>
/// Provides extension methods for numeric assertions.
/// </summary>
public static class NumericAssertionsExtensions
{
    /// <summary>
    /// Asserts that a numeric value is approximately equal to an expected value within a specified precision,
    /// or is NaN if the expected value is NaN.
    /// </summary>
    /// <param name="parent">The numeric assertions instance.</param>
    /// <param name="expectedValue">The expected numeric value.</param>
    /// <param name="precision">The precision within which the actual value should approximate the expected value.</param>
    /// <param name="because">The reason for the assertion.</param>
    /// <param name="becauseArgs">The arguments for the reason.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>An <see cref="AndConstraint{T}"/> to allow for chaining further assertions.</returns>
    public static AndConstraint<NumericAssertions<TNumber>> BeApproximatelyOrNaN<TNumber>(
        this NumericAssertions<TNumber> parent,
        TNumber expectedValue,
        double precision,
        string because = "",
        params object[] becauseArgs)
        where TNumber : struct, INumber<TNumber>, IFloatingPointIeee754<TNumber>
    {
        var precisionTNumber = TNumber.CreateChecked(precision);

        if (TNumber.IsNaN(expectedValue))
        {
            if (!TNumber.IsNaN(parent.Subject))
            {
                _ = parent
                    .CurrentAssertionChain.BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:double} to be NaN{reason}, but found {0}.", parent.Subject);
            }

            return new AndConstraint<NumericAssertions<TNumber>>(parent);
        }

        if (TNumber.IsPositiveInfinity(expectedValue))
        {
            FailIfDifferenceOutsidePrecision(
                TNumber.IsPositiveInfinity(parent.Subject),
                parent,
                expectedValue,
                precisionTNumber,
                TNumber.NaN,
                because,
                becauseArgs);
        }
        else if (TNumber.IsNegativeInfinity(expectedValue))
        {
            FailIfDifferenceOutsidePrecision(
                TNumber.IsNegativeInfinity(parent.Subject),
                parent,
                expectedValue,
                precisionTNumber,
                TNumber.NaN,
                because,
                becauseArgs);
        }
        else
        {
            var actualDifference = TNumber.Abs(expectedValue - parent.Subject);

            FailIfDifferenceOutsidePrecision(
                actualDifference <= precisionTNumber,
                parent,
                expectedValue,
                precisionTNumber,
                actualDifference,
                because,
                becauseArgs);
        }

        return new AndConstraint<NumericAssertions<TNumber>>(parent);
    }

    private static void FailIfDifferenceOutsidePrecision<T>(
        bool differenceWithinPrecision,
        NumericAssertions<T> parent,
        T expectedValue,
        T precision,
        T actualDifference,
        string because,
        object[] becauseArgs)
        where T : struct, IComparable<T>
    {
        _ = parent
            .CurrentAssertionChain.ForCondition(differenceWithinPrecision)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to approximate {1} +/- {2}{reason}, but {0} differed by {3}.",
                parent.Subject,
                expectedValue,
                precision,
                actualDifference);
    }
}

// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using AwesomeAssertions.Numeric;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

/// <summary>
/// Provides extension methods for floating-point numeric assertions.
/// </summary>
public static class FloatingPointNumericAssertionExtensions
{
    /// <summary>
    /// Asserts that all elements in a numeric array are approximately equal to the expected values within a specified precision,
    /// or are NaN if the expected value is NaN.
    /// </summary>
    /// <param name="assertion">The numeric array assertions instance.</param>
    /// <param name="expected">The expected numeric values.</param>
    /// <param name="precision">The precision within which the actual values should approximate the expected values.</param>
    /// <param name="because">The reason for the assertion.</param>
    /// <param name="becauseArgs">The arguments for the reason.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>>The original <see cref="NumericArrayAssertions{TNumber}"/> instance for chaining further assertions.</returns>
    public static NumericArrayAssertions<TNumber> AllBeApproximatelyOrNaN<TNumber>(
        this NumericArrayAssertions<TNumber> assertion,
        TNumber[] expected,
        double precision,
        string because = "",
        params object[] becauseArgs)
        where TNumber : struct, INumber<TNumber>, IFloatingPointIeee754<TNumber>
    {
        _ = assertion
            .CurrentAssertionChain.ForCondition(assertion.Subject is not null)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately {0}{reason}, but found {1}.",
                expected,
                assertion.Subject);

        var subjectArray = assertion.Subject!.ToArray();

        _ = assertion
            .CurrentAssertionChain.ForCondition(assertion.Subject!.Count() == expected.Length)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to have length {0}{reason}, but found {1}.",
                expected.Length,
                subjectArray);

        var precisionTNumber = TNumber.CreateChecked(precision);
        var correctness = new bool[expected.Length];

        for (int i = 0; i < expected.Length; i++)
        {
            var actualValue = subjectArray[i];
            var expectedValue = expected[i];

            if (TNumber.IsNaN(expectedValue))
            {
                correctness[i] = TNumber.IsNaN(actualValue);
            }
            else if (TNumber.IsPositiveInfinity(expectedValue))
            {
                correctness[i] = TNumber.IsPositiveInfinity(actualValue);
            }
            else if (TNumber.IsNegativeInfinity(expectedValue))
            {
                correctness[i] = TNumber.IsNegativeInfinity(actualValue);
            }
            else
            {
                var difference = TNumber.Abs(actualValue - expectedValue);
                correctness[i] = difference <= precisionTNumber;
            }
        }

        var incorrectStatement = new List<string>();

        for (var i = 0; i < correctness.Length; i++)
        {
            if (!correctness[i])
            {
                incorrectStatement.Add(
                    $"At index {i}: expected {expected[i]}, but found {subjectArray[i]}, difference {TNumber.Abs(subjectArray[i] - expected[i])}");
            }
        }

        _ = assertion
            .CurrentAssertionChain.ForCondition(correctness.All(static c => c))
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately correct, within precision {0}{reason}, but found\n{1}.",
                precision,
                string.Join('\n', incorrectStatement));

        return assertion;
    }

    /// <summary>
    /// Asserts that all elements in a numeric array have a relative error less than the specified value,
    /// or are NaN if the expected value is NaN.
    /// </summary>
    /// <param name="assertion">The numeric array assertions instance.</param>
    /// <param name="expected">The expected numeric values.</param>
    /// <param name="relativeError">The maximum allowable relative error.</param>
    /// <param name="because">The reason for the assertion.</param>
    /// <param name="becauseArgs">The arguments for the reason.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>>The original <see cref="NumericArrayAssertions{TNumber}"/> instance for chaining further assertions.</returns>
    public static NumericArrayAssertions<TNumber> AllHaveRelativeErrorLessThanOrNaN<TNumber>(
        this NumericArrayAssertions<TNumber> assertion,
        TNumber[] expected,
        double relativeError,
        string because = "",
        params object[] becauseArgs)
        where TNumber : struct, INumber<TNumber>, IFloatingPointIeee754<TNumber>
    {
        _ = assertion
            .CurrentAssertionChain.ForCondition(assertion.Subject is not null)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately {0}{reason}, but found {1}.",
                expected,
                assertion.Subject);

        var subjectArray = assertion.Subject!.ToArray();

        _ = assertion
            .CurrentAssertionChain.ForCondition(assertion.Subject!.Count() == expected.Length)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to have length {0}{reason}, but found {1}.",
                expected.Length,
                subjectArray);

        var precisionTNumber = TNumber.CreateChecked(relativeError);
        var correctness = new bool[expected.Length];

        for (int i = 0; i < expected.Length; i++)
        {
            var actualValue = subjectArray[i];
            var expectedValue = expected[i];

            if (TNumber.IsNaN(expectedValue))
            {
                correctness[i] = TNumber.IsNaN(actualValue);
            }
            else if (TNumber.IsPositiveInfinity(expectedValue))
            {
                correctness[i] = TNumber.IsPositiveInfinity(actualValue);
            }
            else if (TNumber.IsNegativeInfinity(expectedValue))
            {
                correctness[i] = TNumber.IsNegativeInfinity(actualValue);
            }
            else if (TNumber.IsZero(expectedValue))
            {
                correctness[i] = TNumber.IsZero(actualValue);
            }
            else
            {
                var precisionValue = TNumber.Abs(actualValue - expectedValue) / TNumber.Abs(expectedValue);
                correctness[i] = precisionValue <= precisionTNumber;
            }
        }

        var incorrectStatement = new List<string>();

        for (var i = 0; i < correctness.Length; i++)
        {
            if (!correctness[i])
            {
                var precisionValue = TNumber.Abs(subjectArray[i] - expected[i]) / TNumber.Abs(expected[i]);
                incorrectStatement.Add(
                    $"At index {i}: expected {expected[i]}, but found {subjectArray[i]}, relative error of {precisionValue}");
            }
        }

        _ = assertion
            .CurrentAssertionChain.ForCondition(correctness.All(static c => c))
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately correct, to have a relative error of {0}{reason}, but found\n{1}.",
                relativeError,
                string.Join('\n', incorrectStatement));

        return assertion;
    }

    /// <summary>
    /// Asserts that a numeric value is approximately equal to the expected value within a specified relative error,
    /// or is NaN if the expected value is NaN.
    /// </summary>
    /// <param name="assertion">The numeric assertions instance.</param>
    /// <param name="expected">The expected numeric value.</param>
    /// <param name="relativeError">The maximum allowable relative error.</param>
    /// <param name="because">The reason for the assertion.</param>
    /// <param name="becauseArgs">The arguments for the reason.</param>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    /// <returns>>The original <see cref="NumericAssertions{T}"/> instance for chaining further assertions.</returns>
    public static NumericAssertions<TNumber> AllHaveRelativeErrorLessThanOrNaN<TNumber>(
        this NumericAssertions<TNumber> assertion,
        TNumber expected,
        double relativeError,
        string because = "",
        params object[] becauseArgs)
        where TNumber : struct, INumber<TNumber>, IFloatingPointIeee754<TNumber>
    {
        var precisionTNumber = TNumber.CreateChecked(relativeError);

        var actualValue = assertion.Subject;

        if (TNumber.IsNaN(actualValue) && TNumber.IsNaN(expected))
        {
            return assertion;
        }

        if (TNumber.IsPositiveInfinity(actualValue) && TNumber.IsPositiveInfinity(expected))
        {
            return assertion;
        }

        if (TNumber.IsNegativeInfinity(actualValue) && TNumber.IsNegativeInfinity(expected))
        {
            return assertion;
        }

        if (TNumber.IsZero(actualValue) && TNumber.IsZero(expected))
        {
            return assertion;
        }

        var precisionValue = TNumber.Abs(actualValue - expected) / TNumber.Abs(expected);

        _ = assertion
            .CurrentAssertionChain.ForCondition(precisionValue <= precisionTNumber)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately correct, to have a relative error of {0}{reason}, but found\n{1}.",
                relativeError,
                string.Join('\n', precisionValue));

        return assertion;
    }
}

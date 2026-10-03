// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using AwesomeAssertions.Execution;
using AwesomeAssertions.Numeric;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

/// <summary>
/// Assertions for <see cref="Half" />.
/// </summary>
public class HalfAssertions : NumericAssertions<Half, HalfAssertions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HalfAssertions"/> class.
    /// </summary>
    /// <param name="value">The value to create assertions for.</param>
    /// <param name="chain">The assertion chain.</param>
    public HalfAssertions(Half value, AssertionChain chain)
        : base(value, chain)
    {
    }

    /// <summary>
    /// Asserts that the value is approximately equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="because">The reason why the assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>A <see cref="AndConstraint{TAssertions}" /> object.</returns>
    public new AndConstraint<HalfAssertions> Be(Half expected, string because = "", params object[] becauseArgs)
    {
        _ = CurrentAssertionChain
            .ForCondition(Subject.Equals(expected))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:value} to be approximately {0}{reason}, but found {1}.", expected, Subject);

        return new AndConstraint<HalfAssertions>(this);
    }

    /// <summary>
    /// Asserts that the value is approximately equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="tolerance">The tolerance within which the value is expected to be.</param>
    /// <param name="because">The reason why the assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>A <see cref="AndConstraint{TAssertions}" /> object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the subject is <see langword="null" /> or not comparable (I.E. infinite, NaN).</exception>
    public AndConstraint<HalfAssertions> BeApproximately(
        Half expected,
        float tolerance,
        string because = "",
        params object[] becauseArgs)
    {
        if (Half.IsNaN(Subject))
        {
            throw new InvalidOperationException(
                "Cannot assert that a NaN value is approximately equal to another value.");
        }

        if (Half.IsNaN(expected))
        {
            throw new InvalidOperationException("Cannot assert that a value is approximately equal to a NaN value.");
        }

        if (Half.IsPositiveInfinity(Subject) || Half.IsNegativeInfinity(Subject))
        {
            throw new InvalidOperationException(
                "Cannot assert that an infinity value is approximately equal to another value.");
        }

        if (Half.IsPositiveInfinity(expected) || Half.IsNegativeInfinity(expected))
        {
            throw new InvalidOperationException(
                "Cannot assert that a value is approximately equal to an infinity value.");
        }

        _ = CurrentAssertionChain
            .ForCondition(float.Abs((float)Subject - (float)expected) <= tolerance)
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be approximately {0} +/- {1}{reason}, but found {2}.",
                expected,
                tolerance,
                Subject);

        return new AndConstraint<HalfAssertions>(this);
    }
}

// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using AwesomeAssertions.Collections;
using AwesomeAssertions.Execution;

namespace Sci.NET.Mathematics.UnitTests.TestFramework.Assertions;

/// <summary>
/// Assertions for numeric arrays.
/// </summary>
/// <typeparam name="TNumber">The number type of the array.</typeparam>
public class NumericArrayAssertions<TNumber> : GenericCollectionAssertions<TNumber>
    where TNumber : INumber<TNumber>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericArrayAssertions{TNumber}"/> class.
    /// </summary>
    /// <param name="value">The value to create assertions for.</param>
    /// <param name="chain">The assertion chain.</param>
    public NumericArrayAssertions(ICollection<TNumber> value, AssertionChain chain)
        : base(value, chain)
    {
    }

    /// <summary>
    /// Asserts that the numeric array has no element greater than the specified maximum value.
    /// </summary>
    /// <param name="maxValue">The maximum value that no element in the array should exceed.</param>
    /// <param name="because">The reason why the assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>A <see cref="AndConstraint{TAssertions}" /> object.</returns>
    public AndConstraint<NumericArrayAssertions<TNumber>> HaveNoElementGreaterThan(
        TNumber maxValue,
        string because = "",
        params object[] becauseArgs)
    {
        var subjectList = Subject.ToArray();

        _ = CurrentAssertionChain
            .ForCondition(!subjectList.Any(x => x > maxValue))
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be not have an element above {0} +/- {1}{reason}, but found elements with the values '{2}'.",
                maxValue,
                string.Join(", ", subjectList.Where(x => x > maxValue).Select(static x => x.ToString())));

        return new AndConstraint<NumericArrayAssertions<TNumber>>(this);
    }

    /// <summary>
    /// Asserts that the numeric array has no element less than the specified minimum value.
    /// </summary>
    /// <param name="minValue">The minimum value that no element in the array should be below.</param>
    /// <param name="because">The reason why the assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.</param>
    /// <param name="becauseArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
    /// <returns>A <see cref="AndConstraint{TAssertions}" /> object.</returns>
    public AndConstraint<NumericArrayAssertions<TNumber>> HaveNoElementLessThan(
        TNumber minValue,
        string because = "",
        params object[] becauseArgs)
    {
        var subjectList = Subject.ToArray();

        _ = CurrentAssertionChain
            .ForCondition(!subjectList.Any(x => x < minValue))
            .BecauseOf(because, becauseArgs)
            .FailWith(
                "Expected {context:value} to be not have an element below {0} +/- {1}{reason}, but found elements with the values '{2}'.",
                minValue,
                string.Join(", ", subjectList.Where(x => x < minValue).Select(static x => x.ToString())));

        return new AndConstraint<NumericArrayAssertions<TNumber>>(this);
    }

    /// <summary>
    /// Asserts that all elements in the numeric array are within the specified range [min, max].
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>A <see cref="AndConstraint{TAssertions}" /> object.</returns>
    public AndConstraint<NumericArrayAssertions<TNumber>> AllBeInRange(TNumber min, TNumber max)
    {
        var subjectList = Subject.ToArray();

        _ = CurrentAssertionChain
            .ForCondition(subjectList.All(x => x >= min && x <= max))
            .FailWith(
                "Expected {context:value} to have all elements in range {0} to {1}{reason}, but found elements with the values '{2}'.",
                min,
                max,
                string.Join(", ", subjectList.Where(x => x < min || x > max).Select(static x => x.ToString())));

        return new AndConstraint<NumericArrayAssertions<TNumber>>(this);
    }
}

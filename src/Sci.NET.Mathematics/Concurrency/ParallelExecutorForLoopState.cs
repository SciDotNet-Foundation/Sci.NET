// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Comparison;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// Represents the state of a for loop.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
/// <typeparam name="TState">The type of the local state.</typeparam>
public readonly struct ParallelExecutorForLoopState<TIndex, TState> : IValueEquatable<ParallelExecutorForLoopState<TIndex, TState>>
    where TIndex : struct, IBinaryInteger<TIndex>
    where TState : struct
{
    /// <summary>
    /// Gets the from inclusive.
    /// </summary>
    public required TIndex FromInclusive { get; init; }

    /// <summary>
    /// Gets the to inclusive.
    /// </summary>
    public required TIndex ToExclusive { get; init; }

    /// <summary>
    /// Gets the body.
    /// </summary>
    public required Action<TIndex, TState> Body { get; init; }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public required TState State { get; init; }

    /// <summary>
    /// Gets the from chunk.
    /// </summary>
    public required TIndex Chunk { get; init; }

    /// <summary>
    /// Gets the from remainder.
    /// </summary>
    public required TIndex Remainder { get; init; }

    /// <inheritdoc />
    public static bool operator ==(ParallelExecutorForLoopState<TIndex, TState> left, ParallelExecutorForLoopState<TIndex, TState> right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(ParallelExecutorForLoopState<TIndex, TState> left, ParallelExecutorForLoopState<TIndex, TState> right) => !(left == right);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ParallelExecutorForLoopState<TIndex, TState> other && Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(ParallelExecutorForLoopState<TIndex, TState> other)
    {
        return FromInclusive == other.FromInclusive &&
               ToExclusive == other.ToExclusive &&
               Body == other.Body &&
               State.Equals(other.State) &&
               Chunk == other.Chunk &&
               Remainder == other.Remainder;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            FromInclusive,
            ToExclusive,
            Body,
            State,
            Chunk,
            Remainder);
    }
}
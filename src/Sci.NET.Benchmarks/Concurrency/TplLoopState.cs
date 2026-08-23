// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Comparison;

namespace Sci.NET.Benchmarks.Concurrency;

public readonly struct TplLoopState<TState> : IValueEquatable<TplLoopState<TState>>
    where TState : struct
{
    public long FromInclusive { get; init; }

    public long Remainder { get; init; }

    public long Chunk { get; init; }

    public Action<long, TState> Action { get; init; }

    public TState State { get; init; }

    public static bool operator ==(TplLoopState<TState> left, TplLoopState<TState> right) => left.Equals(right);

    public static bool operator !=(TplLoopState<TState> left, TplLoopState<TState> right) => !(left == right);

    public bool Equals(TplLoopState<TState> other)
    {
        return FromInclusive == other.FromInclusive &&
               Remainder == other.Remainder &&
               Chunk == other.Chunk &&
               Action == other.Action &&
               State.Equals(other.State);
    }

    public override bool Equals(object? obj)
    {
        return obj is TplLoopState<TState> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromInclusive, Remainder, Chunk, Action, State);
    }
}
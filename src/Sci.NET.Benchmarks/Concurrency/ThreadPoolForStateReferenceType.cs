// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Benchmarks.Concurrency;

internal readonly struct ThreadPoolForStateReferenceType<TState>
    where TState : class
{
    public required int ThreadIndex { get; init; }

    public required long Chunk { get; init; }

    public required long Remainder { get; init; }

    public required CountdownEvent Countdown { get; init; }

    public required long FromInclusive { get; init; }

    public required Action<long, TState> Action { get; init; }

    public required TState State { get; init; }
}
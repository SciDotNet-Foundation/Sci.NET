// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Benchmarks.Concurrency;

internal static class ParallelUtils
{
    public static void TplFor<TState>(
        long fromInclusive,
        long toExclusive,
        int numWorkers,
        TState state,
        Action<long, TState> action)
        where TState : struct
    {
        if (numWorkers == 1)
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                action(i, state);
            }

            return;
        }

        var n = toExclusive - fromInclusive;
        var chunk = n / numWorkers;
        var remainder = n % numWorkers;

        using var countdown = new CountdownEvent(numWorkers);

        var loopState = new TplLoopState<TState>
        {
            Chunk = chunk,
            Remainder = remainder,
            FromInclusive = fromInclusive,
            Action = action,
            State = state
        };

        _ = Parallel.For(
            0L,
            numWorkers,
            new ParallelOptions { MaxDegreeOfParallelism = numWorkers },
            tid => Loop(tid, loopState));
    }

    private static void Loop<TState>(long tid, TplLoopState<TState> state)
        where TState : struct
    {
        var start = state.FromInclusive + (tid * state.Chunk) + long.Min(tid, state.Remainder);
        var count = tid < state.Remainder ? state.Chunk + 1 : state.Chunk;

        for (var i = 0L; i < count; i++)
        {
            state.Action(start + i, state.State);
        }
    }
}
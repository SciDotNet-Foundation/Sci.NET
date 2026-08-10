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

    public static void ThreadPoolFor<TState>(
        long fromInclusive,
        long toExclusive,
        int numWorkers,
        bool preferLocal,
        TState state,
        Action<long, TState> action)
        where TState : struct
    {
        if (numWorkers == 1 || Thread.CurrentThread.IsThreadPoolThread)
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

        var loopState = new ThreadPoolForState<TState>
        {
            Chunk = chunk,
            Remainder = remainder,
            Countdown = countdown,
            FromInclusive = fromInclusive,
            Action = action,
            State = state,
            ThreadIndex = 0
        };

        for (var i = 0; i < numWorkers - 1; i++)
        {
            ThreadPool.QueueUserWorkItem(Loop, loopState with { ThreadIndex = i }, preferLocal);
        }

        Loop(loopState with { ThreadIndex = numWorkers - 1 });

        countdown.Wait();
    }

    public static void ThreadPoolForReferenceType<TState>(
        long fromInclusive,
        long toExclusive,
        int numWorkers,
        bool preferLocal,
        TState state,
        Action<long, TState> action)
        where TState : class
    {
        if (numWorkers == 1 || Thread.CurrentThread.IsThreadPoolThread)
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

        var loopState = new ThreadPoolForStateReferenceType<TState>
        {
            Chunk = chunk,
            Remainder = remainder,
            Countdown = countdown,
            FromInclusive = fromInclusive,
            Action = action,
            State = state,
            ThreadIndex = 0
        };

        for (var i = 0; i < numWorkers - 1; i++)
        {
            ThreadPool.QueueUserWorkItem(LoopReferenceType, loopState with { ThreadIndex = i }, preferLocal);
        }

        LoopReferenceType(loopState with { ThreadIndex = numWorkers - 1 });

        countdown.Wait();
    }

    private static void Loop<TState>(ThreadPoolForState<TState> state)
        where TState : struct
    {
        var start = state.FromInclusive + (state.ThreadIndex * state.Chunk) + long.Min(state.ThreadIndex, state.Remainder);
        var count = state.ThreadIndex < state.Remainder ? state.Chunk + 1 : state.Chunk;

        for (var i = 0L; i < count; i++)
        {
            state.Action(start + i, state.State);
        }

        state.Countdown.Signal();
    }

    private static void LoopReferenceType<TState>(ThreadPoolForStateReferenceType<TState> state)
        where TState : class
    {
        var start = state.FromInclusive + (state.ThreadIndex * state.Chunk) + long.Min(state.ThreadIndex, state.Remainder);
        var count = state.ThreadIndex < state.Remainder ? state.Chunk + 1 : state.Chunk;

        for (var i = 0L; i < count; i++)
        {
            state.Action(start + i, state.State);
        }

        state.Countdown.Signal();
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
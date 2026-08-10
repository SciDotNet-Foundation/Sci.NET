// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using Sci.NET.Mathematics.Performance;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// Executes batches of work items in parallel on a <see cref="ParallelExecutorThreadPool"/>.
/// </summary>
[PublicAPI]
public sealed class ParallelExecutor : IDisposable
{
    private readonly ParallelExecutorThreadPool _threadPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelExecutor"/> class.
    /// </summary>
    /// <param name="threadPool">The thread pool to execute work items on.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="threadPool"/> is <c>null</c>.</exception>
    public ParallelExecutor(ParallelExecutorThreadPool threadPool)
    {
        ArgumentNullException.ThrowIfNull(threadPool);

        _threadPool = threadPool;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ParallelExecutor"/> class.
    /// </summary>
    ~ParallelExecutor()
    {
        Dispose(false);
    }

    /// <summary>
    /// Replaces the worker threads with the given number of threads, each of which have the given priority.
    /// </summary>
    /// <param name="numThreads">The number of workers to use.</param>
    /// <param name="priority">The priority of the worker threads.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="ParallelExecutor"/> has been disposed.</exception>
    /// <remarks>
    /// This method will block the current thread until the queued threads have been exhausted, then the threads will be
    /// replaced.
    /// </remarks>
    public void ReplaceWorkerThreads(int numThreads, ThreadPriority priority = ThreadPriority.Normal)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numThreads, Environment.ProcessorCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);

        _threadPool.ReplaceWorkerThreads(numThreads, priority);
    }

    /// <summary>
    /// Executes every task in <paramref name="taskCollection"/> and blocks until they have all
    /// completed. Single-task collections, and calls made from a pool worker thread (nested
    /// parallelism), are executed sequentially on the calling thread to avoid deadlocking the pool.
    /// The caller retains ownership of <paramref name="taskCollection"/> and is responsible for
    /// disposing it.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <param name="taskCollection">The batch of tasks to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskCollection"/> is <c>null</c>.</exception>
    /// <exception cref="AggregateException">Thrown when one or more task bodies threw.</exception>
    public void Run<TIndex>(ParallelExecutorTaskCollection<TIndex> taskCollection)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(taskCollection);

        if (taskCollection.Count == 0)
        {
            return;
        }

        if (taskCollection.Count == 1 || ParallelExecutorThreadPoolThread.IsWorkerThread)
        {
            RunSequential(taskCollection);
            return;
        }

        _threadPool.EnqueueItems(taskCollection, taskCollection.Count - 1);

        taskCollection.TasksSpan[taskCollection.Count - 1].Execute();

        taskCollection.WaitAll();
    }

    /// <summary>
    /// Executes <paramref name="body"/> for every index in the range <paramref name="fromInclusive"/> to
    /// <paramref name="toExclusive"/>.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the loop index.</typeparam>
    /// <param name="fromInclusive">The index to start from (inclusive).</param>
    /// <param name="toExclusive">The index to iterate to (exclusive).</param>
    /// <param name="numWorkers">The number of workers to partition the range across.</param>
    /// <param name="body">The body of the loop.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numWorkers"/> is not positive.</exception>
    /// <exception cref="AggregateException">Thrown when one or more invocations of <paramref name="body"/> threw.</exception>
    public void For<TIndex>(
        TIndex fromInclusive,
        TIndex toExclusive,
        TIndex numWorkers,
        Action<TIndex> body)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numWorkers);

        if (toExclusive <= fromInclusive)
        {
            return;
        }

        var n = toExclusive - fromInclusive;

        if (numWorkers > n)
        {
            numWorkers = n;
        }

        if (numWorkers == TIndex.One || ParallelExecutorThreadPoolThread.IsWorkerThread)
        {
            ForSequential(fromInclusive, toExclusive, body);
            return;
        }

        var chunk = n / numWorkers;
        var remainder = n % numWorkers;

        void Loop(TIndex tid)
        {
            var (start, count) = CalculateForParameters(fromInclusive, tid, chunk, remainder);

            for (var i = TIndex.Zero; i < count; i++)
            {
                body(start + i);
            }
        }

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(numWorkers, Loop);

        Run(tasks);
    }

    /// <summary>
    /// Executes <paramref name="body"/> for every index in the range <paramref name="fromInclusive"/> to
    /// <paramref name="toExclusive"/>.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the loop index.</typeparam>
    /// <typeparam name="TState">The type of the loop action state.</typeparam>
    /// <param name="fromInclusive">The index to start from (inclusive).</param>
    /// <param name="toExclusive">The index to iterate to (exclusive).</param>
    /// <param name="numWorkers">The number of workers to partition the range across.</param>
    /// <param name="state">The state passed to the loop action.</param>
    /// <param name="body">The body of the loop.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numWorkers"/> is not positive.</exception>
    /// <exception cref="AggregateException">Thrown when one or more invocations of <paramref name="body"/> threw.</exception>
    public void For<TIndex, TState>(
        TIndex fromInclusive,
        TIndex toExclusive,
        TIndex numWorkers,
        TState state,
        Action<TIndex, TState> body)
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numWorkers);

        if (toExclusive <= fromInclusive)
        {
            return;
        }

        var n = toExclusive - fromInclusive;

        if (numWorkers > n)
        {
            numWorkers = n;
        }

        var chunk = n / numWorkers;
        var remainder = n % numWorkers;

        if (numWorkers == TIndex.One || ParallelExecutorThreadPoolThread.IsWorkerThread)
        {
            ForSequential(
                new ParallelExecutorForLoopState<TIndex, TState>
                {
                    FromInclusive = fromInclusive,
                    ToExclusive = toExclusive,
                    State = state,
                    Body = body,
                    Chunk = TIndex.Zero,
                    Remainder = TIndex.Zero
                });
            return;
        }

        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numWorkers,
            new ParallelExecutorForLoopState<TIndex, TState>
            {
                FromInclusive = fromInclusive,
                ToExclusive = toExclusive,
                State = state,
                Body = body,
                Chunk = chunk,
                Remainder = remainder
            },
            ForInnerLoop);

        Run(tasks);
    }

    /// <summary>
    /// Executes <paramref name="body"/> for every index in the range <paramref name="fromInclusive"/> to
    /// <paramref name="toExclusive"/>.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the loop index.</typeparam>
    /// <typeparam name="TState">The type for the local state.</typeparam>
    /// <param name="fromInclusive">The index to start from (inclusive).</param>
    /// <param name="toExclusive">The index to iterate to (exclusive).</param>
    /// <param name="numWorkers">The number of workers to partition the range across.</param>
    /// <param name="threadLocalSetup">The setup function for the thread local state.</param>
    /// <param name="body">The body of the loop.</param>
    /// <param name="threadLocalCleanup">The cleanup action for the thread local state.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numWorkers"/> is not positive.</exception>
    /// <exception cref="AggregateException">Thrown when one or more invocations of <paramref name="body"/> threw.</exception>
    public void For<TIndex, TState>(
        TIndex fromInclusive,
        TIndex toExclusive,
        TIndex numWorkers,
        Func<TState> threadLocalSetup,
        Action<TIndex, TState> body,
        Action<TState> threadLocalCleanup)
        where TIndex : IBinaryInteger<TIndex>
        where TState : unmanaged
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(threadLocalSetup);
        ArgumentNullException.ThrowIfNull(threadLocalCleanup);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numWorkers);

        if (toExclusive <= fromInclusive)
        {
            return;
        }

        var n = toExclusive - fromInclusive;

        if (numWorkers > n)
        {
            numWorkers = n;
        }

        if (numWorkers == TIndex.One || ParallelExecutorThreadPoolThread.IsWorkerThread)
        {
            ForSequential(fromInclusive, toExclusive, threadLocalSetup, body, threadLocalCleanup);
            return;
        }

        var chunk = n / numWorkers;
        var remainder = n % numWorkers;

        using var threadLocalState = new ThreadLocal<TState>(threadLocalSetup, trackAllValues: true);

        void Loop(TIndex tid)
        {
            var (start, count) = CalculateForParameters(fromInclusive, tid, chunk, remainder);
            var buffer = threadLocalState.Value;

            for (var i = TIndex.Zero; i < count; i++)
            {
                body(start + i, buffer);
            }
        }

        try
        {
            using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(numWorkers, Loop);

            Run(tasks);
        }
        finally
        {
            foreach (var state in threadLocalState.Values)
            {
                threadLocalCleanup(state);
            }
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> for every element produced by <paramref name="partitioner"/>
    /// and blocks until all elements have been processed.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements produced by the partitioner.</typeparam>
    /// <param name="partitioner">The partitioner producing the elements to process.</param>
    /// <param name="action">The action to invoke for each element.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partitioner"/> or <paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="AggregateException">Thrown when one or more invocations of <paramref name="action"/> threw.</exception>
    public void ForEach<TSource>(Partitioner<TSource> partitioner, Action<TSource> action)
    {
        ArgumentNullException.ThrowIfNull(partitioner);
        ArgumentNullException.ThrowIfNull(action);

        using var tasks = ParallelExecutorTaskFactory.FromPartitioner(partitioner, action);

        Run(tasks);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static void RunSequential<TIndex>(ParallelExecutorTaskCollection<TIndex> tasks)
        where TIndex : IBinaryInteger<TIndex>
    {
        foreach (var task in tasks.TasksSpan)
        {
            task.Execute();
        }

        tasks.WaitAll();
    }

    private static void ForSequential<TIndex, TState>(ParallelExecutorForLoopState<TIndex, TState> state)
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
        try
        {
            for (var i = state.FromInclusive; i < state.ToExclusive; i++)
            {
                state.Body(i, state.State);
            }
        }
        catch (Exception ex)
        {
            throw new AggregateException(ex);
        }
    }

    private static void ForSequential<TIndex>(TIndex fromInclusive, TIndex toExclusive, Action<TIndex> body)
        where TIndex : IBinaryInteger<TIndex>
    {
        try
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                body(i);
            }
        }
        catch (Exception ex)
        {
            throw new AggregateException(ex);
        }
    }

    private static void ForSequential<TIndex, TState>(
        TIndex fromInclusive,
        TIndex toExclusive,
        Func<TState> threadLocalSetup,
        Action<TIndex, TState> body,
        Action<TState> threadLocalCleanup)
        where TIndex : IBinaryInteger<TIndex>
        where TState : unmanaged
    {
        var state = threadLocalSetup();

        try
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                body(i, state);
            }
        }
        catch (Exception ex)
        {
            throw new AggregateException(ex);
        }
        finally
        {
            threadLocalCleanup(state);
        }
    }

    private static void ForInnerLoop<TIndex, TState>(TIndex threadIdx, ParallelExecutorForLoopState<TIndex, TState> state)
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
        var start = state.FromInclusive + (threadIdx * state.Chunk) + TIndex.Min(threadIdx, state.Remainder);
        var count = threadIdx < state.Remainder ? state.Chunk + TIndex.One : state.Chunk;

        for (var i = TIndex.Zero; i < count; i++)
        {
            state.Body(start + i, state.State);
        }
    }

    [MethodImpl(ImplementationOptions.HotPath)]
    private static (TIndex Start, TIndex Count) CalculateForParameters<TIndex>(
        TIndex fromInclusive,
        TIndex tid,
        TIndex chunk,
        TIndex remainder)
        where TIndex : IBinaryInteger<TIndex>
    {
        var start = fromInclusive + (tid * chunk) + TIndex.Min(tid, remainder);
        var count = tid < remainder ? chunk + TIndex.One : chunk;
        return (start, count);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _threadPool.Dispose();
        }
    }
}
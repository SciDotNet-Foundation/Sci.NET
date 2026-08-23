// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// Executes balanced fork-join parallel regions on a <see cref="ParallelExecutorThreadPool"/>. Each
/// region statically partitions its range across the participating threads; the calling thread runs
/// one slice itself and blocks until the workers have finished.
/// </summary>
[PublicAPI]
public sealed unsafe class ParallelExecutor : IDisposable
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
    /// This method will block the current thread until the running workers have exited, then the threads
    /// will be replaced.
    /// </remarks>
    public void ReplaceWorkerThreads(int numThreads, ThreadPriority priority = ThreadPriority.Normal)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numThreads, Environment.ProcessorCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);

        _threadPool.ReplaceWorkerThreads(numThreads, priority);
    }

    /// <summary>
    /// Executes every task in <paramref name="taskCollection"/> and blocks until they have all
    /// completed. Single-task collections, and calls made from a thread already inside a region (nested
    /// parallelism), are executed sequentially on the calling thread to avoid oversubscribing the pool.
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

        if (taskCollection.Count == 1 || ShouldRunSequentially())
        {
            RunSequential(taskCollection);
            return;
        }

        var count = taskCollection.Count;
        var participants = Math.Min(count, _threadPool.MaxParticipants);
        var chunk = (long)count / participants;
        var remainder = (long)count % participants;

        _threadPool.Invoke(participants, &RunTasksTrampoline, taskCollection.Tasks, 0L, chunk, remainder);

        // The barrier has already run every task; the countdown is drained, so this only rethrows any
        // captured task exceptions as an AggregateException.
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

        if (numWorkers == TIndex.One || ShouldRunSequentially())
        {
            ForSequential(fromInclusive, toExclusive, body);
            return;
        }

        var from = long.CreateChecked(fromInclusive);
        var participants = Math.Min(int.CreateChecked(numWorkers), _threadPool.MaxParticipants);
        var length = long.CreateChecked(n);
        var chunk = length / participants;
        var remainder = length % participants;

        _threadPool.Invoke(participants, &ForClosureTrampoline<TIndex>, body, from, chunk, remainder);
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

        if (numWorkers == TIndex.One || ShouldRunSequentially())
        {
            ForSequential(fromInclusive, toExclusive, state, body);
            return;
        }

        var from = long.CreateChecked(fromInclusive);
        var participants = Math.Min(int.CreateChecked(numWorkers), _threadPool.MaxParticipants);
        var length = long.CreateChecked(n);
        var chunk = length / participants;
        var remainder = length % participants;

        var payload = new ForStatePayload<TIndex, TState>(body, state);

        _threadPool.Invoke(participants, &ForStateTrampoline<TIndex, TState>, payload, from, chunk, remainder);
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

        if (numWorkers == TIndex.One || ShouldRunSequentially())
        {
            ForSequential(fromInclusive, toExclusive, threadLocalSetup, body, threadLocalCleanup);
            return;
        }

        using var threadLocalState = new ThreadLocal<TState>(threadLocalSetup, trackAllValues: true);

        try
        {
            For(fromInclusive, toExclusive, numWorkers, idx => body(idx, threadLocalState.Value));
        }
        finally
        {
            foreach (var value in threadLocalState.Values)
            {
                threadLocalCleanup(value);
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

    private static bool ShouldRunSequentially()
    {
        return ParallelExecutorThreadPoolThread.IsWorkerThread || ParallelExecutorThreadPool.IsRegionActive;
    }

    private static void RunTasksTrampoline(object? state, long workerIndex, long from, long chunk, long remainder)
    {
        var tasks = Unsafe.As<IParallelExecutorTask[]>(state)!;
        var (start, count) = SliceFor(workerIndex, from, chunk, remainder);

        for (var i = 0L; i < count; i++)
        {
            tasks[start + i].Execute();
        }
    }

    private static void ForClosureTrampoline<TIndex>(object? state, long workerIndex, long from, long chunk, long remainder)
        where TIndex : IBinaryInteger<TIndex>
    {
        var body = Unsafe.As<Action<TIndex>>(state)!;
        var (start, count) = SliceFor(workerIndex, from, chunk, remainder);
        var index = TIndex.CreateChecked(start);

        for (var i = 0L; i < count; i++)
        {
            body(index);
            index++;
        }
    }

    private static void ForStateTrampoline<TIndex, TState>(object? state, long workerIndex, long from, long chunk, long remainder)
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
        var payload = Unsafe.As<ForStatePayload<TIndex, TState>>(state)!;
        var (start, count) = SliceFor(workerIndex, from, chunk, remainder);
        var index = TIndex.CreateChecked(start);

        for (var i = 0L; i < count; i++)
        {
            payload.Body(index, payload.State);
            index++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (long Start, long Count) SliceFor(long workerIndex, long from, long chunk, long remainder)
    {
        var start = from + (workerIndex * chunk) + Math.Min(workerIndex, remainder);
        var count = workerIndex < remainder ? chunk + 1 : chunk;
        return (start, count);
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
        TState state,
        Action<TIndex, TState> body)
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
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

    private void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _threadPool.Dispose();
        }
    }

    private sealed class ForStatePayload<TIndex, TState>
        where TIndex : struct, IBinaryInteger<TIndex>
        where TState : struct
    {
        public ForStatePayload(Action<TIndex, TState> body, TState state)
        {
            Body = body;
            State = state;
        }

        public Action<TIndex, TState> Body { get; }

        public TState State { get; }
    }
}

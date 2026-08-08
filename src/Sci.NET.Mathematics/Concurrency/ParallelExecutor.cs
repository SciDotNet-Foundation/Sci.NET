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
public sealed class ParallelExecutor
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
    public void Run<TIndex>(
        ParallelExecutorTaskCollection<TIndex> taskCollection)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(taskCollection);

        if (taskCollection.Count == 1 || ParallelExecutorThreadPool.IsWorkerThread)
        {
            RunSequential(taskCollection);
            return;
        }

        _threadPool.EnqueueItems(taskCollection);

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

    private static void RunSequential<TIndex>(ParallelExecutorTaskCollection<TIndex> tasks)
        where TIndex : IBinaryInteger<TIndex>
    {
        foreach (var task in tasks)
        {
            task.Execute();
        }

        tasks.WaitAll();
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
}
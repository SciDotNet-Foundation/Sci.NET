// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;
using Sci.NET.Mathematics.Collections;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A dedicated pool of worker threads which execute <see cref="IParallelExecutorTask"/> work items.
/// The workers are background threads, so an undisposed pool never prevents the process from exiting.
/// </summary>
[PublicAPI]
public sealed class ParallelExecutorThreadPool : IDisposable
{
    private static readonly TimeSpan WorkerJoinTimeout = TimeSpan.FromSeconds(5);

    private readonly PausableBoundedBlockingCollection<IParallelExecutorTask> _tasks;
    private readonly List<ParallelExecutorThreadPoolThread> _threads;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelExecutorThreadPool"/> class and starts
    /// its worker threads.
    /// </summary>
    /// <param name="numThreads">The number of worker threads to start.</param>
    /// <param name="priority">The priority assigned to the worker threads.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="numThreads"/> is not positive.</exception>
    public ParallelExecutorThreadPool(int numThreads, ThreadPriority priority = ThreadPriority.Normal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(numThreads);

        _disposed = false;
        _threads = new List<ParallelExecutorThreadPoolThread>();
        _tasks = new PausableBoundedBlockingCollection<IParallelExecutorTask>(numThreads);

        CreateThreads(numThreads, priority, true);
        ParallelExecutorEventSource.Log.ThreadPoolStarted(numThreads);
    }

    /// <summary>
    /// Gets the worker threads owned by the pool.
    /// </summary>
    public IReadOnlyList<Thread?> Threads => [.. _threads.Select(x => x.GetUnderlyingThread())];

    /// <summary>
    /// Enqueues a single work item for execution on the pool.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <param name="workItem">The work item to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workItem"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been disposed.</exception>
    public void EnqueueItem<TIndex>(ParallelExecutorTask<TIndex> workItem)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(workItem);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _tasks.Add(workItem);
    }

    /// <summary>
    /// Enqueues every task in a batch for execution on the pool.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <param name="workItem">The batch of tasks to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workItem"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been disposed.</exception>
    public void EnqueueItems<TIndex>(ParallelExecutorTaskCollection<TIndex> workItem)
        where TIndex : IBinaryInteger<TIndex>
    {
        EnqueueItems(workItem, workItem?.Count ?? 0);
    }

    /// <summary>
    /// Enqueues the first <paramref name="count"/> tasks of a batch for execution on the pool.
    /// Used by <see cref="ParallelExecutor"/> to hold one task back for the calling thread.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <param name="workItem">The batch of tasks to enqueue.</param>
    /// <param name="count">The number of tasks to enqueue, starting from the first.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workItem"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative or exceeds the batch size.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the pool has been disposed.</exception>
    public void EnqueueItems<TIndex>(ParallelExecutorTaskCollection<TIndex> workItem, int count)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(workItem);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, workItem.Count);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var tasks = workItem.TasksSpan;

        for (var i = 0; i < count; i++)
        {
            _tasks.Add(tasks[i]);
        }

        ParallelExecutorEventSource.Log.BatchEnqueued(count);
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
    public void ReplaceWorkerThreads(int numThreads, ThreadPriority priority)
    {
        _tasks.PauseAdding();

        // Wait until all threads have exited.
        ParallelExecutorEventSource.Log.ThreadPoolStopping();
        JoinRunningThreads();

        _threads.Clear();

        // Don't start the threads yet, they will instantly exit due to the
        // pause still being active.
        CreateThreads(numThreads, priority, false);

        _tasks.ResumeAdding();

        ParallelExecutorEventSource.Log.ThreadPoolStarted(numThreads);
        foreach (var parallelExecutorThreadPoolThread in _threads)
        {
            parallelExecutorThreadPoolThread.Start();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ParallelExecutorEventSource.Log.ThreadPoolStopping();

        _tasks.CompleteAdding();

        JoinRunningThreads(WorkerJoinTimeout);

        _tasks.Dispose();
    }

    private void CreateThreads(int numThreads, ThreadPriority priority, bool shouldStart)
    {
        for (var i = 0; i < numThreads; i++)
        {
            var thread = new ParallelExecutorThreadPoolThread(_tasks)
            {
                ThreadIdx = i,
                Priority = priority
            };

            _threads.Add(thread);

            if (shouldStart)
            {
                thread.Start();
            }
        }
    }

    private void JoinRunningThreads(TimeSpan? timeout = null)
    {
        foreach (var thread in _threads)
        {
            if (!thread.Join(timeout))
            {
                ParallelExecutorEventSource.Log.WorkerThreadJoinTimedOut(thread.ThreadIdx);
            }
        }
    }
}
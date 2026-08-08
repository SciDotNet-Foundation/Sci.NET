// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A dedicated pool of worker threads which execute <see cref="IParallelExecutorTask"/> work items.
/// The workers are background threads, so an undisposed pool never prevents the process from exiting.
/// </summary>
[PublicAPI]
public sealed class ParallelExecutorThreadPool : IDisposable
{
    private static readonly TimeSpan WorkerJoinTimeout = TimeSpan.FromSeconds(5);

    private readonly Thread[] _threads;
    private readonly BlockingCollection<IParallelExecutorTask> _workItems;
    private volatile bool _disposed;

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

        _threads = new Thread[numThreads];
        _workItems = new BlockingCollection<IParallelExecutorTask>();

        for (var i = 0; i < numThreads; i++)
        {
            _threads[i] = new Thread(ThreadBody)
            {
                IsBackground = true,
                Name = $"Sci.NET.ParallelExecutor Worker {i}",
                Priority = priority
            };

            _threads[i]
                .Start(
                    new ParallelExecutorThreadPoolThreadDetails
                    {
                        ThreadIdx = i,
                        ThreadPool = this
                    });
        }

        ParallelExecutorEventSource.Log.ThreadPoolStarted(numThreads);
    }

    /// <summary>
    /// Gets a value indicating whether the calling thread is a worker thread belonging to any
    /// <see cref="ParallelExecutorThreadPool"/>. Used to detect re-entrant (nested) parallelism,
    /// which would otherwise deadlock the pool.
    /// </summary>
    [field: ThreadStatic]
    public static bool IsWorkerThread { get; private set; }

    /// <summary>
    /// Gets the worker threads owned by the pool.
    /// </summary>
    public IReadOnlyList<Thread> Threads => _threads;

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

        _workItems.Add(workItem);
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
        ArgumentNullException.ThrowIfNull(workItem);
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var item in workItem)
        {
            _workItems.Add(item);
        }

        ParallelExecutorEventSource.Log.BatchEnqueued(workItem.Count);
    }

    /// <summary>
    /// Shuts the pool down: no further work can be enqueued, already-queued work is drained, and
    /// the worker threads are joined (with a timeout, so a stuck task body cannot hang Dispose).
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ParallelExecutorEventSource.Log.ThreadPoolStopping();

        _workItems.CompleteAdding();

        for (var i = 0; i < _threads.Length; i++)
        {
            if (!_threads[i].Join(WorkerJoinTimeout))
            {
                ParallelExecutorEventSource.Log.WorkerThreadJoinTimedOut(i);
            }
        }

        _workItems.Dispose();
    }

    private static void ThreadBody(object? boxedThreadParams)
    {
        var threadParams = (ParallelExecutorThreadPoolThreadDetails)(boxedThreadParams ?? throw new ArgumentNullException(nameof(boxedThreadParams)));

        IsWorkerThread = true;

        ParallelExecutorEventSource.Log.WorkerThreadStarted(threadParams.ThreadIdx);

        foreach (var item in threadParams.ThreadPool._workItems.GetConsumingEnumerable())
        {
            item.Execute();
        }

        ParallelExecutorEventSource.Log.WorkerThreadStopped(threadParams.ThreadIdx);
    }
}

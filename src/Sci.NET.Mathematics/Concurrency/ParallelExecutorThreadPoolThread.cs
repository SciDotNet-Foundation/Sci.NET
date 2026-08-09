// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Collections;

namespace Sci.NET.Mathematics.Concurrency;

internal sealed class ParallelExecutorThreadPoolThread
{
    private readonly PausableBoundedBlockingCollection<IParallelExecutorTask> _taskCollection;
    private Thread? _thread;

    public ParallelExecutorThreadPoolThread(PausableBoundedBlockingCollection<IParallelExecutorTask> taskCollection)
    {
        _taskCollection = taskCollection;
    }

    [field: ThreadStatic]
    public static bool IsWorkerThread { get; private set; }

    public required int ThreadIdx { get; init; }

    public required ThreadPriority Priority { get; init; }

    public void Start()
    {
        _thread = new Thread(ThreadBody)
        {
            Name = $"Sci.NET.ParallelExecutor Worker {ThreadIdx}",
            IsBackground = true,
            Priority = Priority
        };

        _thread.Start(this);
    }

    public bool Join(TimeSpan? timeout = null)
    {
        if (_thread is null)
        {
            return true;
        }

        if (!_thread.IsAlive)
        {
            return true;
        }

        if (timeout is not null)
        {
            return _thread.Join(timeout.Value);
        }

        _thread.Join();
        return true;
    }

    public Thread? GetUnderlyingThread()
    {
        return _thread;
    }

    private static void ThreadBody(object? boxedThreadInstance)
    {
        var thread = boxedThreadInstance as ParallelExecutorThreadPoolThread ??
                     throw new InvalidOperationException("The thread started with the wrong parameters");

        IsWorkerThread = true;

        ParallelExecutorEventSource.Log.WorkerThreadStarted(thread.ThreadIdx);

        foreach (var item in thread._taskCollection.GetConsumingEnumerable())
        {
            item.Execute();
        }

        ParallelExecutorEventSource.Log.WorkerThreadStopped(thread.ThreadIdx);
    }
}
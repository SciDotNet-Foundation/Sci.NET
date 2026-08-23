// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Concurrency;

internal sealed class ParallelExecutorThreadPoolThread
{
    private readonly ParallelExecutorThreadPool _pool;
    private Thread? _thread;

    public ParallelExecutorThreadPoolThread(ParallelExecutorThreadPool pool)
    {
        _pool = pool;
    }

    [field: ThreadStatic]
    public static bool IsWorkerThread { get; private set; }

    public required int ThreadIdx { get; init; }

    public required ThreadPriority Priority { get; init; }

    public static void MarkWorkerThread()
    {
        IsWorkerThread = true;
    }

    public void Start()
    {
        _thread = new Thread(static state => ((ParallelExecutorThreadPoolThread)state!).Run())
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

    private void Run()
    {
        _pool.RunWorker(ThreadIdx);
    }
}

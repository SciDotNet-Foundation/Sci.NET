// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal class ParallelExecutorThreadPool : IDisposable
{
    private readonly Thread[] _threads;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly BlockingCollection<IParallelExecutorTask> _workItems;

    public ParallelExecutorThreadPool(int numThreads, ThreadPriority priority = ThreadPriority.Normal)
    {
        _threads = new Thread[numThreads];
        _cancellationTokenSource = new CancellationTokenSource();
        _workItems = new BlockingCollection<IParallelExecutorTask>(numThreads);

        for (var i = 0; i < numThreads; i++)
        {
            _threads[i] = new Thread(ThreadBody);
            _threads[i]
                .Start(
                    new ParallelExecutorThreadPoolThreadDetails
                    {
                        ThreadIdx = i,
                        ThreadPool = this,
                        ThreadPriority = priority
                    });
        }
    }

    ~ParallelExecutorThreadPool()
    {
        Dispose(false);
    }

    public void EnqueueItem<TIndex>(ParallelExecutorTask<TIndex> workItem)
        where TIndex : IBinaryInteger<TIndex>
    {
        _workItems.Add(workItem);
    }

    public void EnqueueItems<TIndex>(ParallelExecutorTaskCollection<TIndex> workItem)
        where TIndex : IBinaryInteger<TIndex>
    {
        foreach (var item in workItem)
        {
            _workItems.Add(item);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static void ThreadBody(object? boxedThreadParams)
    {
        var threadParams = (ParallelExecutorThreadPoolThreadDetails)(boxedThreadParams ?? throw new ArgumentNullException(nameof(boxedThreadParams)));

        Thread.CurrentThread.Priority = threadParams.ThreadPriority;

        foreach (var item in threadParams.ThreadPool._workItems.GetConsumingEnumerable())
        {
            try
            {
                item.InvokeAction();
            }
            finally
            {
                _ = item.WaitHandle.Set();
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Dispose();
            _workItems.Dispose();
        }
    }
}
// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal class ParallelExecutorTaskCollection<TIndex> : IEnumerable<ParallelExecutorTask<TIndex>>, IDisposable
    where TIndex : IBinaryInteger<TIndex>
{
    private readonly ParallelExecutorTask<TIndex>[] _tasks;
    private readonly WaitHandle[] _taskCompleteWaitHandles;

    public ParallelExecutorTaskCollection(ParallelExecutorTask<TIndex>[] tasks)
    {
        _tasks = tasks;
        _taskCompleteWaitHandles = new WaitHandle[tasks.Length];

        for (var i = 0; i < tasks.Length; i++)
        {
            _taskCompleteWaitHandles[i] = tasks[i].WaitHandle;
        }
    }

    public bool IsDisposed { get; private set; }

    public void WaitAll()
    {
        _ = WaitHandle.WaitAll(_taskCompleteWaitHandles);
    }

    public IEnumerator<ParallelExecutorTask<TIndex>> GetEnumerator()
    {
        return new ParallelExecutorTaskCollectionEnumerator<TIndex>(_tasks, this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            foreach (var parallelExecutorTask in _tasks)
            {
                parallelExecutorTask.Dispose();
            }

            IsDisposed = true;
        }
    }
}
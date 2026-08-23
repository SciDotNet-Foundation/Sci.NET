// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A batch of <see cref="ParallelExecutorTask{TIndex}"/> instances which share a single
/// <see cref="CountdownEvent"/> for completion signalling.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
[PublicAPI]
public sealed class ParallelExecutorTaskCollection<TIndex> : IEnumerable<IParallelExecutorCountdownVirtualIndexTask<TIndex>>, IDisposable
    where TIndex : IBinaryInteger<TIndex>
{
    private readonly IParallelExecutorCountdownVirtualIndexTask<TIndex>[] _tasks;
    private readonly CountdownEvent _countdown;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelExecutorTaskCollection{TIndex}"/> class.
    /// </summary>
    /// <param name="tasks">The tasks in the batch.</param>
    /// <param name="countdown">The countdown event shared by all tasks in the batch.</param>
    public ParallelExecutorTaskCollection(IParallelExecutorCountdownVirtualIndexTask<TIndex>[] tasks, CountdownEvent countdown)
    {
        _tasks = tasks;
        _countdown = countdown;
    }

    /// <summary>
    /// Gets the number of tasks in the collection.
    /// </summary>
    public int Count => _tasks.Length;

    /// <summary>
    /// Gets a value indicating whether the collection has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    internal ReadOnlySpan<IParallelExecutorCountdownVirtualIndexTask<TIndex>> TasksSpan => _tasks;

    internal IParallelExecutorTask[] Tasks => _tasks;

    /// <summary>
    /// Blocks until every task in the collection has completed, then rethrows any exceptions
    /// captured on the worker threads as a single <see cref="AggregateException"/>.
    /// </summary>
    /// <exception cref="AggregateException">Thrown when one or more task bodies threw.</exception>
    public void WaitAll()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        _countdown.Wait();

        ThrowIfAnyTaskFaulted();
    }

    /// <summary>
    /// Blocks until every task in the collection has completed or the timeout elapses.
    /// </summary>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns><c>true</c> if all tasks completed within the timeout; otherwise <c>false</c>.</returns>
    /// <exception cref="AggregateException">Thrown when all tasks completed but one or more task bodies threw.</exception>
    public bool WaitAll(TimeSpan timeout)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (!_countdown.Wait(timeout))
        {
            return false;
        }

        ThrowIfAnyTaskFaulted();

        return true;
    }

    /// <summary>
    /// Returns an enumerator that iterates over the tasks in the collection.
    /// </summary>
    /// <returns>An enumerator over the tasks in the collection.</returns>
    public IEnumerator<IParallelExecutorCountdownVirtualIndexTask<TIndex>> GetEnumerator()
    {
        return new ParallelExecutorTaskCollectionEnumerator<TIndex>(_tasks, this);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!IsDisposed)
        {
            _countdown.Dispose();
            IsDisposed = true;
        }
    }

    private void ThrowIfAnyTaskFaulted()
    {
        List<Exception>? exceptions = null;

        foreach (var task in _tasks)
        {
            if (task.Exception is not null)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(task.Exception);
            }
        }

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }
}
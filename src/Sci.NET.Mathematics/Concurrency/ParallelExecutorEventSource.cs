// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// An <see cref="EventSource"/> for the parallel execution engine. On Windows the events are
/// emitted via ETW, on other platforms via EventPipe (e.g. dotnet-trace), so consumers can
/// correlate pool activity with the runtime's own threading events.
/// </summary>
[EventSource(Name = "Sci.NET.Mathematics.ParallelExecutor")]
internal sealed class ParallelExecutorEventSource : EventSource
{
    /// <summary>
    /// The singleton instance of the <see cref="ParallelExecutorEventSource"/>.
    /// </summary>
    public static readonly ParallelExecutorEventSource Log = new();

    private ParallelExecutorEventSource()
    {
    }

    /// <summary>
    /// Emitted when a <see cref="ParallelExecutorThreadPool"/> has started its worker threads.
    /// </summary>
    /// <param name="threadCount">The number of worker threads started.</param>
    [Event(1, Level = EventLevel.Informational)]
    public void ThreadPoolStarted(int threadCount)
    {
        if (IsEnabled())
        {
            WriteEvent(1, threadCount);
        }
    }

    /// <summary>
    /// Emitted when a <see cref="ParallelExecutorThreadPool"/> begins shutting down.
    /// </summary>
    [Event(2, Level = EventLevel.Informational)]
    public void ThreadPoolStopping()
    {
        if (IsEnabled())
        {
            WriteEvent(2);
        }
    }

    /// <summary>
    /// Emitted when a worker thread starts consuming work items.
    /// </summary>
    /// <param name="threadIdx">The pool-local index of the worker thread.</param>
    [Event(3, Level = EventLevel.Verbose)]
    public void WorkerThreadStarted(int threadIdx)
    {
        if (IsEnabled())
        {
            WriteEvent(3, threadIdx);
        }
    }

    /// <summary>
    /// Emitted when a worker thread exits its consuming loop.
    /// </summary>
    /// <param name="threadIdx">The pool-local index of the worker thread.</param>
    [Event(4, Level = EventLevel.Verbose)]
    public void WorkerThreadStopped(int threadIdx)
    {
        if (IsEnabled())
        {
            WriteEvent(4, threadIdx);
        }
    }

    /// <summary>
    /// Emitted when a batch of tasks is enqueued to a pool.
    /// </summary>
    /// <param name="taskCount">The number of tasks in the batch.</param>
    [Event(5, Level = EventLevel.Verbose)]
    public void BatchEnqueued(int taskCount)
    {
        if (IsEnabled())
        {
            WriteEvent(5, taskCount);
        }
    }

    /// <summary>
    /// Emitted when a task body throws an exception on a worker thread.
    /// </summary>
    /// <param name="exceptionType">The type name of the thrown exception.</param>
    [Event(6, Level = EventLevel.Warning)]
    public void TaskFaulted(string exceptionType)
    {
        if (IsEnabled())
        {
            WriteEvent(6, exceptionType);
        }
    }

    /// <summary>
    /// Emitted when a worker thread failed to join within the shutdown timeout.
    /// </summary>
    /// <param name="threadIdx">The pool-local index of the worker thread.</param>
    [Event(7, Level = EventLevel.Warning)]
    public void WorkerThreadJoinTimedOut(int threadIdx)
    {
        if (IsEnabled())
        {
            WriteEvent(7, threadIdx);
        }
    }
}

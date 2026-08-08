// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A single unit of work executed by a <see cref="ParallelExecutorThreadPool"/> worker thread.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
[PublicAPI]
public sealed class ParallelExecutorTask<TIndex> : IParallelExecutorTask
    where TIndex : IBinaryInteger<TIndex>
{
    /// <summary>
    /// Gets the action invoked when the task executes.
    /// </summary>
    public required Action<TIndex> Action { get; init; }

    /// <summary>
    /// Gets the countdown event shared by all tasks in the batch, signalled once per completed task.
    /// </summary>
    public required CountdownEvent Countdown { get; init; }

    /// <summary>
    /// Gets the virtual thread index passed to <see cref="Action"/> when the task executes.
    /// </summary>
    public required TIndex VirtualThreadIdx { get; init; }

    /// <summary>
    /// Gets the exception thrown by the task body, if any.
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// Invokes <see cref="Action"/>, capturing any exception into <see cref="Exception"/> and
    /// signalling <see cref="Countdown"/>. This method never throws for a faulting task body,
    /// so a faulting work item cannot kill a pool worker thread.
    /// </summary>
    public void Execute()
    {
        try
        {
            Action(VirtualThreadIdx);
        }
        catch (Exception ex)
        {
            Exception = ex;
            ParallelExecutorEventSource.Log.TaskFaulted(ex.GetType().Name);
        }
        finally
        {
            _ = Countdown.Signal();
        }
    }
}

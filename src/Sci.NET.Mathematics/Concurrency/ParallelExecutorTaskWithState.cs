// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A single unit of work executed by a <see cref="ParallelExecutorThreadPool"/> worker thread, containing
/// a local state of type <typeparamref name="TState"/>.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
/// <typeparam name="TState">The type of the local state.</typeparam>
public sealed class ParallelExecutorTaskWithState<TIndex, TState> : IParallelExecutorCountdownVirtualIndexTask<TIndex>
    where TIndex : IBinaryInteger<TIndex>
    where TState : struct
{
    /// <summary>
    /// Finalizes an instance of the <see cref="ParallelExecutorTaskWithState{TIndex, TState}"/> class.
    /// </summary>
    ~ParallelExecutorTaskWithState()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the action invoked when the task executes.
    /// </summary>
    public required Action<TIndex, TState> Action { get; init; }

    /// <inheritdoc />
    public required CountdownEvent Countdown { get; init; }

    /// <inheritdoc />
    public required TIndex VirtualThreadIdx { get; init; }

    /// <summary>
    /// Gets the state object for the thread.
    /// </summary>
    public required TState State { get; init; }

    /// <inheritdoc />
    public Exception? Exception { get; private set; }

    /// <inheritdoc />
    public void Execute()
    {
        try
        {
            Action(VirtualThreadIdx, State);
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

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Countdown.Dispose();
        }
    }
}
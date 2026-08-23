// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A single unit of work executed by a <see cref="ParallelExecutorThreadPool"/> worker thread.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
[PublicAPI]
public sealed class ParallelExecutorTask<TIndex> : IParallelExecutorCountdownVirtualIndexTask<TIndex>
    where TIndex : IBinaryInteger<TIndex>
{
    /// <summary>
    /// Gets the action invoked when the task executes.
    /// </summary>
    public required Action<TIndex> Action { get; init; }

    /// <inheritdoc />
    public required CountdownEvent Countdown { get; init; }

    /// <inheritdoc />
    public required TIndex VirtualThreadIdx { get; init; }

    /// <inheritdoc />
    public Exception? Exception { get; private set; }

    /// <inheritdoc />
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
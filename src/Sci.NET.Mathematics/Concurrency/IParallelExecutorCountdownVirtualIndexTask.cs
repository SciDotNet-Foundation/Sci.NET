// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A single unit of work executed by a <see cref="ParallelExecutorThreadPool"/> worker thread with
/// a countdown and virtual thread index.
/// </summary>
/// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
public interface IParallelExecutorCountdownVirtualIndexTask<TIndex> : IParallelExecutorTask, IDisposable
    where TIndex : IBinaryInteger<TIndex>
{
    /// <summary>
    /// Gets the countdown event shared by all tasks in the batch, signalled once per completed task.
    /// </summary>
    public CountdownEvent Countdown { get; init; }

    /// <summary>
    /// Gets the virtual thread index passed to <see cref="Action"/> when the task executes.
    /// </summary>
    public TIndex VirtualThreadIdx { get; init; }
}
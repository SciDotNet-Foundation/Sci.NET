// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// An interface representing a task for parallel execution.
/// </summary>
internal interface IParallelExecutorTask
{
    /// <summary>
    /// Gets the exception thrown by the task body, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Invokes the underlying action of the <see cref="IParallelExecutorTask"/>, capturing any
    /// exception into <see cref="Exception"/> and signalling completion. This method never throws,
    /// so a faulting task body cannot kill a pool worker thread.
    /// </summary>
    public void Execute();
}

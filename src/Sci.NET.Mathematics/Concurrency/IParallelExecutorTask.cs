// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// An interface represeting a task for parallel execution.
/// </summary>
internal interface IParallelExecutorTask : IDisposable
{
    /// <summary>
    /// Gets the wait handle which signals the completion of the <see cref="IParallelExecutorTask"/>.
    /// </summary>
    public EventWaitHandle WaitHandle { get; }

    /// <summary>
    /// Invokes the underlying action of the <see cref="IParallelExecutorTask"/>.
    /// </summary>
    public void InvokeAction();
}
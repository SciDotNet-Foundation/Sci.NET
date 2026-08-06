// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal class ParallelExecutorTask<TIndex> : IParallelExecutorTask
    where TIndex : IBinaryInteger<TIndex>
{
    ~ParallelExecutorTask()
    {
        Dispose(false);
    }

    public required Action<TIndex> Action { get; init; }

    public required EventWaitHandle WaitHandle { get; set; }

    public required TIndex VirtualThreadIdx { get; set; }

    public void InvokeAction()
    {
        Action(VirtualThreadIdx);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            WaitHandle.Dispose();
        }
    }
}
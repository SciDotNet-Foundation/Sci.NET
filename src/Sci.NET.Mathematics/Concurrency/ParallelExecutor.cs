// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal class ParallelExecutor
{
    private readonly ParallelExecutorThreadPool _threadPool;

    internal ParallelExecutor(ParallelExecutorThreadPool threadPool)
    {
        _threadPool = threadPool;
    }

    public void Run<TIndex>(
        TIndex numWorkers,
        Action<TIndex> body)
        where TIndex : IBinaryInteger<TIndex>
    {
        if (numWorkers == TIndex.One)
        {
            body(TIndex.Zero);
            return;
        }

        using var workItems = ParallelExecutorTaskFactory
            .RepeatedConstantOffset(numWorkers, body);

        _threadPool.EnqueueItems(workItems);

        workItems.WaitAll();
    }

    public void For<TIndex>(
        TIndex fromInclusive,
        TIndex toExclusive,
        TIndex numWorkers,
        Action<TIndex> body)
        where TIndex : IBinaryInteger<TIndex>
    {
        var n = toExclusive - fromInclusive;

        void Loop(TIndex tid)
        {
            var start = tid * n / numWorkers;
            var end = (tid + TIndex.One) * n / numWorkers;
            var count = end - start;

            for (var i = TIndex.Zero; i < count; i++)
            {
                body(start + i);
            }
        }

        Run(numWorkers, Loop);
    }
}
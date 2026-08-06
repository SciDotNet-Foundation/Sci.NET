// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

internal static class ParallelExecutorTaskFactory
{
    public static ParallelExecutorTaskCollection<TIndex> RepeatedConstantOffset<TIndex>(TIndex replicas, Action<TIndex> action)
        where TIndex : IBinaryInteger<TIndex>
    {
        var replicasLong = long.CreateChecked(replicas);
        var currentTaskIndex = TIndex.Zero;
        var tasks = new ParallelExecutorTask<TIndex>[replicasLong];

        for (var i = 0; i < replicasLong; i++)
        {
            tasks[i] = new ParallelExecutorTask<TIndex>
            {
                WaitHandle = new ManualResetEvent(false),
                Action = action,
                VirtualThreadIdx = currentTaskIndex
            };

            currentTaskIndex++;
        }

        return new ParallelExecutorTaskCollection<TIndex>(tasks);
    }
}
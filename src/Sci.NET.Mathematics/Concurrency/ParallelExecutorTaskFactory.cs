// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Numerics;

namespace Sci.NET.Mathematics.Concurrency;

/// <summary>
/// A factory for creating batches of <see cref="ParallelExecutorTask{TIndex}"/> instances.
/// </summary>
[PublicAPI]
public static class ParallelExecutorTaskFactory
{
    /// <summary>
    /// Creates a batch of <paramref name="replicas"/> tasks which each invoke <paramref name="action"/>
    /// with their own virtual thread index (0, 1, ..., replicas - 1). All tasks in the batch share a
    /// single <see cref="CountdownEvent"/> for completion signalling.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <param name="replicas">The number of tasks to create.</param>
    /// <param name="action">The action each task invokes.</param>
    /// <returns>A disposable collection of the created tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="replicas"/> is not positive or exceeds <see cref="int.MaxValue"/>.</exception>
    public static ParallelExecutorTaskCollection<TIndex> RepeatedConstantOffset<TIndex>(
        TIndex replicas,
        Action<TIndex> action)
        where TIndex : IBinaryInteger<TIndex>
    {
        ArgumentNullException.ThrowIfNull(action);

        var replicasLong = long.CreateChecked(replicas);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(replicasLong, nameof(replicas));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(replicasLong, int.MaxValue, nameof(replicas));

        var count = (int)replicasLong;
        var countdown = new CountdownEvent(count);
        var currentTaskIndex = TIndex.Zero;
        var tasks = new IParallelExecutorCountdownVirtualIndexTask<TIndex>[count];

        for (var i = 0; i < count; i++)
        {
            tasks[i] = new ParallelExecutorTask<TIndex>
            {
                Countdown = countdown,
                Action = action,
                VirtualThreadIdx = currentTaskIndex
            };

            currentTaskIndex++;
        }

        return new ParallelExecutorTaskCollection<TIndex>(tasks, countdown);
    }

    /// <summary>
    /// Creates a batch of <paramref name="replicas"/> tasks which each invoke <paramref name="action"/>
    /// with their own virtual thread index (0, 1, ..., replicas - 1). All tasks in the batch share a
    /// single <see cref="CountdownEvent"/> for completion signalling.
    /// </summary>
    /// <typeparam name="TIndex">The integer type used for the virtual thread index.</typeparam>
    /// <typeparam name="TState">The type for the local state.</typeparam>
    /// <param name="replicas">The number of tasks to create.</param>
    /// <param name="state">The state passed to the loop action.</param>
    /// <param name="action">The action each task invokes.</param>
    /// <returns>A disposable collection of the created tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="replicas"/> is not positive or exceeds <see cref="int.MaxValue"/>.</exception>
    public static ParallelExecutorTaskCollection<TIndex> RepeatedConstantOffset<TIndex, TState>(
        TIndex replicas,
        TState state,
        Action<TIndex, TState> action)
        where TIndex : IBinaryInteger<TIndex>
        where TState : struct
    {
        ArgumentNullException.ThrowIfNull(action);

        var replicasLong = long.CreateChecked(replicas);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(replicasLong, nameof(replicas));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(replicasLong, int.MaxValue, nameof(replicas));

        var count = (int)replicasLong;
        var countdown = new CountdownEvent(count);
        var currentTaskIndex = TIndex.Zero;
        var tasks = new IParallelExecutorCountdownVirtualIndexTask<TIndex>[count];

        for (var i = 0; i < count; i++)
        {
            tasks[i] = new ParallelExecutorTaskWithState<TIndex, TState>
            {
                Countdown = countdown,
                Action = action,
                VirtualThreadIdx = currentTaskIndex,
                State = state
            };

            currentTaskIndex++;
        }

        return new ParallelExecutorTaskCollection<TIndex>(tasks, countdown);
    }

    /// <summary>
    /// Creates a batch of tasks, one per element produced by <paramref name="partitioner"/>, which
    /// each invoke <paramref name="action"/> with their element. The partitioner is fully enumerated
    /// before this method returns.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements produced by the partitioner.</typeparam>
    /// <param name="partitioner">The partitioner producing the elements to process.</param>
    /// <param name="action">The action each task invokes.</param>
    /// <returns>A disposable collection of the created tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partitioner"/> or <paramref name="action"/> is <c>null</c>.</exception>
    public static ParallelExecutorTaskCollection<int> FromPartitioner<TSource>(
        Partitioner<TSource> partitioner,
        Action<TSource> action)
    {
        ArgumentNullException.ThrowIfNull(partitioner);
        ArgumentNullException.ThrowIfNull(action);

        var partitions = partitioner.GetDynamicPartitions().ToArray();
        var countdown = new CountdownEvent(partitions.Length);
        var tasks = new IParallelExecutorCountdownVirtualIndexTask<int>[partitions.Length];

        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new ParallelExecutorTask<int>
            {
                Countdown = countdown,
                Action = idx => action(partitions[idx]),
                VirtualThreadIdx = i
            };
        }

        return new ParallelExecutorTaskCollection<int>(tasks, countdown);
    }
}
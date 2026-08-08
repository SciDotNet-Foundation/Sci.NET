// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.UnitTests.Concurrency;

public class ParallelExecutorThreadPoolTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ctor_Throws_WhenThreadCountNotPositive(int numThreads)
    {
        var act = () => new ParallelExecutorThreadPool(numThreads);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_StartsBackgroundThreads_SoThePoolCannotKeepTheProcessAlive()
    {
        using var pool = new ParallelExecutorThreadPool(2);

        pool.Threads.Should().HaveCount(2);
        pool.Threads.Should().OnlyContain(thread => thread.IsBackground);
    }

    [Fact]
    public void Ctor_AppliesRequestedThreadPriority()
    {
        using var pool = new ParallelExecutorThreadPool(2, ThreadPriority.BelowNormal);

        pool.Threads.Should().OnlyContain(thread => thread.Priority == ThreadPriority.BelowNormal);
    }

    [Fact]
    public void Dispose_StopsAllWorkerThreads()
    {
        var pool = new ParallelExecutorThreadPool(4);

        pool.Dispose();

        pool.Threads.Should().OnlyContain(thread => !thread.IsAlive);
    }

    [Fact]
    public void Dispose_CompletesQueuedWork_BeforeStoppingThreads()
    {
        var pool = new ParallelExecutorThreadPool(2);
        var executor = new ParallelExecutor(pool);
        var invocations = 0;

        executor.For(0, 8, 8, _ => Interlocked.Increment(ref invocations));

        pool.Dispose();

        invocations.Should().Be(8);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var pool = new ParallelExecutorThreadPool(2);

        pool.Dispose();
        var act = () => pool.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void EnqueueItems_Throws_AfterDispose()
    {
        var pool = new ParallelExecutorThreadPool(2);
        pool.Dispose();

        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(2, _ => { });
        var act = () => pool.EnqueueItems(workItems);

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void EnqueueItems_Throws_WhenCollectionIsNull()
    {
        using var pool = new ParallelExecutorThreadPool(2);

        var act = () => pool.EnqueueItems<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Pool_ProcessesMoreTasksThanThreads()
    {
        using var pool = new ParallelExecutorThreadPool(2);
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(64, _ => { });

        pool.EnqueueItems(workItems);

        workItems.WaitAll(TimeSpan.FromSeconds(30)).Should().BeTrue();
    }

    [Fact]
    public void IsWorkerThread_IsTrueOnWorkerThreads_AndFalseOnCallerThreads()
    {
        using var pool = new ParallelExecutorThreadPool(2);
        var observedOnWorker = false;
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            1,
            _ => observedOnWorker = ParallelExecutorThreadPool.IsWorkerThread);

        pool.EnqueueItems(workItems);
        workItems.WaitAll();

        observedOnWorker.Should().BeTrue();
        ParallelExecutorThreadPool.IsWorkerThread.Should().BeFalse();
    }

    [Fact]
    public void WorkerThread_SurvivesFaultingTask()
    {
        using var pool = new ParallelExecutorThreadPool(1);

        using (var faultingItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(1, _ => throw new InvalidOperationException("boom")))
        {
            pool.EnqueueItems(faultingItems);
            var act = () => faultingItems.WaitAll();

            act.Should().Throw<AggregateException>();
        }

        pool.Threads.Should().OnlyContain(thread => thread.IsAlive);

        using var healthyItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(4, _ => { });

        pool.EnqueueItems(healthyItems);

        healthyItems.WaitAll(TimeSpan.FromSeconds(30)).Should().BeTrue();
    }
}

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
        pool
            .Threads
            .Should()
            .AllSatisfy(x => x.Should().NotBeNull())
            .And
            .OnlyContain(thread => thread!.IsBackground);
    }

    [Fact]
    public void Ctor_AppliesRequestedThreadPriority()
    {
        using var pool = new ParallelExecutorThreadPool(2, ThreadPriority.BelowNormal);

        pool
            .Threads
            .Should()
            .AllSatisfy(x => x.Should().NotBeNull())
            .And
            .OnlyContain(thread => thread!.Priority == ThreadPriority.BelowNormal);
    }

    [Fact]
    public void Dispose_StopsAllWorkerThreads()
    {
        var pool = new ParallelExecutorThreadPool(4);

        pool.Dispose();

        pool
            .Threads.Should()
            .AllSatisfy(x => x.Should().NotBeNull())
            .And
            .OnlyContain(thread => !thread!.IsAlive);
    }

    [Fact]
    public void Dispose_CompletesInFlightWork_BeforeStoppingThreads()
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
    public void Run_Throws_AfterDispose()
    {
        var pool = new ParallelExecutorThreadPool(2);
        pool.Dispose();
        var executor = new ParallelExecutor(pool);

        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(2, _ => { });
        var act = () => executor.Run(workItems);

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Pool_ProcessesMoreTasksThanThreads()
    {
        using var pool = new ParallelExecutorThreadPool(2);
        var executor = new ParallelExecutor(pool);
        var invocations = 0;

        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(64, _ => Interlocked.Increment(ref invocations));

        executor.Run(workItems);

        invocations.Should().Be(64);
    }

    [Fact]
    public void IsWorkerThread_IsTrueOnWorkerThreads_AndFalseOnCallerThreads()
    {
        using var pool = new ParallelExecutorThreadPool(2);
        var executor = new ParallelExecutor(pool);
        var observedOnWorker = 0;

        executor.For(
            0,
            4,
            4,
            _ =>
            {
                if (ParallelExecutorThreadPoolThread.IsWorkerThread)
                {
                    Interlocked.Exchange(ref observedOnWorker, 1);
                }
            });

        observedOnWorker.Should().Be(1, "at least one slice runs on a worker thread");
        ParallelExecutorThreadPoolThread.IsWorkerThread.Should().BeFalse();
    }

    [Fact]
    public void PinnedPool_ExecutesRegion_AndCompletes()
    {
        using var pool = new ParallelExecutorThreadPool(2, ThreadPriority.Normal, pinThreads: true);
        var executor = new ParallelExecutor(pool);
        var invocations = 0;

        executor.For(0, 64, 2, _ => Interlocked.Increment(ref invocations));

        invocations.Should().Be(64);
    }

    [Fact]
    public void WorkerThread_SurvivesFaultingTask()
    {
        using var pool = new ParallelExecutorThreadPool(1);
        var executor = new ParallelExecutor(pool);

        var faulting = () => executor.For(0, 2, 2, _ => throw new InvalidOperationException("boom"));
        faulting.Should().Throw<AggregateException>();

        pool
            .Threads.Should()
            .AllSatisfy(thread => thread.Should().NotBeNull())
            .And
            .OnlyContain(thread => thread!.IsAlive);

        var invocations = 0;
        executor.For(0, 4, 2, _ => Interlocked.Increment(ref invocations));

        invocations.Should().Be(4);
    }

    [Fact]
    public void ReplaceWorkerThreads_DoesItProperly()
    {
        // Arrange
        const int replicas = 4;
        using var pool = new ParallelExecutorThreadPool(replicas);
        var executor = new ParallelExecutor(pool);
        var values = new bool[replicas];

        var oldThreads = pool.Threads.ToArray();

        var workerThread = new Thread(
            () => executor.For(
                0,
                replicas,
                replicas,
                i =>
                {
                    Thread.Sleep(1000);

                    values[i] = true;
                }));
        workerThread.Start();

        // Act
        Thread.Sleep(500);
        pool.ReplaceWorkerThreads(2, ThreadPriority.BelowNormal);

        // Assert
        workerThread.Join();
        var newThreads = pool.Threads.ToArray();

        values.Should().AllBeEquivalentTo(true);

        foreach (var oldThread in oldThreads)
        {
            oldThread.Should().NotBeNull();
            oldThread.IsAlive.Should().BeFalse();
        }

        pool.Threads.Count.Should().Be(2);
        foreach (var newThread in newThreads)
        {
            newThread.Should().NotBeNull();
            newThread.IsAlive.Should().BeTrue();
            newThread.Priority.Should().Be(ThreadPriority.BelowNormal);
        }
    }
}

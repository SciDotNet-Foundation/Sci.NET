// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.UnitTests.Concurrency;

public sealed class ParallelExecutorTests : IDisposable
{
    private readonly ParallelExecutorThreadPool _threadPool = new(4);

    public void Dispose()
    {
        _threadPool.Dispose();
    }

    [Fact]
    public void Ctor_Throws_WhenThreadPoolIsNull()
    {
        var act = () => new ParallelExecutor(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(16)]
    public void Run_InvokesEachTaskExactlyOnce(int numTasks)
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[numTasks];
        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            numTasks,
            tid => Interlocked.Increment(ref invocationsPerIndex[tid]));

        sut.Run(tasks);

        invocationsPerIndex.Should().OnlyContain(count => count == 1);
    }

    [Fact]
    public void Run_ExecutesInline_WhenSingleTask()
    {
        var sut = new ParallelExecutor(_threadPool);
        var callingThreadId = Environment.CurrentManagedThreadId;
        var bodyThreadId = 0;
        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            1,
            _ => bodyThreadId = Environment.CurrentManagedThreadId);

        sut.Run(tasks);

        bodyThreadId.Should().Be(callingThreadId);
    }

    [Fact]
    public void Run_Throws_WhenCollectionIsNull()
    {
        var sut = new ParallelExecutor(_threadPool);

        var act = () => sut.Run<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Run_PropagatesBodyExceptions_AsAggregateException()
    {
        var sut = new ParallelExecutor(_threadPool);
        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            4,
            tid =>
            {
                if (tid == 2)
                {
                    throw new InvalidOperationException("message");
                }
            });

        var act = () => sut.Run(tasks);

        act.Should()
            .Throw<AggregateException>()
            .Which.InnerExceptions.Should()
            .ContainSingle(ex => ex is InvalidOperationException && ex.Message == "message");
    }

    [Fact]
    public void Run_PropagatesBodyExceptions_WhenExecutingSequentially()
    {
        var sut = new ParallelExecutor(_threadPool);
        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            1,
            _ => throw new InvalidOperationException("message"));

        var act = () => sut.Run(tasks);

        act.Should()
            .Throw<AggregateException>()
            .Which.InnerExceptions.Should()
            .ContainSingle(ex => ex is InvalidOperationException);
    }

    [Fact]
    public void Run_PoolRemainsUsable_AfterBodyException()
    {
        var sut = new ParallelExecutor(_threadPool);

        using (var faultingTasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(4, _ => throw new InvalidOperationException("message")))
        {
            var act = () => sut.Run(faultingTasks);

            act.Should().Throw<AggregateException>();
        }

        var invocationsPerIndex = new int[4];
        using var healthyTasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            4,
            tid => Interlocked.Increment(ref invocationsPerIndex[tid]));

        sut.Run(healthyTasks);

        invocationsPerIndex.Should().OnlyContain(count => count == 1);
    }

    [Fact]
    public void Run_ExecutesLastTaskOnCallingThread()
    {
        var sut = new ParallelExecutor(_threadPool);
        var callingThreadId = Environment.CurrentManagedThreadId;
        var threadIdPerTask = new int[4];
        using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            4,
            tid => threadIdPerTask[tid] = Environment.CurrentManagedThreadId);

        sut.Run(tasks);

        threadIdPerTask[3].Should().Be(callingThreadId, "the calling thread should participate by running the last task");
    }

    [Fact]
    public void ReplaceWorkerThreads_AcceptsProcessorCountThreads()
    {
        using var pool = new ParallelExecutorThreadPool(2);
        var sut = new ParallelExecutor(pool);

        var act = () => sut.ReplaceWorkerThreads(Environment.ProcessorCount);

        act.Should().NotThrow("a thread per logical processor is the default configuration and must be allowed");
    }

    [Fact]
    public void Run_FallsBackToSequential_WhenCalledFromWorkerThread()
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[4];

        sut.For(
            0,
            4,
            4,
            _ => sut.For(0, 4, 4, innerIdx => Interlocked.Increment(ref invocationsPerIndex[innerIdx])));

        invocationsPerIndex.Should().OnlyContain(count => count == 4);
    }

    [Fact]
    public void Run_SupportsConcurrentCallers()
    {
        var sut = new ParallelExecutor(_threadPool);
        var totalInvocations = 0;

        Parallel.For(
            0,
            8,
            _ =>
            {
                using var tasks = ParallelExecutorTaskFactory.RepeatedConstantOffset(
                    4,
                    _ => Interlocked.Increment(ref totalInvocations));

                sut.Run(tasks);
            });

        totalInvocations.Should().Be(32);
    }

    [Theory]
    [InlineData(0, 128, 1)]
    [InlineData(0, 128, 4)]
    [InlineData(0, 127, 4)]
    [InlineData(1, 128, 4)]
    [InlineData(17, 91, 3)]
    [InlineData(0, 3, 4)]
    public void For_ExecutesEveryIndexExactlyOnce(int fromInclusive, int toExclusive, int numWorkers)
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[toExclusive];

        sut.For(
            fromInclusive,
            toExclusive,
            numWorkers,
            idx => Interlocked.Increment(ref invocationsPerIndex[idx]));

        var expected = Enumerable
            .Repeat(0, fromInclusive)
            .Concat(Enumerable.Repeat(1, toExclusive - fromInclusive));

        invocationsPerIndex.Should().Equal(expected);
    }

    [Fact]
    public void For_SetsTheValuesCorrectly()
    {
        var sut = new ParallelExecutor(_threadPool);
        var array = new int[128];

        sut.For(
            1,
            128,
            4,
            idx => array[idx] = idx);

        array[0].Should().Be(0);

        for (var i = 1; i < 128; i++)
        {
            array[i].Should().Be(i);
        }
    }

    [Fact]
    public void For_DoesNothing_WhenRangeIsEmpty()
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocations = 0;

        sut.For(10, 10, 4, _ => Interlocked.Increment(ref invocations));
        sut.For(10, 5, 4, _ => Interlocked.Increment(ref invocations));

        invocations.Should().Be(0);
    }

    [Fact]
    public void For_ExecutesEveryIndexExactlyOnce_WithLongIndices()
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[100];

        sut.For(
            5L,
            100L,
            4L,
            idx => Interlocked.Increment(ref invocationsPerIndex[idx]));

        invocationsPerIndex.Skip(5).Should().OnlyContain(count => count == 1);
    }

    [Fact]
    public void For_ExecutesEveryIndexExactlyOnce_WithUnsignedIndices()
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[64];

        sut.For(
            0u,
            64u,
            4u,
            idx => Interlocked.Increment(ref invocationsPerIndex[idx]));

        invocationsPerIndex.Should().OnlyContain(count => count == 1);
    }

    [Fact]
    public void For_Throws_WhenNumWorkersNotPositive()
    {
        var sut = new ParallelExecutor(_threadPool);

        var act = () => sut.For(0, 128, 0, _ => { });

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void For_Throws_WhenBodyIsNull()
    {
        var sut = new ParallelExecutor(_threadPool);

        var act = () => sut.For(0, 128, 4, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ForEach_ProcessesEveryElement()
    {
        var sut = new ParallelExecutor(_threadPool);
        var invocationsPerIndex = new int[100];

        sut.ForEach(
            Partitioner.Create(0L, 100L, 10L),
            range =>
            {
                var (start, end) = range;

                for (var i = start; i < end; i++)
                {
                    Interlocked.Increment(ref invocationsPerIndex[i]);
                }
            });

        invocationsPerIndex.Should().OnlyContain(count => count == 1);
    }

    [Fact]
    public void ForEach_Throws_WhenPartitionerIsNull()
    {
        var sut = new ParallelExecutor(_threadPool);

        var act = () => sut.ForEach<long>(null!, _ => { });

        act.Should().Throw<ArgumentNullException>();
    }
}

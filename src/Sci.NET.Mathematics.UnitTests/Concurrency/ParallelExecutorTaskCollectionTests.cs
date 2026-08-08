// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.UnitTests.Concurrency;

public class ParallelExecutorTaskCollectionTests
{
    [Fact]
    public void Factory_Throws_WhenReplicasNotPositive()
    {
        var act = () => ParallelExecutorTaskFactory.RepeatedConstantOffset(0, _ => { });

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Factory_Throws_WhenActionIsNull()
    {
        var act = () => ParallelExecutorTaskFactory.RepeatedConstantOffset(4, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Factory_AssignsSequentialVirtualThreadIndices()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(4, _ => { });

        workItems.Select(task => task.VirtualThreadIdx).Should().Equal(0, 1, 2, 3);
    }

    [Fact]
    public void Enumerator_YieldsEveryTask_IncludingTheFirst()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });

        workItems.ToArray().Should().HaveCount(3);
        workItems.Count.Should().Be(3);
    }

    [Fact]
    public void Enumerator_StopsYielding_WhenCollectionIsDisposed()
    {
        var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });
        workItems.Dispose();

        workItems.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void WaitAll_ReturnsImmediately_WhenAllTasksAlreadyExecuted()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });

        foreach (var task in workItems)
        {
            task.Execute();
        }

        var act = () => workItems.WaitAll();

        act.Should().NotThrow();
    }

    [Fact]
    public void WaitAll_WithTimeout_ReturnsFalse_WhenTasksHaveNotExecuted()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });

        workItems.WaitAll(TimeSpan.FromMilliseconds(10)).Should().BeFalse();
    }

    [Fact]
    public void WaitAll_Throws_WhenDisposed()
    {
        var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });
        workItems.Dispose();

        var act = () => workItems.WaitAll();

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void WaitAll_AggregatesExceptions_FromAllFaultedTasks()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(
            3,
            tid => throw new InvalidOperationException($"boom {tid}"));

        foreach (var task in workItems)
        {
            task.Execute();
        }

        var act = () => workItems.WaitAll();

        act.Should().Throw<AggregateException>().Which.InnerExceptions.Should().HaveCount(3);
    }

    [Fact]
    public void Execute_CapturesException_InsteadOfThrowing()
    {
        using var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(1, _ => throw new InvalidOperationException("boom"));
        var task = workItems.Single();

        var act = task.Execute;

        act.Should().NotThrow();
        task.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var workItems = ParallelExecutorTaskFactory.RepeatedConstantOffset(3, _ => { });

        workItems.Dispose();
        var act = () => workItems.Dispose();

        act.Should().NotThrow();
    }
}

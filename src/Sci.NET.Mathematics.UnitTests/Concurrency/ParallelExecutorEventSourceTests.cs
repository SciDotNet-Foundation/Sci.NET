// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using Sci.NET.Mathematics.Concurrency;

namespace Sci.NET.Mathematics.UnitTests.Concurrency;

public class ParallelExecutorEventSourceTests
{
    [Fact]
    public void Log_HasExpectedSourceName()
    {
        ParallelExecutorEventSource.Log.Name.Should().Be("Sci.NET.Mathematics.ParallelExecutor");
    }

    [Fact]
    public void PoolLifecycleAndBatchEvents_AreEmitted()
    {
        using var listener = new CollectingEventListener();

        using (var pool = new ParallelExecutorThreadPool(2))
        {
            var executor = new ParallelExecutor(pool);

            executor.For(0, 8, 2, _ => { });
        }

        listener.EventNames.Should().Contain("ThreadPoolStarted");
        listener.EventNames.Should().Contain("BatchEnqueued");
        listener.EventNames.Should().Contain("ThreadPoolStopping");
    }

    [Fact]
    public void TaskFaultedEvent_IsEmitted_WhenBodyThrows()
    {
        using var listener = new CollectingEventListener();
        using var pool = new ParallelExecutorThreadPool(2);
        var executor = new ParallelExecutor(pool);

        var act = () => executor.For(0, 2, 2, _ => throw new InvalidOperationException("boom"));

        act.Should().Throw<AggregateException>();
        listener.EventNames.Should().Contain("TaskFaulted");
    }

    private sealed class CollectingEventListener : EventListener
    {
        private readonly List<string> _eventNames = new();

        public IReadOnlyList<string> EventNames
        {
            get
            {
                lock (_eventNames)
                {
                    return _eventNames.ToList();
                }
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Sci.NET.Mathematics.ParallelExecutor")
            {
                EnableEvents(eventSource, EventLevel.Verbose);
            }

            base.OnEventSourceCreated(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            lock (_eventNames)
            {
                _eventNames.Add(eventData.EventName ?? string.Empty);
            }
        }
    }
}

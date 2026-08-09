// Copyright (c) Sci.NET Foundation. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using Sci.NET.Mathematics.Collections;

namespace Sci.NET.Mathematics.UnitTests.Collections;

public class PausableBoundedBlockingCollectionTests
{
    [Fact]
    public void Add_Then_Consume_RoundTripsAllItems()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(8);

        for (var i = 0; i < 8; i++)
        {
            collection.Add(i);
        }

        collection.CompleteAdding();

        collection.GetConsumingEnumerable().Should().BeEquivalentTo(Enumerable.Range(0, 8));
    }

    [Fact]
    public async Task CompleteAdding_WakesParkedConsumer()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(4);
        var consumer = Task.Run(() => collection.GetConsumingEnumerable().Count());

        await Task.Delay(200).ConfigureAwait(true);
        collection.CompleteAdding();

        var consumed = await consumer.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);

        consumed.Should().Be(0);
    }

    [Fact]
    public async Task PauseAdding_LetsConsumerDrainThenExit()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(4);

        collection.Add(1);
        collection.Add(2);

        collection.PauseAdding();

        var consumer = Task.Run(() => collection.GetConsumingEnumerable().Count());
        var consumed = await consumer.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);

        consumed.Should().Be(2);
    }

    [Fact]
    public async Task CompleteAdding_UnblocksProducerWaitingForFreeSlot()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(1);

        collection.Add(1);

        var producer = Task.Run(() => collection.Add(2));

        await Task.Delay(200).ConfigureAwait(true);
        collection.CompleteAdding();

        var act = async () => await producer.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);

        await act.Should()
            .ThrowAsync<InvalidOperationException>("completing adding must unblock and fail a producer waiting for a free slot")
            .ConfigureAwait(true);
    }

    [Fact]
    public void Add_Throws_WhenAddingCompleted()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(4);

        collection.CompleteAdding();
        var act = () => collection.Add(1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResumeAdding_AllowsConsumptionToContinue()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(4);

        collection.PauseAdding();
        collection.ResumeAdding();

        collection.Add(1);
        collection.CompleteAdding();

        collection.GetConsumingEnumerable().Should().BeEquivalentTo(new[] { 1 });
    }

    [Fact]
    public async Task Consume_WithCancelableToken_StopsWhenTokenCancelled()
    {
        using var collection = new PausableBoundedBlockingCollection<int>(4);
        using var cts = new CancellationTokenSource();

        var consumer = Task.Run(() => collection.GetConsumingEnumerable(cts.Token).Count());

        await Task.Delay(200).ConfigureAwait(true);
        await cts.CancelAsync().ConfigureAwait(true);

        var act = async () => await consumer.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);

        await act.Should().ThrowAsync<OperationCanceledException>("cancelling the token must release a parked consumer").ConfigureAwait(true);
    }
}

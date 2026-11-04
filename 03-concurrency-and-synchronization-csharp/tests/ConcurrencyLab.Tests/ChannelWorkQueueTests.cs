using System.Collections.Concurrent;
using System.Threading.Channels;
using ConcurrencyLab.Core.Channels;
using Shouldly;
using Xunit;

namespace ConcurrencyLab.Tests;

public class ChannelWorkQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_SingleItem_ShouldPreserveValue()
    {
        var queue = new ChannelWorkQueue<string>(capacity: 10);

        var enqueued = await queue.EnqueueAsync("telemetry-frame-1");
        enqueued.ShouldBeTrue();
        queue.Count.ShouldBe(1);

        var item = await queue.DequeueAsync();
        item.ShouldBe("telemetry-frame-1");
        queue.Count.ShouldBe(0);
    }

    [Fact]
    public void TryEnqueue_WhenFull_ShouldReturnFalseWithoutThrowing()
    {
        var queue = new ChannelWorkQueue<int>(capacity: 2);

        queue.TryEnqueue(100).ShouldBeTrue();
        queue.TryEnqueue(200).ShouldBeTrue();
        queue.TryEnqueue(300).ShouldBeFalse();
        queue.Count.ShouldBe(2);

        queue.TryDequeue(out var val1).ShouldBeTrue();
        val1.ShouldBe(100);

        queue.TryEnqueue(300).ShouldBeTrue();
        queue.Count.ShouldBe(2);
    }

    [Fact]
    public async Task MultiProducerMultiConsumer_ShouldProcessAllItemsWithoutLossOrDuplicates()
    {
        const int capacity = 50;
        const int producerCount = 8;
        const int itemsPerProducer = 125; // 8 * 125 = 1,000 total items
        const int consumerCount = 4;

        var queue = new ChannelWorkQueue<int>(capacity);
        var received = new ConcurrentBag<int>();

        // Start consumers
        var consumerTasks = Enumerable.Range(0, consumerCount).Select(async _ =>
        {
            while (!queue.IsCompleted || queue.Count > 0)
            {
                try
                {
                    var item = await queue.DequeueAsync();
                    received.Add(item);
                }
                catch (ChannelClosedException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }).ToList();

        // Start producers
        var producerTasks = Enumerable.Range(0, producerCount).Select(p => Task.Run(async () =>
        {
            for (int i = 0; i < itemsPerProducer; i++)
            {
                var val = p * 1000 + i;
                await queue.EnqueueAsync(val);
            }
        })).ToList();

        await Task.WhenAll(producerTasks);
        queue.Complete();
        await Task.WhenAll(consumerTasks);

        received.Count.ShouldBe(producerCount * itemsPerProducer);
        received.Distinct().Count().ShouldBe(producerCount * itemsPerProducer);
    }
}

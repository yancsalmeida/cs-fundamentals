using System.Collections.Concurrent;
using ConcurrencyLab.Core.Channels;
using Shouldly;
using Xunit;

namespace ConcurrencyLab.Tests;

public class LockBasedWorkQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_SingleItem_ShouldPreserveValue()
    {
        var queue = new LockBasedWorkQueue<string>(capacity: 5);

        var enqueued = await queue.EnqueueAsync("sensor-reading");
        enqueued.ShouldBeTrue();
        queue.Count.ShouldBe(1);

        var item = await queue.DequeueAsync();
        item.ShouldBe("sensor-reading");
        queue.Count.ShouldBe(0);
    }

    [Fact]
    public void TryEnqueue_WhenFull_ShouldReturnFalse()
    {
        var queue = new LockBasedWorkQueue<int>(capacity: 2);

        queue.TryEnqueue(1).ShouldBeTrue();
        queue.TryEnqueue(2).ShouldBeTrue();
        queue.TryEnqueue(3).ShouldBeFalse();
        queue.Count.ShouldBe(2);

        queue.TryDequeue(out var val).ShouldBeTrue();
        val.ShouldBe(1);

        queue.TryEnqueue(3).ShouldBeTrue();
        queue.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ConcurrentWork_ShouldCompleteWithoutDeadlock()
    {
        const int capacity = 20;
        const int producerCount = 4;
        const int itemsPerProducer = 100;
        const int consumerCount = 2;

        var queue = new LockBasedWorkQueue<int>(capacity);
        var received = new ConcurrentBag<int>();

        var consumerTasks = Enumerable.Range(0, consumerCount).Select(_ => Task.Run(async () =>
        {
            while (!queue.IsCompleted || queue.Count > 0)
            {
                try
                {
                    var item = await queue.DequeueAsync();
                    received.Add(item);
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        })).ToList();

        var producerTasks = Enumerable.Range(0, producerCount).Select(p => Task.Run(async () =>
        {
            for (int i = 0; i < itemsPerProducer; i++)
            {
                await queue.EnqueueAsync(p * 1000 + i);
            }
        })).ToList();

        await Task.WhenAll(producerTasks);
        queue.Complete();
        await Task.WhenAll(consumerTasks);

        received.Count.ShouldBe(producerCount * itemsPerProducer);
        received.Distinct().Count().ShouldBe(producerCount * itemsPerProducer);
    }
}

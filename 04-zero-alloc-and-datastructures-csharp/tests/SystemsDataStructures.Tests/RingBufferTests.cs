using System;
using SystemsDataStructures.Core.Buffers;
using Xunit;

namespace SystemsDataStructures.Tests;

public sealed class RingBufferTests
{
    [Fact]
    public void Constructor_NonPositiveCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<int>(-5));
    }

    [Fact]
    public void PushBack_WithinCapacity_PreservesFifoOrder()
    {
        var buffer = new RingBuffer<int>(4);

        buffer.PushBack(10);
        buffer.PushBack(20);
        buffer.PushBack(30);

        Assert.Equal(3, buffer.Count);
        Assert.False(buffer.IsFull);
        Assert.Equal(10, buffer[0]);
        Assert.Equal(20, buffer[1]);
        Assert.Equal(30, buffer[2]);
    }

    [Fact]
    public void PushBack_ExceedingCapacity_OverwritesOldestElements()
    {
        var buffer = new RingBuffer<int>(3);

        buffer.PushBack(1);
        buffer.PushBack(2);
        buffer.PushBack(3);
        Assert.True(buffer.IsFull);

        // Overwrite 1 and 2 with 4 and 5
        buffer.PushBack(4);
        buffer.PushBack(5);

        Assert.Equal(3, buffer.Count);
        Assert.Equal(3, buffer[0]);
        Assert.Equal(4, buffer[1]);
        Assert.Equal(5, buffer[2]);

        Span<int> snapshot = stackalloc int[3];
        int copied = buffer.CopyTo(snapshot);

        Assert.Equal(3, copied);
        Assert.Equal(new[] { 3, 4, 5 }, snapshot.ToArray());
    }

    [Fact]
    public void TryPopFront_DrainsInFifoOrderAndHandlesEmptyState()
    {
        var buffer = new RingBuffer<string>(2);
        buffer.PushBack("A");
        buffer.PushBack("B");
        buffer.PushBack("C"); // Overwrites "A"

        Assert.True(buffer.TryPopFront(out string? first));
        Assert.Equal("B", first);
        Assert.Equal(1, buffer.Count);

        Assert.True(buffer.TryPopFront(out string? second));
        Assert.Equal("C", second);
        Assert.True(buffer.IsEmpty);

        Assert.False(buffer.TryPopFront(out _));
    }
}

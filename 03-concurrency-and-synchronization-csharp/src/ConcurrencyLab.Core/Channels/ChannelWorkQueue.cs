using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace ConcurrencyLab.Core.Channels;

/// <summary>
/// High-throughput asynchronous bounded queue implemented via System.Threading.Channels.
/// Eliminates coarse-grained locking and operating system kernel context switching under contention.
/// </summary>
public class ChannelWorkQueue<T> : IWorkQueue<T> where T : notnull
{
    private readonly Channel<T> _channel;
    private readonly int _capacity;

    public int Capacity => _capacity;
    public int Count => _channel.Reader.Count;
    public bool IsCompleted => _channel.Reader.Completion.IsCompleted;

    public ChannelWorkQueue(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        _capacity = capacity;
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<T>(options);
    }

    public async ValueTask<bool> EnqueueAsync(T item, CancellationToken ct = default)
    {
        try
        {
            await _channel.Writer.WriteAsync(item, ct);
            return true;
        }
        catch (ChannelClosedException)
        {
            return false;
        }
    }

    public bool TryEnqueue(T item)
    {
        return _channel.Writer.TryWrite(item);
    }

    public ValueTask<T> DequeueAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAsync(ct);
    }

    public bool TryDequeue([NotNullWhen(true)] out T? item)
    {
        return _channel.Reader.TryRead(out item);
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }
}

namespace ConcurrencyLab.Core.Channels;

/// <summary>
/// Common abstraction for producer-consumer work queues.
/// </summary>
public interface IWorkQueue<T> where T : notnull
{
    int Capacity { get; }
    int Count { get; }
    bool IsCompleted { get; }
    ValueTask<bool> EnqueueAsync(T item, CancellationToken ct = default);
    bool TryEnqueue(T item);
    ValueTask<T> DequeueAsync(CancellationToken ct = default);
    bool TryDequeue(out T? item);
    void Complete();
}

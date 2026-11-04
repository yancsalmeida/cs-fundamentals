using System.Diagnostics.CodeAnalysis;

namespace ConcurrencyLab.Core.Channels;

/// <summary>
/// Classic thread-safe bounded queue implemented with Monitor (lock + Monitor.Wait / Monitor.PulseAll).
/// Serves as a reference baseline for lock contention and thread synchronization analysis.
/// </summary>
public class LockBasedWorkQueue<T> : IWorkQueue<T> where T : notnull
{
    private readonly Queue<T> _queue;
    private readonly int _capacity;
    private readonly object _syncRoot = new();
    private bool _isCompleted;

    public int Capacity => _capacity;

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _queue.Count;
            }
        }
    }

    public bool IsCompleted
    {
        get
        {
            lock (_syncRoot)
            {
                return _isCompleted && _queue.Count == 0;
            }
        }
    }

    public LockBasedWorkQueue(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        _capacity = capacity;
        _queue = new Queue<T>(capacity);
    }

    public ValueTask<bool> EnqueueAsync(T item, CancellationToken ct = default)
    {
        lock (_syncRoot)
        {
            while (_queue.Count >= _capacity && !_isCompleted)
            {
                ct.ThrowIfCancellationRequested();
                Monitor.Wait(_syncRoot);
            }

            if (_isCompleted)
                return ValueTask.FromResult(false);

            _queue.Enqueue(item);
            Monitor.PulseAll(_syncRoot);
            return ValueTask.FromResult(true);
        }
    }

    public bool TryEnqueue(T item)
    {
        lock (_syncRoot)
        {
            if (_isCompleted || _queue.Count >= _capacity)
                return false;

            _queue.Enqueue(item);
            Monitor.PulseAll(_syncRoot);
            return true;
        }
    }

    public ValueTask<T> DequeueAsync(CancellationToken ct = default)
    {
        lock (_syncRoot)
        {
            while (_queue.Count == 0 && !_isCompleted)
            {
                ct.ThrowIfCancellationRequested();
                Monitor.Wait(_syncRoot);
            }

            if (_queue.Count == 0 && _isCompleted)
                throw new InvalidOperationException("Queue has completed and is empty.");

            var item = _queue.Dequeue();
            Monitor.PulseAll(_syncRoot);
            return ValueTask.FromResult(item);
        }
    }

    public bool TryDequeue([NotNullWhen(true)] out T? item)
    {
        lock (_syncRoot)
        {
            if (_queue.Count > 0)
            {
                item = _queue.Dequeue()!;
                Monitor.PulseAll(_syncRoot);
                return true;
            }

            item = default;
            return false;
        }
    }

    public void Complete()
    {
        lock (_syncRoot)
        {
            _isCompleted = true;
            Monitor.PulseAll(_syncRoot);
        }
    }
}

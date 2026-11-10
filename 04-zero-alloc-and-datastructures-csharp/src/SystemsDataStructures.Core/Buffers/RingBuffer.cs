namespace SystemsDataStructures.Core.Buffers;

/// <summary>
/// Fixed-capacity contiguous circular buffer (Ring Buffer) providing O(1) insertion,
/// O(1) removal from the front, and O(1) logical indexing with zero heap allocations after construction.
/// When full, PushBack overwrites the oldest element to support continuous sliding-window telemetry.
/// </summary>
public sealed class RingBuffer<T>
{
    private readonly T[] _buffer;
    private int _head;
    private int _tail;
    private int _count;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be strictly positive.");

        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public int Capacity => _buffer.Length;
    public int Count => _count;
    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _buffer.Length;

    /// <summary>
    /// Appends an item to the back of the buffer in O(1) time.
    /// If the buffer is full, overwrites the oldest element at the head and advances the head pointer.
    /// </summary>
    public void PushBack(T item)
    {
        _buffer[_tail] = item;
        _tail = (_tail + 1) % _buffer.Length;

        if (_count == _buffer.Length)
        {
            _head = (_head + 1) % _buffer.Length;
        }
        else
        {
            _count++;
        }
    }

    /// <summary>
    /// Removes and returns the oldest element from the front of the buffer in O(1) time.
    /// </summary>
    public bool TryPopFront(out T? item)
    {
        if (_count == 0)
        {
            item = default;
            return false;
        }

        item = _buffer[_head];
        _buffer[_head] = default!;
        _head = (_head + 1) % _buffer.Length;
        _count--;
        return true;
    }

    /// <summary>
    /// Logical O(1) indexer where 0 is the oldest element (head) and Count - 1 is the newest element.
    /// </summary>
    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var physicalIndex = (_head + index) % _buffer.Length;
            return _buffer[physicalIndex];
        }
    }

    /// <summary>
    /// Copies the current contents in logical order (oldest to newest) into a caller-provided Span&lt;T&gt;
    /// without allocating a new array.
    /// </summary>
    public int CopyTo(Span<T> destination)
    {
        if (destination.Length < _count)
            throw new ArgumentException("Destination span is shorter than the current buffer count.", nameof(destination));

        for (var i = 0; i < _count; i++)
        {
            destination[i] = this[i];
        }

        return _count;
    }

    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _head = 0;
        _tail = 0;
        _count = 0;
    }
}

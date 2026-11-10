namespace SystemsDataStructures.Core.Graphs;

/// <summary>
/// Contiguous array-backed binary min-heap priority queue.
/// Guarantees O(log N) insertion (SiftUp), O(log N) minimum extraction (SiftDown), and O(1) minimum inspection.
/// </summary>
public sealed class BinaryMinHeap<TElement, TPriority>
    where TPriority : IComparable<TPriority>
{
    private (TElement Element, TPriority Priority)[] _heap;
    private int _count;

    public BinaryMinHeap(int initialCapacity = 16)
    {
        if (initialCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));

        _heap = new (TElement, TPriority)[initialCapacity];
        _count = 0;
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void Enqueue(TElement element, TPriority priority)
    {
        if (_count == _heap.Length)
        {
            Array.Resize(ref _heap, _heap.Length * 2);
        }

        _heap[_count] = (element, priority);
        SiftUp(_count);
        _count++;
    }

    public (TElement Element, TPriority Priority) Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("The binary heap is empty.");

        return _heap[0];
    }

    public (TElement Element, TPriority Priority) DequeueMin()
    {
        if (_count == 0)
            throw new InvalidOperationException("The binary heap is empty.");

        var min = _heap[0];
        _count--;

        if (_count > 0)
        {
            _heap[0] = _heap[_count];
            _heap[_count] = default;
            SiftDown(0);
        }
        else
        {
            _heap[0] = default;
        }

        return min;
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            var parentIndex = (index - 1) / 2;
            if (_heap[index].Priority.CompareTo(_heap[parentIndex].Priority) >= 0)
                break;

            (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
            index = parentIndex;
        }
    }

    private void SiftDown(int index)
    {
        while (true)
        {
            var leftChild = 2 * index + 1;
            var rightChild = 2 * index + 2;
            var smallest = index;

            if (leftChild < _count && _heap[leftChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
            {
                smallest = leftChild;
            }

            if (rightChild < _count && _heap[rightChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
            {
                smallest = rightChild;
            }

            if (smallest == index)
                break;

            (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
            index = smallest;
        }
    }
}

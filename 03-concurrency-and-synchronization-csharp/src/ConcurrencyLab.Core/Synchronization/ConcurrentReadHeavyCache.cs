using System.Diagnostics.CodeAnalysis;

namespace ConcurrencyLab.Core.Synchronization;

/// <summary>
/// Thread-safe in-memory key-value cache optimized for read-heavy workloads using ReaderWriterLockSlim.
/// Demonstrates concurrent non-blocking reads, exclusive writes, and upgradeable read lock semantics.
/// </summary>
public class ConcurrentReadHeavyCache<TKey, TValue> : IDisposable
    where TKey : notnull
    where TValue : notnull
{
    private readonly Dictionary<TKey, TValue> _store = new();
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);
    private long _readCount;
    private long _writeCount;
    private long _hitCount;
    private long _missCount;

    public int Count
    {
        get
        {
            _rwLock.EnterReadLock();
            try
            {
                return _store.Count;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }

    public long TotalReads => Interlocked.Read(ref _readCount);
    public long TotalWrites => Interlocked.Read(ref _writeCount);
    public long CacheHits => Interlocked.Read(ref _hitCount);
    public long CacheMisses => Interlocked.Read(ref _missCount);

    public bool TryGet(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        Interlocked.Increment(ref _readCount);
        _rwLock.EnterReadLock();
        try
        {
            if (_store.TryGetValue(key, out value))
            {
                Interlocked.Increment(ref _hitCount);
                return true;
            }

            Interlocked.Increment(ref _missCount);
            value = default;
            return false;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public void Set(TKey key, TValue value)
    {
        Interlocked.Increment(ref _writeCount);
        _rwLock.EnterWriteLock();
        try
        {
            _store[key] = value;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Thread-safe GetOrAdd using EnterUpgradeableReadLock.
    /// Allows concurrent readers to continue while checking for existence,
    /// upgrading to an exclusive write lock only when the key is absent.
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        Interlocked.Increment(ref _readCount);

        _rwLock.EnterUpgradeableReadLock();
        try
        {
            if (_store.TryGetValue(key, out var existing))
            {
                Interlocked.Increment(ref _hitCount);
                return existing;
            }

            Interlocked.Increment(ref _missCount);
            var computed = valueFactory(key);

            _rwLock.EnterWriteLock();
            try
            {
                // Double-checked locking pattern inside upgradeable lock
                if (_store.TryGetValue(key, out existing))
                {
                    return existing;
                }

                Interlocked.Increment(ref _writeCount);
                _store[key] = computed;
                return computed;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        finally
        {
            _rwLock.ExitUpgradeableReadLock();
        }
    }

    public bool Remove(TKey key)
    {
        Interlocked.Increment(ref _writeCount);
        _rwLock.EnterWriteLock();
        try
        {
            return _store.Remove(key);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        Interlocked.Increment(ref _writeCount);
        _rwLock.EnterWriteLock();
        try
        {
            _store.Clear();
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _rwLock.Dispose();
        GC.SuppressFinalize(this);
    }
}

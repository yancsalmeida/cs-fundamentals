namespace ConcurrencyLab.Core.Synchronization;

/// <summary>
/// Throttled task execution pool using SemaphoreSlim to enforce a strict upper limit
/// on simultaneous asynchronous executions, protecting downstream bottlenecks.
/// </summary>
public class ThrottledExecutionPool : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrency;
    private int _currentRunning;
    private int _peakRunning;
    private long _totalExecutions;

    public int MaxConcurrency => _maxConcurrency;
    public int CurrentRunning => Volatile.Read(ref _currentRunning);
    public int PeakRunning => Volatile.Read(ref _peakRunning);
    public long TotalExecutions => Interlocked.Read(ref _totalExecutions);

    public ThrottledExecutionPool(int maxConcurrency)
    {
        if (maxConcurrency <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Max concurrency must be greater than zero.");

        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> work,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        var current = Interlocked.Increment(ref _currentRunning);
        UpdatePeak(current);

        try
        {
            Interlocked.Increment(ref _totalExecutions);
            return await work(ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _currentRunning);
            _semaphore.Release();
        }
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> work,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        var current = Interlocked.Increment(ref _currentRunning);
        UpdatePeak(current);

        try
        {
            Interlocked.Increment(ref _totalExecutions);
            await work(ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref _currentRunning);
            _semaphore.Release();
        }
    }

    public async Task<bool> TryExecuteAsync(
        Func<CancellationToken, Task> work,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (!await _semaphore.WaitAsync(timeout, ct).ConfigureAwait(false))
        {
            return false;
        }

        var current = Interlocked.Increment(ref _currentRunning);
        UpdatePeak(current);

        try
        {
            Interlocked.Increment(ref _totalExecutions);
            await work(ct).ConfigureAwait(false);
            return true;
        }
        finally
        {
            Interlocked.Decrement(ref _currentRunning);
            _semaphore.Release();
        }
    }

    private void UpdatePeak(int current)
    {
        int initial;
        do
        {
            initial = Volatile.Read(ref _peakRunning);
            if (current <= initial)
                break;
        } while (Interlocked.CompareExchange(ref _peakRunning, current, initial) != initial);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}

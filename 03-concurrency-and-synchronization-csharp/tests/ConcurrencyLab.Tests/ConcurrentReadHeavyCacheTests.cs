using ConcurrencyLab.Core.Synchronization;
using Shouldly;
using Xunit;

namespace ConcurrencyLab.Tests;

public class ConcurrentReadHeavyCacheTests
{
    [Fact]
    public void SetAndTryGet_ShouldRetrieveValueAndTrackStats()
    {
        using var cache = new ConcurrentReadHeavyCache<string, double>();

        cache.Set("TT-101", 85.5);
        cache.Count.ShouldBe(1);

        cache.TryGet("TT-101", out var temp).ShouldBeTrue();
        temp.ShouldBe(85.5);

        cache.TryGet("PT-101", out _).ShouldBeFalse();

        cache.TotalReads.ShouldBe(2);
        cache.TotalWrites.ShouldBe(1);
        cache.CacheHits.ShouldBe(1);
        cache.CacheMisses.ShouldBe(1);
    }

    [Fact]
    public async Task GetOrAdd_WithContendedAccess_ShouldRunValueFactoryOnlyOnce()
    {
        using var cache = new ConcurrentReadHeavyCache<string, int>();
        int factoryInvocations = 0;

        const int threadCount = 16;
        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            return cache.GetOrAdd("shared-key", key =>
            {
                Interlocked.Increment(ref factoryInvocations);
                Thread.Sleep(20); // simulate expensive computation
                return 42;
            });
        })).ToArray();

        var results = await Task.WhenAll(tasks);

        factoryInvocations.ShouldBe(1);
        foreach (var val in results)
        {
            val.ShouldBe(42);
        }
        cache.Count.ShouldBe(1);
    }

    [Fact]
    public async Task StressTest_ConcurrentReadersAndWriters_ShouldRemainConsistent()
    {
        using var cache = new ConcurrentReadHeavyCache<int, int>();

        // Preload some keys
        for (int i = 0; i < 50; i++)
        {
            cache.Set(i, i * 10);
        }

        const int readerCount = 10;
        const int iterations = 1000;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var readers = Enumerable.Range(0, readerCount).Select(_ => Task.Run(() =>
        {
            int found = 0;
            for (int i = 0; i < iterations; i++)
            {
                if (cache.TryGet(i % 50, out var val))
                {
                    val.ShouldBeGreaterThanOrEqualTo(0);
                    found++;
                }
            }
            return found;
        })).ToArray();

        var writers = Enumerable.Range(0, 2).Select(w => Task.Run(() =>
        {
            for (int i = 0; i < 200; i++)
            {
                cache.Set(i % 50, i * 100);
            }
        })).ToArray();

        await Task.WhenAll([.. readers, .. writers]);
        cache.Count.ShouldBe(50);
        cache.TotalReads.ShouldBe(readerCount * iterations);
        cache.TotalWrites.ShouldBeGreaterThanOrEqualTo(450); // 50 initial + 400 updates
    }
}

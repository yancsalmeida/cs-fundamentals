using ConcurrencyLab.Core.Synchronization;
using Shouldly;
using Xunit;

namespace ConcurrencyLab.Tests;

public class ThrottledExecutionPoolTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldStrictlyEnforceMaxConcurrency()
    {
        const int maxConcurrency = 3;
        const int totalTasks = 15;

        using var pool = new ThrottledExecutionPool(maxConcurrency);

        var tasks = Enumerable.Range(0, totalTasks).Select(async _ =>
        {
            return await pool.ExecuteAsync(async _ =>
            {
                await Task.Delay(30);
                return pool.CurrentRunning;
            });
        });

        var results = await Task.WhenAll(tasks);

        pool.PeakRunning.ShouldBeLessThanOrEqualTo(maxConcurrency);
        pool.PeakRunning.ShouldBeGreaterThan(1);
        pool.TotalExecutions.ShouldBe(totalTasks);
        pool.CurrentRunning.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskThrows_ShouldReleaseSemaphoreGracefully()
    {
        using var pool = new ThrottledExecutionPool(maxConcurrency: 1);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await pool.ExecuteAsync<int>(_ => throw new InvalidOperationException("Simulated hardware error"));
        });

        pool.CurrentRunning.ShouldBe(0);

        // Next task should succeed without being blocked by the previous exception
        var result = await pool.ExecuteAsync(_ => Task.FromResult(100));
        result.ShouldBe(100);
        pool.TotalExecutions.ShouldBe(2);
    }

    [Fact]
    public async Task TryExecuteAsync_WhenSaturated_ShouldTimeoutGracefully()
    {
        using var pool = new ThrottledExecutionPool(maxConcurrency: 1);
        var tcs = new TaskCompletionSource<bool>();

        // Occupy the pool
        var blockingTask = pool.ExecuteAsync(async _ =>
        {
            await tcs.Task;
        });

        // Attempt execution with a short timeout
        var acquired = await pool.TryExecuteAsync(
            _ => Task.CompletedTask,
            TimeSpan.FromMilliseconds(20)
        );

        acquired.ShouldBeFalse();

        // Release blocking task
        tcs.SetResult(true);
        await blockingTask;

        // Pool should now be free
        var nowAcquired = await pool.TryExecuteAsync(
            _ => Task.CompletedTask,
            TimeSpan.FromMilliseconds(100)
        );
        nowAcquired.ShouldBeTrue();
    }
}

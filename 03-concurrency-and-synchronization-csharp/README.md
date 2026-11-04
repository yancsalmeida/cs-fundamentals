# 03 — Concurrency & Synchronization in C# (.NET 8)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Practical systems laboratory in **C# (.NET 8)** exploring multi-threaded synchronization, producer-consumer queues, read-heavy caching, and asynchronous throttling.

---

## Architectural Topics Explored

### 1. Producer-Consumer Architecture: `Channels` vs. `Monitor`
- **Asynchronous Bounded Queues (`ChannelWorkQueue<T>`):**
  Implemented using `System.Threading.Channels.Channel<T>` (`BoundedChannelFullMode.Wait`).
  Eliminates coarse-grained kernel lock contention and thread context switches under high load by utilizing lock-free and light synchronization structures.
- **Classic Monitor-Based Blocking Queue (`LockBasedWorkQueue<T>`):**
  Implemented via `Monitor.Wait` and `Monitor.PulseAll` over a shared `object` sync root. Serves as a reference implementation demonstrating condition signaling and wait-set wakeup dynamics.

### 2. Read-Heavy Concurrency: `ConcurrentReadHeavyCache<TKey, TValue>`
- Utilizes `ReaderWriterLockSlim` configured with `LockRecursionPolicy.NoRecursion`.
- Multiple concurrent reader threads acquire non-exclusive read locks (`EnterReadLock`) simultaneously without blocking each other.
- Single exclusive write lock (`EnterWriteLock`) blocks all readers and writers during store mutation.
- Upgradeable read lock (`EnterUpgradeableReadLock`) enables double-checked cache hydration without losing atomicity or blocking concurrent readers prematurely.

### 3. Resource Throttling: `ThrottledExecutionPool`
- Implements asynchronous rate-limiting and bounded resource pooling via `SemaphoreSlim`.
- Guarantees strict non-exceedance of $N$ maximum concurrent executions.
- Enforces reliable permit release in `finally` blocks, preventing resource leaks upon unhandled exceptions or cooperative cancellation.
- Supports timeout-based try-acquire semantics (`TryExecuteAsync`).

---

## Solution Structure

```
03-concurrency-and-synchronization-csharp/
├── ConcurrencyLab.sln
├── src/
│   └── ConcurrencyLab.Core/
│       ├── Channels/
│       │   ├── IWorkQueue.cs
│       │   ├── ChannelWorkQueue.cs
│       │   └── LockBasedWorkQueue.cs
│       └── Synchronization/
│           ├── ConcurrentReadHeavyCache.cs
│           └── ThrottledExecutionPool.cs
└── tests/
    └── ConcurrencyLab.Tests/
        ├── ChannelWorkQueueTests.cs
        ├── LockBasedWorkQueueTests.cs
        ├── ConcurrentReadHeavyCacheTests.cs
        └── ThrottledExecutionPoolTests.cs
```

---

## Running the Tests

Execute the automated xUnit test suite:

```bash
dotnet test ConcurrencyLab.sln
```

```text
Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12, Duration: 208 ms
```

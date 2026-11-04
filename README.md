# CS Fundamentals — Low-Level Systems (C11), Concurrency (.NET 8) & Modeling (Python)

[![CI](https://github.com/yancsalmeida/cs-fundamentals/actions/workflows/ci.yml/badge.svg)](https://github.com/yancsalmeida/cs-fundamentals/actions/workflows/ci.yml)

Practical computer science and systems programming laboratories in **C11 (`gcc`)**, **C# (.NET 8)**, and **Python 3** on **Linux (`x86_64`)**, exploring virtual memory layout, pointers, multi-threaded concurrency primitives, and discrete process telemetry modeling.

---

## Laboratories

### 1. [`01-memory-and-pointers`](./01-memory-and-pointers) (`C11`)
Direct inspection of process virtual memory on Linux `x86_64`:
- **Stack Frame Layout:** Inspecting contiguous byte allocation, alignment, and downward stack growth across primitive types (`int`, `double`, `char`).
- **Pointers & Indirection:** Address inspection (`&`), dereferencing (`*`), and in-place memory mutation.
- **Pointer Arithmetic:** Type scaling (`ptr + i` advancing by `i * sizeof(T)` bytes) over contiguous arrays.
- **Virtual Memory Segments (`.data` vs. `Heap` vs. `Stack`):** Comparing virtual address ranges across static/global data, `malloc`/`free` dynamic heap allocation, and automatic stack allocation.

```bash
make -C 01-memory-and-pointers run
```

### 2. [`02-algorithms-and-telemetry-python`](./02-algorithms-and-telemetry-python) (`Python 3`)
Discrete-time tank volume balance and process telemetry analyzer with automated `unittest` verification:
- **Cumulative State Tracking:** Discrete volume integration over flow rate variations with sentinel termination.
- **Safety Interlock Thresholds:** Automated classification across `CRITICAL_LOW`, `NORMAL`, and `CRITICAL_HIGH` operating regimes.

```bash
python3 02-algorithms-and-telemetry-python/tank_telemetry.py -v
```

### 3. [`03-concurrency-and-synchronization-csharp`](./03-concurrency-and-synchronization-csharp) (`C# / .NET 8`)
Multi-threaded concurrency patterns, thread synchronization, and asynchronous resource pooling with automated `xUnit` test suites:
- **Producer-Consumer Queues:** Asynchronous lock-free bounded channels (`ChannelWorkQueue<T>`) vs. monitor-based condition signaling (`LockBasedWorkQueue<T>`).
- **Read-Heavy Caching:** Non-blocking concurrent reads and atomic upgradeable write locks via `ReaderWriterLockSlim`.
- **Resource Throttling:** Strict concurrency bounding and graceful exception/cancellation release via `SemaphoreSlim`.

```bash
dotnet test 03-concurrency-and-synchronization-csharp/ConcurrencyLab.sln
```

---

## Author

**Yan Almeida**
- LinkedIn: [linkedin.com/in/yancsa](https://www.linkedin.com/in/yancsa/)
- GitHub: [github.com/yancsalmeida](https://github.com/yancsalmeida)

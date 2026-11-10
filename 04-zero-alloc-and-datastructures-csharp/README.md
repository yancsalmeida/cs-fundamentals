# 04 — Zero-Allocation Parsing (`Span<T>`) & Core Data Structures (`C# / .NET 8`)

Implementation from first principles of zero-allocation wire-protocol parsing and classic memory-contiguous data structures in **C# 12 / .NET 8**, verified by automated `xUnit` tests including managed heap allocation assertions (`GC.GetAllocatedBytesForCurrentThread()`).

---

## Components

### 1. Zero-Allocation Telemetry Wire Parser (`SpanTelemetryParser`)
- **`TelemetryFrameSlice` (`readonly ref struct`):** Stack-only representation of a delimiter-separated telemetry frame (`SENSOR_TAG|TIMESTAMP_UNIX_MS|VALUE|QUALITY`) holding `ReadOnlySpan<char>` slices directly into the caller's buffer without allocating intermediate `string` or `string[]` instances on the managed heap.
- **`SpanLineEnumerator` (`ref struct`):** Custom stack-only enumerator that iterates over `\n` and `\r\n` delimited lines in-place.
- **Zero-GC Verification:** `TryAggregateGoodReadings` scans multi-line payloads and computes `Count`, `Average`, `Min`, and `Max` with **0 bytes** allocated on the managed thread heap.

### 2. Fixed-Capacity Circular Buffer (`RingBuffer<T>`)
- Backed by a single contiguous `T[]` array allocated once at initialization.
- Supports $O(1)$ `PushBack` (automatically advancing the head pointer to overwrite the oldest sample when at full capacity), $O(1)$ `TryPopFront`, $O(1)$ logical indexing (`buffer[i]`), and `CopyTo(Span<T>)`.

### 3. Contiguous Binary Min-Heap & Shortest-Path Router (`BinaryMinHeap<TElement, TPriority>` & `DijkstraRouter`)
- **`BinaryMinHeap<TElement, TPriority>`:** Contiguous array-backed binary min-heap implementing $O(\log N)$ `Enqueue` (`SiftUp`), $O(\log N)$ `DequeueMin` (`SiftDown`), and $O(1)$ `Peek`.
- **`DijkstraRouter`:** Directed weighted graph shortest-path solver backed by `BinaryMinHeap<string, double>` with predecessor path reconstruction and unreachable-node detection.

---

## Running the Test Suite

```bash
dotnet test SystemsDataStructuresLab.sln -c Release
```

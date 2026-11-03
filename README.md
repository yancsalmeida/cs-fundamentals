# CS Fundamentals — Low-Level Systems & Memory Architecture in C

Practical systems programming laboratories in **C11 (`gcc`)** on **Linux (`x86_64`)**, exploring memory layout, pointers, virtual address space segments, and data structures.

---

## Laboratories

### [`01-memory-and-pointers`](./01-memory-and-pointers)
Direct inspection of process virtual memory on Linux `x86_64`:
- **Stack Frame Layout:** Inspecting contiguous byte allocation, alignment, and downward stack growth across primitive types (`int`, `double`, `char`).
- **Pointers & Indirection:** Address inspection (`&`), dereferencing (`*`), and in-place memory mutation.
- **Pointer Arithmetic:** Type scaling (`ptr + i` advancing by `i * sizeof(T)` bytes) over contiguous arrays.
- **Virtual Memory Segments (`.data` vs. `Heap` vs. `Stack`):** Comparing virtual address ranges across static/global data, `malloc`/`free` dynamic heap allocation, and automatic stack allocation.

#### Build & Run
```bash
make -C 01-memory-and-pointers run
```

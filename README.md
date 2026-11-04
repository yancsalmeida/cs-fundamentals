# CS Fundamentals — Low-Level Systems (C11) & Algorithmic Modeling (Python)

[![C & Python CI](https://github.com/yancsalmeida/cs-fundamentals/actions/workflows/ci.yml/badge.svg)](https://github.com/yancsalmeida/cs-fundamentals/actions/workflows/ci.yml)

Practical computer science and systems programming laboratories in **C11 (`gcc`)** and **Python 3** on **Linux (`x86_64`)**, exploring virtual memory layout, pointers, data structures, and discrete process telemetry modeling.

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

---

## Author

**Yan Almeida**
- LinkedIn: [linkedin.com/in/yancsa](https://www.linkedin.com/in/yancsa/)
- GitHub: [github.com/yancsalmeida](https://github.com/yancsalmeida)

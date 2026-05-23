# Related Work

A short, opinionated bibliography. Each entry is annotated with what we are borrowing
and what we are deliberately not borrowing.

## Allocators with boundary tags

* **Donald Knuth, _The Art of Computer Programming, Vol. 1, §2.5_** — original
  description of boundary-tag allocators (1968). Borrowed: coalesce-on-free.
  Not borrowed: variable-sized blocks, since our cells are uniform.

* **Doug Lea, _A Memory Allocator_ (1996)** — the canonical reference for `dlmalloc`.
  Borrowed: best-fit search over a size-sorted free list, immediate coalescing,
  the idea that the free chunk metadata _lives inside the free chunk_.
  Not borrowed: bin-by-class segregation — overkill at our scale.

* **`jemalloc`**, **`tcmalloc`** — both employ size classes and per-thread caches.
  We are single-threaded inside a `SynchronizedLinks` wrapper, so the complexity is
  unnecessary.

## Tagged-pointer / sentinel schemes for in-line metadata

* **Lua 5.4 strings** — small strings are stored inline; long strings are referenced
  by pointer with a tag bit. The "marker word at the head of a record" idea is the
  same as our `RawMarker` (and analogous to Lua's `LUA_TLNGSTR` tag).
* **SQLite "frequent" records** — SQLite reuses the first byte of a record as a type
  tag. Our `Source == RawMarker` convention is conceptually identical.

## Allocators inside persistent stores

* **PostgreSQL `FreeSpaceMap`** — uses a fan-out tree of per-page free-space records.
  Heavier than what we need but illustrates the "free space embedded in the page" idea.
* **LMDB / BoltDB free-page lists** — both maintain a sorted free-page list inside the
  database file. We are exactly mirroring this design at finer granularity.
* **MS Exchange Information Store (`.edb`) "RPS"** — Microsoft's research database
  layer also stores allocations as fixed-size cells with a free list, and uses tags
  to denote "this cell is a continuation of the previous one".

## Doublets ecosystem (internal)

* `UnitedMemoryLinks` — the existing single-cell allocator we are evolving.
* `SplitMemoryLinks` — an alternative storage that keeps doublet "index" data and
  "data" data in two separate files. Out of scope for this issue, but we keep its
  conventions in mind for future merging.
* `Platform.Memory.IResizableDirectMemory` — the unified API we re-use for storage
  expansion.
* `Platform.Collections.Methods.Lists.AbsoluteCircularDoublyLinkedListMethods` — the
  base class used by the existing unused-link list. We instantiate a second one for
  the address-sorted free-range list to keep the implementation small.

## Online research notes

Search queries used during the design phase (kept here for traceability):

* "boundary tag allocator linked list free range coalesce"
* "uniform cell allocator fragmentation"
* "tagged pointer marker first cell binary blob in memory store"
* "linksplatform doublets storage layout"
* "LMDB freelist coalesce"

No external code is _copied_ into this repository.

# Design

## Goals recap

* Allocate / deallocate contiguous **ranges of link cells** (`R3`, `R4`).
* No fragmentation — never split unless inevitable, coalesce on free (`R7`).
* "Prefer empty space" — best-fit, growth at tail only as a last resort (`R8`).
* Embed raw **binary blobs** in the same address space (`R5`, `R6`, `R9`).
* Stay drop-in compatible with `UnitedMemoryLinks` and `ILinks<>` (`R2`, `R10`).

## Design alternatives considered

| Allocator | Pros | Cons | Verdict |
| --- | --- | --- | --- |
| **Per-cell free list (status quo)** | Simplest, used today. | `O(N)` cells to allocate a range; no contiguous guarantee for ranges. | Kept for single cells, but insufficient for ranges. |
| **Bitmap (1 bit per cell)** | Predictable space, easy "find N contiguous". | Linear scan; extra header bytes; not aligned to existing on-disk format. | Rejected — adds a parallel index. |
| **Buddy allocator** | Fast power-of-two ranges. | Internal fragmentation for non-power-of-two requests; requires careful split/coalesce. | Rejected — violates "no fragmentation". |
| **Segregated free lists by size** | Best-fit in O(1) when a size class exists. | Many overflow size classes for `ulong` ranges; tricky coalescing. | Rejected — over-engineered. |
| **Address-sorted doubly-linked list of free ranges, best-fit** | Trivial coalescing; small constant factor; **stored inside the cells themselves**. | `O(F)` search where F is the number of free ranges. | **Chosen**. |

The chosen allocator is a [boundary-tag](https://en.wikipedia.org/wiki/Boundary_tag)
free-list allocator, simplified by the fact that cell sizes are uniform: there is no
need to keep a "size" word at every allocation boundary, only at the head of free
runs.

## Two markers, no ambiguity

The implementation uses **two distinct sentinels** stamped into `Source` to
discriminate the three flavours of cell that can appear in the allocated range:

| Cell flavour | `Source` value |
| --- | --- |
| Regular doublet | A link index (`≤ InternalReferencesRange.Maximum`) or `Null` |
| Raw binary blob head | `RawMarker` = `LinksConstants.Itself` |
| Multi-cell free range head | `FreeRangeMarker` = `LinksConstants.Error` |

Both sentinels live above `InternalReferencesRange.Maximum` (they are housekeeping
slots `LinksConstants<T>` already reserves), so they cannot be confused with valid
link indices. Using two distinct sentinels removes the need for any high-bit
discriminator on `Target`, and keeps the descriptor easy to read in a debugger.

## Free-range descriptors

Each free range of length `≥ 2` is described by the **first** cell of the range.
Continuation cells are zeroed. The head cell's fields are used as follows:

| Field | Free-range usage |
| --- | --- |
| `Source` | `FreeRangeMarker` |
| `Target` | `Length` of the run in cells, including this header cell. |
| `LeftAsSource` | `Previous` pointer in the address-sorted free-range list (`0` if none). |
| `RightAsSource` | `Next` pointer in the address-sorted free-range list (`0` if none). |
| `SizeAsSource` … `SizeAsTarget` | reserved (`0`). |

A single address-sorted list is sufficient: best-fit search walks the list once
in `O(F)` time. A second size-sorted list was considered but ultimately rejected
because (a) `F` stays small in practice thanks to eager coalescing and (b) the
additional bookkeeping doubles the maintenance cost of every insert/detach without
materially improving the common case.

The list head is stored in `LinksHeader.Reserved8`, which was previously unused.
No on-disk header layout change is required: databases produced by
`UnitedMemoryLinks` have `Reserved8 = 0`, which `UnitedRangedMemoryLinks` reads
as "no free ranges" — so old files open cleanly.

## Binary blob layout

A binary blob occupies one **header cell** followed by zero or more continuation
cells. The header cell holds:

| Field | Binary-blob usage |
| --- | --- |
| `Source` | `RawMarker` |
| `Target` | `Length` of the blob in **bytes**. Must be a multiple of `sizeof(TLinkAddress)`. |
| `LeftAsSource` … `SizeAsTarget` | First six `TLinkAddress` words of payload (treated as opaque bytes). |

Each continuation cell carries eight more `TLinkAddress` words of payload (no
continuation marker, no length — the head cell's `Target` drives iteration). So
a blob of `B` bytes occupies:

```text
cells = 1                         if B ≤ 6 * sizeof(TLinkAddress)
cells = 1 + ceil((B - 6 * sizeof(TLinkAddress)) / (8 * sizeof(TLinkAddress)))   otherwise
```

The encoding is unambiguous because:

* `Source == RawMarker` is never produced by `Create` (which initialises `Source`
  and `Target` to `Null` and only ever stores values inside the references range).
* The marker is **never** sampled in a continuation cell — iteration of a blob
  starts at the head cell, picks up the length, and consumes the right number of
  bytes from contiguous addresses without re-examining `Source` of any inner cell.
* Intermediate cell indices inside a blob are **not** valid link handles. This is
  a deliberate trade-off: it removes the need to scan from address `1` to detect
  whether a given index belongs to a blob's interior.

## Range allocation algorithm

```
AllocateRange(length):
    assert length >= 1
    range = freeRanges.FindBestFit(length)          // address-sorted scan
    if range != null:
        if range.Length == length:
            freeRanges.Detach(range)
            return range.Start
        if range.Length == length + 1:              // 1-cell remainder can't be a range
            freeRanges.Detach(range)
            unusedLinks.AttachAsFirst(range.Start + length)
            return range.Start
        return freeRanges.CarveFromFront(range, length)
    if length == 1:
        free = unusedLinks.TryDetachFirst()          // recycle a single-cell hole
        if free != null:
            return free
    return BumpAllocatedLinks(length)                // tail growth, last resort
```

`BumpAllocatedLinks` increments `AllocatedLinks` by `length`, growing the backing
memory if the reserved capacity is exceeded — exactly like base `Create` does,
but in one shot.

`Create(...)` itself overrides base behaviour just enough to prefer a carve from
the smallest free range whose length is `≥ 3` when the per-cell unused list is
empty (a 2-cell range can't be carved by 1 because the leftover would be smaller
than the minimum free-range size; in that case we fall through to base `Create`,
which will grow at the tail).

## Range deallocation

```
DeallocateRange(start, length):
    if start + length - 1 == AllocatedLinks:        // tail fast path
        ClearCells(start, length)
        AllocatedLinks -= length
        TrimTail()
        return
    if length == 1:                                 // single-cell hole
        ClearCells(start, 1)
        unusedLinks.AttachAsFirst(start)
        return
    freeRanges.Insert(start, length)                // coalesces with neighbours
    TrimTail()
```

`Insert` coalesces with the predecessor (if it ends exactly at `start`) and the
successor (if it begins exactly at `start + length`); it can swallow zero, one,
or two neighbours per call. `TrimTail` then walks the high-water mark down past
any trailing single-cell unused links and trailing free ranges — the asymptotic
optimality guarantee that makes long alloc/free sequences leave the database the
same size as if they had never happened.

## Marking & interaction with `Each` / `Count`

`UnitedRangedMemoryLinks` overrides `Each(...)` and `Count(...)` for the
unrestricted case. Both walk allocated addresses from `1` to `AllocatedLinks`
and skip a cell entirely when its `Source` matches either marker, advancing past
all of its continuation cells in one step. The restricted overloads delegate to
the base implementation, which already walks tree indexes that only contain real
doublet references.

`Create`/`Delete` keep their existing semantics for callers: a fresh `Create()`
returns a freshly-initialised single-cell address, and `Delete(link)` puts a
mid-range cell back on the per-cell unused list or trims the tail when removing
the highest cell.

## On-disk compatibility

* No header byte layout change. The free-range list head reuses `Reserved8`,
  which previous releases of `UnitedMemoryLinks` left at zero.
* Databases produced by `UnitedMemoryLinks` open cleanly in
  `UnitedRangedMemoryLinks`: `Reserved8 == 0` means "no free ranges yet", and
  the per-cell unused list keeps working for single-cell allocations.
* Databases produced by `UnitedRangedMemoryLinks` that contain no blobs and no
  multi-cell free ranges round-trip back through `UnitedMemoryLinks` bit-for-bit.
* Databases that **do** contain blobs or multi-cell free ranges are intentionally
  not backwards-compatible with old readers — the issue body does not require
  cross-version compatibility, and the new file flag in `LinksHeader.Reserved8`
  makes it cheap to add a version check later.

## Invariants

1. **No internal fragmentation** — every link cell is either part of an allocated
   doublet, part of an allocated blob, part of a multi-cell free range, or on the
   single-cell unused list. The union of all four sets is exactly `[1, AllocatedLinks]`.
2. **No external fragmentation buildup** — coalescing happens on every
   `DeallocateRange`; appending at the tail is the only way to grow.
3. **`AllocatedLinks` is tight** — after every deallocation, the high-water mark
   is the address of the highest still-in-use cell, never more.

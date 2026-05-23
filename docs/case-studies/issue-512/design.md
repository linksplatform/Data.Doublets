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
| **Sorted-by-address doubly-linked list of free ranges, best-fit** | Trivial coalescing; small constant factor; **stored inside the cells themselves**. | `O(F)` search where F is the number of free ranges. | **Chosen**. |

The chosen allocator is a [boundary-tag](https://en.wikipedia.org/wiki/Boundary_tag)
free-list allocator, simplified by the fact that cell sizes are uniform: there is no
need to keep a "size" word at every allocation boundary, only at the head of free
runs.

## Free-range descriptors

Each free range of length `≥ 2` is described by the **first** cell of the range. We
reuse the bits as follows:

| Field | Free-range usage |
| --- | --- |
| `Source` | `RawMarker` (sentinel — see below) |
| `Target` | `Length` of the run in cells, including this header cell. |
| `LeftAsSource` | `Previous` pointer in the size-sorted doubly-linked free-range list. |
| `RightAsSource` | `Next` pointer in the size-sorted doubly-linked free-range list. |
| `SizeAsSource` | `Previous` pointer in the address-sorted list. |
| `LeftAsTarget` | `Next` pointer in the address-sorted list. |
| `RightAsTarget` | reserved (`0`). |
| `SizeAsTarget` | reserved (`0`). |

> Why two linked lists?
> * The **address-sorted** list lets us coalesce with O(1) work — the predecessor and
>   successor of a freed range are the address-list neighbours.
> * The **size-sorted** list lets best-fit lookup return early — we walk the list from
>   the smallest range upwards and pick the first one that fits, then re-link the
>   leftover (if any) back into the free-list.

The size-sorted list head is stored in `LinksHeader.Reserved8`
(renamed to `FreeRangesHead` via the alias in `LinksRangedHeader`); the address-sorted
list head and the **count of free ranges** are stored in unused tail words of the
header that are currently zero-valued in `UnitedMemoryLinks` databases. To stay
binary-compatible we **do not** widen the on-disk header: the address-sorted list head
is simply rebuilt from the address-list pointers stored inside each free range cell at
open time, and there is no count cached.

This is functionally equivalent to the classic GNU `malloc` implementation's
[`free_list`](https://sourceware.org/glibc/wiki/MallocInternals#Free_chunks) when bins
are uniform.

## Binary blob layout

A binary blob occupies one **header cell** followed by `ceil(length / 8) - 1` payload
cells. The header cell holds:

| Field | Binary-blob usage |
| --- | --- |
| `Source` | `RawMarker` (sentinel). |
| `Target` | `Length` of the blob in `TLinkAddress` words **including** the header cell's payload words. |
| `LeftAsSource` … `SizeAsTarget` | continuation of the blob's payload. |

So a 7-word blob fits into a single cell: `Source` holds the marker, `Target` holds the
length `7`, and the remaining 6 fields (`LeftAsSource`, …, `SizeAsTarget`) hold the
six payload words. A 15-word blob spans two cells: 6 payload words in the header cell
and up to 8 payload words in the following cell. Generally,

```
cells = max(1, ceil((length - 6) / 8) + 1)   // length measured in TLinkAddress words
                                              // 6 = words available in the header cell after Source+Target
```

The encoding is unambiguous because:

* `Source == RawMarker` is never produced by `Create` (which initialises `Source` and
  `Target` to `Null` and only ever stores values inside `InternalReferencesRange`).
* The marker is **never** stored in a payload word interior to the blob, because
  consumers read raw bytes — they only look at words `[2..]` of the header cell and
  `[0..]` of the following cells.

`RawMarker` is `Constants.Continue + 1`. The references range stops at
`Continue` (since `LinksConstants` reserves the topmost values as housekeeping); the
words just past it are otherwise unused and far above `InternalReferencesRange.Maximum`,
which is the protected zone for "values that look like link indices".

## Range allocation algorithm

```
AllocateRange(length):
    assert length >= 1
    if length == 1:
        return UnusedLinksListMethods.Detach() ?? AppendOneCell()
    range = FindSmallestFreeRange(length)  // walks size-sorted list
    if range == NULL:
        return GrowAtTail(length)          // R7 fallback
    if range.Length == length:
        UnlinkFreeRange(range)
        return range.Start
    Carve(range, length)                   // shrink free-range head in place
    return range.Start
```

`GrowAtTail` bumps `AllocatedLinks` by `length` and grows the backing memory if the
reserved capacity is exceeded, exactly like `Create` does today but in one shot.

## Range deallocation

```
DeallocateRange(start, length):
    Coalesce with predecessor (if predecessor.End == start)
    Coalesce with successor   (if start + length == successor.Start)
    Insert resulting range into free-range lists
    If start + length == AllocatedLinks + 1, trim the tail and try again
```

The "trim the tail" step is what gives the allocator its asymptotic optimality: long
sequences of allocate/free at the end of the file leave the database the same size as
if the operations had never happened.

## Marking & interaction with `Each` / `Count`

When the storage iterates over allocated cells, it tests each cell against the marker
to determine whether to skip it:

```csharp
bool IsBlobHeader(ref RawLink<TLinkAddress> cell)
    => AreEqual(cell.Source, _rawMarker);

bool IsFreeRangeHeader(ref RawLink<TLinkAddress> cell)
    => AreEqual(cell.Source, _rawMarker) && BlobLengthIsFreeMarker(cell.Target);
```

Because `RawMarker` doubles for both "binary blob" and "free range header", we need a
way to discriminate the two. We use the convention that:

* a **blob** stores its true length in `Target`,
* a **free range** stores `Length` in `Target` but additionally stores the address-list
  prev/next in `SizeAsSource`/`LeftAsTarget`, which are zero in a blob's continuation
  cells but the blob _header_ can also have non-zero values there as payload. To
  remove the ambiguity, we add a second discriminator: free-range descriptors set the
  high bit of `Target` to one (since blob lengths cover at most a fraction of the
  available `TLinkAddress` range). On read we strip the high bit before reporting the
  length.

## On-disk compatibility

* `LinksRangedHeader<TLinkAddress>` has the **same byte layout** as `LinksHeader` —
  fields are reused via an `Explicit` layout with `FreeRangesHead` overlaying the
  existing `Reserved8` slot.
* Databases produced by `UnitedMemoryLinks` open cleanly in
  `UnitedRangedMemoryLinks`: at open time the free-range list head is read; if it is
  zero the storage is treated as having no free ranges (so existing databases work
  immediately, with the existing per-cell unused list still serving single-cell
  allocations).
* Databases produced by `UnitedRangedMemoryLinks` that contain only doublets — i.e. no
  blobs and no multi-cell free ranges — round-trip back through `UnitedMemoryLinks`
  bit-for-bit.

## Invariants

1. **No internal fragmentation** — every link cell is either part of an allocated
   doublet, part of an allocated blob, part of a free range, or on the single-cell
   unused list. The union of all four sets is exactly `[1, AllocatedLinks]`.
2. **No external fragmentation buildup** — coalescing happens on every deallocation;
   appending at the tail is the only way to grow.
3. **`AllocatedLinks` is tight** — after every deallocation, the high-water mark is the
   address of the highest still-in-use cell, never more.

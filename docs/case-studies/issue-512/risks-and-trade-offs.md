# Risks & Trade-offs

## Known trade-offs of the chosen design

* **Best-fit search is `O(F)`**, where `F` is the number of free ranges. In a healthy
  database this number stays small because we coalesce eagerly, but a pathological
  write pattern (allocate / free / allocate / free of differing sizes that never
  coalesce) could grow `F`. A future enhancement could add size-class bins.
* **Two linked lists per free range** consume 4 words inside the free cell — that's
  still well within the 8-word cell, but means the "smallest free range we can
  describe" is one full cell. Free runs of length 1 are punted to the existing
  single-cell unused list, which is unchanged.
* **Marker collisions** — `RawLinkSequenceMarker` is chosen above
  `InternalReferencesRange.Maximum` so it cannot be confused with a valid link index.
  Older `LinksConstants` instances that ship without the new constant simply do not
  see the marker at all, so an old reader of a new file would (a) think a raw link
  sequence head is a regular link and (b) attempt to walk the source tree from it.
  Cross-version compatibility is explicitly **not** a goal of this PR (the issue body
  says nothing about it), and the reused `Reserved8` word makes it cheap to add a
  version check later.

## Risks that the design _eliminates_

* **Internal fragmentation** — the uniform cell granularity carries over.
* **External fragmentation that grows without bound** — coalescing on deallocation,
  tail-trimming after coalescing, and best-fit allocation jointly keep the free list
  short.

## Things that are not done

* No SIMD / vectorised search through free ranges.
* No multi-threaded allocator — the existing single-writer assumption holds.
* No serialisation format change beyond reusing the `Reserved8` slot.
* No FFI surface (`Platform.Data.Doublets.FFI`) update — that lives in a separate
  repository and tracks the C ABI; we intentionally keep the new C# class additive so
  the FFI surface is unaffected.

## Future work

* Promote the free-range allocator into a stand-alone library to be reused by
  `SplitMemoryLinks`.
* Add a CLI utility (`platform-doublets defrag`) that walks the free list and
  rebuilds it from scratch, useful after offline upgrades.
* Add a raw-link-sequence cursor type to the public API that exposes the payload as a
  `Span<byte>`.

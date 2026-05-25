# Solution Plan

The plan below maps each requirement to a concrete change and lists the order of
implementation. Every checkbox corresponds to one logical commit; the commits land on
branch `issue-512-557a0a3ca78d` (PR #513).

## Step 1 — Constants scaffolding (`R5`, `R10`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/UnitedRangedLinksConstants.cs`
  — `LinksConstants<TLinkAddress>` subclass that exposes `RawLinkSequenceMarker`
  (reuses `Itself`) and `FreeRangeMarker` (reuses `Error`).
* No `LinksHeader` layout change is needed: the free-range list head reuses the
  existing `Reserved8` word, which previous releases left at zero.

## Step 2 — Range allocator (`R3`, `R7`, `R8`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/RangedFreeListMethods.cs`
  — an address-sorted, doubly-linked free-range allocator stored in-cell. The
  allocator exposes `FindBestFit(length)`, `Insert(start, length)` (with
  predecessor/successor coalescing), `Detach(start)`, `CarveFromFront`,
  `CarveFromBack`, and `TryDetachTail`.

## Step 3 — Raw link sequences (`R5`, `R6`, `R9`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/RawLinkSequenceMethods.cs`
  — encodes/decodes raw link sequences over the allocator. Exposes
  `Write(start, payload)`, `Read(start, destination)`,
  `ComputeCellsForPayload(byteLength)`, `IsRawLinkSequence(address)`,
  `GetLengthInBytes(address)`, and `GetCellCount(address)`.
* Add extension methods over `UnitedRangedMemoryLinks<TLinkAddress>` for the
  non-essential convenience API: `AllocateRawLinkSequence`,
  `WriteRawLinkSequence`, `ReadRawLinkSequence`, `DeallocateRawLinkSequence`,
  `IsRawLinkSequence`, `GetRawLinkSequenceLengthInBytes`, and
  `GetRawLinkSequenceCellCount`.
* Add an `ILinks<>` extension that identifies raw link sequence heads returned by
  `Each`, so callers can inspect sequence heads through the universal interface.

## Step 4 — `UnitedRangedMemoryLinks` (`R1`, `R2`)

* Add `csharp/Platform.Data.Doublets/Memory/UnitedRanged/Generic/UnitedRangedMemoryLinks.cs`
  — a single concrete class that inherits directly from `UnitedMemoryLinks`,
  mirrors its five constructors, overrides `SetPointers`/`ResetPointers` to wire
  up the new helpers, and overrides `Create`/`Delete`/`Update`/`Each`/`Count` so that
  raw link sequences and free-range cells are correctly handled. Exposes the
  implementation-level range API `AllocateRange` / `DeallocateRange` and the
  `IncludeRawLinkSequences` configuration. A separate `UnitedRangedMemoryLinksBase` was
  considered but proved unnecessary — direct inheritance is sufficient.

## Step 5 — Tests (`R2`, `R3`, `R4`, `R5`, `R6`, `R7`, `R8`, `R9`)

* Add `csharp/Platform.Data.Doublets.Tests/UnitedRangedMemoryLinksTests.cs` containing:
  * `BasicMemoryOperations_Substitution` — equivalent to
    `ResizableDirectMemoryLinksTests.BasicHeapMemoryTest` but using the new class.
  * `AllocateRange_ReturnsContiguousBlock`.
  * `AllocateRange_FasterThanIndividualCreates` — counts memory-resize events.
  * `AllocateRange_PrefersExistingFreeRange`.
  * `DeallocateRange_CoalescesNeighbours`.
  * `DeallocateRange_TrimsTail`.
  * `AllocateRange_OneCellRemainderFeedsSingleCellFreeList`.
  * `RawLinkSequence_Roundtrip_SingleCell`.
  * `RawLinkSequence_Roundtrip_MultiCell`.
  * `RawLinkSequence_ZeroLength_RoundtripAndUsesOneCell`.
  * `RawLinkSequence_LengthMustBeWordAligned`.
  * `RawLinkSequence_AppearsInEachByDefault`.
  * `RawLinkSequence_CanBeExcludedFromEachByConfiguration`.
  * `RawLinkSequence_CanBeReturnedByEachRestriction`.
  * `Delete_DeallocatesRawLinkSequenceThroughUniversalInterface`.
  * `Each_SkipsFreeRangesAndIncludesConfiguredRawLinkSequences`.
  * `NoFragmentation_ChaosTest` — deterministic random allocations/deallocations.

## Step 6 — Documentation (`R11`)

* Populate the `docs/case-studies/issue-512` folder (this directory).
* Reference the case study from the PR description.

## Step 7 — Final review (`R12`)

* Verify the full build / test pass.
* Ensure PR description summarises the design and points to the case study.
* Mark PR #513 ready for review.

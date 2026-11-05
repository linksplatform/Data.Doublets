C# Platform.Memory 0.4.1 throws when shrinking a nonempty reserved buffer even when UsedCapacity is zero. The C++ 0.1.0 implementation also zero-fills `newReservedCapacity - oldReservedCapacity` without checking whether capacity grew, which underflows when shrinking.

C# finite reproduction:

```csharp
using Platform.Memory;
using var memory = new HeapResizableDirectMemory();
memory.ReservedCapacity = 1024; // default reservation is a page (4096 on this Linux host)
```

Expected: reserve 1024 bytes and keep the zero-length used region. Actual on Linux/.NET 8: `ArgumentOutOfRangeException`, `toExclusive (-3072) must be greater than fromInclusive (0)`, through `Partitioner.Create` in the zero-fill path. This was exposed by constructing a SplitMemoryLinks store with a small reservation step. The complete failure trace is preserved by Data.Doublets PR https://github.com/linksplatform/Data.Doublets/pull/526.

C++ analogous path: `HeapResizableDirectMemory{4096}.ReservedCapacity(1024)` reaches `Internal::ZeroBlock(pointer + 4096, 1024 - 4096)`, a huge unsigned size past the reallocated buffer. Avoid running this unbounded probe; the source arithmetic identifies the problem, and any regression should use ASan and a finite allocator bound.

Workaround in Data.Doublets: do not shrink an already sufficient caller-owned reservation during store initialization; grow only when current capacity is less than the required minimum. The remedy is applied to both C# and C++ split stores and tested with small reservation steps.

Suggested library fix: keep the old allocation until realloc succeeds; zero-fill only if `newReservedCapacity > oldReservedCapacity`, for exactly their difference. Shrink only down to UsedCapacity; preserve the existing prefix. Add tests for 4096 -> 1024 at UsedCapacity 0, 4096 -> 8192 growth, equal capacity, and invalid shrink below UsedCapacity. Do not increase test timeouts or suppress sanitizer errors.

<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": ["# Structure formatting in C# and C++", "*"]
  }
}
-->

# Structure formatting in C# and C++

`FormatStructure` renders a reachable link structure as parenthesized source
and target pairs. `renderIndex` adds the index before each expanded pair;
`renderDebug` adds `*` before an already expanded link and `~` before an address
with no stored link. Both options default to false and apply at every depth.
Formatting reads the store without allocating or creating links in it.

For a fresh C# store, a link with raw endpoints can be formatted directly:

```csharp
using var links = new UnitedMemoryLinks<ulong>(
    new HeapResizableDirectMemory(), 4096);
var root = links.CreateAndUpdate(100UL, 101UL);
Console.WriteLine(links.FormatStructure(root, _ => false, true, true));
// (1:~100 ~101)
```

The C++ API uses the same argument order and notation:

```cpp
#include <Platform.Data.Doublets.h>
using namespace Platform::Data::Doublets;
using Storage = Memory::United::Generic::UnitedMemoryLinks<
    LinksOptions<std::uint64_t>, Platform::Memory::HeapResizableDirectMemory>;
Storage links{Platform::Memory::HeapResizableDirectMemory{}};
auto root = Create(links);
Update(links, root, std::uint64_t{100}, std::uint64_t{101});
auto notation = FormatStructure(
    links, root, [](const auto&) { return false; }, true, true);
// (1:~100 ~101)
```

The element predicate receives available endpoint links. When it returns true,
that endpoint is rendered as its index, or with the custom element callback
supplied to the overload. C# callbacks receive `(StringBuilder, Link<T>)`;
C++ callbacks receive `(std::ostream&, const Link<T>&)`. Missing internal
references are rendered numerically without invoking either callback. A raw
value whose address happens to name a stored link is expanded normally: use
distinct external references or actual element links when encoding text for
long-lived data.

With external-reference support enabled, the storage APIs expose external
addresses as virtual points. The formatter retains that behavior: they can be
recognized as elements, and otherwise expand like self-referencing points.

Self references are numeric leaves, so a point at address 1 renders as
`(1:1 1)` even in debug mode. General cycles and shared subtrees are expanded
once per formatting operation. For `(1: 2 2)` and `(2: 1 1)`, debug output is
`(1:(2:*1 *1) *2)`. Without debug markers it is `(1:(2:1 1) 2)`.
Null, Any and Itself constants are skipped at roots and endpoints, preserving
the C# API's existing convention. Deleted endpoints are missing references.

`AppendStructure` accepts the caller's output builder/stream and visited set,
which can be reused across calls. A later reference to an already visited
address uses the same repeated-link notation. The traversal remains recursive;
the visited set prevents cycles but does not bound acyclic graph depth.

## Issue 242 reproduction

The issue's `BalancedVariantConverter<ulong>` pairs the character codes of
`"super_name"` into nine links. Those raw values (for example `115` for `s`)
need not exist as stored links. Before this fix, C# dereferenced each endpoint
before checking whether it existed and threw while formatting this sequence.
On current main before this fix, the exception is
`KeyNotFoundException: Link 115 does not exist.` Older versions used a different
missing-link path; the original report did not specify an exception type.

The missing recursive `renderDebug` argument separately suppressed nested debug
markers. It did not cause infinite recursion: the visited set was already shared
by recursive calls. The C++ port had no `FormatStructure` or `AppendStructure`
API; the new native implementation follows the corrected C# behavior.

Run the standalone C# reproduction with
`dotnet run --project examples/Structure -c Release`. It builds the converter's
same adjacent-pair layers without requiring the separate Sequences package and
checks this output:

```text
(9:(8:(6:(1:~115 ~117) (2:~112 ~101)) (7:(3:~114 ~95) (4:~110 ~97))) (5:~109 ~101))
```

The exact issue snippet with the published Sequences 0.6.5 converter is also
available in `experiments/issue-242/OriginalReport`.
Run `dotnet run --project experiments/issue-242/OriginalReport -c Release`.
It asserts the same output. The normal unit suites use the equivalent pair
layers to keep their dependencies limited to this repository.

`FormatStructureTests` covers this structure, both source and target leaves,
deleted and external addresses, reserved constants, self references, cycles,
shared subtrees and custom callbacks in the normal C# and C++ test suites.

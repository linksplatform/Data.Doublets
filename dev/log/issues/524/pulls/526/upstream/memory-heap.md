`platform.memory/0.1.0` contains a mismatched allocation/deallocation pair in its C++ heap memory class. Data.Doublets PR https://github.com/linksplatform/Data.Doublets/pull/526 exposed it by executing the previously untested native suite under AddressSanitizer.

Reproduction (Clang 18, Linux, C++20, resolved Platform.Memory headers and its normal cpuinfo/clog dependencies):

```cpp
#include <Platform.Memory.h>
int main() {
    Platform::Memory::HeapResizableDirectMemory memory{4096};
    memory.UsedCapacity(1);
    *static_cast<std::byte*>(memory.Pointer()) = std::byte{42};
}
```

Compile and link with `-fsanitize=address,undefined -O0 -g0`, then run. The finite reproduction allocates one 4 KiB page. Expected: normal exit. Actual: `AddressSanitizer: alloc-dealloc-mismatch (malloc vs operator delete)` in `HeapResizableDirectMemory::~HeapResizableDirectMemory`.

The constructor/resizer uses `std::malloc` and `std::realloc`, while the destructor calls `delete static_cast<std::byte*>(Pointer());`. The fix is to replace that destructor statement with `std::free(Pointer());` (which is also safe for null). No sanitizer suppression is needed.

Workaround: apply the one-line header correction through the Conan source recipe before packaging. Data.Doublets does this with an exact-count check against the pinned recipe/source, so upstream source changes require review instead of silently bypassing the patch. A runnable reproducer and before/after diagnostics are preserved in `experiments/heap_memory_sanitizer.cpp`, `experiments/run_heap_sanitizer.py`, and `dev/log/issues/524/pulls/526/local/heap-sanitizer-{before,after}.log` in PR 526. The full GCC and Clang/sanitizer suites verify the local remedy.

Please add this small ASan destructor test to the C++ CI suite and publish a corrected package. Keep leak and allocation/deallocation checks enabled.

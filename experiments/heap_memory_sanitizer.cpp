#include <Platform.Memory.h>

// A finite, one-page reproduction of the upstream malloc/delete mismatch.
int main()
{
    Platform::Memory::HeapResizableDirectMemory memory{4096};
    memory.UsedCapacity(1);
    *static_cast<std::byte*>(memory.Pointer()) = std::byte{42};
}

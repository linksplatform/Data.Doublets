#include <Platform.Data.Doublets.h>
#include <iostream>

int main()
{
    using namespace Platform::Data;
    using namespace Platform::Data::Doublets;
    using Storage = Memory::Split::Generic::SplitMemoryLinks<Platform::Data::Doublets::LinksOptions<std::uint64_t>,
                                                             Platform::Memory::HeapResizableDirectMemory>;
    Storage storage{Platform::Memory::HeapResizableDirectMemory{4096},
                    Platform::Memory::HeapResizableDirectMemory{4096}, 4};
    for (int i = 0; i < 128; ++i)
    {
        std::cerr << "create=" << i + 1 << " data=" << storage._dataMemory.UsedCapacity() << '/'
                  << storage._dataMemory.ReservedCapacity() << " index=" << storage._indexMemory.UsedCapacity() << '/'
                  << storage._indexMemory.ReservedCapacity() << '\n';
        Create(storage);
    }
    return Count(storage) != 128;
}

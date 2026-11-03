#include <Platform.Data.Doublets.h>
#include <iostream>

int main()
{
    using namespace Platform::Data;
    using namespace Platform::Data::Doublets;
    using Storage = Memory::United::Generic::UnitedMemoryLinks<Platform::Data::Doublets::LinksOptions<std::uint64_t>,
                                                              Platform::Memory::HeapResizableDirectMemory>;
    Storage storage{Platform::Memory::HeapResizableDirectMemory{}, 4096};
    EnsurePointsCreated(storage, Storage::LinkType{1, 3, 5});
    std::cerr << "count=" << Count(storage) << ", exists(2)=" << Exists(storage, 2)
              << ", exists(4)=" << Exists(storage, 4) << std::endl;
    return Count(storage) != 3 || Exists(storage, 2) || Exists(storage, 4);
}

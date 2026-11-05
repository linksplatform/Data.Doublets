#include <Platform.Data.Doublets.h>
#include <iostream>

template <typename TStorage>
bool CreateRequestedPoints(TStorage& storage)
{
    Platform::Data::Doublets::EnsurePointsCreated(storage, typename TStorage::LinkType{1, 3, 5});
    const auto point = Platform::Data::GetLink(storage, 5);
    const auto count = Platform::Data::Count(storage);
    std::cout << "Created " << count << " requested points; last = (" << point[1] << ", " << point[2] << ")\n";
    return count == 3 && !Platform::Data::Exists(storage, 2) && !Platform::Data::Exists(storage, 4) && point[1] == 5 &&
           point[2] == 5;
}

int main()
{
    using Options = Platform::Data::Doublets::LinksOptions<std::uint64_t>;
    using Heap = Platform::Memory::HeapResizableDirectMemory;
    Platform::Data::Doublets::Memory::United::Generic::UnitedMemoryLinks<Options, Heap> united{Heap{}, 4096};
    Platform::Data::Doublets::Memory::Split::Generic::SplitMemoryLinks<Options, Heap> split{Heap{}, Heap{}, 64};
    return CreateRequestedPoints(united) && CreateRequestedPoints(split) ? 0 : 1;
}

#include "../Tools/TemporaryDirectory.h"
namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = uint64_t;

    TEST(ResizableDirectMemoryLinksTests, CountRequiresBothSpecifiedPartsToMatch)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory> storage{HeapResizableDirectMemory{}};
        auto link = Create(storage);
        Update(storage, link, 1, 2);
        EXPECT_EQ(0, storage.Count(typename decltype(storage)::LinkType{link, 1, 3}));
        EXPECT_EQ(0, storage.Count(typename decltype(storage)::LinkType{link, 3, 2}));
        EXPECT_EQ(1, storage.Count(typename decltype(storage)::LinkType{link, 1, 2}));
    }

    template <typename TStorage>
    static void TestNonexistentReferences(TStorage& storage)
    {
        using namespace Platform::Interfaces;
        constexpr auto constants = std::remove_cvref_t<decltype(storage)>::Constants;
        auto $break = constants.Break;
        auto linkAddress = Create(storage);
        Update(storage, linkAddress, std::numeric_limits<TLinkAddress>::max(),
               std::numeric_limits<TLinkAddress>::max());
        TLinkAddress resultLinkAddress{constants.Null};
        storage.Each(typename TStorage::LinkType{constants.Any, std::numeric_limits<TLinkAddress>::max(),
                                                 std::numeric_limits<TLinkAddress>::max()},
                     [&resultLinkAddress, $break](typename TStorage::LinkType foundLink) {
                         resultLinkAddress = foundLink[constants.IndexPart];
                         return $break;
                     });
        Expects(resultLinkAddress == linkAddress);
        Expects(0 == Count(storage, std::numeric_limits<TLinkAddress>::max()));
        Delete(storage, linkAddress);
    }

    template <typename TStorage>
    static void TestBasicMemoryOperations(TStorage& storage)
    {
        auto linkAddress{Create(storage)};
        Delete(storage, linkAddress);
    }

    TEST(ResizableDirectMemoryLinksTests, BasicFileMappedMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        Tools::TemporaryDirectory temporary;
        auto tempName = temporary.File("data.links");
        try
        {
            FileMappedResizableDirectMemory memory{tempName};
            UnitedMemoryLinks<LinksOptionsType> storage{std::move(memory)};
            TestBasicMemoryOperations(storage);
        }
        catch (...)
        {
            std::remove(tempName.c_str());
            throw;
        }
        std::remove(tempName.c_str());
    };

    TEST(ResizableDirectMemoryLinksTests, BasicHeapMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        HeapResizableDirectMemory memory{UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory> storage{
            std::move(memory), UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        TestBasicMemoryOperations(storage);
    }

    TEST(ResizableDirectMemoryLinksTests, NonexistentReferencesHeapMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        HeapResizableDirectMemory memory{UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory> storage{
            std::move(memory), UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        TestNonexistentReferences(storage);
    }
} // namespace Platform::Data::Doublets::Tests

namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = uint64_t;

    template<typename TStorage>
    static void TestNonexistentReferences(TStorage& storage)
    {
        using namespace Platform::Interfaces;
        constexpr auto constants = storage.Constants;
        auto $break = constants.Break;
        auto linkAddress = Create(storage);
        Update(storage, linkAddress, std::numeric_limits<TLinkAddress>::max(), std::numeric_limits<TLinkAddress>::max());
        TLinkAddress resultLinkAddress{constants.Null};
        storage.Each(typename TStorage::LinkType{constants.Any, std::numeric_limits<TLinkAddress>::max(), std::numeric_limits<TLinkAddress>::max()}, [&resultLinkAddress, $break] (typename TStorage::LinkType foundLink) {
            resultLinkAddress = foundLink[constants.IndexPart];
            return $break;
        });
        Expects(resultLinkAddress == linkAddress);
        Expects(0 == Count(storage, std::numeric_limits<TLinkAddress>::max()));
        Delete(storage, linkAddress);
    }

    template<typename TStorage>
    static void TestBasicMemoryOperations(TStorage& storage)
    {
        auto linkAddress {Create(storage)};
        Delete(storage, linkAddress);
    }

    TEST(ResizableDirectMemoryLinksTests, BasicFileMappedMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        auto tempName {std::tmpnam(nullptr)};
        try
        {
            FileMappedResizableDirectMemory memory { tempName };
            UnitedMemoryLinks<LinksOptionsType> storage {std::move(memory)};
            TestBasicMemoryOperations(storage);
        }
        catch (...)
        {
            std::remove(tempName);
            throw;
        }
        std::remove(tempName);
    };

    TEST(ResizableDirectMemoryLinksTests, BasicHeapMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        HeapResizableDirectMemory memory {UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory> storage {std::move(memory), UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        TestBasicMemoryOperations(storage);
    }

    TEST(ResizableDirectMemoryLinksTests, NonexistentReferencesHeapMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        HeapResizableDirectMemory memory {UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory> storage {std::move(memory), UnitedMemoryLinks<LinksOptionsType>::DefaultLinksSizeStep};
        TestNonexistentReferences(storage);
    }
}

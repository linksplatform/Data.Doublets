namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = uint64_t;
    static constexpr LinksConstants<TLinkAddress> constants {LinksConstants<TLinkAddress>{true}};

    template<typename TStorage>
    static void TestNonexistentReferences(TStorage& storage)
    {
        using namespace Platform::Interfaces;
        auto linkAddress = Create(storage);
        Update(storage, linkAddress, std::numeric_limits<TLinkAddress>::max(), std::numeric_limits<TLinkAddress>::max());
        TLinkAddress resultLinkAddress{constants.Null};
        storage.Each(typename TStorage::LinkType{constants.Any, std::numeric_limits<TLinkAddress>::max(), std::numeric_limits<TLinkAddress>::max()}, [&resultLinkAddress] (typename TStorage::LinkType foundLink) {
            resultLinkAddress = foundLink[constants.IndexPart];
            return constants.Break;
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
        auto tempName {std::tmpnam(nullptr)};
        try
        {
            FileMappedResizableDirectMemory memory { tempName };
            UnitedMemoryLinks<LinksOptions<TLinkAddress>> storage {std::move(memory)};
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
        HeapResizableDirectMemory memory {UnitedMemoryLinks<LinksOptions<TLinkAddress>>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory> storage {std::move(memory), UnitedMemoryLinks<LinksOptions<TLinkAddress>>::DefaultLinksSizeStep};
        TestBasicMemoryOperations(storage);
    }

    TEST(ResizableDirectMemoryLinksTests, NonexistentReferencesHeapMemoryTest)
    {
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Memory;
        HeapResizableDirectMemory memory {UnitedMemoryLinks<LinksOptions<TLinkAddress>>::DefaultLinksSizeStep};
        UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory> storage {std::move(memory), UnitedMemoryLinks<LinksOptions<TLinkAddress>>::DefaultLinksSizeStep};
        TestNonexistentReferences(storage);
    }
}

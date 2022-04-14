namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = uint64_t;
    static constexpr LinksConstants<TLinkAddress > constants {LinksConstants<TLinkAddress>{}};

    template<typename TStorage>
    static void TestNonexistentReferences(TStorage& & storage)
    {
        using namespace Platform::Interfaces;
        auto linkAddress = storage.Create();
        Update(storage, linkAddress, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max());
        TLinkAddress resultLinkAddress{constants.Null};
        storage.Each(std::vector{constants.Any, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max()}, [&resultLinkAddress] (typename TStorage::LinkType foundLink) {
            resultLinkAddress = foundLink[constants.IndexPart];
            return constants.Break;
        });
        Expects(resultLinkAddress == linkAddress);
        Expects(0 == Count(storage, std::numeric_limits<std::uint64_t>::max()));
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
        auto tempName {std::tmpnam(nullptr)};
        try
        {
            UInt64UnitedMemoryLinks memoryAdapter { tempName };
            TestBasicMemoryOperations(memoryAdapter);
            File.Delete(tempFilename);
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
        HeapResizableDirectMemory memory {UInt64UnitedMemoryLinks.DefaultLinksSizeStep};
        UInt64UnitedMemoryLinks memoryAdapter {memory, UInt64UnitedMemoryLinks.DefaultLinksSizeStep};
        TestBasicMemoryOperations(memoryAdapter);
    }

    TEST(ResizableDirectMemoryLinksTests, NonexistentReferencesHeapMemoryTest)
    {
        HeapResizableDirectMemory memory {UInt64UnitedMemoryLinks.DefaultLinksSizeStep};
        UInt64UnitedMemoryLinks memoryAdapter {memory, UInt64UnitedMemoryLinks.DefaultLinksSizeStep};
        TestNonexistentReferences(memoryAdapter);
    }
}

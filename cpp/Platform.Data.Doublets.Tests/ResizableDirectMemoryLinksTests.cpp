namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = uint64_t;
    static LinksConstants<TLinkAddress > _constants {LinksConstants<TLinkAddress>{}};
    static void TestNonexistentReferences(auto&& &memoryAdapter)
    {
        auto link = memoryAdapter.Create();
        Update(memoryAdapter, link, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max());
        TLinkAddress resultLink {_constants.Null};
        memoryAdapter.Each(std::array{_constants.Any, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max()}, [&resultLink] (Interfaces::CArray auto foundLink) {
            resultLink = foundLink[_constants.IndexPart];
            return _constants.Break;
        });
        Expects(resultLink == link);
        Expects(0 == Count(memoryAdapter, std::numeric_limits<std::uint64_t>::max()));
        Delete(memoryAdapter, link);
    }

    static void TestBasicMemoryOperations(auto&& memoryAdapter)
    {
        auto link {Create(memoryAdapter)};
        Delete(memoryAdapter, link);
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

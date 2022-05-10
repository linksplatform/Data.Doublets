namespace Platform::Data::Doublets::Tests
{
    template <typename TLinkAddress>
    static void Using(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>, HeapResizableDirectMemory> storage{ dataMemory, indexMemory };
        action(storage);
    }

    template <typename TLinkAddress>
    static void UsingWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<LinksOptions<>, HeapResizableDirectMemory> storage{ dataMemory, indexMemory };
        action(storage);
    }
    
    template <typename TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        using StorageType = SplitMemoryLinks<LinksOptions<>, HeapResizableDirectMemory>; 
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        DecoratedStorageType storage{ dataMemory, indexMemory };
        action(storage);
    }

    TEST(SplitMemoryGenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { return storage.TestCrudOperations(); });
        Using<std::uint16_t>([] (auto&& storage) { return storage.TestCrudOperations(); });
        Using<std::uint32_t>([] (auto&& storage) { return storage.TestCrudOperations(); });
        Using<std::uint64_t>([] (auto&& storage) { return storage.TestCrudOperations(); });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
        UsingWithExternalReferences<std::uint8_t>([] (auto&& storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint16_t>([] (auto&& storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint32_t>([] (auto&& storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint64_t>([] (auto&& storage) { return storage.TestRawNumbersCrudOperations(); });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { return  storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(16));
        Using<std::uint16_t>([] (auto&& storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint32_t>([] (auto&& storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint64_t>([] (auto&& storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
    }
}

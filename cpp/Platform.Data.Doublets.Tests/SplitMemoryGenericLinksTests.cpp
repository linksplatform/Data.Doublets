namespace Platform::Data::Doublets::Tests
{
    template <typename TLinkAddress>
    static void Using(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>, HeapResizableDirectMemory> storage{ std::move(dataMemory), std::move(indexMemory) };
        action(storage);
    }

    template <typename TLinkAddress>
    static void UsingWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory> storage{ std::move(dataMemory), std::move(indexMemory) };
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
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        DecoratedStorageType storage{ std::move(dataMemory), std::move(indexMemory) };
        action(storage);
    }

//    TEST(SplitMemoryGenericLinksTests, CrudTest)
//    {
//        Using<std::uint8_t>([] (auto&& storage) { return TestCrudOperations(storage); });
//        Using<std::uint16_t>([] (auto&& storage) { return TestCrudOperations(storage); });
//        Using<std::uint32_t>([] (auto&& storage) { return TestCrudOperations(storage); });
//        Using<std::uint64_t>([] (auto&& storage) { return TestCrudOperations(storage); });
//    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
        UsingWithExternalReferences<std::uint8_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
//        UsingWithExternalReferences<std::uint16_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
//        UsingWithExternalReferences<std::uint32_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
//        UsingWithExternalReferences<std::uint64_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
    }

//    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
//    {
//        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
//        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
//        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
//        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
//    }
}

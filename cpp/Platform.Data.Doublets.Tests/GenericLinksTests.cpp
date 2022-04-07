namespace Platform::Data::Doublets::Tests
{
   template <typename TLinkAddress>
    static void Using(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using namespace Platform::Collections;
        UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory> storage {HeapResizableDirectMemory{}};
        action(storage);
    }

    TEST(GenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestCrudOperations(storage); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Collections;
        using namespace Platform::Data::Doublets;
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint8_t>, HeapResizableDirectMemory>> UInt8TDecoratedStorage{HeapResizableDirectMemory{}};
        TestMultipleRandomCreationsAndDeletions(UInt8TDecoratedStorage, 16);
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint16_t>, HeapResizableDirectMemory>> UInt16TDecoratedStorage{HeapResizableDirectMemory{}};
        TestMultipleRandomCreationsAndDeletions(UInt16TDecoratedStorage, 100);
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint32_t>, HeapResizableDirectMemory>> UInt32TDecoratedStorage{HeapResizableDirectMemory{}};
        TestMultipleRandomCreationsAndDeletions(UInt32TDecoratedStorage, 100);
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint64_t>, HeapResizableDirectMemory>> UInt64TDecoratedStorage{HeapResizableDirectMemory{}};
        TestMultipleRandomCreationsAndDeletions(UInt64TDecoratedStorage, 100);
    }
}

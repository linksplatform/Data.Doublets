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

    template <typename TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::United::Generic;
      using namespace Platform::Data::Doublets::Decorators;
      HeapResizableDirectMemory dataMemory { };
      HeapResizableDirectMemory indexMemory { };
      using StorageType = UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
      using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
      DecoratedStorageType storage{ std::move(dataMemory), std::move(indexMemory) };
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
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

namespace Platform::Data::Doublets::Tests::Static::GenericLinksTests
{
    template<std::integral TLinkAddress>
    static void UsingStorage(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::United::Generic;
      using StorageType = UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
      StorageType storage{ HeapResizableDirectMemory{ } };
      action(storage);
    }

    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::United::Generic;
      using namespace Platform::Data::Doublets::Decorators;
      using StorageType = UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
      using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
      DecoratedStorageType storage{ HeapResizableDirectMemory{ } };
      action(storage);
    }

    TEST(GenericLinksTests, CrudTest)
    {
      UsingStorage<std::uint8_t>(
          [](auto &&storage) { TestCrudOperations(storage); });
      UsingStorage<std::uint16_t>(
          [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorage<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorage<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
      UsingStorage<std::uint8_t>(
          [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
      UsingStorage<std::uint16_t>(
          [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorage<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorage<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

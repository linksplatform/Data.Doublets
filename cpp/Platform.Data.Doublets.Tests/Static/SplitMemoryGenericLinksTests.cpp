namespace Platform::Data::Doublets::Tests::Static::SplitMemoryGenericLinksTests
{
    template<typename TStorage>
    static void UsingStorage(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::Split::Generic;
      TStorage storage{ HeapResizableDirectMemory{ }, HeapResizableDirectMemory{ } };
      action(storage);
    }

    template <std::integral TLinkAddress>
    static void UsingStorageWithoutExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>, HeapResizableDirectMemory>;
        UsingStorage<StorageType>(action);
    }

    template <std::integral TLinkAddress>
    static void UsingStorageWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
        UsingStorage<StorageType>(action);
    }
    
    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        UsingStorage<DecoratedStorageType>(action);
    }

    TEST(SplitMemoryGenericLinksTests, CrudTest)
    {
      UsingStorageWithoutExternalReferences<std::uint8_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint16_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint32_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint64_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
      UsingStorageWithExternalReferences<std::uint8_t>(
          [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint16_t>([](auto &&storage) {
          return TestRawNumbersCrudOperations(storage);
        });
        UsingStorageWithExternalReferences<std::uint32_t>([](auto &&storage) {
          return TestRawNumbersCrudOperations(storage);
        });
        UsingStorageWithExternalReferences<std::uint64_t>([](auto &&storage) {
          return TestRawNumbersCrudOperations(storage);
        });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

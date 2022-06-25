namespace Platform::Data::Doublets::Tests
{
    template<typename TStorage>
    static void UsingStorage(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::Split::Generic;
      TStorage storage{ HeapResizableDirectMemory{ }, HeapResizableDirectMemory{ } };
      action(storage);
    }

    template <typename TLinkAddress>
    static void UsingStorageWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>, HeapResizableDirectMemory>;
        UsingStorage<StorageType>(action);
    }

    template <typename TLinkAddress>
    static void UsingWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using StorageType = SplitMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
        UsingStorage<StorageType>(action);
    }
    
    template <typename TLinkAddress>
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
      UsingStorageWithExternalReferences<std::uint8_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
      UsingStorageWithExternalReferences<std::uint16_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint32_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint64_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
        UsingWithExternalReferences<std::uint8_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
        UsingWithExternalReferences<std::uint16_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
        UsingWithExternalReferences<std::uint32_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
        UsingWithExternalReferences<std::uint64_t>([] (auto&& storage) { return TestRawNumbersCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

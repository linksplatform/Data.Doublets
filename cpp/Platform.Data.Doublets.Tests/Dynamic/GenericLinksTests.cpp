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
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<TLinkAddress>, Doublets::ILinks<LinksOptionsType>>;
        UsingStorage<StorageType>(action);
    }

    template <typename TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::United::Generic;
      using namespace Platform::Data::Doublets::Decorators;
      using LinksOptionsType = LinksOptions<TLinkAddress>;
      using StorageType = UnitedMemoryLinks<
          LinksOptionsType, HeapResizableDirectMemory, LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<TLinkAddress>, Doublets::ILinks<LinksOptionsType>>;
      using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
      UsingStorage<DecoratedStorageType>(action);
    }

    TEST(DynamicPolymorphismGenericLinksTests, CrudTest)
    {
      UsingStorageWithExternalReferences<std::uint8_t>(
          [](auto &&storage) { TestCrudOperations(storage); });
      UsingStorageWithExternalReferences<std::uint16_t>(
          [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(DynamicPolymorphismGenericLinksTests, RawNumbersCrudTest)
    {
      UsingStorageWithExternalReferences<std::uint8_t>(
          [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
      UsingStorageWithExternalReferences<std::uint16_t>(
          [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferences<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(DynamicPolymorphismGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

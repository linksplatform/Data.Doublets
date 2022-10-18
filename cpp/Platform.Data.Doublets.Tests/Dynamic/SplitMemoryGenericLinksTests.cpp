namespace Platform::Data::Doublets::Tests::Dynamic::SplitMemoryGenericLinksTests
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
    static void UsingStorageWithExternalReference(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::Split::Generic;
      using LinksOptionsType = LinksOptions<TLinkAddress>;
      using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Doublets::ILinks<LinksOptionsType>>;
      UsingStorage<StorageType>(action);
    }

    template <typename TLinkAddress>
    static void UsingStorageWithoutExternalReferences(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::Split::Generic;
      using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
      using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Doublets::ILinks<LinksOptionsType>>;
      UsingStorage<StorageType>(action);
    }

    template <typename TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Doublets::ILinks<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        UsingStorage<DecoratedStorageType>(action)
    }

    TEST(SplitMemoryGenericLinksTests, CrudTest)
    {
      UsingStorageWithExternalReference<std::uint8_t>(
          [](auto &&storage) { return TestCrudOperations(storage); });
        UsingStorageWithExternalReference<std::uint16_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
        UsingStorageWithExternalReference<std::uint32_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
        UsingStorageWithExternalReference<std::uint64_t>(
            [](auto &&storage) { return TestCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
      UsingStorageWithoutExternalReferences<std::uint8_t>(
          [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint16_t>(
          [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint32_t>(
          [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
      UsingStorageWithoutExternalReferences<std::uint64_t>(
          [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

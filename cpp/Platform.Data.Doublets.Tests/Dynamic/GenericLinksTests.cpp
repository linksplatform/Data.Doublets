namespace Platform::Data::Doublets::Tests::Dynamic::GenericLinksTests
{
    template<typename TStorage>
    static void UsingStorage(auto&& action)
    {
        using namespace Platform::Memory;
        TStorage storage{ HeapResizableDirectMemory{ } };
        Platform::Data::ILinks<typename TStorage::LinksOptionsType>& upcastedStorage = storage;
        action(storage);
    }

    template<std::integral TLinkAddress>
    static void UsingStorageWithExternalReferencesWithSizeBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                                                LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>,
                                                LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>,
                                                UnusedLinksListMethods<LinksOptionsType>,
                                                Platform::Data::ILinks<LinksOptionsType>>;
        StorageType storage{ HeapResizableDirectMemory{ } };
        UsingStorage<StorageType>(action);
    }

    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>,
                            Platform::Data::ILinks<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        DecoratedStorageType storage { HeapResizableDirectMemory{ } };
        UsingStorage<DecoratedStorageType>(action);
    }

    template<std::integral TLinkAddress>
    static void UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                                                LinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>,
                                                LinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>,
                                                UnusedLinksListMethods<LinksOptionsType>,
                                                Platform::Data::ILinks<LinksOptionsType>>;
        StorageType storage{ HeapResizableDirectMemory{ } };
        UsingStorage<StorageType>(action);
    }

    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>,
                            Platform::Data::ILinks<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        DecoratedStorageType storage { HeapResizableDirectMemory{ } };
        UsingStorage<DecoratedStorageType>(action);
    }

    template<std::integral TLinkAddress>
    static void UsingStorageWithExternalReferencesWithAvlTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesAvlBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsAvlBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>,
                            Platform::Data::ILinks<LinksOptionsType>>;
        StorageType storage{ HeapResizableDirectMemory{ } };
        UsingStorage<StorageType>(action);
    }

    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesAvlBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsAvlBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>,
                            Platform::Data::ILinks<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        DecoratedStorageType storage { HeapResizableDirectMemory{ } };
        UsingStorage<DecoratedStorageType>(action);
    }

    TEST(DynamicGenericLinksTests, CrudTestWithSizeBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, RawNumbersCrudTestWithSizeBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithSizeBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }

    TEST(DynamicGenericLinksTests, CrudTestWithRecursionlessSizeBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, RawNumbersCrudTestWithRecursionlessSizeBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithRecursionlessSizeBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
    
    TEST(DynamicGenericLinksTests, CrudTestWithAvlTrees)
    {
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint8_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint16_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, RawNumbersCrudTestWithAvlTrees)
    {
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint8_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint16_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlTrees<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(DynamicGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithAvlBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

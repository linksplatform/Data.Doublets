namespace Platform::Data::Doublets::Tests::Static::GenericLinksTests
{

    template<typename TStorage>
    static void UsingStorage(auto&& action)
    {
        using namespace Platform::Memory;
        TStorage storage{ HeapResizableDirectMemory{ } };
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
                            UnusedLinksListMethods<LinksOptionsType>>;
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
                            UnusedLinksListMethods<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
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
                            UnusedLinksListMethods<LinksOptionsType>>;
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
                                                UnusedLinksListMethods<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        UsingStorage<DecoratedStorageType>(action);
    }

    template<std::integral TLinkAddress>
    static void UsingStorageWithExternalReferencesWithAvlBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesAvlBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsAvlBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>>;
        UsingStorage<StorageType>(action);
    }

    template <std::integral TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlBalancedTrees(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Decorators;
        using LinksOptionsType = LinksOptions<TLinkAddress>;
        using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                            LinksSourcesAvlBalancedTreeMethods<LinksOptionsType>,
                            LinksTargetsAvlBalancedTreeMethods<LinksOptionsType>,
                            UnusedLinksListMethods<LinksOptionsType>>;
        using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
        UsingStorage<DecoratedStorageType>(action);
    }

    TEST(StaticGenericLinksTests, CrudTestWithSizeBalancedTrees)
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

    TEST(StaticGenericLinksTests, RawNumbersCrudTestWithSizeBalancedTrees)
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

    TEST(StaticGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithSizeBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }

    TEST(StaticGenericLinksTests, CrudTestWithRecursionlessSizeBalancedTrees)
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

    TEST(StaticGenericLinksTests, RawNumbersCrudTestWithRecursionlessSizeBalancedTrees)
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

    TEST(StaticGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithRecursionlessSizeBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }

    TEST(StaticGenericLinksTests, CrudTestWithAvlBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestCrudOperations(storage); });
    }

    TEST(StaticGenericLinksTests, RawNumbersCrudTestWithAvlBalancedTrees)
    {
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint8_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint16_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint32_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
        UsingStorageWithExternalReferencesWithAvlBalancedTrees<std::uint64_t>(
            [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(StaticGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithAvlBalancedTrees)
    {
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
        UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithAvlBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
    }
}

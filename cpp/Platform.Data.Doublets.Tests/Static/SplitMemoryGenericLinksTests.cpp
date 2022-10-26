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
static void UsingStorageWithoutExternalReferencesWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingStorageWithExternalReferencesWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using namespace Platform::Data::Doublets::Decorators;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
  UsingStorage<DecoratedStorageType>(action);
}

TEST(SplitMemoryGenericLinksTests, CrudTestWithSizeBalancedTrees)
{
  UsingStorageWithoutExternalReferencesWithSizeBalancedTrees<std::uint8_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithSizeBalancedTrees<std::uint16_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithSizeBalancedTrees<std::uint32_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithSizeBalancedTrees<std::uint64_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
}

TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTestWithSizeBalancedTrees)
{
  UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint8_t>(
      [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
  UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint16_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
  UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint32_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
  UsingStorageWithExternalReferencesWithSizeBalancedTrees<std::uint64_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
}

TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithSizeBalancedTrees)
{
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
}

template <std::integral TLinkAddress>
static void UsingStorageWithoutExternalReferencesWithRecursionlessSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using namespace Platform::Data::Doublets::Decorators;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>>;
  using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
  UsingStorage<DecoratedStorageType>(action);
}

TEST(SplitMemoryGenericLinksTests, CrudTestWithRecursionlessSizeBalancedTrees)
{
  UsingStorageWithoutExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint8_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint16_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint32_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithoutExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint64_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
}

TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTestWithRecursionlessSizeBalancedTrees)
{
  UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint8_t>(
      [](auto &&storage) { return TestRawNumbersCrudOperations(storage); });
  UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint16_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
  UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint32_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
  UsingStorageWithExternalReferencesWithRecursionlessSizeBalancedTrees<std::uint64_t>([](auto &&storage) {
    return TestRawNumbersCrudOperations(storage);
  });
}

TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithRecursionlessSizeBalancedTrees)
{
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithRecursionlessSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
}
}

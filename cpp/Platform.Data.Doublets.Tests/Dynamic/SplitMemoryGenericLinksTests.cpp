namespace Platform::Data::Doublets::Tests::Dynamic::SplitMemoryGenericLinksTests
{

template<typename TStorage>
static void UsingStorage(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  TStorage storage{ HeapResizableDirectMemory{ }, HeapResizableDirectMemory{ } };
  Data::ILinks<typename TStorage::LinksOptionsType>& dynamicStorage = storage;
  action(dynamicStorage);
//  action(storage);
}

template <std::integral TLinkAddress>
static void UsingStorageWithoutExternalReferencesWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>,Platform::Data::ILinks<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingStorageWithExternalReferencesWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>,Platform::Data::ILinks<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using namespace Platform::Data::Doublets::Decorators;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Platform::Data::ILinks<LinksOptionsType>>;
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
}

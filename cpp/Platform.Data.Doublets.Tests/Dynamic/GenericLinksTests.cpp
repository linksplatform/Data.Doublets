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

template<std::integral TLinkAddress>
static void UsingStorageWithExternalReferencesWithAVLTrees(auto&& action)
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

TEST(GenericLinksTests, CrudTestWithSizeBalancedTrees)
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

TEST(GenericLinksTests, RawNumbersCrudTestWithSizeBalancedTrees)
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

TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTestWithSizeBalancedTrees)
{
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolutionWithSizeBalancedTrees<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
}
}

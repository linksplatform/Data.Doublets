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
static void UsingStorageWithExternalReferences(auto&& action)
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
static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
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

TEST(GenericLinksTests, CrudTest)
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

TEST(GenericLinksTests, RawNumbersCrudTest)
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

TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
{
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint8_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,16); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint16_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint32_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
  UsingDecoratedWithAutomaticUniquenessAndUsagesResolution<std::uint64_t>([] (auto&& storage) { return  TestMultipleRandomCreationsAndDeletions(storage,100); });
}
}

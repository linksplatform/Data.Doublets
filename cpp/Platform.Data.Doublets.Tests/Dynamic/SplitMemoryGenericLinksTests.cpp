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
static void UsingStorageWithoutExternalReferences(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Data::ILinks<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingStorageWithExternalReferences(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, Data::ILinks<LinksOptionsType>>;
  UsingStorage<StorageType>(action);
}

template <std::integral TLinkAddress>
static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using namespace Platform::Data::Doublets::Decorators;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{false}>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, InternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, InternalLinksSourcesLinkedListMethods<LinksOptionsType>, InternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, ExternalLinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<LinksOptionsType>, ILinks<LinksOptionsType>>;
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

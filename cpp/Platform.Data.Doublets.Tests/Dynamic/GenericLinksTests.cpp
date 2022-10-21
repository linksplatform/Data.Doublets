namespace Platform::Data::Doublets::Tests::Dynamic::GenericLinksTests
{
template<std::integral TLinkAddress>
static void UsingStorage(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::United::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress>;
  using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory,
                                        LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>,
                                        LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>,
                                        UnusedLinksListMethods<LinksOptionsType>,
                                        ILinks<LinksOptionsType>>;
  StorageType storage{ HeapResizableDirectMemory{ } };
  ILinks<LinksOptionsType>& upcastedStorage = storage;
  action(upcastedStorage);
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
                                        ILinks<LinksOptions<TLinkAddress>>>;
  using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
  DecoratedStorageType storage { HeapResizableDirectMemory{ } };
  ILinks<LinksOptions<TLinkAddress>>& upcastedStorage = storage;
  action(upcastedStorage);
}

TEST(GenericLinksTests, CrudTest)
{
  UsingStorage<std::uint8_t>(
      [](auto &&storage) { TestCrudOperations(storage); });
  UsingStorage<std::uint16_t>(
      [](auto &&storage) { TestCrudOperations(storage); });
  UsingStorage<std::uint32_t>(
      [](auto &&storage) { TestCrudOperations(storage); });
  UsingStorage<std::uint64_t>(
      [](auto &&storage) { TestCrudOperations(storage); });
}

TEST(GenericLinksTests, RawNumbersCrudTest)
{
  UsingStorage<std::uint8_t>(
      [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
  UsingStorage<std::uint16_t>(
      [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
  UsingStorage<std::uint32_t>(
      [](auto &&storage) { TestRawNumbersCrudOperations(storage); });
  UsingStorage<std::uint64_t>(
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

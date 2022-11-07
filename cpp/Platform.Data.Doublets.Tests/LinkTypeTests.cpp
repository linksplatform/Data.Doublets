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

template <std::integral TLinkAddress>
static void UsingStorageWithSpanLinkType(auto&& action)
{
  using namespace Platform::Memory;
  using namespace Platform::Data::Doublets::Memory::Split::Generic;
  using LinksOptionsType = LinksOptions<TLinkAddress, LinksConstants<TLinkAddress>{true}, std::span<TLinkAddress>>;
  using StorageType = SplitMemoryLinks<LinksOptionsType, HeapResizableDirectMemory>;
  UsingStorage<StorageType>(action);
}

TEST(SplitMemoryGenericLinksTests, CrudTestWithSizeBalancedTrees)
{
  UsingStorageWithSpanLinkType<std::uint8_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithSpanLinkType<std::uint16_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithSpanLinkType<std::uint32_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
  UsingStorageWithSpanLinkType<std::uint64_t>(
      [](auto &&storage) { return TestCrudOperations(storage); });
}
}

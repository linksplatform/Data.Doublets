namespace Platform::Data::Doublets::Tests::Dynamic::ILinksTests
{
  template<typename TLinkAddress>
  static void FunctionWithILinksParameter(Doublets::ILinks<TLinkAddress>)
  {

  }

  TEST(ILinksTests, CastToILinksTest)
  {
    using namespace Platform::Memory;
    using namespace Platform::Data::Doublets::Memory::United::Generic;
    using namespace Platform::Data::Doublets::Memory::United;
    using LinksOptionsType = LinksOptions<TLinkAddress>;
    using StorageType = UnitedMemoryLinks<LinksOptionsType, HeapResizableDirectMemory, LinksSourcesSizeBalancedTreeMethods<LinksOptionsType>, LinksTargetsSizeBalancedTreeMethods<LinksOptionsType>, UnusedLinksListMethods<TLinkAddress>, Doublets::ILinks<LinksOptionsType>>;
    UsingStorage<StorageType>(action);
    StorageType storage { HeapResizableDirectMemory{}, HeapResizableDirectMemory {}};
    FunctionWithILinksParameter(storage);
  }
}

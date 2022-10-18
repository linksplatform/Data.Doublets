namespace Platform::Data::Doublets::Tests::Static::GenericLinksTests
{
    template<typename TStorage>
    static void UsingStorage(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::Split::Generic;
      TStorage storage{ HeapResizableDirectMemory{ }, HeapResizableDirectMemory{ } };
      action(storage);
    }

   template <typename TLinkAddress>
    static void UsingStorageWithExternalReferences(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using StorageType = UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
        UsingStorage<StorageType>(action);
    }

    template <typename TLinkAddress>
    static void UsingDecoratedWithAutomaticUniquenessAndUsagesResolution(auto&& action)
    {
      using namespace Platform::Memory;
      using namespace Platform::Data::Doublets::Memory::United::Generic;
      using namespace Platform::Data::Doublets::Decorators;
      using StorageType = UnitedMemoryLinks<LinksOptions<TLinkAddress>, HeapResizableDirectMemory>;
      using DecoratedStorageType = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<StorageType>;
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

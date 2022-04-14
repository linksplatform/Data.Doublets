namespace Platform::Data::Doublets::Tests
{
    TEST(GenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestCrudOperations(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestCrudOperations(storage); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestRawNumbersCrudOperations(storage); });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
        {
            using namespace Platform::Memory;
            using namespace Platform::Data::Doublets::Memory::Split::Generic;
            using namespace Platform::Collections;
            LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint8_t>, HeapResizableDirectMemory>> UInt8TDecoratedStorage{HeapResizableDirectMemory{}};
            TestMultipleRandomCreationsAndDeletions(UInt8TDecoratedStorage, 16);
            LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint16_t>, HeapResizableDirectMemory>> UInt16TDecoratedStorage{HeapResizableDirectMemory{}};
            TestMultipleRandomCreationsAndDeletions(UInt16TDecoratedStorage, 100);
            LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint32_t>, HeapResizableDirectMemory>> UInt32TDecoratedStorage{HeapResizableDirectMemory{}};
            TestMultipleRandomCreationsAndDeletions(UInt32TDecoratedStorage, 100);
            LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<std::uint64_t>, HeapResizableDirectMemory>> UInt64TDecoratedStorage{HeapResizableDirectMemory{}};
            TestMultipleRandomCreationsAndDeletions(UInt64TDecoratedStorage, 100);
        }

    template <typename TLinkAddress>
    static void Using(auto&& action)
        {
            static constexpr constants {LinksConstants<TLinkAddress>{false}}
            using LinksOptionsType = LinksOptions<TLinkAddress, constants>
            HeapResizableDirectMemory memory{};
            HeapResizableDirectMemory indexMemory = {};
            using TStorage = SplitMemoryLinks<LinksOptionsType>;
            TStorage storage {memory, indexMemory, TStorage::DefaultLinksSizeStep}
            action(storage);
        }

    template <typename TLinkAddress>
    static void UsingWithExternalReferences(auto&& action)
        {
            static constexpr constants {LinksConstants<TLinkAddress>{true}};
            using LinksOptionsType = LinksOptions<TLinkAddress, constants>;
            using TStorage = SplitMemoryLinks<LinksOptionsType>;
            HeapResizableDirectMemory memory{};
            HeapResizableDirectMemory indexMemory = {};
            TStorage storage {memory, indexMemory, TStorage::DefaultLinksSizeStep}
            action(storage);
        }
}

namespace Platform::Data::Doublets::Tests
{
//   template <typename TLinkAddress>
//    static void Using(auto&& action)
//    {
//        using namespace Platform::Memory;
//        using namespace Platform::Data::Doublets::Memory::United::Generic;
//        using namespace Platform::Data::Doublets::Memory::United;
//        using namespace Platform::Collections;
//        constexpr LinksConstants<TLinkAddress> constants {true};
//        HeapResizableDirectMemory memory {};
//        UnitedMemoryLinks<TLinkAddress, HeapResizableDirectMemory, constants> ffiStorage {memory};
//        action(ffiStorage);
//    }

//    template <typename TLinkAddress>
//    static void UsingMultipleRandomCreationsAndDeletionsTest(auto&& action)
//    {
//        using namespace Platform::Memory;
//        using namespace Platform::Data::Doublets::Memory::United::Generic;
//        using namespace Platform::Data::Doublets::Memory::United;
//        using namespace Platform::Collections;
//        constexpr LinksConstants<TLinkAddress> constants {true};
//        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<TLinkAddress, HeapResizableDirectMemory, constants>> decoratedStorage {HeapResizableDirectMemory{}};
//        action(decoratedStorage);
//    }

//    TEST(GenericLinksTests, CrudTest)
//    {
//        Using<std::uint8_t>([] (auto&& storage) { TestCrudOperations<std::uint8_t>(storage); });
//        Using<std::uint16_t>([] (auto&& storage) { TestCrudOperations<std::uint16_t>(storage); });
//        Using<std::uint32_t>([] (auto&& storage) { TestCrudOperations<std::uint32_t>(storage); });
//        Using<std::uint64_t>([] (auto&& storage) { TestCrudOperations<std::uint64_t>(storage); });
//    }
//
//    TEST(GenericLinksTests, RawNumbersCrudTest)
//    {
//        Using<std::uint8_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint8_t>(storage); });
//        Using<std::uint16_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint16_t>(storage); });
//        Using<std::uint32_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint32_t>(storage); });
//        Using<std::uint64_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint64_t>(storage); });
//    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Collections;
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<LinksOptions<>, HeapResizableDirectMemory>> decoratedStorage {HeapResizableDirectMemory{}};
//        TestMultipleRandomCreationsAndDeletions(decoratedStorage, 16);
//        UsingMultipleRandomCreationsAndDeletionsTest<std::uint8_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint8_t>(decoratedStorage, 16);
//        });
//        UsingMultipleRandomCreationsAndDeletionsTest<std::uint16_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint16_t>(decoratedStorage, 100);
//        });
//        UsingMultipleRandomCreationsAndDeletionsTest<std::uint32_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint32_t>(decoratedStorage, 100);
//        });
//        UsingMultipleRandomCreationsAndDeletionsTest<std::uint64_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint64_t>(decoratedStorage, 100);
//        });
    }
}

namespace Platform::Data::Doublets::Tests
{
            using namespace Platform::Memory;
            using namespace Platform::Data::Doublets::Memory::United::Generic;
            using namespace Platform::Data::Doublets::Memory::United;
            using namespace Platform::Collections;
   /* template <typename TLinkAddress>
    static void Using(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using namespace Platform::Collections;
        std::string tempFilePath { std::tmpnam(nullptr) };
        Expects(!Collections::IsWhiteSpace(tempFilePath));
        try
        {
            constexpr LinksConstants<TLinkAddress> constants {true};
//            Ffi::UnitedMemoryLinks<TLinkAddress, constants> ffiStorage {tempFilePath};
            HeapResizableDirectMemory memory {};
            UnitedMemoryLinks<TLinkAddress, HeapResizableDirectMemory, constants> ffiStorage {memory};
            action(ffiStorage);
        }
        catch (...)
        {
            std::remove(tempFilePath.c_str());
            throw;
        }
        std::remove(tempFilePath.c_str());
    }
    TEST(GenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestCrudOperations<std::uint8_t>(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestCrudOperations<std::uint16_t>(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestCrudOperations<std::uint32_t>(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestCrudOperations<std::uint64_t>(storage); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint8_t>(storage); });
        Using<std::uint16_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint16_t>(storage); });
        Using<std::uint32_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint32_t>(storage); });
        Using<std::uint64_t>([] (auto&& storage) { TestRawNumbersCrudOperations<std::uint64_t>(storage); });
    }
*/
    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<UnitedMemoryLinks<TLinkAddress, HeapResizableDirectMemory, constants>> storage {HeapResizableDirectMemory{}};
//        UnitedMemoryLinks<std::uint64_t, HeapResizableDirectMemory, LinksConstants<std::uint64_t>{}> storage {memory};
        TestMultipleRandomCreationsAndDeletions<std::uint64_t>(storage, 16);
//        Using<std::uint16_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint16_t>(decoratedStorage, 100);
//        });
//        Using<std::uint32_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint32_t>(decoratedStorage, 100);
//        });
//        Using<std::uint64_t>([] (auto&& storage){
//            auto decoratedStorage = LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<decltype(storage)>(storage);
//            TestMultipleRandomCreationsAndDeletions<std::uint64_t>(decoratedStorage, 100);
//        });
//         Ffi
//        Using<std::uint8_t>([] (auto&& storage){
//            TestMultipleRandomCreationsAndDeletions<std::uint8_t>(storage, 16);
//        });
//        Using<std::uint16_t>([] (auto&& storage){
//            TestMultipleRandomCreationsAndDeletions<std::uint16_t>(storage, 100);
//        });
//        Using<std::uint32_t>([] (auto&& storage){
//            TestMultipleRandomCreationsAndDeletions<std::uint32_t>(storage, 100);
//        });
//        Using<std::uint64_t>([] (auto&& storage){
//            TestMultipleRandomCreationsAndDeletions<std::uint64_t>(storage, 100);
//        });
    }
}

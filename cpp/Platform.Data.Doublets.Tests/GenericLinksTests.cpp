namespace Platform::Data::Doublets::Tests
{
    template <typename TLink, typename Action>
    static void Using(Action action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using namespace Platform::Collections;
        HeapResizableDirectMemory memory {};
        UnitedMemoryLinks<TLink, HeapResizableDirectMemory> storage { memory };
        action(storage);
        char fileName[L_tmpnam] {};
        char* fileNamePtr { fileName };
        std::tmpnam(fileNamePtr);
        Expects(NULL != fileName);
        std::cout << "\n\nFilename: " << fileName << std::endl;
        Ffi::UnitedMemoryLinks<TLink> ffiStorage { fileName };
        std::remove(fileName);
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

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (auto&& storage){
        auto decoratedStorage = DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
        TestMultipleRandomCreationsAndDeletions<std::uint8_t>(decoratedStorage, 16);
        });
        Using<std::uint16_t>([] (auto&& storage){
            auto decoratedStorage = DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
            TestMultipleRandomCreationsAndDeletions<std::uint16_t>(decoratedStorage, 100);
        });
        Using<std::uint32_t>([] (auto&& storage){
            auto decoratedStorage = DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
            TestMultipleRandomCreationsAndDeletions<std::uint32_t>(decoratedStorage, 100);
        });
        Using<std::uint64_t>([] (auto&& storage){
            auto decoratedStorage = DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
            TestMultipleRandomCreationsAndDeletions<std::uint64_t>(decoratedStorage, 100);
        });
    }
}

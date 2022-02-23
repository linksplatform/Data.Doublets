namespace Platform::Data::Doublets::Tests
{
    template <typename TLink, typename Action>
    static void Using(Action action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        HeapResizableDirectMemory memory {};
        UnitedMemoryLinks<TLink, HeapResizableDirectMemory> storage { memory };
        action(storage);
    }
    TEST(GenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { ILinksTestExtensions::TestCrudOperations<std::uint8_t>(storage); });
        Using<std::uint16_t>([] (auto&& storage) { ILinksTestExtensions::TestCrudOperations<std::uint16_t>(storage); });
        Using<std::uint32_t>([] (auto&& storage) { ILinksTestExtensions::TestCrudOperations<std::uint32_t>(storage); });
        Using<std::uint64_t>([] (auto&& storage) { ILinksTestExtensions::TestCrudOperations<std::uint64_t>(storage); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& storage) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint8_t>(storage); });
        Using<std::uint16_t>([] (auto&& storage) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint16_t>(storage); });
        Using<std::uint32_t>([] (auto&& storage) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint32_t>(storage); });
        Using<std::uint64_t>([] (auto&& storage) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint64_t>(storage); });
    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (auto&& storage){
        auto decoratedStorage = ILinksExtensions::DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
        ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint8_t>(decoratedStorage, 16);
        });
        Using<std::uint16_t>([] (auto&& storage){
            auto decoratedStorage = ILinksExtensions::DecorateWithAutomaticUniquenessAndUsagesResolution(storage);
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint16_t>(decoratedStorage, 100);
        });
        Using<std::uint32_t>([] (auto&& storage){
            auto decoratedStorage = storage.DecorateWithAutomaticUniquenessAndUsagesResolution();
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint32_t>(decoratedStorage, 100);
        });
        Using<std::uint64_t>([] (auto&& storage){
            auto decoratedStorage = storage.DecorateWithAutomaticUniquenessAndUsagesResolution();
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint64_t>(decoratedStorage, 100);
        });
    }
}

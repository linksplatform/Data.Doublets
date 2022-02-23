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
        Using<std::uint8_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations<std::uint8_t>(links); });
        Using<std::uint16_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations<std::uint16_t>(links); });
        Using<std::uint32_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations<std::uint32_t>(links); });
        Using<std::uint64_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations<std::uint64_t>(links); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint8_t>(links); });
        Using<std::uint16_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint16_t>(links); });
        Using<std::uint32_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint32_t>(links); });
        Using<std::uint64_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations<std::uint64_t>(links); });
    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (auto&& links){
        auto decoratedStorage = ILinksExtensions::DecorateWithAutomaticUniquenessAndUsagesResolution(links);
        ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint8_t>(decoratedStorage, 16);
        });
        Using<std::uint16_t>([] (auto&& links){
            auto decoratedStorage = ILinksExtensions::DecorateWithAutomaticUniquenessAndUsagesResolution(links);
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint16_t>(decoratedStorage, 100);
        });
        Using<std::uint32_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint32_t>(decoratedStorage, 100);
        });
        Using<std::uint64_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            ILinksTestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint64_t>(decoratedStorage, 100);
        });
    }
}

﻿#include "ILinksTestExtensions.h"

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
        Using<std::uint8_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations(links); });
        Using<std::uint16_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations(links); });
        Using<std::uint32_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations(links); });
        Using<std::uint64_t>([] (auto&& links) { ILinksTestExtensions::TestCrudOperations(links); });
    }

    TEST(GenericLinksTests, RawNumbersCrudTest)
    {
        Using<std::uint8_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations(links); });
        Using<std::uint16_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations(links); });
        Using<std::uint32_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations(links); });
        Using<std::uint64_t>([] (auto&& links) { ILinksTestExtensions::TestRawNumbersCrudOperations(links); });
    }

    TEST(GenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            TextExtensions::TestMultipleRandomCreationsAndDeletions(decoratedStorage, 16);
        });
        Using<std::uint16_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            TextExtensions::TestMultipleRandomCreationsAndDeletions(decoratedStorage, 100);
        });
        Using<std::uint32_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            TextExtensions::TestMultipleRandomCreationsAndDeletions(decoratedStorage, 100);
        });
        Using<std::uint64_t>([] (auto&& links){
            auto decoratedStorage = links.DecorateWithAutomaticUniquenessAndUsagesResolution();
            TextExtensions::TestMultipleRandomCreationsAndDeletions(decoratedStorage, 100);
        });
    }
}

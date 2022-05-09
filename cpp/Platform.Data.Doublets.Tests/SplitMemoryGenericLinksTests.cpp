namespace Platform::Data::Doublets::Tests
{
    template <typename TLinkAddress>
    static void Using(auto&& action)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<LinksOptions<>, HeapResizableDirectMemory> memory { dataMemory, indexMemory };
        action(memory);
    }

    TEST(SplitMemoryGenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations(); });
        Using<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations(); });
        Using<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations(); });
        Using<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations(); });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
        UsingWithExternalReferences<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations(); });
        UsingWithExternalReferences<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations(); });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(16));
        Using<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution(); }.TestMultipleRandomCreationsAndDeletions(100));
    }
}

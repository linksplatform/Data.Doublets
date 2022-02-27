namespace Platform::Data::Doublets::Tests
{
    TEST(SplitMemoryGenericLinksTests, CrudTest)
    {
        Using<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations() });
        Using<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations() });
        Using<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations() });
        Using<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.TestCrudOperations() });
    }

    TEST(SplitMemoryGenericLinksTests, RawNumbersCrudTest)
    {
        UsingWithExternalReferences<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations() });
        UsingWithExternalReferences<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations() });
        UsingWithExternalReferences<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations() });
        UsingWithExternalReferences<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.TestRawNumbersCrudOperations() });
    }

    TEST(SplitMemoryGenericLinksTests, MultipleRandomCreationsAndDeletionsTest)
    {
        Using<std::uint8_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution() }.TestMultipleRandomCreationsAndDeletions(16));
        Using<std::uint16_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution() }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint32_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution() }.TestMultipleRandomCreationsAndDeletions(100));
        Using<std::uint64_t>([] (Interfaces::CArray auto storage) { return storage.DecorateWithAutomaticUniquenessAndUsagesResolution() }.TestMultipleRandomCreationsAndDeletions(100));
    }

    private: template <typename TLinkAddress>
    static void Using(std::invocable auto&& action)
    {
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        SplitMemoryLinks<TLinkAddress> memory { dataMemory, indexMemory };
        action(memory);
    }

    private: template <typename TLinkAddress>
    static void Using(std::invocable auto&& action)
    {
        HeapResizableDirectMemory dataMemory { };
        HeapResizableDirectMemory indexMemory { };
        LinksConstants<TLinkAddress> constants { true };
        SplitMemoryLinks<TLinkAddress> memory { dataMemory, indexMemory, SplitMemoryLinks<TLinkAddress>::DefaultIndexSize, constants };
        action(memory);
    }
}

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(SplitMemoryGenericLinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using<std::uint8_t>(storage => storage.TestCRUDOperations());
            Using<std::uint16_t>(storage => storage.TestCRUDOperations());
            Using<std::uint32_t>(storage => storage.TestCRUDOperations());
            Using<std::uint64_t>(storage => storage.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            UsingWithExternalReferences<std::uint8_t>(storage => storage.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint16_t>(storage => storage.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint32_t>(storage => storage.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint64_t>(storage => storage.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using<std::uint8_t>(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(16));
            Using<std::uint16_t>(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<std::uint32_t>(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<std::uint64_t>(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }

        private: template <typename TLinkAddress> static void Using(Action<ILinks<TLinkAddress>> action)
        {
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = SplitMemoryLinks<TLinkAddress>(dataMemory, indexMemory))
            {
                action(memory);
            }
        }

        private: template <typename TLinkAddress> static void UsingWithExternalReferences(Action<ILinks<TLinkAddress>> action)
        {
            auto contants = LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = SplitMemoryLinks<TLinkAddress>(dataMemory, indexMemory, SplitMemoryLinks<TLinkAddress>.DefaultLinksSizeStep, contants))
            {
                action(memory);
            }
        }
    };
}

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(SplitMemoryGenericLinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using<std::uint8_t>(links => links.TestCRUDOperations());
            Using<std::uint16_t>(links => links.TestCRUDOperations());
            Using<std::uint32_t>(links => links.TestCRUDOperations());
            Using<std::uint64_t>(links => links.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            UsingWithExternalReferences<std::uint8_t>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint16_t>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint32_t>(links => links.TestRawNumbersCRUDOperations());
            UsingWithExternalReferences<std::uint64_t>(links => links.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using<std::uint8_t>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(16));
            Using<std::uint16_t>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<std::uint32_t>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<std::uint64_t>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }

        private: template <typename TLink> static void Using(Action<ILinks<TLink>> action)
        {
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = SplitMemoryLinks<TLink>(dataMemory, indexMemory))
            {
                action(memory);
            }
        }

        private: template <typename TLink> static void UsingWithExternalReferences(Action<ILinks<TLink>> action)
        {
            auto contants = LinksConstants<TLink>(enableExternalReferencesSupport: true);
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = SplitMemoryLinks<TLink>(dataMemory, indexMemory, SplitMemoryLinks<TLink>.DefaultLinksSizeStep, contants))
            {
                action(memory);
            }
        }
    };
}

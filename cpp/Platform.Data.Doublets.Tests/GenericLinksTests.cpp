namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(GenericLinksTests)
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
            Using<std::uint8_t>(links => links.TestRawNumbersCRUDOperations());
            Using<std::uint16_t>(links => links.TestRawNumbersCRUDOperations());
            Using<std::uint32_t>(links => links.TestRawNumbersCRUDOperations());
            Using<std::uint64_t>(links => links.TestRawNumbersCRUDOperations());
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
            using (auto scope = Scope<Types<HeapResizableDirectMemory, UnitedMemoryLinks<TLink>>>())
            {
                action(scope.Use<ILinks<TLink>>());
            }
        }
    };
}

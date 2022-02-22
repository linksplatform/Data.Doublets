namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(GenericLinksTests)
    {
        public: TEST_METHOD(CrudTest)
        {
            Using<std::uint8_t>(links => links.TestCrudOperations());
            Using<std::uint16_t>(links => links.TestCrudOperations());
            Using<std::uint32_t>(links => links.TestCrudOperations());
            Using<std::uint64_t>(links => links.TestCrudOperations());
        }

        public: TEST_METHOD(RawNumbersCrudTest)
        {
            Using<std::uint8_t>(links => links.TestRawNumbersCrudOperations());
            Using<std::uint16_t>(links => links.TestRawNumbersCrudOperations());
            Using<std::uint32_t>(links => links.TestRawNumbersCrudOperations());
            Using<std::uint64_t>(links => links.TestRawNumbersCrudOperations());
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

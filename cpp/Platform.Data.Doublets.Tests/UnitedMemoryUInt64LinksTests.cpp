

using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(UnitedMemoryUInt64LinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using(links => links.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            Using(links => links.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }

        private: static void Using(Action<ILinks<TLink>> action)
        {
            using (auto scope = Scope<Types<HeapResizableDirectMemory, UInt64UnitedMemoryLinks>>())
            {
                action(scope.Use<ILinks<TLink>>());
            }
        }
    };
}



using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(UnitedMemoryUInt64LinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using(storage => storage.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            Using(storage => storage.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }

        private: static void Using(Action<ILinks<TLinkAddress>> action)
        {
            using (auto scope = Scope<Types<HeapResizableDirectMemory, UInt64UnitedMemoryLinks>>())
            {
                action(scope.Use<ILinks<TLinkAddress>>());
            }
        }
    };
}

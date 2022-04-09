

using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(UnitedMemoryUInt32LinksTests)
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
            using (auto scope = Scope<Types<HeapResizableDirectMemory, UInt32UnitedMemoryLinks>>())
            {
                action(scope.Use<ILinks<TLinkAddress>>());
            }
        }
    };
}

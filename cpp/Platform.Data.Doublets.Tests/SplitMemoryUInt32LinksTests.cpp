

using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(SplitMemoryUInt32LinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using(storage => storage.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            UsingWithExternalReferences(storage => storage.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using(storage => storage.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(500));
        }

        private: static void Using(Action<ILinks<TLinkAddress>> action)
        {
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = UInt32SplitMemoryLinks(dataMemory, indexMemory))
            {
                action(memory);
            }
        }

        private: static void UsingWithExternalReferences(Action<ILinks<TLinkAddress>> action)
        {
            auto contants = LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = UInt32SplitMemoryLinks(dataMemory, indexMemory, UInt32SplitMemoryLinks.DefaultLinksSizeStep, contants))
            {
                action(memory);
            }
        }
    };
}

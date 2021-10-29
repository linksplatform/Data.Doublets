

using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Tests
{
    public unsafe TEST_CLASS(SplitMemoryUInt64LinksTests)
    {
        public: TEST_METHOD(CRUDTest)
        {
            Using(links => links.TestCRUDOperations());
        }

        public: TEST_METHOD(RawNumbersCRUDTest)
        {
            UsingWithExternalReferences(links => links.TestRawNumbersCRUDOperations());
        }

        public: TEST_METHOD(MultipleRandomCreationsAndDeletionsTest)
        {
            Using(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(500));
        }

        private: static void Using(Action<ILinks<TLink>> action)
        {
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = UInt64SplitMemoryLinks(dataMemory, indexMemory))
            {
                action(memory);
            }
        }

        private: static void UsingWithExternalReferences(Action<ILinks<TLink>> action)
        {
            auto contants = LinksConstants<TLink>(enableExternalReferencesSupport: true);
            using (auto dataMemory = HeapResizableDirectMemory())
            using (auto indexMemory = HeapResizableDirectMemory())
            using (auto memory = UInt64SplitMemoryLinks(dataMemory, indexMemory, UInt64SplitMemoryLinks.DefaultLinksSizeStep, contants))
            {
                action(memory);
            }
        }
    };
}

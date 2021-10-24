namespace Platform::Data::Doublets::Tests
{
    TEST_CLASS(ResizableDirectMemoryLinksTests)
    {
        private: static readonly LinksConstants<std::uint64_t> _constants = Default<LinksConstants<std::uint64_t>>.Instance;

        public: TEST_METHOD(BasicFileMappedMemoryTest)
        {
            auto tempFilename = Path.GetTempFileName();
            using (auto memoryAdapter = UInt64UnitedMemoryLinks(tempFilename))
            {
                TestBasicMemoryOperations(memoryAdapter, );
            }
            File.Delete(tempFilename);
        }

        public: TEST_METHOD(BasicHeapMemoryTest)
        {
            using (auto memory = HeapResizableDirectMemory(UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (auto memoryAdapter = UInt64UnitedMemoryLinks(memory, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            {
                TestBasicMemoryOperations(memoryAdapter, );
            }
        }

        private: static void TestBasicMemoryOperations(ILinks<std::uint64_t> &memoryAdapter)
        {
            auto link = memoryAdapter.Create();
            memoryAdapter.Delete(link);
        }

        public: TEST_METHOD(NonexistentReferencesHeapMemoryTest)
        {
            using (auto memory = HeapResizableDirectMemory(UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (auto memoryAdapter = UInt64UnitedMemoryLinks(memory, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            {
                TestNonexistentReferences(memoryAdapter, );
            }
        }

        private: static void TestNonexistentReferences(ILinks<std::uint64_t> &memoryAdapter)
        {
            auto link = memoryAdapter.Create();
            memoryAdapter.Update(link, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max());
            auto resultLink = _constants.Null;
            memoryAdapter.Each(foundLink =>
            {
                resultLink = foundLink[_constants.IndexPart];
                return _constants.Break;
            }, _constants.Any, std::numeric_limits<std::uint64_t>::max(), std::numeric_limits<std::uint64_t>::max());
            Assert::IsTrue(resultLink == link);
            Assert::IsTrue(memoryAdapter.Count()(std::numeric_limits<std::uint64_t>::max()) == 0);
            memoryAdapter.Delete(link);
        }
    };
}

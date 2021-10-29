namespace Platform::Data::Doublets::Benchmarks
{
    class MemoryBenchmarks
    {
        private: static SplitMemoryLinks<std::uint32_t> _splitMemory;
        private: static ILinks<std::uint32_t> *_splitMemoryLinks;
        private: static UnitedMemoryLinks<std::uint32_t> _unitedMemory;
        private: static ILinks<std::uint32_t> *_unitedMemoryLinks;

        public: static void Setup()
        {
            auto dataMemory = HeapResizableDirectMemory();
            auto indexMemory = HeapResizableDirectMemory();
            _splitMemory = SplitMemoryLinks<std::uint32_t>(dataMemory, indexMemory);
            _splitMemoryLinks = _splitMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();

            auto memory = HeapResizableDirectMemory();
            _unitedMemory = UnitedMemoryLinks<std::uint32_t>(memory);
            _unitedMemoryLinks = _unitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        public: static void Cleanup()
        {
            _splitMemory.Dispose();
            _unitedMemory.Dispose();
        }

        public: void Split()
        {
            _TestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint32_t>(splitMemoryLinks, 1000);
        }

        public: void United()
        {
            _TestExtensions::TestMultipleRandomCreationsAndDeletions<std::uint32_t>(unitedMemoryLinks, 1000);
        }
    };
}
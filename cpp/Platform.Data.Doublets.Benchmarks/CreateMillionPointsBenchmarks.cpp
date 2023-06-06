namespace Platform::Data::Doublets::Benchmarks
{
    using TLinkAddress = std::uint64_t;

    // static void BM_CreateMillionPointsFfi(benchmark::State& state)
    // {
    //     using namespace Platform::Memory;
    //     using namespace Platform::Data::Doublets::Ffi;
    //     for (auto _ : state)
    //     {
    //         std::string memoryFilePath{ std::tmpnam(nullptr) };
    //         Expects(!Collections::IsWhiteSpace(memoryFilePath));
    //         try
    //         {
    //             Links<LinksOptions<TLinkAddress>> ffiStorage{memoryFilePath};
    //             for (std::size_t i = 0; i < state.range(0); ++i)
    //             {
    //                 CreatePoint(ffiStorage);
    //             }
    //         }
    //         catch (...)
    //         {
    //             std::remove(memoryFilePath.c_str());
    //             throw;
    //         }
    //         std::remove(memoryFilePath.c_str());
    //     }
    // }

    static void BM_CreateMillionPointsUnitedMemory(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        for (auto _ : state)
        {
            std::string memoryFilePath{ std::tmpnam(nullptr) };
            Expects(!Collections::IsWhiteSpace(memoryFilePath));
            try
            {
                UnitedMemoryLinks<LinksOptions<TLinkAddress>, FileMappedResizableDirectMemory> storage{FileMappedResizableDirectMemory{memoryFilePath}};
                for (std::size_t i = 0; i < state.range(0); ++i)
                {
                    CreatePoint(storage);
                }
            }
            catch (...)
            {
                std::remove(memoryFilePath.c_str());
                throw;
            }
            std::remove(memoryFilePath.c_str());
        }
    }

    static void BM_CreateMillionPointsSplitMemory(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::Split::Generic;
        for (auto _ : state)
        {
            std::string dataMemoryFilePath{ std::tmpnam(nullptr) };
            std::string indexMemoryFilePath{ std::tmpnam(nullptr) };
            Expects(!Collections::IsWhiteSpace(dataMemoryFilePath));
            try
            {
                SplitMemoryLinks<LinksOptions<TLinkAddress>, FileMappedResizableDirectMemory> storage{FileMappedResizableDirectMemory{dataMemoryFilePath}, FileMappedResizableDirectMemory{indexMemoryFilePath}};
                for (std::size_t i = 0; i < state.range(0); ++i)
                {
                    CreatePoint(storage);
                }
            }
            catch (...)
            {
                std::remove(dataMemoryFilePath.c_str());
                throw;
            }
            std::remove(dataMemoryFilePath.c_str());
        }
    }

    //BENCHMARK(BM_CreateMillionPointsFfi)->Arg(1000000);
    BENCHMARK(BM_CreateMillionPointsUnitedMemory)->Arg(1000000);
    BENCHMARK(BM_CreateMillionPointsSplitMemory)->Arg(1000000);
}

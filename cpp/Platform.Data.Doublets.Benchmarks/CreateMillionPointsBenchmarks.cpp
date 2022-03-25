namespace Platform::Data::Doublets::Benchmarks
{
    using TLinkAddress = std::uint64_t;
    static void BM_CreateMillionPointsFfi(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United;
        std::string tempFilePath { std::tmpnam(nullptr) };
        Expects(!Collections::IsWhiteSpace(tempFilePath));
        try
        {
            Ffi::UnitedMemoryLinks<LinksOptions<>> ffiStorage {tempFilePath};
            for (auto _ : state)
            {
                for (std::size_t i = 0; i < state.range(0); ++i)
                {
                    CreatePoint(ffiStorage);
                }
            }
        }
        catch (...)
        {
            std::remove(tempFilePath.c_str());
            throw;
        }
        std::remove(tempFilePath.c_str());
    }

    using TLinkAddress = std::uint64_t;
    static void BM_CreateMillionPoints(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        UnitedMemoryLinks<LinksOptions<>, HeapResizableDirectMemory> storage {HeapResizableDirectMemory{}};
        for (auto _ : state)
        {
            for (std::size_t i = 0; i < state.range(0); ++i)
            {
                CreatePoint(storage);
            }
        }
    }

    BENCHMARK(BM_CreateMillionPointsFfi)->Arg(1000000);
    BENCHMARK(BM_CreateMillionPoints)->Arg(1000000);
}

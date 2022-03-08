namespace Platform::Data::Doublets::Benchmarks
{
    using TLinkAddress = std::uint64_t;
    static void BM_CreateMillionPoints(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        using namespace Platform::Data::Doublets::Memory::United;
        using namespace Platform::Collections;
        std::string tempFilePath { std::tmpnam(nullptr) };
        Expects(!Collections::IsWhiteSpace(tempFilePath));
        try
        {
            constexpr LinksConstants<TLinkAddress> constants {true};
            Ffi::UnitedMemoryLinks<TLinkAddress, constants> ffiStorage {tempFilePath};
            for (auto _ : state)
            {
                for (std::size_t i = 0; i < state.range(0); ++i)
                {
                    CreatePoint<TLinkAddress>(ffiStorage);
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

    BENCHMARK(BM_CreateMillionPoints)->Arg(1000000);
}

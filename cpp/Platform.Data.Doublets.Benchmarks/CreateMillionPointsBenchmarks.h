#include "../Tools/TemporaryDirectory.h"
namespace Platform::Data::Doublets::Benchmarks
{
    using TLinkAddress = std::uint64_t;

    static void BM_CreateMillionPointsUnitedMemory(benchmark::State& state)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        for ([[maybe_unused]] auto _ : state)
        {
            Tools::TemporaryDirectory temporary;
            std::string memoryFilePath = temporary.File("data.links");
            Expects(!Collections::IsWhiteSpace(memoryFilePath));
            try
            {
                UnitedMemoryLinks<LinksOptions<TLinkAddress>, FileMappedResizableDirectMemory> storage{
                    FileMappedResizableDirectMemory{memoryFilePath}};
                for (std::int64_t i = 0; i < state.range(0); ++i)
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
        for ([[maybe_unused]] auto _ : state)
        {
            Tools::TemporaryDirectory temporary;
            std::string dataMemoryFilePath = temporary.File("data.links");
            std::string indexMemoryFilePath = temporary.File("index.links");
            Expects(!Collections::IsWhiteSpace(dataMemoryFilePath));
            try
            {
                SplitMemoryLinks<LinksOptions<TLinkAddress>, FileMappedResizableDirectMemory> storage{
                    FileMappedResizableDirectMemory{dataMemoryFilePath},
                    FileMappedResizableDirectMemory{indexMemoryFilePath}};
                for (std::int64_t i = 0; i < state.range(0); ++i)
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

    BENCHMARK(BM_CreateMillionPointsUnitedMemory)->Arg(1000000);
    BENCHMARK(BM_CreateMillionPointsSplitMemory)->Arg(1000000);
} // namespace Platform::Data::Doublets::Benchmarks

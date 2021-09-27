#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>

using namespace Platform::Memory;
using namespace Platform::Data::Doublets;

auto main() -> int {
    using link_type = std::size_t;
    using doublet = Doublet<link_type>;
    auto file = FileMappedResizableDirectMemory("db.links");
    auto mem = DirectMemoryAsArrayMemoryAdapter<doublet>(file);
    auto block_size = sizeof(doublet);

    mem[0*block_size] = doublet(1, 1);
    mem[1*block_size] = doublet(2, 1);
    mem[2*block_size] = doublet(3, 1);
    mem[3*block_size] = doublet(3, 2);

    std::cout << (doublet)mem[0*block_size] << std::endl;
    std::cout << (doublet)mem[1*block_size] << std::endl;
    std::cout << (doublet)mem[2*block_size] << std::endl;
    std::cout << (doublet)mem[3*block_size] << std::endl;
}
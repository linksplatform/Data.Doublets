#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>

using namespace Platform::Memory;
using namespace Platform::Data::Doublets;

auto main() -> int {
    using link_type = std::size_t;
    using doublet = Doublet<link_type>;
    auto file = FileMappedResizableDirectMemory("db.links");
    auto mem = DirectMemoryAsArrayMemoryAdapter<doublet>(file);

    mem[0] = doublet(1, 1);
    mem[1] = doublet(2, 1);
    mem[2] = doublet(3, 1);
    mem[3] = doublet(3, 2);

    std::cout << (doublet)mem[0] << std::endl;
    std::cout << (doublet)mem[1] << std::endl;
    std::cout << (doublet)mem[2] << std::endl;
    std::cout << (doublet)mem[3] << std::endl;
}
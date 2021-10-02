#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>

using namespace Platform::Memory;
using namespace Platform::Data::Doublets;

auto _0main() -> int {
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


auto _1main() -> int {
    auto link = Link(1, 2, 3);

    std::cout << Link<int>::ToString(1, 2) << std::endl;
    std::cout << Link<int>::ToString(1, 2, 3) << std::endl;
    std::cout << "(" << link[0] << ": " << link[1] << "->" << link[2] << ")" << std::endl;
    std::cout << link << std::endl;
    for (auto item : link) {
        std::cout << item << " ";
    }
    std::cout << std::endl;

    try { link[1488]; }
    catch (std::exception& e) {
        std::cout << e.what() << std::endl;
    }
}

auto main() -> int {
    std::filesystem::remove("db.links");

    auto start = std::chrono::high_resolution_clock::now();

    using namespace Memory::United::Generic;

    auto mem = FileMappedResizableDirectMemory("db.links");
    //auto mem = HeapResizableDirectMemory();
    auto links = UnitedMemoryLinks<int>(mem);
    auto constants = links.Constants;

    auto root = links.Create(std::vector<int>{});
    int n = 20;
    while (n--)
    {
        int count = links.Count(std::vector<int>{});
        links.Each([&](auto&& link)
        {
            if (count == 0) return constants.Break;
            auto current = links.Create(std::vector<int>{});
            links.Update(std::vector{current}, std::vector{current, current, link[0]});

            count--;
            return constants.Continue;
        }, std::vector<int>{});
    }

    auto end = std::chrono::high_resolution_clock::now();

    std::cout << "links count: " << links.Count(std::vector<int>{}) << std::endl;
    std::cout << "links count: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms" << std::endl;

    //inks.Each([&](auto&& link) {
    //   if (link[1] == 0) std::cout << link[0]; else std::cout << link[1] << "->";
    //   if (link[2] == 0) std::cout << link[0]; else std::cout << link[2] << "\n";
    //   return constants.Continue;
    //, std::vector<int>{});
}
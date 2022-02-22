#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>

#include "UnitedMemoryLinksFfiTests.cpp"

using namespace Platform::Memory;
using namespace Platform::Data::Doublets;
using namespace Memory::United::Generic;

auto main() -> int {
//    std::filesystem::remove("db.links"); // let's try
//    auto mem = FileMappedResizableDirectMemory("db.links");
//    //auto mem = HeapResizableDirectMemory();
//    auto links = UnitedMemoryLinks<std::size_t/*, HeapResizableDirectMemory*/>(std::move(mem));
//
//    auto start = std::chrono::high_resolution_clock::now();
//
//   for (auto i = 0; i < 1'000'000; i++) {
//       try {
//           if (links.SearchOrDefault(1, 1) != 0) {
//               throw std::runtime_error("links ... is exists");
//           } else {
//               links.CreateAndUpdate(1, 1);
//           }
//       } catch (...) {
//
//       }
//   }
//
//    links.SearchOrDefault(1, 1);
//
//    links.DeleteByQuery(Link{ links.Constants.Continue,links.Constants.Continue,links.Constants.Continue, });
//
//    auto end = std::chrono::high_resolution_clock::now();
//    std::cout << "elapsed: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms" << std::endl;
}

#include "../Tools/TemporaryDirectory.h"

#include <fstream>

namespace Platform::Data::Doublets::Tests
{
    TEST(TemporaryDirectoryTests, OwnsDistinctDirectoryAndCleansItUp)
    {
        std::filesystem::path path;
        {
            Tools::TemporaryDirectory first;
            Tools::TemporaryDirectory second;
            path = first.File("links");
            EXPECT_NE(path.parent_path(), std::filesystem::path(second.File("links")).parent_path());
            std::ofstream file(path);
            file << "fixture";
            file.close();
            EXPECT_TRUE(std::filesystem::is_regular_file(path));
        }
        EXPECT_FALSE(std::filesystem::exists(path.parent_path()));
    }
} // namespace Platform::Data::Doublets::Tests

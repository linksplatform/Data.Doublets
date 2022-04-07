namespace Platform::Data::Doublets::Profiling
{
    using TLinkAddress = std::uint64_t;

    constexpr std::size_t pointsNumberToCreate = 1000000;

    void CreatePoints()
    {
        using namespace Platform;
        using namespace Platform::Memory;
        using namespace Platform::Data;
        using namespace Platform::Data::Doublets;
        using namespace Platform::Data::Doublets::Memory::United::Generic;
        std::string tempFilePath { std::tmpnam(nullptr) };
        Expects(!Collections::IsWhiteSpace(tempFilePath));
        try
        {
            UnitedMemoryLinks<LinksOptions<>, FileMappedResizableDirectMemory> storage{FileMappedResizableDirectMemory{tempFilePath}};
            for (std::size_t i = 0; i < pointsNumberToCreate; ++i)
            {
                CreatePoint(storage);
            }
        }
        catch (...)
        {
            std::remove(tempFilePath.c_str());
            throw;
        }
        std::remove(tempFilePath.c_str());
    }
}

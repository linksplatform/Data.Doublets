namespace Platform::Data::Doublets::Tests
{
    using TLinkAddress = std::uint64_t;
//    TEST(ILinksBasicTests, DeleteAllUsages)
//    {
//        auto mem = HeapResizableDirectMemory();
//        auto storage = UnitedMemoryLinks<uint>(mem);
//
//        auto root = CreatePoint<TLinkAddress>(storage);
//
//        auto a = CreatePoint<TLinkAddress>(storage);
//        auto b = CreatePoint<TLinkAddress>(storage);
//
//        storage.CreateAndUpdate(a, root);
//        storage.CreateAndUpdate(b, root);
//
//        ASSERT_EQ(5U, Count<TLinkAddress>(storage));
//
//        DeleteAllUsages(storage, root);
//
//        ASSERT_EQ(3U, Count<TLinkAddress>(storage));
//    }

    TEST(ILinksBasicTests, FfiDeleteAllUsages)
    {
        using namespace Platform::Memory;
        using namespace Platform::Data::Doublets::Memory::United;
        std::string tempFilePath { std::tmpnam(nullptr) };
        Expects(!Collections::IsWhiteSpace(tempFilePath));
        try
        {
            auto storage = Ffi::UnitedMemoryLinks<uint>(tempFilePath);

            auto root = CreatePoint<TLinkAddress>(storage);

            auto a = CreatePoint<TLinkAddress>(storage);
            auto b = CreatePoint<TLinkAddress>(storage);

            CreateAndUpdate(storage, a, root);
            CreateAndUpdate(storage, b, root);

            ASSERT_EQ(5U, Count<TLinkAddress>(storage));

            DeleteAllUsages(storage, root);

            ASSERT_EQ(3U, Count<TLinkAddress>(storage));
            Ffi::UnitedMemoryLinks<TLink> ffiStorage {tempFilePath};
            action(ffiStorage);
        }
        catch (...)
        {
            std::remove(tempFilePath.c_str());
            throw;
        }
        std::remove(tempFilePath.c_str());
    }
}

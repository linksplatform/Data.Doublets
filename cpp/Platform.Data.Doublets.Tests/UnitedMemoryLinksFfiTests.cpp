//using namespace Platform::Data::Doublets::Memory::United::Ffi;
//using TLinkAddressByte = uint8_t;
//
//namespace Platform::Data::Doublets::Tests
//{
//    TEST(UnitedMemoryLinksFfiTests, ConstructorTest)
//    {
//        UnitedMemoryLinksFfi<TLinkAddressByte> storage {"db.links"};
//    };
//
//    TEST(UnitedMemoryLinksFfiTests, CreateTest)
//    {
//        UnitedMemoryLinksFfi<TLinkAddressByte> storage {"db.links"};
//        Link link {1, 2, 3};
//        storage.Create(link, null);
//        Assert.Equal(1, storage.Count());
//    }
//
//    TEST(UnitedMemoryLinksFfiTests, DeleteTest)
//    {
//        UnitedMemoryLinksFfi<TLinkAddressByte> storage {"db.links"};
//        Link link {1, 2, 3};
//        storage.Create(link, null);
//        storage.Delete(link);
//        Assert.Equal(0, storage.Count());
//    }
//
//    TEST(UnitedMemoryLinksFfiTests, DeleteTest)
//    {
//        UnitedMemoryLinksFfi<TLinkAddressByte> storage {"db.links"};
//        Link link {1, 2, 3};
//        storage.Create(link, null);
//        Link newLink {4, 5, 6};
//        storage.Update(link, newLink, null)
//        Assert.Equal(1, storage.Count());
//        Assert.Equal(1, storage.Count(newLink));
//    }
//
//    TEST(UnitedMemoryLinksFfiTests, DeleteTest)
//    {
//        UnitedMemoryLinksFfi<TLinkAddressByte> storage {"db.links"};
//        Link link {1, 2, 3};
//        storage.Create(link, std::ranges::size(link), null);
//        Link newLink {4, 5, 6};
//        storage.Update(link, newLink, null)
//        Assert.Equal(1, storage.Count());
//        Assert.Equal(1, storage.Count(newLink));
//    }
//}

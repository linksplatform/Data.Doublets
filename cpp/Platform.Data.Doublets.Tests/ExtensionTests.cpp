namespace Platform::Data::Doublets::Tests
{
    using ExtensionStorage = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                                        Platform::Memory::HeapResizableDirectMemory>;

    TEST(ExtensionTests, EnsureCreatedPreservesOnlyRequestedNewAddresses)
    {
        ExtensionStorage storage{Platform::Memory::HeapResizableDirectMemory{}};
        auto existing = Create(storage);
        EnsureCreated(storage, ExtensionStorage::LinkType{existing, 3, 5});
        EXPECT_TRUE(Exists(storage, existing));
        EXPECT_TRUE(Exists(storage, 3));
        EXPECT_TRUE(Exists(storage, 5));
        EXPECT_FALSE(Exists(storage, 2));
        EXPECT_FALSE(Exists(storage, 4));
        EXPECT_EQ(3, Count(storage));
    }

    TEST(ExtensionTests, EnsurePointsCreatedMakesSelfReferences)
    {
        ExtensionStorage storage{Platform::Memory::HeapResizableDirectMemory{}};
        EnsurePointsCreated(storage, ExtensionStorage::LinkType{1, 1, 1});
        auto link = GetLink(storage, 1);
        EXPECT_EQ(1, link[1]);
        EXPECT_EQ(1, link[2]);
        EXPECT_EQ(1, Count(storage));
    }

    TEST(ExtensionTests, DeleteByQueryRemovesOnlyMatches)
    {
        ExtensionStorage storage{Platform::Memory::HeapResizableDirectMemory{}};
        auto first = Create(storage);
        auto second = Create(storage);
        Update(storage, first, 1, 2);
        Update(storage, second, 2, 1);
        DeleteByQuery(storage, ExtensionStorage::LinkType{storage.Constants.Any, 1, 2});
        EXPECT_FALSE(Exists(storage, first));
        EXPECT_TRUE(Exists(storage, second));
        EXPECT_EQ(1, Count(storage));
    }
} // namespace Platform::Data::Doublets::Tests

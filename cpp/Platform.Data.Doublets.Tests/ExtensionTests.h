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
        EnsurePointsCreated(storage, ExtensionStorage::LinkType{1, 3, 5});
        auto link = GetLink(storage, 1);
        EXPECT_EQ(1, link[1]);
        EXPECT_EQ(1, link[2]);
        EXPECT_EQ(3, Count(storage));
        EXPECT_FALSE(Exists(storage, 2));
        EXPECT_FALSE(Exists(storage, 4));
        auto last = GetLink(storage, 5);
        EXPECT_EQ(5, last[1]);
        EXPECT_EQ(5, last[2]);
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

    TEST(ExtensionTests, AllVariadicArgumentsRestrictTheResult)
    {
        ExtensionStorage storage{Platform::Memory::HeapResizableDirectMemory{}};
        auto first = Create(storage);
        auto second = Create(storage);
        auto selected = All(storage, second);
        ASSERT_EQ(1, selected.size());
        EXPECT_EQ(second, selected.front()[0]);
        EXPECT_TRUE(Exists(storage, first));
    }

    template <typename TStorage>
    void CheckAddressLimit(TStorage& storage)
    {
        auto maximum = storage.Constants.InternalReferencesRange.Maximum;
        for (unsigned int address = 1; address <= maximum; ++address)
            EXPECT_EQ(address, Create(storage));
        EXPECT_THROW(Create(storage), std::logic_error);
        EXPECT_EQ(maximum, Count(storage));
        Delete(storage, 1);
        EXPECT_EQ(1, Create(storage));
        EXPECT_THROW(Create(storage), std::logic_error);
    }

    TEST(ExtensionTests, UnitedRejectsExhaustedAddressesButReusesFreeLinks)
    {
        Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint8_t>,
                                                   Platform::Memory::HeapResizableDirectMemory>
            storage{Platform::Memory::HeapResizableDirectMemory{}};
        CheckAddressLimit(storage);
    }

    TEST(ExtensionTests, SplitRejectsExhaustedAddressesButReusesFreeLinks)
    {
        Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint8_t>,
                                                 Platform::Memory::HeapResizableDirectMemory>
            storage{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{}};
        CheckAddressLimit(storage);
    }
} // namespace Platform::Data::Doublets::Tests

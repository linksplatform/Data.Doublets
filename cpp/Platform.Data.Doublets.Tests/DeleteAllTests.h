namespace Platform::Data::Doublets::Tests
{
    // Issue #354: DeleteAll must delete every link and leave a store that works like a new one.
    template <typename TStorage>
    void CheckDeleteAll(TStorage& storage, auto fill)
    {
        using Address = std::remove_cv_t<decltype(storage.Constants.Any)>;
        using Link = std::vector<Address>;
        auto any = storage.Constants.Any;
        fill(storage);
        DeleteAll(storage);
        EXPECT_EQ(Address{0}, Count(storage));
        // Addresses start from 1 again, and the endpoint indexes contain only new links.
        auto first = CreatePoint(storage);
        auto second = CreatePoint(storage);
        // GetOrCreate searches the index first, so a stale entry would be returned instead of a new link.
        auto link = GetOrCreate(storage, first, second);
        EXPECT_EQ(Address{1}, first);
        EXPECT_EQ(Address{3}, link);
        EXPECT_EQ(Address{3}, Count(storage));
        EXPECT_EQ(link, SearchOrDefault(storage, first, second));
        EXPECT_EQ(Address{2}, storage.Count(Link{any, first, any}));
        EXPECT_EQ(Address{2}, storage.Count(Link{any, any, second}));
    }

    template <typename TStorage>
    void CheckAllDeleteAll(auto makeStorage)
    {
        using Address = typename TStorage::LinkAddressType;
        auto points = [](TStorage& storage, int count) {
            std::vector<Address> result;
            for (auto i = 0; i < count; ++i)
                result.push_back(CreatePoint(storage));
            return result;
        };
        auto check = [&](auto fill) {
            TStorage storage = makeStorage();
            CheckDeleteAll(storage, fill);
        };
        check([](TStorage&) {});
        check([&](TStorage& storage) { points(storage, 3); });
        // Without resetting endpoints first, deletion leaves stale entries in the endpoint indexes.
        check([&](TStorage& storage) {
            auto point = points(storage, 2);
            GetOrCreate(storage, point[0], point[1]);
            GetOrCreate(storage, point[1], point[0]);
        });
        // Count is 3 while the highest address is 5: links 4 and 5 are above the count.
        check([&](TStorage& storage) {
            auto point = points(storage, 5);
            Delete(storage, point[1], [&](const auto&, const auto&) { return storage.Constants.Continue; });
            Delete(storage, point[2], [&](const auto&, const auto&) { return storage.Constants.Continue; });
            Update(storage, point[4], point[0], point[3]);
        });
        // A link may use links with higher addresses.
        check([&](TStorage& storage) {
            auto point = points(storage, 3);
            Update(storage, point[0], point[1], point[2]);
        });
    }

    TEST(DeleteAllTests, UnitedDeletesEveryLink)
    {
        using Storage = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                                   Platform::Memory::HeapResizableDirectMemory>;
        CheckAllDeleteAll<Storage>([] { return Storage{Platform::Memory::HeapResizableDirectMemory{}}; });
    }

    TEST(DeleteAllTests, SplitDeletesEveryLink)
    {
        using Storage = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint32_t>,
                                                                 Platform::Memory::HeapResizableDirectMemory>;
        CheckAllDeleteAll<Storage>([] {
            return Storage{Platform::Memory::HeapResizableDirectMemory{},
                           Platform::Memory::HeapResizableDirectMemory{}};
        });
    }
} // namespace Platform::Data::Doublets::Tests

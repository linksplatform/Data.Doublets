namespace Platform::Data::Doublets::Tests
{
    // Issue #515: re-pointing a usage must replace only the endpoint naming the merged link.
    template <typename TStorage>
    void CheckMergeUsages(TStorage& storage, bool deleteMerged, bool withHandler)
    {
        using Address = std::remove_cv_t<decltype(storage.Constants.Any)>;
        using Link = std::vector<Address>;
        auto any = storage.Constants.Any;
        auto $continue = storage.Constants.Continue;
        auto one = CreatePoint(storage);
        auto two = CreatePoint(storage);
        auto three = CreatePoint(storage);
        for (auto point : Link{one, two, three})
            Update(storage, point, Address{0}, Address{0});
        auto asSource = Create(storage);
        Update(storage, asSource, one, three);
        auto asTarget = Create(storage);
        Update(storage, asTarget, three, one);
        auto asBoth = Create(storage);
        Update(storage, asBoth, one, one);
        std::vector<std::pair<Link, Link>> changes;
        auto handler = [&](const auto& before, const auto& after) {
            // Deletion reports an empty after state, so copy each state whole.
            changes.emplace_back(Link(before.begin(), before.end()), Link(after.begin(), after.end()));
            return $continue;
        };
        if (deleteMerged && withHandler)
            EXPECT_EQ($continue, MergeAndDelete(storage, one, two, handler));
        else if (deleteMerged)
            EXPECT_EQ(two, MergeAndDelete(storage, one, two));
        else if (withHandler)
            EXPECT_EQ($continue, MergeUsages(storage, one, two, handler));
        else
            MergeUsages(storage, one, two);
        auto link = [&](Address address) {
            auto value = GetLink(storage, address);
            return Link{value[0], value[1], value[2]};
        };
        EXPECT_EQ(!deleteMerged, Exists(storage, one));
        EXPECT_EQ((Link{two, 0, 0}), link(two));
        EXPECT_EQ((Link{three, 0, 0}), link(three));
        // The issue expects (4: 1 3) -> (4: 2 3) and (5: 3 1) -> (5: 3 2).
        EXPECT_EQ((Link{asSource, two, three}), link(asSource));
        EXPECT_EQ((Link{asTarget, three, two}), link(asTarget));
        EXPECT_EQ((Link{asBoth, two, two}), link(asBoth));
        EXPECT_EQ(deleteMerged ? 5 : 6, Count(storage));
        EXPECT_EQ(0, storage.Count(Link{any, one, any}));
        EXPECT_EQ(0, storage.Count(Link{any, any, one}));
        if (withHandler)
        {
            std::ranges::sort(changes);
            std::vector<std::pair<Link, Link>> expected{
                {{asSource, one, three}, {asSource, two, three}},
                {{asTarget, three, one}, {asTarget, three, two}},
                {{asBoth, one, one}, {asBoth, two, one}},
                {{asBoth, two, one}, {asBoth, two, two}},
            };
            if (deleteMerged)
                expected.push_back({{one, 0, 0}, {}});
            std::ranges::sort(expected);
            EXPECT_EQ(expected, changes);
        }
    }

    template <typename TStorage>
    void CheckAllMergeUsages(auto makeStorage)
    {
        for (auto deleteMerged : {false, true})
        {
            for (auto withHandler : {false, true})
            {
                TStorage storage = makeStorage();
                CheckMergeUsages(storage, deleteMerged, withHandler);
            }
        }
    }

    TEST(MergeUsagesTests, UnitedRewritesOnlyTheMergedEndpoint)
    {
        using Storage = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                                   Platform::Memory::HeapResizableDirectMemory>;
        CheckAllMergeUsages<Storage>([] { return Storage{Platform::Memory::HeapResizableDirectMemory{}}; });
    }

    TEST(MergeUsagesTests, SplitRewritesOnlyTheMergedEndpoint)
    {
        using Storage = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint32_t>,
                                                                 Platform::Memory::HeapResizableDirectMemory>;
        CheckAllMergeUsages<Storage>([] {
            return Storage{Platform::Memory::HeapResizableDirectMemory{},
                           Platform::Memory::HeapResizableDirectMemory{}};
        });
    }
} // namespace Platform::Data::Doublets::Tests

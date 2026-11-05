namespace Platform::Data::Doublets::Tests
{
    template <typename TStorage>
    void CheckPositionalQueries(TStorage& storage, bool count)
    {
        using Address = std::remove_cv_t<decltype(storage.Constants.Any)>;
        using Link = std::vector<Address>;
        const auto any = storage.Constants.Any;
        const auto a = CreatePoint(storage);
        const auto b = CreatePoint(storage);
        const auto asymmetric = CreateAndUpdate(storage, a, b);
        CreateAndUpdate(storage, b, a);
        const auto external = static_cast<Address>(storage.Constants.ExternalReferencesRange.Minimum + 20);
        CreateAndUpdate(storage, external, a);
        CreateAndUpdate(storage, b, external);
        CreateAndUpdate(storage, external, external);
        CreateAndUpdate(storage, Address{0}, a);
        CreateAndUpdate(storage, b, Address{0});
        Create(storage);
        const auto deleted = CreatePoint(storage);
        const auto last = CreatePoint(storage);
        Update(storage, deleted, Address{0}, Address{0});
        Delete(storage, deleted);
        const Link values{any, Address{0}, a, b, external, Address{50}};
        const auto check = [&] {
            std::vector<Link> rows;
            storage.Each({}, [&](const auto& link) {
                rows.emplace_back(link.begin(), link.end());
                return storage.Constants.Continue;
            });
            Link indices{Address{0}, deleted, external, Address{50}};
            for (const auto& row : rows)
            {
                indices.push_back(row[0]);
            }
            for (const auto index : indices)
            {
                const auto found =
                    std::find_if(rows.begin(), rows.end(), [&](const auto& row) { return row[0] == index; });
                const auto each = [&](const Link& query, bool matches) {
                    for (const auto flow : {storage.Constants.Continue, storage.Constants.Break})
                    {
                        auto calls = 0;
                        const auto result = storage.Each(query, [&](const auto& link) {
                            ++calls;
                            EXPECT_TRUE(matches);
                            if (found != rows.end())
                            {
                                EXPECT_EQ(*found, (Link{link.begin(), link.end()}));
                            }
                            return flow;
                        });
                        EXPECT_EQ(matches ? 1 : 0, calls);
                        EXPECT_EQ(matches ? flow : storage.Constants.Continue, result);
                    }
                };
                for (const auto source : values)
                {
                    for (const auto target : values)
                    {
                        SCOPED_TRACE(::testing::Message()
                                     << "query [" << +index << ", " << +source << ", " << +target << "]");
                        const auto matches = found != rows.end() && (source == any || (*found)[1] == source) &&
                                             (target == any || (*found)[2] == target);
                        const Link query{index, source, target};
                        if (count)
                        {
                            EXPECT_EQ(matches ? Address{1} : Address{0}, storage.Count(query));
                            EXPECT_EQ(matches ? Address{1} : Address{0}, std::as_const(storage).Count(query));
                        }
                        else
                        {
                            each(query, matches);
                        }
                    }
                    const auto matches =
                        found != rows.end() && (source == any || (*found)[1] == source || (*found)[2] == source);
                    const Link query{index, source};
                    if (count)
                    {
                        EXPECT_EQ(matches ? Address{1} : Address{0}, storage.Count(query));
                        EXPECT_EQ(matches ? Address{1} : Address{0}, std::as_const(storage).Count(query));
                    }
                    else
                    {
                        each(query, matches);
                    }
                }
            }
        };
        check();
        Update(storage, asymmetric, b, asymmetric);
        EXPECT_EQ(deleted, CreateAndUpdate(storage, last, a));
        check();
    }

    template <typename TAddress>
    void CheckPositionalStores(bool count)
    {
        using United = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<TAddress>,
                                                                  Platform::Memory::HeapResizableDirectMemory>;
        using Split = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<TAddress>,
                                                               Platform::Memory::HeapResizableDirectMemory>;
        using DecoratedUnited = Decorators::LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<United>;
        using DecoratedSplit = Decorators::LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<Split>;
        United united{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckPositionalQueries(united, count);
        Split split{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckPositionalQueries(split, count);
        DecoratedUnited decoratedUnited{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckPositionalQueries(decoratedUnited, count);
        DecoratedSplit decoratedSplit{Platform::Memory::HeapResizableDirectMemory{},
                                      Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckPositionalQueries(decoratedSplit, count);
    }

    TEST(PositionalQueries, Count)
    {
        CheckPositionalStores<std::uint32_t>(true);
        CheckPositionalStores<std::uint64_t>(true);
    }

    TEST(PositionalQueries, Each)
    {
        CheckPositionalStores<std::uint32_t>(false);
        CheckPositionalStores<std::uint64_t>(false);
    }
} // namespace Platform::Data::Doublets::Tests

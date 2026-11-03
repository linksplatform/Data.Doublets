namespace Platform::Data::Doublets::Tests
{
    template <typename TStorage>
    void CheckEitherEndpointOriginal(TStorage& storage)
    {
        using Address = std::remove_cv_t<decltype(storage.Constants.Any)>;
        using Link = std::vector<Address>;
        auto a = CreatePoint(storage);
        auto b = CreatePoint(storage);
        Update(storage, b, a, b);
        auto query = Link{storage.Constants.Any, a};
        EXPECT_EQ(2, storage.Count(query));
        std::vector<Address> visited;
        EXPECT_EQ(storage.Constants.Continue, storage.Each(query, [&](const auto& link) {
            visited.push_back(link[0]);
            return storage.Constants.Continue;
        }));
        EXPECT_EQ((Link{a, b}), visited);
    }

    template <typename TStorage>
    void CheckEitherEndpointUnion(TStorage& storage, bool decorated)
    {
        using Address = std::remove_cv_t<decltype(storage.Constants.Any)>;
        using Link = std::vector<Address>;
        auto any = storage.Constants.Any;
        auto a = CreatePoint(storage);
        auto b = CreatePoint(storage);
        auto sourceOnly = Create(storage);
        Update(storage, sourceOnly, a, b);
        CreateAndUpdate(storage, b, a);
        auto external = static_cast<Address>(storage.Constants.ExternalReferencesRange.Minimum + 20);
        CreateAndUpdate(storage, external, external);
        CreateAndUpdate(storage, external, a);
        CreateAndUpdate(storage, b, external);
        Create(storage);
        for (int stage = 0; stage < 2; ++stage)
        {
            if (stage == 1 && !decorated)
            {
                CreateAndUpdate(storage, a, a);
                CreateAndUpdate(storage, a, a);
                CreateAndUpdate(storage, external, external);
            }
            for (auto value : Link{a, b, external, Address{0}, Address{50}})
            {
                Link expected;
                storage.Each(Link{}, [&](const auto& link) {
                    if (link[1] == value || link[2] == value)
                        expected.push_back(link[0]);
                    return storage.Constants.Continue;
                });
                auto query = Link{any, value};
                EXPECT_EQ(expected.size(), storage.Count(query));
                Link actual;
                EXPECT_EQ(storage.Constants.Continue, storage.Each(query, [&](const auto& link) {
                    actual.push_back(link[0]);
                    return storage.Constants.Continue;
                }));
                auto sorted = actual;
                std::ranges::sort(sorted);
                EXPECT_EQ(expected, sorted);
                for (std::size_t stopAt = 1; stopAt <= expected.size(); ++stopAt)
                {
                    Link prefix;
                    EXPECT_EQ(storage.Constants.Break, storage.Each(query, [&](const auto& link) {
                        prefix.push_back(link[0]);
                        return prefix.size() == stopAt ? storage.Constants.Break : storage.Constants.Continue;
                    }));
                    EXPECT_EQ((Link{actual.begin(), actual.begin() + stopAt}), prefix);
                }
            }
            if (stage == 0)
                Update(storage, sourceOnly, b, sourceOnly);
        }
        EXPECT_EQ(Count(storage), storage.Count(Link{any, any}));
        EXPECT_EQ(1, storage.Count(Link{a, a}));
    }

    template <typename TStorage>
    void CheckEitherEndpointQueries()
    {
        TStorage original{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointOriginal(original);
        TStorage unionStorage{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointUnion(unionStorage, false);
    }

    TEST(EitherEndpointQueryTests, UnitedPlain)
    {
        using Storage = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                                   Platform::Memory::HeapResizableDirectMemory>;
        CheckEitherEndpointQueries<Storage>();
    }

    TEST(EitherEndpointQueryTests, SplitPlain)
    {
        using Storage = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint64_t>,
                                                                 Platform::Memory::HeapResizableDirectMemory>;
        Storage original{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{},
                         4096};
        CheckEitherEndpointOriginal(original);
        Storage unionStorage{Platform::Memory::HeapResizableDirectMemory{},
                             Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointUnion(unionStorage, false);
    }

    TEST(EitherEndpointQueryTests, UnitedAutomatic)
    {
        using Raw = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                               Platform::Memory::HeapResizableDirectMemory>;
        using Storage = Decorators::LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<Raw>;
        Storage original{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointOriginal(original);
        Storage unionStorage{Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointUnion(unionStorage, true);
    }

    TEST(EitherEndpointQueryTests, SplitAutomatic)
    {
        using Raw = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint64_t>,
                                                             Platform::Memory::HeapResizableDirectMemory>;
        using Storage = Decorators::LinksDecoratedWithAutomaticUniquenessAndUsagesResolution<Raw>;
        Storage original{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{},
                         4096};
        CheckEitherEndpointOriginal(original);
        Storage unionStorage{Platform::Memory::HeapResizableDirectMemory{},
                             Platform::Memory::HeapResizableDirectMemory{}, 4096};
        CheckEitherEndpointUnion(unionStorage, true);
    }
} // namespace Platform::Data::Doublets::Tests

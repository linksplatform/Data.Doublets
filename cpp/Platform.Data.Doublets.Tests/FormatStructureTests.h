namespace Platform::Data::Doublets::Tests
{
    template <typename TStorage>
    void CheckFormatStructure(auto makeStorage)
    {
        using Address = typename TStorage::LinkAddressType;
        auto noElements = [](const Link<Address>&) { return false; };
        {
            TStorage storage = makeStorage();
            // Adjacent-pair layers emitted by BalancedVariantConverter for the issue's input.
            std::vector<Address> layer;
            for (auto c : std::string{"super_name"})
                layer.push_back(static_cast<Address>(c));
            while (layer.size() > 1)
            {
                std::vector<Address> next;
                for (std::size_t i = 0; i < layer.size(); i += 2)
                    next.push_back(i + 1 < layer.size() ? GetOrCreate(storage, layer[i], layer[i + 1]) : layer[i]);
                layer = std::move(next);
            }
            EXPECT_EQ(9, layer[0]);
            EXPECT_EQ("(9:(8:(6:(1:~115 ~117) (2:~112 ~101)) (7:(3:~114 ~95) (4:~110 ~97))) (5:~109 ~101))",
                      FormatStructure(storage, layer[0], noElements, true, true));
            EXPECT_EQ("((((115 117) (112 101)) ((114 95) (110 97))) (109 101))",
                      FormatStructure(storage, layer[0], noElements));
            EXPECT_EQ(9, Count(storage));
            EXPECT_FALSE(Exists(storage, Address{115}));
        }
        {
            TStorage storage = makeStorage();
            auto root = Create(storage);
            Update(storage, root, Address{100}, Address{101});
            auto unexpected = [](const Link<Address>&) {
                ADD_FAILURE() << "Only stored elements reach the predicate";
                return false;
            };
            EXPECT_EQ("(1:~100 ~101)", FormatStructure(storage, root, unexpected, true, true));
            EXPECT_EQ("(100 101)", FormatStructure(storage, root, unexpected));
            EXPECT_EQ("~100", FormatStructure(storage, Address{100}, unexpected, true, true));
            EXPECT_EQ("100", FormatStructure(storage, Address{100}, unexpected));
            for (auto value : {storage.Constants.Null, storage.Constants.Any, storage.Constants.Itself})
            {
                EXPECT_EQ("", FormatStructure(storage, value, unexpected, true, true));
            }
            // Query constants aren't valid stored internal endpoints. Null endpoints are supported.
            Update(storage, root, Address{0}, Address{0});
            EXPECT_EQ("(1: )", FormatStructure(storage, root, unexpected, true, true));
            auto external = static_cast<Address>(storage.Constants.ExternalReferencesRange.Minimum + 20);
            Update(storage, root, external, external);
            auto leaf = std::to_string(external);
            EXPECT_EQ("(1:(" + leaf + ":" + leaf + " " + leaf + ") *" + leaf + ")",
                      FormatStructure(storage, root, noElements, true, true));
            EXPECT_EQ("(1:" + leaf + " " + leaf + ")",
                      FormatStructure(storage, root, [](const auto&) { return true; }, true, true));
        }
        {
            TStorage storage = makeStorage();
            auto point = CreatePoint(storage);
            EXPECT_EQ("(1:1 1)", FormatStructure(storage, point, noElements, true, true));
            EXPECT_EQ("(1 1)", FormatStructure(storage, point, noElements));
            auto root = Create(storage);
            Update(storage, root, point, point);
            EXPECT_EQ("(2:(1:1 1) *1)", FormatStructure(storage, root, noElements, true, true));
            auto allElements = [](const Link<Address>&) { return true; };
            EXPECT_EQ("(2:1 1)", FormatStructure(storage, root, allElements, true, true));
            auto calls = 0;
            auto appendElement = [&](std::ostream& output, const Link<Address>& element) {
                EXPECT_EQ(point, element.Index);
                ++calls;
                output << "leaf";
            };
            EXPECT_EQ("(2:leaf leaf)", FormatStructure(storage, root, allElements, appendElement, true, true));
            EXPECT_EQ(2, calls);
            std::ostringstream output;
            output << "prefix:";
            std::unordered_set<Address> visited;
            auto appendIndex = [](std::ostream& text, const Link<Address>& link) {
                text << std::to_string(link.Index);
            };
            AppendStructure(storage, output, visited, point, noElements, appendIndex, true, true);
            AppendStructure(storage, output, visited, point, noElements, appendIndex, true, true);
            EXPECT_EQ("prefix:(1:1 1)*1", output.str());
            EXPECT_EQ((std::unordered_set<Address>{point}), visited);
            Delete(storage, point);
            EXPECT_EQ("(2:~1 ~1)", FormatStructure(storage, root, noElements, true, true));
        }
        {
            TStorage storage = makeStorage();
            auto one = Create(storage);
            auto two = Create(storage);
            Update(storage, two, one, one);
            Update(storage, one, two, two);
            EXPECT_EQ("((1 1) 2)", FormatStructure(storage, one, noElements));
            EXPECT_EQ("((*1 *1) *2)", FormatStructure(storage, one, noElements, false, true));
            EXPECT_EQ("(1:(2:1 1) 2)", FormatStructure(storage, one, noElements, true, false));
            EXPECT_EQ("(1:(2:*1 *1) *2)", FormatStructure(storage, one, noElements, true, true));
        }
    }

    TEST(FormatStructureTests, UnitedMatchesCSharpStructureFormatting)
    {
        using Storage = Memory::United::Generic::UnitedMemoryLinks<LinksOptions<std::uint64_t>,
                                                                   Platform::Memory::HeapResizableDirectMemory>;
        CheckFormatStructure<Storage>([] { return Storage{Platform::Memory::HeapResizableDirectMemory{}, 4096}; });
    }

    TEST(FormatStructureTests, SplitMatchesCSharpStructureFormatting)
    {
        using Storage = Memory::Split::Generic::SplitMemoryLinks<LinksOptions<std::uint32_t>,
                                                                 Platform::Memory::HeapResizableDirectMemory>;
        CheckFormatStructure<Storage>([] {
            return Storage{Platform::Memory::HeapResizableDirectMemory{}, Platform::Memory::HeapResizableDirectMemory{},
                           4096};
        });
    }
} // namespace Platform::Data::Doublets::Tests

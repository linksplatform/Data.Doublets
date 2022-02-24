namespace Platform::Data::Doublets::Tests
{
    template<typename TLinkAddress>
    static void TestCrudOperations(auto&& storage)
    {
        const auto constants = storage.Constants;
        ASSERT_EQ(0, storage.Count());
        // Create link
        Setters::Setter<TLinkAddress, TLinkAddress> setter { constants.Continue, constants.Break, constants.Null };
        storage.Each( std::array{constants.Any, constants.Any, constants.Any} , setter.SetFirstAndReturnTrue);
        ASSERT_EQ(constants.Null, setter.Result());
        auto linkAddress = storage.Create();
        Link<TLinkAddress> link { storage.GetLink(linkAddress) };
        ASSERT_EQ(3, link.Count);
        ASSERT_EQ(linkAddress, link.Index);
        ASSERT_EQ(constants.Null, link.Source);
        ASSERT_EQ(constants.Null, link.Target);
        ASSERT_EQ(1, storage.Count());
        // Get first link
        setter = {constants.Continue, constants.Break, constants.Null};
        storage.Each(Link{constants.Any, constants.Any, constants.Any}, setter.SetFirstFromListAndReturnTrue);
        ASSERT_EQ(linkAddress, setter.Result());
        // Update link to reference itself
        storage.Update(linkAddress, linkAddress, linkAddress);
        link = Link{storage.GetLink(linkAddress)};
        ASSERT_EQ(linkAddress, link.Source);
        ASSERT_EQ(linkAddress, link.Target);
        // Update link to reference null (prepare for delete)
        auto updatedLinkAddress = storage.Update(linkAddress, constants.Null, constants.Null);
        ASSERT_EQ(linkAddress, updatedLinkAddress);
        link = {storage.GetLink(linkAddress)};
        ASSERT_EQ(constants.Null, link.Source);
        ASSERT_EQ(constants.Null, link.Target);
        // Delete link
        storage.Delete(linkAddress);
        ASSERT_EQ(0, storage.Count());
        setter = {constants.Continue, constants.Break, constants.Null};
        storage.Each(Link{constants.Any, constants.Any, constants.Any}, setter.SetFirstFromListAndReturnTrue);
        ASSERT_EQ(constants.Null, setter.Result());
    }

    template<typename TLinkAddress>
    static void TestRawNumbersCrudOperations(auto&& storage)
    {
        // Constants
        const auto constants = storage.Constants;
        Hybrid<TLinkAddress> h106E {106L, true};
        Hybrid<TLinkAddress> h107E {107L, true};
        Hybrid<TLinkAddress> h108E {-108L, true};
        ASSERT_EQ(106L, h106E.AbsoluteValue());
        ASSERT_EQ(107L, h107E.AbsoluteValue());
        ASSERT_EQ(108L, h108E.AbsoluteValue());
        // Create link (External -> External)
        auto linkAddress1 = storage.Create();
        storage.Update(linkAddress1, h106E, h108E);
        Link<TLinkAddress> link1 { storage.GetLink(linkAddress1) };
        ASSERT_EQ(h106E, link1.Source);
        ASSERT_EQ(h108E, link1.Target);
        // Create link (Internal -> External)
        auto linkAddress2 { storage.Create() };
        storage.Update(linkAddress2, linkAddress1, h108E);
        Link<TLinkAddress> link2 { storage.GetLink(linkAddress2) };
        ASSERT_EQ(linkAddress1, link2.Source);
        ASSERT_EQ(h108E, link2.Target);
        // Create link (Internal -> Internal)
        auto linkAddress3 { storage.Create() };
        storage.update(linkAddress3, linkAddress1, linkAddress2);
        Link<TLinkAddress> link3 { storage.GetLink(linkAddress3) };
        ASSERT_EQ(linkAddress1, link3.Source);
        ASSERT_EQ(linkAddress1, link3.Target);
        // Search for created link
        Setters::Setter<TLinkAddress, TLinkAddress> setter1 { constants.Continue, constants.Break, constants.Null };
        storage.Each(Link<TLinkAddress>{constants.Any, h106E, h108E}, setter1.SetFirstFromListAndReturnTrue);
        ASSERT_EQ(linkAddress1, setter1.Result);
        // Search for nonexistent link
        Setters::Setter<TLinkAddress, TLinkAddress> setter2 { constants.Continue, constants.Break, constants.Null };
        storage.Each(Link<TLinkAddress>{constants.Any, h106E, h108E}, setter2.SetFirstFromListAndReturnTrue);
        ASSERT_EQ(constants.Null, setter2.Result);
        // Update link to reference null (prepare for delete)
        auto updatedLinkAddress { storage.Update(linkAddress3, constants.Null, constants.Null) };
        ASSERT_EQ(linkAddress3, updatedLinkAddress);
        link3 = { storage.GetLink(linkAddress3) };
        ASSERT_EQ(constants.Null, link3.Source);
        ASSERT_EQ(constants.Null, link3.Target);
        // Delete link
        storage.Delete(linkAddress3);
        ASSERT_EQ(2, storage.Count());
        Setters::Setter<TLinkAddress, TLinkAddress> setter3 { constants.Continue, constants.Break, constants.Null };
        storage.Each(Link<TLinkAddress>{ constants.Any, constants.Any, constants.Any }, setter3.SetFirstFromListAndReturnTrue);
        ASSERT_EQ(linkAddress2, setter3.Result);
    }

    template<typename TLinkAddress>
    static void TestMultipleCreationsAndDeletions(auto&& storage, int numberOfOperations)
    {
        for (int i = 0; i < numberOfOperations; i++)
        {
            storage.Create();
        }
        for (int i = 0; i < numberOfOperations; i++)
        {
            storage.Delete(storage.Count());
        }
    }

    template<typename TLinkAddress>
    static void TestMultipleRandomCreationsAndDeletions(auto&& storage, int maximumOperationsPerCycle)
    {
        using namespace Platform::Random;
        for (auto N { 1 }; N < maximumOperationsPerCycle; ++N)
        {
            std::srand(N);
            auto randomGen64 { RandomHelpers::Default };
            auto created { 0UL };
            auto deleted { 0UL };
            for (auto i { 0 }; i < N; ++i)
            {
                auto linksCount { storage.Count() };
                auto createPoint { Random::NextBoolean(randomGen64) };
                if (linksCount >= 2 && createPoint)
                {
                    Ranges::Range<TLinkAddress> linksAddressRange { 1, linksCount };
                    TLinkAddress source { Random::NextUInt64(randomGen64, linksAddressRange) };
                    TLinkAddress target { Random::NextUInt64(randomGen64, linksAddressRange) }; //-V3086
                    auto resultLink { storage.GetOrCreate(source, target) };
                    if (resultLink > linksCount)
                    {
                        ++created;
                    }
                }
                else
                {
                    storage.Create();
                    ++created;
                }
            }
            ASSERT_EQ(storage.Count(), created);
            for (auto i { 0 }; i < N; ++i)
            {
                TLinkAddress link = i + 1;
                if (storage.Exists(link))
                {
                    storage.Delete(link);
                    ++deleted;
                }
            }
            ASSERT_EQ(0, storage.Count());
        }
    }
}

namespace Platform::Data::Doublets::Tests
{
    using namespace Platform::Data;
    using namespace Platform::Data::Doublets;
    class CrudOperationsTester
    {
    public:
        template<typename TLinkAddress>
        static TLinkAddress TestCreate(auto&& storage)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue = constants.Continue;
            Link<TLinkAddress> linkStruct;
            storage.Each( std::array{constants.Any, constants.Any, constants.Any} , [_continue, &linkStruct](Interfaces::CArray auto&& link) {
                linkStruct = link;
                return _continue;
            });
            Ensures(constants.Null == linkStruct.Index);
            auto linkAddress { Create<TLinkAddress>(storage) };
            linkStruct = { GetLink(storage, linkAddress) };
            Ensures(3 == linkStruct.size());
            Ensures(linkAddress == linkStruct.Index);
            Ensures(constants.Null == linkStruct.Source);
            Ensures(constants.Null == linkStruct.Target);
            Ensures(1 == Count<TLinkAddress>(storage));
            return linkAddress;
        }

        template<typename TLinkAddress>
        static TLinkAddress GetFirstLink(auto&& storage, TLinkAddress linkAddress)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue { constants.Continue };
            TLinkAddress linkAddressFromEach {};
            storage.Each(std::array{constants.Any, constants.Any, constants.Any}, [_continue, &linkAddressFromEach](Interfaces::CArray auto&& link) {
                linkAddressFromEach = link[0];
                return _continue;
            });
            Ensures(linkAddress == linkAddressFromEach);
            return linkAddressFromEach;
        }

        template<typename TLinkAddress>
        static TLinkAddress TestUpdateLinkToReferenceItself(auto&& storage, TLinkAddress linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link link { GetLink(storage, updatedLinkAddress) };
            Ensures(link.Index == link.Source);
            Ensures(link.Index == link.Target);
            return link.Index;
        }

        template<typename TLinkAddress>
        static TLinkAddress TestUpdateLinkToReferenceNull(auto&& storage, TLinkAddress linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link linkStruct { GetLink(storage, updatedLinkAddress) };
            Ensures(constants.Null == linkStruct.Source);
            Ensures(constants.Null == linkStruct.Target);
            return linkStruct.Index;
        }

        template<typename TLinkAddress>
        static void TestDelete(auto&& storage, TLinkAddress linkAddress)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue = constants.Continue;
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link linkStruct { GetLink(storage, updatedLinkAddress) };
            Data::Delete(storage, linkAddress);
            Ensures(0 == Count<TLinkAddress>(storage));
            TLinkAddress deletedLinkAddress;
            storage.Each(std::array{constants.Any, constants.Any, constants.Any}, [_continue, &deletedLinkAddress](Interfaces::CArray auto&& link) {
                deletedLinkAddress = link[0];
                return _continue;
            });
            Ensures(constants.Null == deletedLinkAddress);
        }
    };

    
    template<typename TLinkAddress>
    static void TestCrudOperations(auto&& storage)
    {
        const auto constants = storage.Constants;
        Ensures(0 == Platform::Data::Count<TLinkAddress>(storage));
        // Create link
        auto linkAddress { CrudOperationsTester::TestCreate<TLinkAddress>(storage) };
        // Get first link
        linkAddress = { CrudOperationsTester::GetFirstLink<TLinkAddress>(storage, linkAddress) };
        // Update link to reference itself
        linkAddress = { CrudOperationsTester::TestUpdateLinkToReferenceItself(storage, linkAddress) };
        // Update link to reference null (prepare for delete)
        linkAddress = { CrudOperationsTester::TestUpdateLinkToReferenceNull(storage, linkAddress) };
        // Delete link
        CrudOperationsTester::TestDelete(storage, linkAddress);
    }

    template<typename TLinkAddress>
    static void TestRawNumbersCrudOperations(auto&& storage)
    {
        // Constants
        const auto constants = storage.Constants;
        auto _continue = constants.Continue;
        auto any = constants.Any;
        Hybrid<TLinkAddress> h106E {106L, true};
        Hybrid<TLinkAddress> h107E {107L, true};
        Hybrid<TLinkAddress> h108E {108L, true};
        Ensures(106L == h106E.Value);
        Ensures(107L == h107E.Value);
        Ensures(108L == h108E.Value);
        // Create link (External -> External)
        auto linkAddress1 = Create<TLinkAddress>(storage);
        Update<TLinkAddress>(storage, linkAddress1, h106E.Value, h108E.Value);
        Link<TLinkAddress> link1 { GetLink(storage, linkAddress1) };
        Ensures(h106E.Value == link1.Source);
        Ensures(h108E.Value == link1.Target);
        // Create link (Internal -> External)
        auto linkAddress2 { Create<TLinkAddress>(storage) };
        Update(storage, linkAddress2, linkAddress1, h108E.Value);
        Link<TLinkAddress> link2 { GetLink(storage, linkAddress2) };
        Ensures(linkAddress1 == link2.Source);
        Ensures(h108E.Value == link2.Target);
        // Create link (Internal -> Internal)
        auto linkAddress3 { Create<TLinkAddress>(storage) };
        Update(storage,linkAddress3, linkAddress1, linkAddress2);
        Link<TLinkAddress> link3 { GetLink(storage, linkAddress3) };
        Ensures(linkAddress1 == link3.Source);
        Ensures(linkAddress1 == link3.Target);
        // Search for created link
        TLinkAddress searchedLinkAddress;
        storage.Each(std::array{linkAddress3, h106E.Value, h108E.Value}, [&searchedLinkAddress, _continue](Interfaces::CArray auto&& link) {
            searchedLinkAddress = link[0];
            return _continue;
        });
        Ensures(linkAddress1 == searchedLinkAddress);
        // Search for nonexistent link
        TLinkAddress searchedNonExistentLinkAddress;
        storage.Each(std::array{any, h106E.Value, h108E.Value}, [&searchedNonExistentLinkAddress, _continue](Interfaces::CArray auto&& link) {
            searchedNonExistentLinkAddress = link[0];
            return _continue;
        });
        Ensures(constants.Null == searchedNonExistentLinkAddress);
        // Update link to reference null (prepare for delete)
        auto updatedLinkAddress { Update(storage, linkAddress3, constants.Null, constants.Null) };
        Ensures(linkAddress3 == updatedLinkAddress);
        link3 = { GetLink(storage, linkAddress3) };
        Ensures(constants.Null == link3.Source);
        Ensures(constants.Null == link3.Target);
        // Delete link
        Data::Delete(storage, linkAddress3);
        Ensures(2 == Count<TLinkAddress>(storage));
        TLinkAddress deletedLinkAddress;
        storage.Each(std::array{any, any, any}, [&deletedLinkAddress, _continue](Interfaces::CArray auto&& link) {
            deletedLinkAddress = link[0];
            return _continue;
        });
        Ensures(linkAddress2 == deletedLinkAddress);
    }

    template<typename TLinkAddress>
    static void TestMultipleCreationsAndDeletions(auto&& storage, int numberOfOperations)
    {
        for (int i = 0; i < numberOfOperations; i++)
        {
            Create<TLinkAddress>(storage);
        }
        for (int i = 0; i < numberOfOperations; i++)
        {
            storage.Delete(Count<TLinkAddress>(storage));
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
                auto linksCount { Count<TLinkAddress>(storage) };
                auto createPoint { Random::NextBoolean(randomGen64) };
                if (linksCount >= 2 && createPoint)
                {
                    Ranges::Range<TLinkAddress> linksAddressRange { 1, linksCount };
                    TLinkAddress source { Random::NextUInt64(randomGen64, linksAddressRange) };
                    TLinkAddress target { Random::NextUInt64(randomGen64, linksAddressRange) }; //-V3086
                    auto resultLink { GetOrCreate(storage, source, target) };
                    if (resultLink > linksCount)
                    {
                        ++created;
                    }
                }
                else
                {
                    Create<TLinkAddress>(storage);
                    ++created;
                }
            }
            Ensures(Count<TLinkAddress>(storage) == created);
            for (auto i { 0 }; i < N; ++i)
            {
                TLinkAddress link = i + 1;
                if (Data::Exists(storage, link))
                {
                    Data::Delete(storage, link);
                    ++deleted;
                }
            }
            Ensures(0 == Count<TLinkAddress>(storage));
        }
    }
}

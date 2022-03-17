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
            storage.Each( Link{constants.Any, constants.Any, constants.Any} , [&linkStruct, _continue](CArray<TLinkAddress> auto&& link) {
                linkStruct = Link(link);
                return _continue;
            });
            Expects(constants.Null == linkStruct.Index);
            auto linkAddress { Create<TLinkAddress>(storage) };
            linkStruct = { GetLink(storage, linkAddress) };
            Expects(3 == linkStruct.size());
            Expects(linkAddress == linkStruct.Index);
            Expects(constants.Null == linkStruct.Source);
            Expects(constants.Null == linkStruct.Target);
            Expects(1 == Count<TLinkAddress>(storage));
            return linkAddress;
        }

        template<typename TLinkAddress>
        static TLinkAddress GetFirstLink(auto&& storage, TLinkAddress linkAddress)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue { constants.Continue };
            TLinkAddress linkAddressFromEach {};
            storage.Each(Link{constants.Any, constants.Any, constants.Any}, [_continue, &linkAddressFromEach](CArray<TLinkAddress> auto&& link) {
                linkAddressFromEach = link[0];
                return _continue;
            });
            Expects(linkAddress == linkAddressFromEach);
            return linkAddressFromEach;
        }

        template<typename TLinkAddress>
        static TLinkAddress TestUpdateLinkToReferenceItself(auto&& storage, TLinkAddress linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link link { GetLink(storage, updatedLinkAddress) };
            Expects(link.Index == link.Source);
            Expects(link.Index == link.Target);
            return link.Index;
        }

        template<typename TLinkAddress>
        static TLinkAddress TestUpdateLinkToReferenceNull(auto&& storage, TLinkAddress linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, constants.Null, constants.Null) };
            Link linkStruct { GetLink(storage, updatedLinkAddress) };
            Expects(constants.Null == linkStruct.Source);
            Expects(constants.Null == linkStruct.Target);
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
            Data::Delete<TLinkAddress>(storage, linkAddress);
            Expects(0 == Count<TLinkAddress>(storage));
            TLinkAddress deletedLinkAddress {};
            storage.Each(Link{constants.Any, constants.Any, constants.Any}, [_continue, &deletedLinkAddress](CArray<TLinkAddress> auto&& link) {
                deletedLinkAddress = link[0];
                return _continue;
            });
            Expects(constants.Null == deletedLinkAddress);
        }
    };

    
    template<typename TLinkAddress>
    static void TestCrudOperations(auto&& storage)
    {
        const auto constants = storage.Constants;
        Expects(0 == Platform::Data::Count<TLinkAddress>(storage));
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
        Expects(106L == h106E.AbsoluteValue());
        Expects(107L == h107E.AbsoluteValue());
        Expects(108L == h108E.AbsoluteValue());
        // Create link (External -> External)
        auto linkAddress1 = Create<TLinkAddress>(storage);
        Update<TLinkAddress>(storage, linkAddress1, h106E.Value, h108E.Value);
        Link<TLinkAddress> link1 { GetLink(storage, linkAddress1) };
        Expects(h106E.Value == link1.Source);
        Expects(h108E.Value == link1.Target);
        // Create link (Internal -> External)
        auto linkAddress2 { Create<TLinkAddress>(storage) };
        Update(storage, linkAddress2, linkAddress1, h108E.Value);
        Link<TLinkAddress> link2 { GetLink(storage, linkAddress2) };
        Expects(linkAddress1 == link2.Source);
        Expects(h108E.Value == link2.Target);
        // Create link (Internal -> Internal)
        auto linkAddress3 { Create<TLinkAddress>(storage) };
        Update(storage,linkAddress3, linkAddress1, linkAddress2);
        Link<TLinkAddress> link3 { GetLink(storage, linkAddress3) };
        Expects(linkAddress1 == link3.Source);
        Expects(linkAddress2 == link3.Target);
        // Search for created link
        TLinkAddress searchedLinkAddress {};
        storage.Each(Link{any, h106E.Value, h108E.Value}, [&searchedLinkAddress, _continue](CArray<TLinkAddress> auto&& link) {
            searchedLinkAddress = link[0];
            return _continue;
        });
        auto aaa = GetLink(storage, linkAddress1);
        Expects(linkAddress1 == searchedLinkAddress);
        // Search for nonexistent link
//        TLinkAddress searchedNonExistentLinkAddress {};
//        storage.Each(Link{any, h106E.Value, h108E.Value}, [&searchedNonExistentLinkAddress, _continue](CArray<TLinkAddress> auto&& link) {
//            searchedNonExistentLinkAddress = link[0];
//            return _continue;
//        });
//        Expects(constants.Null == searchedNonExistentLinkAddress);
        // Update link to reference null (prepare for delete)
        auto updatedLinkAddress { Update(storage, linkAddress3, constants.Null, constants.Null) };
        Expects(linkAddress3 == updatedLinkAddress);
        link3 = { GetLink(storage, linkAddress3) };
        Expects(constants.Null == link3.Source);
        Expects(constants.Null == link3.Target);
        // Delete link
        Data::Delete<TLinkAddress>(storage, linkAddress3);
        Expects(2 == Count<TLinkAddress>(storage));
        TLinkAddress deletedLinkAddress {};
        storage.Each(Link{any, any, any}, [&deletedLinkAddress, _continue](CArray<TLinkAddress> auto&& link) {
            deletedLinkAddress = link[0];
            return _continue;
        });
        Expects(linkAddress2 == deletedLinkAddress);
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
            std::cout << "\n\n\n N == " << N;
            std::srand(N);
            auto randomGen64 { RandomHelpers::Default };
            auto created { 0UL };
            auto deleted { 0UL };
            for (auto i { 0 }; i < N; ++i)
            {
                std::cout << "\n\n i == " << i;
                auto linksCount { Count<TLinkAddress>(storage) };
                auto createPoint { Random::NextBoolean(randomGen64) };
                if (linksCount >= 2 && createPoint)
                {
                    Ranges::Range<TLinkAddress> linksAddressRange { 1, linksCount };
                    auto source { Random::NextUInt64(randomGen64, linksAddressRange) };
                    auto target { Random::NextUInt64(randomGen64, linksAddressRange) }; //-V3086
                    std::cout << "linksAddressRange: " << linksAddressRange << std::endl;
                    std::cout << "GetOrCreate " << source << " to " << target << std::endl;
                    auto resultLink { GetOrCreate(storage, source, target) };
                    auto linksCountAfterGetOrCreate { Count<TLinkAddress>(storage) };
                    std::cout << "linksCountAfterGetOrCreate: " << linksCountAfterGetOrCreate << std::endl;
                    std::cout << "resultLink: " << resultLink << std::endl;
                    if (resultLink > linksCount)
                    {
                        std::cout << "resultLink > linksCount" << std::endl;
                        ++created;
                    }
                }
                else
                {
                    Create<TLinkAddress>(storage);
                    ++created;
                }
            }
//            auto allLinks { All<TLinkAddress>(storage)};

            Expects(Count<TLinkAddress>(storage) == created);
            for (auto i { 0 }; i < N; ++i)
            {
                TLinkAddress link = i + 1;
                if (Exists(storage, link))
                {
                    Data::Delete<TLinkAddress>(storage, link);
                    ++deleted;
                }
            }
            Expects(0 == Count<TLinkAddress>(storage));
        }
    }
}

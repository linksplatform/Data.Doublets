namespace Platform::Data::Doublets::Tests
{
    using namespace Platform::Data;
    using namespace Platform::Data::Doublets;
    class CrudOperationsTester
    {
    public:
        template<typename TStorage>
        static typename TStorage::LinkAddressType TestCreate(TStorage& storage)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue = constants.Continue;
            Link<typename TStorage::LinkAddressType> linkStruct;
            storage.Each(std::vector{constants.Any, constants.Any, constants.Any} , [&linkStruct, _continue](const typename TStorage::LinkType& link) {
                linkStruct = Link(link);
                return _continue;
            });
            Expects(constants.Null == linkStruct.Index);
            auto linkAddress { Create(storage) };
            linkStruct = { GetLink(storage, linkAddress) };
            Expects(3 == linkStruct.size());
            Expects(linkAddress == linkStruct.Index);
            Expects(constants.Null == linkStruct.Source);
            Expects(constants.Null == linkStruct.Target);
            Expects(1 == Count(storage));
            return linkAddress;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType GetFirstLink(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue { constants.Continue };
            typename TStorage::LinkAddressType linkAddressFromEach {};
            storage.Each(std::vector{constants.Any, constants.Any, constants.Any}, [_continue, &linkAddressFromEach](const typename TStorage::LinkType& link) {
                linkAddressFromEach = link[0];
                return _continue;
            });
            Expects(linkAddress == linkAddressFromEach);
            return linkAddressFromEach;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType TestUpdateLinkToReferenceItself(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link link { GetLink(storage, updatedLinkAddress) };
            Expects(link.Index == link.Source);
            Expects(link.Index == link.Target);
            return link.Index;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType TestUpdateLinkToReferenceNull(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            auto constants { storage.Constants };
            auto updatedLinkAddress { Update(storage, linkAddress, constants.Null, constants.Null) };
            Link linkStruct { GetLink(storage, updatedLinkAddress) };
            Expects(constants.Null == linkStruct.Source);
            Expects(constants.Null == linkStruct.Target);
            return linkStruct.Index;
        }

        template<typename TStorage>
        static void TestDelete(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            using namespace Platform::Interfaces;
            auto constants { storage.Constants };
            auto _continue = constants.Continue;
            auto updatedLinkAddress { Update(storage, linkAddress, linkAddress, linkAddress) };
            Link linkStruct { GetLink(storage, updatedLinkAddress) };
            Data::Delete(storage, linkAddress);
            Expects(0 == Count(storage));
            typename TStorage::LinkAddressType deletedLinkAddress {};
            storage.Each(std::vector{constants.Any, constants.Any, constants.Any}, [_continue, &deletedLinkAddress](const typename TStorage::LinkType& link) {
                deletedLinkAddress = link[0];
                return _continue;
            });
            Expects(constants.Null == deletedLinkAddress);
        }
    };

    
    template<typename TStorage>
    static void TestCrudOperations(TStorage& storage)
    {
        constexpr auto constants = storage.Constants;
        Expects(0 == Platform::Data::Count(storage));
        // Create link
        auto linkAddress { CrudOperationsTester::TestCreate(storage) };
        // Get first link
        linkAddress = { CrudOperationsTester::GetFirstLink(storage, linkAddress) };
        // Update link to reference itself
        linkAddress = { CrudOperationsTester::TestUpdateLinkToReferenceItself(storage, linkAddress) };
        // Update link to reference null (prepare for delete)
        linkAddress = { CrudOperationsTester::TestUpdateLinkToReferenceNull(storage, linkAddress) };
        // Delete link
        CrudOperationsTester::TestDelete(storage, linkAddress);
    }

    template<typename TStorage>
    static void TestRawNumbersCrudOperations(TStorage& storage)
    {
        // Constants
        constexpr auto constants = storage.Constants;
        auto _continue = constants.Continue;
        auto any = constants.Any;
        Hybrid<typename TStorage::LinkAddressType> h106E {106L, true};
        Hybrid<typename TStorage::LinkAddressType> h107E {107L, true};
        Hybrid<typename TStorage::LinkAddressType> h108E {108L, true};
        Expects(106L == h106E.AbsoluteValue());
        Expects(107L == h107E.AbsoluteValue());
        Expects(108L == h108E.AbsoluteValue());
        // Create link (External -> External)
        auto linkAddress1 = Create(storage);
        Update(storage, linkAddress1, h106E.Value, h108E.Value);
        Link<typename TStorage::LinkAddressType> link1 { GetLink(storage, linkAddress1) };
        Expects(h106E.Value == link1.Source);
        Expects(h108E.Value == link1.Target);
        // Create link (Internal -> External)
        auto linkAddress2 { Create(storage) };
        Update(storage, linkAddress2, linkAddress1, h108E.Value);
        Link<typename TStorage::LinkAddressType> link2 { GetLink(storage, linkAddress2) };
        Expects(linkAddress1 == link2.Source);
        Expects(h108E.Value == link2.Target);
        // Create link (Internal -> Internal)
        auto linkAddress3 { Create(storage) };
        Update(storage,linkAddress3, linkAddress1, linkAddress2);
        Link<typename TStorage::LinkAddressType> link3 { GetLink(storage, linkAddress3) };
        Expects(linkAddress1 == link3.Source);
        Expects(linkAddress2 == link3.Target);
        // Search for created link
        typename TStorage::LinkAddressType searchedLinkAddress {};
        storage.Each(std::vector{any, h106E.Value, h108E.Value}, [&searchedLinkAddress, _continue](const typename TStorage::LinkType& link) {
            searchedLinkAddress = link[0];
            return _continue;
        });
        auto aaa = GetLink(storage, linkAddress1);
        Expects(linkAddress1 == searchedLinkAddress);
        // Search for nonexistent link
//        typename TStorage::LinkAddressType searchedNonExistentypename TStorage::LinkAddressType {};
//        storage.Each(std::vector{any, h106E.Value, h108E.Value}, [&searchedNonExistentypename TStorage::LinkAddressType, _continue](const typename TStorage::LinkType& link) {
//            searchedNonExistentypename TStorage::LinkAddressType = link[0];
//            return _continue;
//        });
//        Expects(constants.Null == searchedNonExistentypename TStorage::LinkAddressType);
        // Update link to reference null (prepare for delete)
        auto updatedLinkAddress { Update(storage, linkAddress3, constants.Null, constants.Null) };
        Expects(linkAddress3 == updatedLinkAddress);
        link3 = { GetLink(storage, linkAddress3) };
        Expects(constants.Null == link3.Source);
        Expects(constants.Null == link3.Target);
        // Delete link
        Data::Delete(storage, linkAddress3);
        Expects(2 == Count(storage));
        typename TStorage::LinkAddressType deletedLinkAddress {};
        storage.Each(std::vector{any, any, any}, [&deletedLinkAddress, _continue](const typename TStorage::LinkType& link) {
            deletedLinkAddress = link[0];
            return _continue;
        });
        Expects(linkAddress2 == deletedLinkAddress);
    }

    template<typename TStorage>
    static void TestMultipleCreationsAndDeletions(TStorage& storage, int numberOfOperations)
    {
        for (int i = 0; i < numberOfOperations; i++)
        {
            Create(storage);
        }
        for (int i = 0; i < numberOfOperations; i++)
        {
            storage.Delete(Count(storage));
        }
    }

    template<typename TStorage>
    static void TestMultipleRandomCreationsAndDeletions(TStorage& storage, int maximumOperationsPerCycle)
    {
        using namespace Platform::Random;
        using namespace Platform::Data;
        for (auto N { 1 }; N < maximumOperationsPerCycle; ++N)
        {
            std::srand(N);
            auto& randomGen64 { RandomHelpers::Default };
            auto created { 0UL };
            auto deleted { 0UL };
            for (auto i { 0 }; i < N; ++i)
            {
                auto linksCount { Count(storage) };
                auto createPoint { Random::NextBoolean(randomGen64) };
                if (linksCount >= 2 && createPoint)
                {
                    Ranges::Range<typename TStorage::LinkAddressType> linksAddressRange { 1, linksCount };
                    auto source { Random::NextUInt64(randomGen64, linksAddressRange) };
                    auto target { Random::NextUInt64(randomGen64, linksAddressRange) }; //-V3086
                    auto resultLink { GetOrCreate(storage, source, target) };
                    auto linksCountAfterGetOrCreate { Count(storage) };
                    if (resultLink > linksCount)
                    {
                        ++created;
                    }
                }
                else
                {
                    Create(storage);
                    ++created;
                }
            }
            std::vector<typename TStorage::LinkType> allLinks { All(storage) };
            for(auto link : allLinks)
            {
            }

            Expects(Count(storage) == created);
            for (auto i { 0 }; i < N; ++i)
            {
                typename TStorage::LinkAddressType linkAddress = i + 1;
                if (Exists(storage, linkAddress))
                {

                    Delete(storage, linkAddress);
                    ++deleted;
                    allLinks = { All(storage)};
                    for(auto link : allLinks)
                    {
                    }
                }
            }
            Expects(0 == Count(storage));
        }
    }
}

namespace Platform::Data::Doublets
{
    template<typename TStorage>
    static void RunRandomCreations(TStorage& storage, typename TStorage::LinkAddressType amountOfCreations)
    {
        using namespace Platform::Data;
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        auto& randomGenerator64 = Random::RandomHelpers::Default;
        for (typename TStorage::LinkAddressType i { 0 }; i < amountOfCreations; ++i)
        {
            Range<typename TStorage::LinkAddressType> linksAddressRange { 0, Count(storage) };
            auto source = Random::NextUInt64(randomGenerator64, linksAddressRange);
            auto target = Random::NextUInt64(randomGenerator64, linksAddressRange);
            GetOrCreate(storage, source, target);
        }
    }

    template<typename TStorage>
    static void RunRandomSearches(const TStorage& storage, typename TStorage::LinkAddressType amountOfSearches)
    {
        using namespace Platform::Data;
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        auto randomGenerator64 = Random::RandomHelpers::Default;
        for (typename TStorage::LinkAddressType i { 0 }; i < amountOfSearches; ++i)
        {
            auto linksAddressRange = Range<typename TStorage::LinkAddressType>(0, Count(storage));
            auto source = Random::NextUInt64(randomGenerator64, linksAddressRange);
            auto target = Random::NextUInt64(randomGenerator64, linksAddressRange);
            SearchOrDefault(storage, source, target);
        }
    }

    template<typename TStorage>
    static void RunRandomDeletions(TStorage& storage, typename TStorage::LinkAddressType amountOfDeletions)
    {
        using namespace Platform::Data;
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        auto& randomGenerator64 = RandomHelpers::Default;
        auto linksCount = Count(storage);
        typename TStorage::LinkAddressType min = amountOfDeletions > linksCount ? 0 : linksCount - amountOfDeletions;
        for (typename TStorage::LinkAddressType i { 0 }; i < amountOfDeletions; ++i)
        {
            linksCount = Count(storage);
            if (linksCount <= min)
            {
                break;
            }
            auto linksAddressRange = Range<typename TStorage::LinkAddressType>(min, linksCount);
            auto linkAddress = Random::NextUInt64(randomGenerator64, linksAddressRange);
            Delete(storage, linkAddress);
        }
    }

    template<typename TStorage>
    void TestRandomCreationsAndDeletions(TStorage& storage, typename TStorage::LinkAddressType maximumOperationsPerCycle)
    {
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        using namespace Platform::Interfaces;
        for (auto N = 1; N < maximumOperationsPerCycle; N++)
        {
            auto& randomGen64 = RandomHelpers::Default;
            typename TStorage::LinkAddressType created = 0;
            typename TStorage::LinkAddressType deleted = 0;
            for (auto i = 0; i < N; i++)
            {
                auto linksCount = Count(storage);
                auto createPoint = Random::NextBoolean(randomGen64);
                if (linksCount >= 2 && createPoint)
                {
                    Ranges::Range linksAddressRange {1, linksCount};
                    auto source = Random::NextUInt64(randomGen64, linksAddressRange);
                    auto target = Random::NextUInt64(randomGen64, linksAddressRange); //-V3086
                    auto resultLink = GetOrCreate(storage, source, target);
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
            auto count = Count(storage);
            for (auto i = 0; i < count; i++)
            {
                auto link = i + 1;
                {
                    DIRECT_METHOD_CALL(TStorage, storage, Update,link, 0, 0);
                    Delete(storage, link);
                    ++deleted;
                }
            }
        }
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType Delete(TStorage& storage, typename TStorage::LinkAddressType linkToDelete, const typename TStorage::WriteHandlerType& handler)
    {
        if (Exists(storage, linkToDelete))
        {
            EnforceResetValues(storage, linkToDelete, handler);
        }
        return Delete(storage, linkToDelete, handler);
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType First(const TStorage& storage)
    {
        constexpr auto constants = storage.Constants;
        auto $break {constants.Break};
        typename TStorage::LinkAddressType firstLink = 0;
        Expects(constants.Null != firstLink);
        DIRECT_METHOD_CALL(TStorage, storage, Each,typename TStorage::LinkType{storage.Constants.Any, storage.Constants.Any, storage.Constants.Any}, [&firstLink, $break](const typename TStorage::LinkType& link){
            firstLink = link[0];
            return $break;
                                                                                                });
        Ensures(constants.Null != firstLink);
        return firstLink;
    }

    template<typename TStorage>
    static typename TStorage::HandlerParameterType SingleOrDefault(const TStorage& storage,  const typename TStorage::LinkType& query)
    {
        typename TStorage::LinkType result {};
        auto count = 0;
        constexpr auto constants = storage.Constants;
        auto linkHandler { [&result, &count, &constants] (const typename TStorage::LinkType& link) {
            if (count == 0)
            {
                result = link;
                ++count;
                return constants.Continue;
            }
            else
            {
                result = {};
                return constants.Break;
            }
        }};
        DIRECT_METHOD_CALL(TStorage, storage, Each,query, linkHandler);
        return result;
    }

    template<typename TStorage>
    static bool CheckPathExistence(const TStorage& storage, std::convertible_to<typename TStorage::LinkAddressType> auto ... pathPack)
    {
        typename TStorage::HandlerParameterType path{ static_cast<typename TStorage::LinkdAddressType>(pathPack)... };
        auto current = path[0];
        if (!Exists(storage, current))
        {
            return false;
        }
        constexpr auto constants = storage.Constants;
        for (typename TStorage::LinkAddressType i { 1 }; i < std::ranges::size(path); ++i)
        {
            auto next = path[i];
            auto values = GetLink(storage, current);
            auto source = GetSource(storage, values);
            auto target = GetTarget(storage, values);
            if (target == source &&  next == source)
            {
                return false;
            }
            if ((source != next) && (target != next))
            {
                return false;
            }
            current = next;
        }
        return true;
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetByKeys(TStorage& storage, typename TStorage::LinkAddressType root, std::convertible_to<typename TStorage::LinkAddressType> auto ... pathPack)
    {
        typename TStorage::HandlerParameterType path{ static_cast<typename TStorage::LinkdAddressType>(pathPack)... };
        storage.EnsureLinkExists(root, "root");
        auto currentLink = root;
        for (typename TStorage::LinkAddressType i { 0 }; i < std::ranges::size(path); ++i)
        {
            currentLink = GetLink(storage, currentLink)[path[i]];
        }
        return currentLink;
    }

    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType GetSquareMatrixSequenceElementByIndex(TStorage& storage, typename TStorage::LinkAddressType root, std::uint64_t size, std::uint64_t index)
    //    {
    //        using namespace Platform::Numbers;
    //        constexpr auto constants = storage.Constants;
    //        auto source = constants.SourcePart;
    //        auto target = constants.TargetPart;
    //        if (!Numbers::Math::IsPowerOfTwo(size))
    //        {
    //            throw std::invalid_argument("size", "Sequences with sizes other than powers of two are not supported.");
    //        }
    //        auto path = BitArray(BitConverter.GetBytes(index));
    //        auto length = Bit.GetLowestPosition(size);
    //        storage.EnsureLinkExists(root, "root");
    //        auto currentLink = root;
    //        for (auto i { length - 1 }; i >= 0; i--)
    //        {
    //            currentLink = GetLink(storage, currentLink)[path[i] ? target : source];
    //        }
    //        return currentLink;
    //    }

    template<typename TStorage>
    static auto GetIndex(const TStorage& storage,  const typename TStorage::LinkType& link) { return link[storage.Constants.IndexPart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetSource(const TStorage& storage, typename TStorage::LinkAddressType linkAddress) { return GetLink(storage, linkAddress)[storage.Constants.SourcePart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetSource(const TStorage& storage,  const typename TStorage::LinkType& link) { return link[storage.Constants.SourcePart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetTarget(const TStorage& storage, typename TStorage::LinkAddressType linkAddress) { return GetLink(storage, linkAddress)[storage.Constants.TargetPart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetTarget(const TStorage& storage,  const typename TStorage::LinkType& link) { return link[storage.Constants.TargetPart]; }

    template<typename TStorage>
    static std::vector<typename TStorage::LinkType> All(const TStorage& storage, const typename TStorage::LinkType& restriction)
    {
        using namespace Platform::Collections;
        auto $continue {storage.Constants.Continue};
        std::vector<typename TStorage::LinkType> allLinks {};
        DIRECT_METHOD_CALL(TStorage, storage, Each,restriction, [&allLinks, $continue](const typename TStorage::LinkType& link){
            allLinks.push_back(link);
            return $continue;
        });
        return allLinks;
    }

        template<typename TStorage>
        static auto All(const TStorage& storage, std::convertible_to<typename TStorage::LinkAddressType> auto ... restrictionPack)
        {
            typename TStorage::LinkType restriction { static_cast<typename TStorage::LinkAddressType>(restrictionPack)... };
            return All(storage, typename TStorage::LinkType{});
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType AllIndices(TStorage& storage,  const typename TStorage::LinkType& restriction)
        {
            std::vector<typename TStorage::LinkType> allIndices {};
            auto usages = std::vector<typename TStorage::LinkType>();
            auto $continue = storage.Constants.Continue;
            DIRECT_METHOD_CALL(TStorage, storage, Each,restriction, [&allIndices, $continue](const typename TStorage::LinkType& link){
                allIndices.push_back(link);
                return $continue;
            });
            return allIndices;
        }


        template<typename TStorage>
        static void EnsureLinkExists(TStorage& storage,  const typename TStorage::LinkType& restriction)
        {
            for (auto i { 0 }; i < restriction.Count(); ++i)
            {
                if (!storage.Exists(restriction[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        template<typename TStorage>
        static bool Exists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            return DIRECT_METHOD_CALL(TStorage, storage, Count,typename TStorage::LinkType{storage.Constants.Any, source, target}) > 0;
        }


        template<typename TStorage>
        static void EnsureInnerReferenceExists(TStorage& storage, typename TStorage::LinkAddressType reference, std::string argumentName)
        {
            using namespace Platform::Exceptions;
            if (storage.Constants.IsInternalReference(reference) && !storage.Exists(reference))
            {
                throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(reference, argumentName);
            }
        }

        template<typename TStorage>
        static void EnsureInnerReferenceExists(TStorage& storage,  const typename TStorage::LinkType& restriction, std::string argumentName)
        {
            using namespace Platform::Exceptions;
            for (std::int32_t i = 0; i < restriction.Count(); ++i)
            {
                storage.EnsureInnerReferenceExists(restriction[i], argumentName);
            }
        }

        template<typename TStorage>
        static void EnsureLinkIsAnyOrExists(TStorage& storage,  const typename TStorage::LinkType& restriction)
        {
            using namespace Platform::Exceptions;
            auto any = storage.Constants.Any;
            for (auto i { 0 }; i < restriction.Count(); ++i)
            {
                if ((any != restriction[i]) && !storage.Exists(restriction[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        template<typename TStorage>
        static void EnsureLinkIsAnyOrExists(TStorage& storage, typename TStorage::LinkAddressType linkAddress, std::string argumentName)
        {
            using namespace Platform::Exceptions;
            if ((storage.Constants.Any != linkAddress) && !storage.Exists(linkAddress))
            {
                throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(linkAddress, argumentName);
            }
        }

        template<typename TStorage>
        static void EnsureLinkIsItselfOrExists(TStorage& storage, typename TStorage::LinkAddressType linkAddress, std::string argumentName)
        {
            using namespace Platform::Exceptions;
            if ((storage.Constants.Itself != linkAddress) && !storage.Exists(linkAddress))
            {
                throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(linkAddress, argumentName);
            }
        }

        template<typename TStorage>
        static void EnsureDoesNotExists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            using namespace Platform::Exceptions;
            if (storage.Exists(source, target))
            {
                throw Platform::Exceptions::LinkWithSameValueAlreadyExistsException();
            }
        }

        template<typename TStorage>
        static void EnsureNoUsages(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            using namespace Platform::Exceptions;
            if (storage.HasUsages(linkAddress))
            {
                throw ArgumentLinkHasDependenciesException<typename TStorage::LinkAddressType>(linkAddress);
            }
        }

        template<typename TStorage>
        static void EnsureCreated(TStorage& storage,  const typename TStorage::LinkType& addresses)
        {
            storage.EnsureCreated(storage.Create, addresses);
        }

        template<typename TStorage>
        static void EnsurePointsCreated(TStorage& storage,  const typename TStorage::LinkType& addresses)
        {
            storage.EnsureCreated(storage.CreatePoint, addresses);
        }

        template<typename TStorage>
        static void EnsureCreated(TStorage& storage, std::function<typename TStorage::LinkAddressType()> creator,  const typename TStorage::LinkType& addresses)
        {
            std::unordered_set<typename TStorage::LinkAddressType> nonExistentLinkAddresses{};
            typename TStorage::LinkAddressType maxLinkAddress {};
            for(auto linkAddress : nonExistentLinkAddresses)
            {
                if(!storage.Exists(linkAddress))
                {
                    nonExistentLinkAddresses.insert(linkAddress);
                    if(linkAddress > maxLinkAddress)
                    {
                        maxLinkAddress = linkAddress;
                    }
                }
            }
            if (!nonExistentLinkAddresses.empty())
            {
                maxLinkAddress = std::min(maxLinkAddress, storage.Constants.InternalReferencesRange.Maximum);
                std::vector<typename TStorage::LinkType> createdLinks {};
                typename TStorage::LinkAddressType createdLink = creator();
                while (maxLinkAddress != createdLink)
                {
                    createdLinks.Add(createdLink);
                }
                for (auto i { 0 }; i < createdLinks.size(); ++i)
                {
                    if (!nonExistentLinkAddresses.contains(createdLinks[i]))
                    {
                        Delete(storage, createdLinks[i]);
                    }
                }
            }
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType CountUsages(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            constexpr auto constants = storage.Constants;
            auto values = GetLink(storage, linkAddress);
            auto usagesAsSource = DIRECT_METHOD_CALL(TStorage, storage, Count,typename TStorage::LinkType{constants.Any, storage, linkAddress, constants.Any});
            if (linkAddress == GetSource(storage, values))
            {
                --usagesAsSource;
            }
            auto usagesAsTarget = DIRECT_METHOD_CALL(TStorage, storage, Count,typename TStorage::LinkType{constants.Any, storage, constants.Any, linkAddress});
            if (linkAddress == GetTarget(storage, values))
            {
                --usagesAsTarget;
            }
            return usagesAsSource + usagesAsTarget;
        }

        template<typename TStorage>
        static bool HasUsages(TStorage& storage, typename TStorage::LinkAddressType linkAddress) { return CountUsages(storage,linkAddress)>0; }

        template<typename TStorage>
        typename TStorage::LinkAddressType SearchOrDefault(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            constexpr auto constants = storage.Constants;
            auto _break = constants.Break;
            typename TStorage::LinkAddressType searchedLinkAddress {};
            DIRECT_METHOD_CALL(TStorage, storage, Each,typename TStorage::LinkType{storage.Constants.Any, source, target}, [&searchedLinkAddress, _break] (const typename TStorage::LinkType& link) {
                searchedLinkAddress = link[0];
                return _break;
            });
            return searchedLinkAddress;
        }
        template<typename TStorage>
        auto CreatePoint(TStorage& storage)
        {
            auto point = Platform::Data::Create(storage);
            return Update(storage, point, point, point);
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType CreatePoint(TStorage& storage, const typename TStorage::WriteHandlerType& handler)
        {
            constexpr auto constants = storage.Constants;
            WriteHandlerState<TStorage> handlerState {constants.Continue, constants.Break, handler};
            typename TStorage::LinkAddressType linkAddress;
            typename TStorage::LinkAddressType HandlerWrapper = [&linkAddress, &handlerState](const typename TStorage::LinkType& before, const typename TStorage::LinkType& after){
                linkAddress = after[0];
                return handlerState.Handle(before, after);
            };
            handlerState.Apply(DIRECT_METHOD_CALL(TStorage, storage, Create,typename TStorage::LinkType{}, HandlerWrapper));
            return handlerState.Apply(DIRECT_METHOD_CALL(TStorage, storage, Update,linkAddress, linkAddress, linkAddress, HandlerWrapper));
        }

        template<typename TStorage>
        typename TStorage::LinkAddressType CreateAndUpdate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            auto $continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType linkAddress;
            return CreateAndUpdate(storage, source, target, [&linkAddress, $continue](const typename TStorage::LinkType& before, const typename TStorage::LinkType& after){
                linkAddress = after[0];
                return $continue;
            });
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType CreateAndUpdate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, const typename TStorage::WriteHandlerType& handler)
        {
            constexpr auto constants = storage.Constants;
            typename TStorage::LinkAddressType createdLinkAddress;
            WriteHandlerState<TStorage> handlerState {constants.Continue, constants.Break, handler};
            handlerState.Apply(DIRECT_METHOD_CALL(TStorage, storage, Create,typename TStorage::LinkType{}, [&createdLinkAddress, &handlerState](const typename TStorage::LinkType& before, const typename TStorage::LinkType& after){
                createdLinkAddress = after[0];
                return handlerState.Handle(before, after);
            }));
            return handlerState.Apply(Update(storage, createdLinkAddress, source, target, handlerState.Handler));
        }

        template<typename TStorage>
        typename TStorage::LinkAddressType Update(TStorage& storage, CArray<typename TStorage::LinkAddressType> auto&& restriction, CArray<typename TStorage::LinkAddressType> auto&& substitution)
        {
            auto $continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType updatedLinkAddress;
            DIRECT_METHOD_CALL(TStorage, storage, Update,restriction, substitution, [&updatedLinkAddress, $continue] (const typename TStorage::LinkType& before, const typename TStorage::LinkType& after) {
                updatedLinkAddress = after[0];
                return $continue;
            });
            return updatedLinkAddress;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage,  const typename TStorage::LinkType& restriction, const typename TStorage::WriteHandlerType& handler)
        {
            const auto length { std::ranges::size(restriction) };
            if (length == 2)
            {
                return MergeAndDelete(storage, restriction[0], restriction[1], handler);
            }
            else if (length == 4)
            {
                return UpdateOrCreateOrGet(storage, restriction[0], restriction[1], restriction[2], restriction[3], handler);
            }
            else
            {
                return Update(storage, restriction[0], restriction[1], restriction[2], handler);
            }
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, typename TStorage::LinkAddressType linkAddress, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget, const typename TStorage::WriteHandlerType& handler)
        {
          return DIRECT_METHOD_CALL(TStorage, storage, Update, typename TStorage::LinkType{linkAddress}, typename TStorage::LinkType{linkAddress, newSource, newTarget}, handler);
//          return std::is_abstract<TStorage>::value ? DIRECT_METHOD_CALL(TStorage, storage, Update,typename TStorage::LinkType{linkAddress}, typename TStorage::LinkType{linkAddress, newSource, newTarget}, handler) : storage.TStorage::Update(typename TStorage::LinkType{linkAddress}, typename TStorage::LinkType{linkAddress, newSource, newTarget}, handler);
//          if constexpr (std::is_abstract<TStorage>::value) {
//            return DIRECT_METHOD_CALL(TStorage, storage, Update,typename TStorage::LinkType{linkAddress}, typename TStorage::LinkType{linkAddress, newSource, newTarget}, handler);
//          } else {
//            return storage.TStorage::Update(typename TStorage::LinkType{linkAddress}, typename TStorage::LinkType{linkAddress, newSource, newTarget}, handler);
//          }
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, typename TStorage::LinkAddressType linkAddress, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget)
        {
            auto $continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType updatedLinkAddress;
            Update(storage, linkAddress, newSource, newTarget, [&updatedLinkAddress, $continue] (const typename TStorage::LinkType& before, const typename TStorage::LinkType& after) {
                updatedLinkAddress = after[0];
                return $continue;
            });
            return updatedLinkAddress;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage,  const typename TStorage::LinkType& restriction)
        {
            auto $continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType updatedLinkAddress;
            return Update(storage, restriction, [&updatedLinkAddress, $continue](const typename TStorage::LinkType& before, const typename TStorage::LinkType& after){
                updatedLinkAddress = after;
                return $continue;
            });
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, const typename TStorage::WriteHandlerType& handler, std::convertible_to<typename TStorage::LinkAddressType> auto ... restrictionPack)
        {
            typename TStorage::HandlerParameterType restriction{
                static_cast<typename TStorage::HandlerParameterType>(restrictionPack)... };
            return Update(storage, restriction, handler);
        }
        //
        template<typename TStorage>
        static typename TStorage::LinkType ResolveConstantAsSelfReference(const TStorage& storage, typename TStorage::LinkAddressType constant,  const typename TStorage::LinkType& restriction,  const typename TStorage::LinkType& substitution)
        {
            constexpr auto constants = storage.Constants;
            auto restrictionIndex = GetIndex(storage, restriction);
            auto substitutionIndex = GetIndex(storage, substitution);
            if ( constants.Null == substitutionIndex)
            {
                substitutionIndex = restrictionIndex;
            }
            auto source = GetSource(storage, substitution);
            auto target = GetTarget(storage, substitution);
            source = constant == source ? substitutionIndex : source;
            target = constant == target ? substitutionIndex : target;
            return typename TStorage::LinkType{substitutionIndex, source, target};
        }

        template<typename TStorage>
        typename TStorage::LinkAddressType GetOrCreate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            constexpr auto constants = storage.Constants;
            auto link = SearchOrDefault(storage, source, target);
            if (link == constants.Null)
            {
                link = CreateAndUpdate(storage, source, target);
            }
            return link;
        }

    template<typename TStorage>
    static typename TStorage::LinkAddressType UpdateOrCreateOrGet(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget, const typename TStorage::WriteHandlerType& handler)
    {
        auto link = SearchOrDefault(storage, source, target);
        if (storage.Constants.Null == link)
        {
            return CreateAndUpdate(storage, newSource, newTarget, handler);
        }
        if ((source == newSource) && (target == newTarget))
        {
            return link;
        }
        return Update(storage, link, newSource, newTarget, handler);
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType UpdateOrCreateOrGet(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget)
    {
        auto $continue = storage.Constants.Continue;
        typename TStorage::LinkAddressType resultLink;
        UpdateOrCreateOrGet(storage, source, target, newSource, newTarget, [&resultLink, $continue](const typename TStorage::LinkType& before, const typename TStorage::LinkType& after) {
            resultLink = after[0];
            return $continue;
        });
        return resultLink;
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType DeleteIfExists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    {
        constexpr auto constants = storage.Constants;
        auto linkAddress = storage.SearchOrDefault(source, target);
        if (linkAddress != constants.Null)
        {
            return Delete(storage, linkAddress);
        }
        return constants.Null;
    }

    template<typename TStorage>
    static void DeleteMany(TStorage& storage,  const typename TStorage::LinkType& linkAddressesToDelete)
    {
        for (auto linkAddress : linkAddressesToDelete) {
            Delete(storage, linkAddress);
        }
    }


    template<typename TStorage>
    static typename TStorage::LinkAddressType DeleteAllUsages(TStorage& storage, typename TStorage::LinkAddressType linkIndex, const typename TStorage::WriteHandlerType& handler)
    {
        constexpr auto constants = storage.Constants;
        auto any = constants.Any;
        auto $continue = storage.Constants.Continue;
        auto $break = storage.Constants.Break;
        auto usagesAsSourceQuery = typename TStorage::LinkType{any, linkIndex, any};
        auto usagesAsTargetQuery = typename TStorage::LinkType{any, any, linkIndex};
        auto usages = std::vector<typename TStorage::LinkType>();
        DIRECT_METHOD_CALL(TStorage, storage, Each,usagesAsSourceQuery, [&usages, $continue](const typename TStorage::LinkType& link) {
            usages.push_back(link);
            return $continue;
        });
        DIRECT_METHOD_CALL(TStorage, storage, Each,usagesAsTargetQuery, [&usages, $continue](auto&& link) {
            usages.push_back(link);
            return $continue;
        });
        WriteHandlerState<TStorage> handlerState {$continue, $break, handler};
        for (auto usage : usages)
        {
            auto usageAddress {GetIndex(storage, usage)};
            if (linkIndex == usageAddress || Exists(storage, usageAddress))
            {
                continue;
            }
            auto result = DIRECT_METHOD_CALL(TStorage, storage, Delete,usage, handlerState.Handler);
            handlerState.Apply(result);
        }
        return handlerState.Result;
    }

    template<typename TStorage>
    static void DeleteAllUsages(TStorage& storage, typename TStorage::LinkAddressType linkIndex)
    {
        DeleteAllUsages(storage, linkIndex, nullptr);
    }

    // Do not translate this
//    template<typename TStorage>
//    auto DeleteByQuery(TStorage& storage,  CArray<typename TStorage::LinkAddressType> auto&& query)
//    {
//        auto count = DIRECT_METHOD_CALL(TStorage, storage, Count,query);
//        auto toDelete = typename TStorage::LinkType(count);
//        DIRECT_METHOD_CALL(TStorage, storage, Each,[&](auto link) {
//            toDelete.push_back(link[0]);
//            return storage.Constants.Continue;
//        }, query);
//
//        for (auto link : toDelete | std::views::reverse)
//        {
//            Delete(storage, link);
//        }
//    }

    template<typename TStorage>
    auto DeleteAll(TStorage& storage)
    {
        auto handler = [](typename TStorage::LinkType before, typename TStorage::LinkType substitution) {
            return typename TStorage::LinkAddressType {};
        };
        auto any = storage.Constants.Any;
        for (auto count = Count(storage); count != storage.Constants.Null; count = Count(storage))
        {
            DIRECT_METHOD_CALL(TStorage, storage, Delete, {count, any, any}, handler);
        }
    }

    template<typename TStorage>
    static bool AreValuesReset(const TStorage& storage, typename TStorage::LinkAddressType linkIndex)
    {
        auto nullConstant = storage.Constants.Null;
        auto link = GetLink(storage, linkIndex);
        for (typename TStorage::LinkAddressType i = typename TStorage::LinkAddressType{1}; i < std::ranges::size(link); ++i)
        {
            if ( nullConstant != link[i])
            {
                return false;
            }
        }
        return true;
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType ResetValues(TStorage& storage, typename TStorage::LinkAddressType linkAddress, const typename TStorage::WriteHandlerType& handler)
    {
        return Update(storage, linkAddress, 0, 0, handler);
    }

    template<typename TStorage>
    void ResetValues(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
    {
        auto $break = storage.Constants.Break;
        ResetValues(storage, linkAddress, nullptr);
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType EnforceResetValues(TStorage& storage, typename TStorage::LinkAddressType linkIndex, const typename TStorage::WriteHandlerType& handler)
    {
        if (!AreValuesReset(storage, linkIndex))
        {
            return ResetValues(storage, linkIndex, handler);
        }
        return storage.Constants.Continue;
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType EnforceResetValues(TStorage& storage, typename TStorage::LinkAddressType linkIndex)
    {
         return EnforceResetValues(storage, linkIndex, nullptr);
    }

        template<typename TStorage>
        static typename TStorage::LinkAddressType MergeUsages(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex, const typename TStorage::WriteHandlerType& handler)
        {
            if (newLinkIndex == oldLinkIndex)
            {
                return newLinkIndex;
            }
            constexpr auto constants = storage.Constants;
            auto usagesAsSource = All(storage, typename TStorage::LinkType(constants.Any, oldLinkIndex, constants.Any));
            WriteHandlerState<TStorage> handlerState {constants.Continue, constants.Break, handler};
            for (auto i { 0 }; i < std::ranges::size(usagesAsSource); ++i)
            {
                auto usageAsSource = usagesAsSource[i];
                if ( oldLinkIndex == GetIndex(storage, usageAsSource))
                {
                    continue;
                }
                typename TStorage::LinkType restriction {GetIndex(storage, usageAsSource)};
                typename TStorage::LinkType substitution = (newLinkIndex, GetTarget(storage, usageAsSource));
                handlerState.Apply(DIRECT_METHOD_CALL(TStorage, storage, Update,restriction, substitution, handlerState.Handler));
            }
            auto usagesAsTarget = All(storage, typename TStorage::LinkType(constants.Any, constants.Any, oldLinkIndex));
            for (auto i { 0 }; i < std::ranges::size(usagesAsTarget); ++i)
            {
                auto usageAsTarget = usagesAsTarget[i];
                if ( oldLinkIndex == GetIndex(storage, usageAsTarget))
                {
                    continue;
                }
                auto substitution = typename TStorage::LinkType(GetTarget(storage, usageAsTarget), newLinkIndex);
                handlerState.Apply(DIRECT_METHOD_CALL(TStorage, storage, Update,usageAsTarget, substitution, handlerState.Handler));
            }
            return handlerState.Result;
        }

        template<typename TStorage>
        static void MergeUsages(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex)
        {
            MergeUsages(storage, oldLinkIndex, newLinkIndex, nullptr);
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType MergeAndDelete(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex)
        {
            if ( newLinkIndex != oldLinkIndex)
            {
                MergeUsages(storage, oldLinkIndex, newLinkIndex);
                Delete(storage, oldLinkIndex);
            }
            return newLinkIndex;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType MergeAndDelete(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex, const typename TStorage::WriteHandlerType& handler)
        {
            constexpr auto constants = storage.Constants;
            WriteHandlerState<TStorage> handlerState {constants.Continue, constants.Break, handler};
            if ( newLinkIndex != oldLinkIndex)
            {
                handlerState.Apply(MergeUsages(storage, oldLinkIndex, newLinkIndex, handlerState.Handler));
                handlerState.Apply(Delete(storage, oldLinkIndex, handlerState.Handler));
            }
            return handlerState.Result;
        }

        template<typename TStorage>
        static std::string Format(TStorage& storage,  const typename TStorage::LinkType& link)
        {
            constexpr auto constants = storage.Constants;
            return std::string{}
                .append("(")
                .append(std::to_string(GetIndex(storage, link)))
                .append("): ")
                .append("(")
                .append(std::to_string(GetSource(storage, link)))
                .append(")")
                .append(" -> ")
                .append("(")
                .append(std::to_string(GetTarget(storage,link)))
                .append(")");
        }

        template<typename TStorage>
        std::string Format(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
        {
            return Format(GetLink(storage, linkAddress));
        }

    template<typename TStorage>
    typename TStorage::LinkAddressType HasUsages(TStorage& storage, typename TStorage::LinkAddressType linkAddress)
    {
        return storage.CountUsages(linkAddress) != 0;
    }



    template<typename TStorage>
    static void DeleteByQuery(TStorage& storage, typename TStorage::LinkType query)
    {
        typename TStorage::LinkType queryResult;
        DIRECT_METHOD_CALL(TStorage, storage, Each,query, [&](typename TStorage::LinkType vec) -> typename TStorage::LinkAddressType{
            return storage.Constants.Continue;
        });

        for (auto link: queryResult)
        {
            Delete(storage, link);
        }
    }



}

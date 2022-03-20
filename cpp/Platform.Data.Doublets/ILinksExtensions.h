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
                    Ranges::Range linksAddressRange {1, linksCount}
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
                    storage.Update(link, 0, 0);
                    Delete(storage, link);
                    ++deleted;
                }
            }
        }
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType Delete(TStorage& storage, typename TStorage::LinkAddressType linkToDelete, auto&& handler)
    {
        if (Exists(storage, linkToDelete))
        {
            EnforceResetValues(storage, linkToDelete, handler);
        }
        return .Delete(storage, linkToDelete, handler);
    }

    template<typename TStorage>
    static void DeleteAll(TStorage& storage)
    {
        for (auto i { Count(storage) }; i > typename TStorage::LinkAddressType{0}; --i)
        {
            Delete(storage, i);
            if (i - 1 != Count(storage))
            {
                i = Count(storage);
            }
        }
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType First(const TStorage& storage)
    {
        auto constants = storage.Constants;
        typename TStorage::LinkAddressType firstLink = 0;
        Expects(typename TStorage::LinkAddressType{0} != firstLink);
        storage.Each(Link{storage.Constants.Any, storage.Constants.Any, storage.Constants.Any}, setter.SetFirstAndReturnFalse[&firstLink, $break](typename TStorage::HandlerParameterType link){
            firstLink = link[0];
            return $break;
                                                                                                });
        Ensures(typename TStorage::LinkAddressType{0} != firstLink);
        return firstLink;
    }

    template<typename TStorage>
    static typename TStorage::HandlerParameterType SingleOrDefault(const TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& query)
    {
        std::vector<typename TStorage::LinkAddressType> result {};
        auto count = 0;
        auto constants = storage.Constants;
        auto linkHandler { [&result, &count, &constants] (typename TStorage::HandlerParameterType link) {
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
        storage.Each(query, linkHandler);
        return result;
    }

    template<typename TStorage>
    static bool CheckPathExistence(const TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> std::convertible_to<typename TStorage::LinkAddressType> auto path)
    {
        typename TStorage::HandlerParameterType pathContainer { static_cast<typename TStorage::LinkdAddressType>(path)... };
        auto current = pathContainer[0];
        if (!Exists(storage, current))
        {
            return false;
        }
        auto constants = storage.Constants;
        for (typename TStorage::LinkAddressType i { 1 }; i < std::ranges::size(pathContainer); ++i)
        {
            auto next = pathContainer[i];
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
    static typename TStorage::LinkAddressType GetByKeys(TStorage& storage, typename TStorage::LinkAddressType root, std::convertible_to<typename TStorage::LinkAddressType> auto path)
    {
        typename TStorage::HandlerParameterType pathContainer { static_cast<typename TStorage::LinkdAddressType>(path)... };
        storage.EnsureLinkExists(root, "root");
        auto currentLink = root;
        for (typename TStorage::LinkAddressType i { 0 }; i < std::ranges::size(pathContainer); ++i)
        {
            currentLink = GetLink(storage, currentLink)[pathContainer[i]];
        }
        return currentLink;
    }

    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType GetSquareMatrixSequenceElementByIndex(TStorage& storage, typename TStorage::LinkAddressType root, std::uint64_t size, std::uint64_t index)
    //    {
    //        using namespace Platform::Numbers;
    //        auto constants = storage.Constants;
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
    static auto GetIndex(const TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& link) { return link[storage.Constants.IndexPart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetSource(const TStorage& storage, typename TStorage::LinkAddressType link) { return GetLink(storage, link)[storage.Constants.SourcePart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetSource(const TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& link) { return link[storage.Constants.SourcePart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetTarget(const TStorage& storage, typename TStorage::LinkAddressType link) { return GetLink(storage, link)[storage.Constants.TargetPart]; }

    template<typename TStorage>
    static typename TStorage::LinkAddressType GetTarget(const TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& link) { return link[storage.Constants.TargetPart]; }

        template<typename TStorage>
        static auto All(const TStorage& storage, std::convertible_to<typename TStorage::LinkAddressType> auto ...restriction)
        {
            using namespace Platform::Collections;
            typename TStorage::HandlerParameterType restrictionContainer { static_cast<typename TStorage::LinkAddressType>(restriction)... };
            auto $continue {storage.Constants.Continue};
            auto allLinks = std::vector<typename TStorage::HanlderParameterType>();
            storage.Each(restrictionArray, [&allLinks, $continue](typename TStorage::HandlerParameterType link){
                allLinks.push_back(link);
                return $continue;
            });
            return allLinks;
        }
    //
    //    static Interfaces::CArray<typename TStorage::LinkAddressType>auto AllIndices<typename TStorage::LinkAddressType>(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction)
    //    {
    //        auto allIndices = List<typename TStorage::LinkAddressType>();
    //        auto filler = ListFiller<typename TStorage::LinkAddressType, typename TStorage::LinkAddressType>(allIndices, storage.Constants.Continue);
    //        storage.Each(filler.AddFirstAndReturnConstant, restriction);
    //        return allIndices;
    //    }
    //
    //
    //    template<typename TStorage>
    //    static void EnsureLinkExists(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction)
    //    {
    //        for (auto i { 0 }; i < restriction.Count(); ++i)
    //        {
    //            if (!storage.Exists(restriction[i]))
    //            {
    //                throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
    //            }
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureInnerReferenceExists(TStorage& storage, typename TStorage::LinkAddressType reference, std::string argumentName)
    //    {
    //        if (storage.Constants.IsInternalReference(reference) && !storage.Exists(reference))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(reference, argumentName);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureInnerReferenceExists(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction, std::string argumentName)
    //    {
    //        for (std::int32_t i = 0; i < restriction.Count(); ++i)
    //        {
    //            storage.EnsureInnerReferenceExists(restriction[i], argumentName);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureLinkIsAnyOrExists(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction)
    //    {
    //        auto any = storage.Constants.Any;
    //        for (auto i { 0 }; i < restriction.Count(); ++i)
    //        {
    //            if ((any != restriction[i]) && !storage.Exists(restriction[i]))
    //            {
    //                throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
    //            }
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureLinkIsAnyOrExists(TStorage& storage, typename TStorage::LinkAddressType link, std::string argumentName)
    //    {
    //        if ((storage.Constants.Any != link) && !storage.Exists(link))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(link, argumentName);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureLinkIsItselfOrExists(TStorage& storage, typename TStorage::LinkAddressType link, std::string argumentName)
    //    {
    //        if ((storage.Constants.Itself != link) && !storage.Exists(link))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<typename TStorage::LinkAddressType>(link, argumentName);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureDoesNotExists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    //    {
    //        if (storage.Exists(source, target))
    //        {
    //            throw LinkWithSameValueAlreadyExistsException();
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureNoUsages(TStorage& storage, typename TStorage::LinkAddressType link)
    //    {
    //        if (storage.HasUsages(link))
    //        {
    //            throw ArgumentLinkHasDependenciesException<typename TStorage::LinkAddressType>(link);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void EnsureCreated(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& addresses) { storage.EnsureCreated(storage.Create, addresses); }
    //
    //    template<typename TStorage>
    //    static void EnsurePointsCreated(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& addresses) { storage.EnsureCreated(storage.CreatePoint, addresses); }
    //
    //    template<typename TStorage>
    //    static void EnsureCreated(TStorage& storage, std::function<typename TStorage::LinkAddressType()> creator, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& addresses)
    //    {
    //        auto nonExistentAddresses = HashSet<typename TStorage::LinkAddressType>(addresses.Where(x => !storage.Exists(x)));
    //        if (nonExistentAddresses.Count() > 0)
    //        {
    //            auto max = nonExistentAddresses.Max();
    //            max = System::Math::Min(max), storage.Constants.InternalReferencesRange.Maximum
    //            auto createdLinks = List<typename TStorage::LinkAddressType>();
    //            typename TStorage::LinkAddressType createdLink = creator();
    //            while ( max != createdLink)
    //            {
    //                createdLinks.Add(createdLink);
    //            }
    //            for (auto i { 0 }; i < createdLinks.Count(); ++i)
    //            {
    //                if (!nonExistentAddresses.Contains(createdLinks[i]))
    //                {
    //                    storage.Delete(createdLinks[i]);
    //                }
    //            }
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType CountUsages(TStorage& storage, typename TStorage::LinkAddressType link)
    //    {
    //        auto constants = storage.Constants;
    //        auto values = GetLink(storage, link);
    //        typename TStorage::LinkAddressType usagesAsSource = Count(storage)(Link<typename TStorage::LinkAddressType>(constants.Any, link, constants.Any));
    //        if ( link == GetSource(storage, values))
    //        {
    //            usagesAsSource = usagesAsSource - 1;
    //        }
    //        typename TStorage::LinkAddressType usagesAsTarget = Count(storage)(Link<typename TStorage::LinkAddressType>(constants.Any, constants.Any, link));
    //        if ( link == GetTarget(storage, values))
    //        {
    //            usagesAsTarget = usagesAsTarget - 1;
    //        }
    //        return usagesAsSource + usagesAsTarget;
    //    }
    //
    //    template<typename TStorage>
    //    static bool HasUsages(TStorage& storage, typename TStorage::LinkAddressType link) { return Comparer<typename TStorage::LinkAddressType>.Default.Compare(Count(storage)Usages(link), 0) > 0; }
    //
    //    template<typename TStorage>
    //    static bool operator ==(TStorage& storage, typename TStorage::LinkAddressType link, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType &target) const
    //    {
    //        auto constants = storage.Constants;
    //        auto values = GetLink(storage, link);
    //        return  source == GetSource(storage, values) &&  target == GetTarget(storage, values);
    //    }
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType SearchOrDefault(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    //    {
    //        auto contants = storage.Constants;
    //        auto setter = Setter<typename TStorage::LinkAddressType, typename TStorage::LinkAddressType>(contants.Continue, contants.Break, 0);
    //        storage.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
    //        return setter.Result;
    //    }
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType CreatePoint(TStorage& storage)
    //    {
    //        auto constants = storage.Constants;
    //        auto setter = Setter<typename TStorage::LinkAddressType, typename TStorage::LinkAddressType>(constants.Continue, constants.Break);
    //        storage.CreatePoint(setter.SetFirstFromSecondListAndReturnTrue);
    //        return setter.Result;
    //    }
    //
    //    template<typename typename TStorage::LinkAddressType, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CArray<typename TStorage::LinkAddressType> auto, Interfaces::CArray<typename TStorage::LinkAddressType> auto>
    //    static typename TStorage::LinkAddressType CreatePoint(TStorage& storage, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        WriteHandlerState<typename TStorage::LinkAddressType> handlerState = new(constants.Continue, constants.Break, handler);
    //        typename TStorage::LinkAddressType link = 0;
    //        typename TStorage::LinkAddressType HandlerWrapper = [&]()Address {
    //            link = storage.GetIndex(after);
    //            return handlerState.Handle(before, after);;
    //        };
    //        handlerState.Apply(storage.Create({}, HandlerWrapper));
    //        handlerState.Apply(storage.Update(link, link, link, HandlerWrapper));
    //        return handlerState.Result;
    //    }
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType CreateAndUpdate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    //    {
    //        auto constants = storage.Constants;
    //        auto setter = Setter<typename TStorage::LinkAddressType, typename TStorage::LinkAddressType>(constants.Continue, constants.Break);
    //        storage.CreateAndUpdate(source, target, setter.SetFirstFromSecondListAndReturnTrue);
    //        return setter.Result;
    //    }
    //
    //    template<typename typename TStorage::LinkAddressType, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CArray<typename TStorage::LinkAddressType> auto, Interfaces::CArray<typename TStorage::LinkAddressType> auto>
    //    static typename TStorage::LinkAddressType CreateAndUpdate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        typename TStorage::LinkAddressType createdLink = 0;
    //        WriteHandlerState<typename TStorage::LinkAddressType> handlerState = new(constants.Continue, constants.Break, handler);
    //        handlerState.Apply(storage.Create({}, (before, after) =>
    //        {
    //            createdLink = storage.GetIndex(after);
    //            return handlerState.Handle(before, after);;
    //        }));
    //        handlerState.Apply(storage.Update(createdLink, source, target, handler));
    //        return handlerState.Result;
    //    }
    //
        template<typename TStorage>
        typename TStorage::LinkAddressType Update(TStorage& storage, CArray<typename TStorage::LinkAddressType> auto&& restriction, CArray<typename TStorage::LinkAddressType> auto&& substitution)
        {
            auto _continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType updatedLinkAddress;
            storage.Update(restriction, substitution, [&updatedLinkAddress, _continue] (typename TStorage::HandlerParameterType before, typename TStorage::HandlerParameterType after) {
                updatedLinkAddress = after[0];
                return _continue;
            });
            return updatedLinkAddress;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction, auto&& handler)
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
        static typename TStorage::LinkAddressType Update(TStorage& storage, typename TStorage::LinkAddressType linkAddress, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget)
        {
            auto _continue {storage.Constants.Continue};
            typename TStorage::LinkAddressType updatedLinkAddress;
            Update(storage, restriction, substitution, [&updatedLinkAddress, _continue] (typename TStorage::HandlerParameterType before, typename TStorage::HandlerParameterType after) {
                updatedLinkAddress = after[0];
                return _continue;
            });
            return updatedLinkAddress;
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction)
        {
            return Update(storage, Link{linkAddress, newSource, newTarget});
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType Update(TStorage& storage, autoo&& handler, std::convertible_to<typename TStorage::LinkAddressType> auto ...restriction)
        {
            typename TStorage::HandlerParameterType restrictionContainer {
                static_cast<typename TStorage::HandlerParameterType>(restriction)... };
            return Update(storage, restrictionContainer, handler);
        }

    template<typename TStorage>
    static typename TStorage::LinkAddressType UpdateOrCreateOrGet(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget)
    {
        auto constants = storage.Constants;
        auto _continue = constants.Continue;
        typename TStorage::LinkAddressType resultLink;
        UpdateOrCreateOrGet(storage, source, target, newSource, newTarget, [&resultLink, _continue](typename TStorage::HandlerParameterType before, typename TStorage::HandlerParameterType after) {
            resultLink = after[0];
            return _continue;
        });
        return resultLink;
    }

    template<typename TStorage>
    static typename TStorage::LinkAddressType UpdateOrCreateOrGet(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target, typename TStorage::LinkAddressType newSource, typename TStorage::LinkAddressType newTarget, auto&& handler)
    {
        auto link = SearchOrDefault(storage, source, target);
        if (0 == link)
        {
            return storage.CreateAndUpdate(newSource, newTarget, handler);
        }
        if ((source == newSource) && (target == newTarget))
        {
            auto linkStruct = Link{link, source, target};
            return link;
        }
        return storage.Update(link, newSource, newTarget, handler);
    }

        template<typename TStorage>
        static Interfaces::CArray<typename TStorage::LinkAddressType>auto ResolveConstantAsSelfReference(TStorage& storage, typename TStorage::LinkAddressType constant, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& restriction, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& substitution)
        {
            auto constants = storage.Constants;
            auto restrictionIndex = storage.GetIndex(restriction);
            auto substitutionIndex = storage.GetIndex(substitution);
            if ( 0 == substitutionIndex)
            {
                substitutionIndex = restrictionIndex;
            }
            auto source = GetSource(storage, substitution);
            auto target = GetTarget(storage, substitution);
            source =  constant == source ? substitutionIndex : source;
            target =  constant == target ? substitutionIndex : target;
            return Link(substitutionIndex, source, target);
        }

    template<typename TStorage>
    typename TStorage::LinkAddressType CreateAndUpdate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    {
        return Update(storage, Create(storage), source, target);
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType GetOrCreate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    {
        auto constants = storage.Constants;
        auto link = SearchOrDefault(storage, source, target);
        if (link == constants.Null)
        {
            std::cout << "CreateAndUpdate " << std::endl;
            link = CreateAndUpdate(storage, source, target);
        }
        return link;
    }
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType GetOrCreate(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    //    {
    //        auto link = SearchOrDefault(storage, source, target);
    //        if (link == 0)
    //        {
    //            link = CreateAndUpdate(storage, source, target);
    //        }
    //        return link;
    //    }
    //


    template<typename TStorage>
    auto DeleteAll(TStorage& storage)
    {
        for (auto count = Count(storage); count != 0; count = Count(storage))
        {
            storage.Delete(count);
        }
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType DeleteIfExists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    {
        auto constants = storage.Constants;
        auto link = storage.SearchOrDefault(source, target);
        if (link != constants.Null)
        {
            return Delete(storage, link);
        }
        return constants.Null;
    }

    template<typename TStorage>
    auto DeleteByQuery(TStorage& storage,  CArray<typename TStorage::LinkAddressType> auto&& query)
    {
        auto count = storage.Count(query);
        auto toDelete = std::vector<typename TStorage::LinkAddressType>(count);

        storage.Each([&](auto link) {
            toDelete.push_back(link[0]);
            return storage.Constants.Continue;
        }, query);

        for (auto link : toDelete | std::views::reverse)
        {
            Delete(storage, link);
        }
    }

    template<typename TStorage>
    auto ResetValues(TStorage& storage,  typename TStorage::LinkAddressType link)
    {
        storage.Update(link, 0, 0);
    }

    template<typename TStorage>
    auto CreatePoint(TStorage& storage)
    {
        auto point = Platform::Data::Create(storage);
        return Update(storage, point, point, point);
    }

    template<typename TStorage>
    static bool Exists(TStorage& storage, CArray auto&& restriction)
	{
		return storage.Count(restriction) > 0;
	}

    template<typename TStorage>
    static bool Exists(TStorage& storage, std::convertible_to<typename TStorage::LinkAddressType index> auto ...restriction)
    {
        TStorage::HandlerParameterType restriction
        auto any { storage.Constants.Any };
        return Exists(storage, std::array{index, any, any});
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType CountUsages(TStorage& storage, typename TStorage::LinkAddressType link)
    {
        auto constants = storage.Constants;
        auto values = GetLink(storage, link);

        typename TStorage::LinkAddressType usages = 0;
        usages += storage.Count(constants.Any, link, constants.Any) - bool(values.Source == link);
        usages += storage.Count(constants.Any, constants.Any, link) - bool(values.Target == link);
        return usages;
    }

    template<typename TStorage>
    typename TStorage::LinkAddressType HasUsages(TStorage& storage, typename TStorage::LinkAddressType link)
    {
        return storage.CountUsages(link) != 0;
    }

    template<typename TStorage>
    std::string Format(TStorage& storage, typename TStorage::LinkAddressType link)
    {
        auto values = GetLink(storage, link);
        return Format(values);
    }

    template<typename TStorage>
    std::string Format(TStorage& storage, Link<typename TStorage::LinkAddressType> link)
    {
        return std::string{}
                .append("(")
                .append(std::to_string(link.Index))
                .append("): ")
                .append("(")
                .append(std::to_string(link.Source))
                .append(")")
                .append(" -> ")
                .append("(")
                .append(std::to_string(link.Target))
                .append(")");
    }
    //
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType DeleteIfExists(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
    //    {
    //        auto link = SearchOrDefault(storage, source, target);
    //        if (link != 0)
    //        {
    //            Delete(storage, link);
    //            return link;
    //        }
    //        return 0;
    //    }
    //
    //    template<typename TStorage>
    //    static void DeleteMany(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& deletedLinks)
    //    {
    //        for (std::int32_t i = 0; i < deletedLinks.Count(); ++i)
    //        {
    //            storage.Delete(deletedLinks[i]);
    //        }
    //    }
    //
    //    template<typename TStorage>
    //    static void DeleteAllUsages(TStorage& storage, typename TStorage::LinkAddressType linkIndex) { storage.DeleteAllUsages(linkIndex, {}); }
    //

        template<typename TStorage>
        static typename TStorage::LinkAddressType DeleteAllUsages(TStorage& storage, typename TStorage::LinkAddressType linkIndex, auto&& handler)
        {
            auto constants = storage.Constants;
            auto any = constants.Any;
            auto $continue = storage.Constants.Continue;
            auto $break = storage.Constants.Break;
            auto usagesAsSourceQuery = Link(any, linkIndex, any);
            auto usagesAsTargetQuery = Link(any, any, linkIndex);
            auto usages = std::vector<typename TStorage::HandlerParameterType>();
            storage.Each(usagesAsSourceQuery, [&usages, $continue](typename TStorage::HandlerParameterType link) {
                usages.push_back(link);
                return $continue;
            });
            storage.Each(usagesAsTargetQuery, [&usages, $continue](auto&& link) {
                usages.push_back(link);
                return $continue;
            });
            typename TStorage::LinkAddressType result {};
            for (auto usage : usages)
            {
                auto usageAddress {GetIndex(storage, usage)};
                if (linkIndex == usageAddress || Exists(storage, usageAddress))
                {
                    continue;
                }
                result = storage.Delete(usage, handler);
            }
            return result;
        }
    //
    //    template<typename TStorage>
    //    static void DeleteByQuery(TStorage& storage, Link<typename TStorage::LinkAddressType> query)
    //    {
    //        auto queryResult = List<typename TStorage::LinkAddressType>();
    //        auto queryResultFiller = ListFiller<typename TStorage::LinkAddressType, typename TStorage::LinkAddressType>(queryResult, storage.Constants.Continue);
    //        storage.Each(queryResultFiller.AddFirstAndReturnConstant, query);
    //        foreach (auto link in queryResult)
    //        {
    //            Delete(storage, link);
    //        }
    //    }
    //
        template<typename TStorage>
        static bool AreValuesReset(TStorage& storage, typename TStorage::LinkAddressType linkIndex)
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
        static void ResetValues(TStorage& storage, typename TStorage::LinkAddressType linkIndex) { storage.ResetValues(linkIndex, {}); }

        template<typename TStorage>
        static typename TStorage::LinkAddressType ResetValues(TStorage& storage, typename TStorage::LinkAddressType linkIndex, auto&& handler)
        {
            auto nullConstant = storage.Constants.Null;
            LinkAddress restriction {linkIndex};
            auto substitution = Link<typename TStorage::LinkAddressType>(linkIndex, nullConstant, nullConstant);
            return storage.Update(restriction, substitution, handler);
        }

        template<typename TStorage>
        static typename TStorage::LinkAddressType EnforceResetValues(TStorage& storage, typename TStorage::LinkAddressType linkIndex, auto&& handler)
        {
            if (!AreValuesReset(storage, linkIndex))
            {
                return ResetValues(storage, linkIndex, handler);
            }
            return storage.Constants.Continue;
        }
    //
    //    template<typename TStorage>
    //    static void MergeUsages(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex) { storage.MergeUsages(oldLinkIndex, newLinkIndex, {}); }
    //
    //    template<typename typename TStorage::LinkAddressType, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CArray<typename TStorage::LinkAddressType> auto, Interfaces::CArray<typename TStorage::LinkAddressType> auto>
    //    static typename TStorage::LinkAddressType MergeUsages(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex, Handler handler)
    //    {
    //        if ( newLinkIndex == oldLinkIndex)
    //        {
    //            return newLinkIndex;
    //        }
    //        auto constants = storage.Constants;
    //        auto usagesAsSource = storage.All(Link<typename TStorage::LinkAddressType>(constants.Any, oldLinkIndex, constants.Any));
    //        WriteHandlerState<typename TStorage::LinkAddressType> handlerState = new(constants.Continue, constants.Break, handler);
    //        for (auto i { 0 }; i < usagesAsSource.Count(); ++i)
    //        {
    //            auto usageAsSource = usagesAsSource[i];
    //            if ( oldLinkIndex == storage.GetIndex(usageAsSource))
    //            {
    //                continue;
    //            }
    //            auto restriction = LinkAddress{storage.GetIndex(usageAsSource});
    //            auto substitution = Link<typename TStorage::LinkAddressType>(newLinkIndex, GetTarget(storage, usageAsSource));
    //            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
    //        }
    //        auto usagesAsTarget = storage.All(Link<typename TStorage::LinkAddressType>(constants.Any, constants.Any, oldLinkIndex));
    //        for (auto i { 0 }; i < usagesAsTarget.Count(); ++i)
    //        {
    //            auto usageAsTarget = usagesAsTarget[i];
    //            if ( oldLinkIndex == storage.GetIndex(usageAsTarget))
    //            {
    //                continue;
    //            }
    //            auto restriction = GetLink(storage, storage.GetIndex(usageAsTarget));
    //            auto substitution = Link<typename TStorage::LinkAddressType>(GetTarget(storage, usageAsTarget), newLinkIndex);
    //            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
    //        }
    //        return handlerState.Result;
    //    }
    //
    //    template<typename TStorage>
    //    static typename TStorage::LinkAddressType MergeAndDelete(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex)
    //    {
    //        if ( newLinkIndex != oldLinkIndex)
    //        {
    //            storage.MergeUsages(oldLinkIndex, newLinkIndex);
    //            storage.Delete(oldLinkIndex);
    //        }
    //        return newLinkIndex;
    //    }
    //
    //    template<typename typename TStorage::LinkAddressType, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CArray<typename TStorage::LinkAddressType> auto, Interfaces::CArray<typename TStorage::LinkAddressType> auto>
    //    static typename TStorage::LinkAddressType MergeAndDelete(TStorage& storage, typename TStorage::LinkAddressType oldLinkIndex, typename TStorage::LinkAddressType newLinkIndex, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        WriteHandlerState<typename TStorage::LinkAddressType> handlerState = new(constants.Continue, constants.Break, handler);
    //        if ( newLinkIndex != oldLinkIndex)
    //        {
    //            handlerState.Apply(storage.MergeUsages(oldLinkIndex, newLinkIndex, handlerState.Handler));
    //            handlerState.Apply(storage.Delete(oldLinkIndex, handlerState.Handler));
    //        }
    //        return handlerState.Result;
    //    }
    //

    //
    //    template<typename TStorage>
    //    static std::string Format(TStorage& storage, Interfaces::CArray<typename TStorage::LinkAddressType> auto&& link)
    //    {
    //        auto constants = storage.Constants;
    //        return "({storage.GetIndex(link)}: {GetSource(storage, link)} {GetTarget(storage, link)})";
    //    }
    //
    //    template<typename TStorage>
    //    static std::string Format(TStorage& storage, typename TStorage::LinkAddressType link) { return storage.Format(GetLink(storage, link)); }

        template<typename TStorage>
        typename TStorage::LinkAddressType SearchOrDefault(TStorage& storage, typename TStorage::LinkAddressType source, typename TStorage::LinkAddressType target)
        {
            auto constants = storage.Constants;
            auto _break = constants.Break;
            typename TStorage::LinkAddressType searchedLinkAddress {};
            storage.Each(Link{storage.Constants.Any, source, target}, [&searchedLinkAddress, _break] (typename TStorage::HandlerParameterType link) {
                searchedLinkAddress = link[0];
                return _break;
            });
            return searchedLinkAddress;
        }
}

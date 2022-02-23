namespace Platform::Data::Doublets
{
    template<typename TLinkAddress>
    static void RunRandomCreations(auto&& storage, std::uint64_t amountOfCreations)
    {
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        auto randomGenerator64 = Random::RandomHelpers::Default;
        for (auto i { 0UL }; i < amountOfCreations; ++i)
        {
            Range<std::uint64_t> linksAddressRange { 0, storage.Count() };
            auto source = Random::NextUInt64(randomGenerator64, linksAddressRange);
            auto target = Random::NextUInt64(randomGenerator64, linksAddressRange);
            storage.GetOrCreate(source, target);
        }
    }

    template<typename TLinkAddress>
    static void RunRandomSearches(auto&& storage, std::uint64_t amountOfSearches)
    {
        auto randomGenerator64 = Random::RandomHelpers::Default;
        for (auto i { 0UL }; i < amountOfSearches; ++i)
        {
            auto linksAddressRange = Range<std::uint64_t>(0, storage.Count());
            auto source = Random::NextUInt64(randomGenerator64, linksAddressRange);
            auto target = Random::NextUInt64(randomGenerator64, linksAddressRange);
            SearchOrDefault(storage, source, target);
        }
    }

    template<typename TLinkAddress>
    static void RunRandomDeletions(auto&& storage, std::uint64_t amountOfDeletions)
    {
        auto randomGenerator64 = Random::RandomHelpers::Default;
        auto linksCount = storage.Count();
        auto min = amountOfDeletions > linksCount ? 0UL : linksCount - amountOfDeletions;
        for (auto i { 0UL }; i < amountOfDeletions; ++i)
        {
            linksCount = storage.Count();
            if (linksCount <= min)
            {
                break;
            }
            auto linksAddressRange = Range<std::uint64_t>(min, linksCount);
            auto link = Random::NextUInt64(randomGenerator64, linksAddressRange);
            storage.Delete(link);
        }
    }


    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires Interfaces::CList<TList1, TLinkAddress> && Interfaces::CList<TList2, TLinkAddress> && std::invocable<Handler&, TList1, TList2>
    static TLinkAddress Delete(auto&& storage, TLinkAddress linkToDelete, Handler handler)
    {
        if (storage.Exists(linkToDelete))
        {
            storage.EnforceResetValues(linkToDelete, handler);
        }
        return storage.Delete(LinkAddress{linkToDelete}, handler);
    }

    template<typename TLinkAddress>
    static void DeleteAll(auto&& storage)
    {
        for (auto i { storage.Count() }; i > 0; --i)
        {
            storage.Delete(i);
            if (i - 1 != storage.Count())
            {
                i = storage.Count();
            }
        }
    }

    template<typename TLinkAddress>
    static TLinkAddress First(auto&& storage)
    {
        auto constants = storage.Constants;
        TLinkAddress firstLink = 0;
        if (0 == storage.Count())
        {
            throw std::runtime_error("No links in the storage..");
        }
        Setters::Setter setter { constants.Continue, constants.Break, 0 };
        storage.Each(Link<TLinkAddress>(storage.Constants.Any, storage.Constants.Any, storage.Constants.Any), setter.SetFirstAndReturnFalse);
        if ( 0 == firstLink)
        {
            throw std::runtime_error("No links are found in the storage.");
        }
        return firstLink;
    }

    static CList auto SingleOrDefault(auto&& storage, CList auto&& query)
    {
        IList<TLinkAddress>? result = {};
        auto count = 0;
        auto constants = storage.Constants;
        auto continue = constants.Continue;
        auto break = constants.Break;
        storage.Each(query, linkHandler);
        return result;

        TLinkAddress linkHandler(CList auto&& link)
        {
            if (count == 0)
            {
                result = link;
                count++;
                return continue;
            }
            else
            {
                result = {};
                return break;
            }
        }
    }

    template<typename TLinkAddress>
    static bool CheckPathExistance(auto&& storage, CList auto&& path)
    {
        auto current = path[0];
        if (!storage.Exists(current))
        {
            return false;
        }
        auto constants = storage.Constants;
        for (auto i { 1 }; i < path.Length; ++i)
        {
            auto next = path[i];
            auto values = storage.GetLink(current);
            auto source = storage.GetSource(values);
            auto target = storage.GetTarget(values);
            if ( target == source &&  next == source)
            {
                return false;
            }
            if (!equalityComparer.Equals(next, source) && !equalityComparer.Equals(next, target))
            {
                return false;
            }
            current = next;
        }
        return true;
    }

    template<typename TLinkAddress>
    static TLinkAddress GetByKeys(auto&& storage, TLinkAddress root, params std::int32_t path[])
    {
        storage.EnsureLinkExists(root, "root");
        auto currentLink = root;
        for (auto i { 0 }; i < path.Length; ++i)
        {
            currentLink = storage.GetLink(currentLink)[path[i]];
        }
        return currentLink;
    }

    template<typename TLinkAddress>
    static TLinkAddress GetSquareMatrixSequenceElementByIndex(auto&& storage, TLinkAddress root, std::uint64_t size, std::uint64_t index)
    {
        auto constants = storage.Constants;
        auto source = constants.SourcePart;
        auto target = constants.TargetPart;
        if (!Platform.Numbers.Math.IsPowerOfTwo(size))
        {
            throw std::invalid_argument("size", "Sequences with sizes other than powers of two are not supported.");
        }
        auto path = BitArray(BitConverter.GetBytes(index));
        auto length = Bit.GetLowestPosition(size);
        storage.EnsureLinkExists(root, "root");
        auto currentLink = root;
        for (auto i { length - 1 }; i >= 0; i--)
        {
            currentLink = storage.GetLink(currentLink)[path[i] ? target : source];
        }
        return currentLink;
    }

    template<typename TLinkAddress>
    static TLinkAddress GetIndex(auto&& storage, CList auto&& link) { return link[storage.Constants.IndexPart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetSource(auto&& storage, TLinkAddress link) { return storage.GetLink(link)[storage.Constants.SourcePart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetSource(auto&& storage, CList auto&& link) { return link[storage.Constants.SourcePart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetTarget(auto&& storage, TLinkAddress link) { return storage.GetLink(link)[storage.Constants.TargetPart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetTarget(auto&& storage, CList auto&& link) { return link[storage.Constants.TargetPart]; }

    static IList<IList<TLinkAddress>?> All<TLinkAddress>(auto&& storage, CList auto&& restriction)
    {
        auto allLinks = List<IList<TLinkAddress>?>();
        auto filler = ListFiller<IList<TLinkAddress>?, TLinkAddress>(allLinks, storage.Constants.Continue);
        storage.Each(filler.AddAndReturnConstant, restriction);
        return allLinks;
    }

    static CList auto AllIndices<TLinkAddress>(auto&& storage, CList auto&& restriction)
    {
        auto allIndices = List<TLinkAddress>();
        auto filler = ListFiller<TLinkAddress, TLinkAddress>(allIndices, storage.Constants.Continue);
        storage.Each(filler.AddFirstAndReturnConstant, restriction);
        return allIndices;
    }

    template<typename TLinkAddress>
    static bool Exists(auto&& storage, TLinkAddress source, TLinkAddress target) { return Comparer<TLinkAddress>.Default.Compare(storage.Count()(storage.Constants.Any, source, target), 0) > 0; }

    template<typename TLinkAddress>
    static void EnsureLinkExists(auto&& storage, CList auto&& restriction)
    {
        for (auto i { 0 }; i < restriction.Count(); ++i)
        {
            if (!storage.Exists(restriction[i]))
            {
                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
            }
        }
    }

    template<typename TLinkAddress>
    static void EnsureInnerReferenceExists(auto&& storage, TLinkAddress reference, std::string argumentName)
    {
        if (storage.Constants.IsInternalReference(reference) && !storage.Exists(reference))
        {
            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(reference, argumentName);
        }
    }

    template<typename TLinkAddress>
    static void EnsureInnerReferenceExists(auto&& storage, CList auto&& restriction, std::string argumentName)
    {
        for (std::int32_t i = 0; i < restriction.Count(); ++i)
        {
            storage.EnsureInnerReferenceExists(restriction[i], argumentName);
        }
    }

    template<typename TLinkAddress>
    static void EnsureLinkIsAnyOrExists(auto&& storage, CList auto&& restriction)
    {
        auto any = storage.Constants.Any;
        for (auto i { 0 }; i < restriction.Count(); ++i)
        {
            if (!equalityComparer.Equals(restriction[i], any) && !storage.Exists(restriction[i]))
            {
                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
            }
        }
    }

    template<typename TLinkAddress>
    static void EnsureLinkIsAnyOrExists(auto&& storage, TLinkAddress link, std::string argumentName)
    {
        if ((storage.Constants.Any != link) && !storage.Exists(link))
        {
            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
        }
    }

    template<typename TLinkAddress>
    static void EnsureLinkIsItselfOrExists(auto&& storage, TLinkAddress link, std::string argumentName)
    {
        if ((storage.Constants.Itself != link) && !storage.Exists(link))
        {
            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
        }
    }

    template<typename TLinkAddress>
    static void EnsureDoesNotExists(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        if (storage.Exists(source, target))
        {
            throw LinkWithSameValueAlreadyExistsException();
        }
    }

    template<typename TLinkAddress>
    static void EnsureNoUsages(auto&& storage, TLinkAddress link)
    {
        if (storage.HasUsages(link))
        {
            throw ArgumentLinkHasDependenciesException<TLinkAddress>(link);
        }
    }

    template<typename TLinkAddress>
    static void EnsureCreated(auto&& storage, CList auto&& addresses) { storage.EnsureCreated(storage.Create, addresses); }

    template<typename TLinkAddress>
    static void EnsurePointsCreated(auto&& storage, CList auto&& addresses) { storage.EnsureCreated(storage.CreatePoint, addresses); }

    template<typename TLinkAddress>
    static void EnsureCreated(auto&& storage, std::function<TLinkAddress()> creator, CList auto&& addresses)
    {
        auto nonExistentAddresses = HashSet<TLinkAddress>(addresses.Where(x => !storage.Exists(x)));
        if (nonExistentAddresses.Count() > 0)
        {
            auto max = nonExistentAddresses.Max();
            max = System::Math::Min(max), storage.Constants.InternalReferencesRange.Maximum
            auto createdLinks = List<TLinkAddress>();
            TLinkAddress createdLink = creator();
            while (!equalityComparer.Equals(createdLink, max))
            {
                createdLinks.Add(createdLink);
            }
            for (auto i { 0 }; i < createdLinks.Count(); ++i)
            {
                if (!nonExistentAddresses.Contains(createdLinks[i]))
                {
                    storage.Delete(createdLinks[i]);
                }
            }
        }
    }

    template<typename TLinkAddress>
    static TLinkAddress CountUsages(auto&& storage, TLinkAddress link)
    {
        auto constants = storage.Constants;
        auto values = storage.GetLink(link);
        TLinkAddress usagesAsSource = storage.Count()(Link<TLinkAddress>(constants.Any, link, constants.Any));
        if ( link == storage.GetSource(values))
        {
            usagesAsSource = usagesAsSource - 1;
        }
        TLinkAddress usagesAsTarget = storage.Count()(Link<TLinkAddress>(constants.Any, constants.Any, link));
        if ( link == storage.GetTarget(values))
        {
            usagesAsTarget = usagesAsTarget - 1;
        }
        return usagesAsSource + usagesAsTarget;
    }

    template<typename TLinkAddress>
    static bool HasUsages(auto&& storage, TLinkAddress link) { return Comparer<TLinkAddress>.Default.Compare(storage.Count()Usages(link), 0) > 0; }

    template<typename TLinkAddress>
    static bool operator ==(const auto&& storage, TLinkAddress link, TLinkAddress source, TLinkAddress &target) const
    {
        auto constants = storage.Constants;
        auto values = storage.GetLink(link);
        return  source == storage.GetSource(values) &&  target == storage.GetTarget(values);
    }

    template<typename TLinkAddress>
    static TLinkAddress SearchOrDefault(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto contants = storage.Constants;
        auto setter = Setter<TLinkAddress, TLinkAddress>(contants.Continue, contants.Break, 0);
        storage.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
        return setter.Result;
    }

    template<typename TLinkAddress>
    static TLinkAddress CreatePoint(auto&& storage)
    {
        auto constants = storage.Constants;
        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
        storage.CreatePoint(setter.SetFirstFromSecondListAndReturnTrue);
        return setter.Result;
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress CreatePoint(auto&& storage, Handler handler)
    {
        auto constants = storage.Constants;
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        TLinkAddress link = 0;
        auto HandlerWrapper = [&]() -> TLinkAddress {
            link = storage.GetIndex(after);
            return handlerState.Handle(before, after);;
        };
        handlerState.Apply(storage.Create({}, HandlerWrapper));
        handlerState.Apply(storage.Update(link, link, link, HandlerWrapper));
        return handlerState.Result;
    }

    template<typename TLinkAddress>
    static TLinkAddress CreateAndUpdate(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto constants = storage.Constants;
        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
        storage.CreateAndUpdate(source, target, setter.SetFirstFromSecondListAndReturnTrue);
        return setter.Result;
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress CreateAndUpdate(auto&& storage, TLinkAddress source, TLinkAddress target, Handler handler)
    {
        auto constants = storage.Constants;
        TLinkAddress createdLink = 0;
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        handlerState.Apply(storage.Create({}, (before, after) =>
        {
            createdLink = storage.GetIndex(after);
            return handlerState.Handle(before, after);;
        }));
        handlerState.Apply(storage.Update(createdLink, source, target, handler));
        return handlerState.Result;
    }

    template<typename TLinkAddress>
    static TLinkAddress Update(auto&& storage, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget) { return storage.Update(LinkAddress{link}, Link<TLinkAddress>(link, newSource, newTarget)); }

    template<typename TLinkAddress>
    static TLinkAddress Update(auto&& storage, CList auto&& restriction) { return storage.Update((IList<TLinkAddress>)restriction); }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress Update(auto&& storage, Handler handler, CList auto&& restriction) { return storage.Update(restriction, handler); }

    template<typename TLinkAddress>
    static TLinkAddress Update(auto&& storage, CList auto&& restriction)
    {
        auto constants = storage.Constants;
        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
        storage.Update(restriction, setter.SetFirstFromSecondListAndReturnTrue);
        return setter.Result;
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress Update(auto&& storage, CList auto&& restriction, Handler handler)
    {
        return restriction.Count() switch
        {
            2 => storage.MergeAndDelete(restriction[0], restriction[1], handler),
            4 => storage.UpdateOrCreateOrGet(restriction[0], restriction[1], restriction[2], restriction[3], handler),
            _ => storage.Update(restriction[0], restriction[1], restriction[2], handler)
        };
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress Update(auto&& storage, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget, Handler handler) { return storage.Update(LinkAddress{link}, Link<TLinkAddress>(link, newSource, newTarget), handler); }

    static CList auto ResolveConstantAsSelfReference<TLinkAddress>(auto&& storage, TLinkAddress constant, CList auto&& restriction, CList auto&& substitution)
    {
        auto constants = storage.Constants;
        auto restrictionIndex = storage.GetIndex(restriction);
        auto substitutionIndex = storage.GetIndex(substitution);
        if ( 0 == substitutionIndex)
        {
            substitutionIndex = restrictionIndex;
        }
        auto source = storage.GetSource(substitution);
        auto target = storage.GetTarget(substitution);
        source =  constant == source ? substitutionIndex : source;
        target =  constant == target ? substitutionIndex : target;
        return Link<TLinkAddress>(substitutionIndex, source, target);
    }

    template<typename TLinkAddress>
    static TLinkAddress GetOrCreate(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto link = SearchOrDefault(storage, source, target);
        if (link == 0)
        {
            link = CreateAndUpdate(storage, source, target);
        }
        return link;
    }

    template<typename TLinkAddress>
    static TLinkAddress UpdateOrCreateOrGet(auto&& storage, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget)
    {
        auto constants = storage.Constants;
        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
        UpdateOrCreateOrGet(storage, source, target, newSource, newTarget, setter.SetFirstFromSecondListAndReturnTrue);
        return setter.Result;
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress UpdateOrCreateOrGet(auto&& storage, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget, Handler handler)
    {
        auto link = SearchOrDefault(storage, source, target);
        if ( 0 == link)
        {
            return storage.CreateAndUpdate(newSource, newTarget, handler);
        }
        if ( source == newSource &&  target == newTarget)
        {
            auto linkStruct = Link<TLinkAddress>(link, source, target);
            return link;
        }
        return storage.Update(link, newSource, newTarget, handler);
    }

    template<typename TLinkAddress>
    static TLinkAddress DeleteIfExists(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto link = SearchOrDefault(storage, source, target);
        if (link != 0)
        {
            storage.Delete(link);
            return link;
        }
        return 0;
    }

    template<typename TLinkAddress>
    static void DeleteMany(auto&& storage, CList auto&& deletedLinks)
    {
        for (std::int32_t i = 0; i < deletedLinks.Count(); ++i)
        {
            storage.Delete(deletedLinks[i]);
        }
    }

    template<typename TLinkAddress>
    static void DeleteAllUsages(auto&& storage, TLinkAddress linkIndex) { storage.DeleteAllUsages(linkIndex, {}); }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress DeleteAllUsages(auto&& storage, TLinkAddress linkIndex, Handler handler)
    {
        auto constants = storage.Constants;
        auto any = constants.Any;
        auto usagesAsSourceQuery = Link<TLinkAddress>(any, linkIndex, any);
        auto usagesAsTargetQuery = Link<TLinkAddress>(any, any, linkIndex);
        auto usages = List<IList<TLinkAddress>?>();
        auto usagesFiller = ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, constants.Continue);
        storage.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
        storage.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        foreach (auto usage in usages)
        {
            if ( linkIndex == storage.GetIndex(usage) || !storage.Exists(storage.GetIndex(usage)))
            {
                continue;
            }
            handlerState.Apply(storage.Delete(storage.GetIndex(usage), handlerState.Handler));
        }
        return handlerState.Result;
    }

    template<typename TLinkAddress>
    static void DeleteByQuery(auto&& storage, Link<TLinkAddress> query)
    {
        auto queryResult = List<TLinkAddress>();
        auto queryResultFiller = ListFiller<TLinkAddress, TLinkAddress>(queryResult, storage.Constants.Continue);
        storage.Each(queryResultFiller.AddFirstAndReturnConstant, query);
        foreach (auto link in queryResult)
        {
            storage.Delete(link);
        }
    }

    template<typename TLinkAddress>
    static bool AreValuesReset(auto&& storage, TLinkAddress linkIndex)
    {
        auto nullConstant = storage.Constants.Null;
        auto link = storage.GetLink(linkIndex);
        for (std::int32_t i = 1; i < link.Count(); ++i)
        {
            if (!equalityComparer.Equals(link[i], nullConstant))
            {
                return false;
            }
        }
        return true;
    }

    template<typename TLinkAddress>
    static void ResetValues(auto&& storage, TLinkAddress linkIndex) { storage.ResetValues(linkIndex, {}); }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress ResetValues(auto&& storage, TLinkAddress linkIndex, Handler handler)
    {
        auto nullConstant = storage.Constants.Null;
        auto updateRequest = Link<TLinkAddress>(linkIndex, nullConstant, nullConstant);
        return storage.Update(updateRequest, handler);
    }

    template<typename TLinkAddress>
    static void EnforceResetValues(auto&& storage, TLinkAddress linkIndex) { storage.EnforceResetValues(linkIndex, {}); }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress EnforceResetValues(auto&& storage, TLinkAddress linkIndex, Handler handler)
    {
        if (!storage.AreValuesReset(linkIndex))
        {
            return storage.ResetValues(linkIndex, handler);
        }
        return storage.Constants.Continue;
    }

    template<typename TLinkAddress>
    static void MergeUsages(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex) { storage.MergeUsages(oldLinkIndex, newLinkIndex, {}); }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress MergeUsages(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, Handler handler)
    {
        if ( newLinkIndex == oldLinkIndex)
        {
            return newLinkIndex;
        }
        auto constants = storage.Constants;
        auto usagesAsSource = storage.All(Link<TLinkAddress>(constants.Any, oldLinkIndex, constants.Any));
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        for (auto i { 0 }; i < usagesAsSource.Count(); ++i)
        {
            auto usageAsSource = usagesAsSource[i];
            if ( oldLinkIndex == storage.GetIndex(usageAsSource))
            {
                continue;
            }
            auto restriction = LinkAddress{storage.GetIndex(usageAsSource});
            auto substitution = Link<TLinkAddress>(newLinkIndex, storage.GetTarget(usageAsSource));
            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
        }
        auto usagesAsTarget = storage.All(Link<TLinkAddress>(constants.Any, constants.Any, oldLinkIndex));
        for (auto i { 0 }; i < usagesAsTarget.Count(); ++i)
        {
            auto usageAsTarget = usagesAsTarget[i];
            if ( oldLinkIndex == storage.GetIndex(usageAsTarget))
            {
                continue;
            }
            auto restriction = storage.GetLink(storage.GetIndex(usageAsTarget));
            auto substitution = Link<TLinkAddress>(storage.GetTarget(usageAsTarget), newLinkIndex);
            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
        }
        return handlerState.Result;
    }

    template<typename TLinkAddress>
    static TLinkAddress MergeAndDelete(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex)
    {
        if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
        {
            storage.MergeUsages(oldLinkIndex, newLinkIndex);
            storage.Delete(oldLinkIndex);
        }
        return newLinkIndex;
    }

    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    static TLinkAddress MergeAndDelete(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, Handler handler)
    {
        auto constants = storage.Constants;
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
        {
            handlerState.Apply(storage.MergeUsages(oldLinkIndex, newLinkIndex, handlerState.Handler));
            handlerState.Apply(storage.Delete(oldLinkIndex, handlerState.Handler));
        }
        return handlerState.Result;
    }

    static ILinks<TLinkAddress> DecorateWithAutomaticUniquenessAndUsagesResolution<TLinkAddress>(auto&& storage)
    {
        storage = LinksCascadeUsagesResolver<TLinkAddress>(storage);
        storage = NonNullContentsLinkDeletionResolver<TLinkAddress>(storage);
        storage = LinksCascadeUniquenessAndUsagesResolver<TLinkAddress>(storage);
        return storage;
    }

    template<typename TLinkAddress>
    static std::string Format(auto&& storage, CList auto&& link)
    {
        auto constants = storage.Constants;
        return "({storage.GetIndex(link)}: {storage.GetSource(link)} {storage.GetTarget(link)})";
    }

    template<typename TLinkAddress>
    static std::string Format(auto&& storage, TLinkAddress link) { return storage.Format(storage.GetLink(link)); }
}

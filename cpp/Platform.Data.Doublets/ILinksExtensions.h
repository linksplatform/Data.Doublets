namespace Platform::Data::Doublets
{
    template<typename TLinkAddress>
    void TestRandomCreationsAndDeletions(auto&& storage, std::size_t maximumOperationsPerCycle)
    {
        using namespace Platform::Random;
        using namespace Platform::Ranges;
        
        for (auto N = 1; N < maximumOperationsPerCycle; N++)
        {
            auto& randomGen64 = RandomHelpers::Default;
            auto created = 0UL;
            auto deleted = 0UL;
            for (auto i = 0; i < N; i++)
            {
                auto linksCount = storage.Count();
                auto createPoint = Random::NextBoolean(randomGen64);
                if (linksCount >= 2 && createPoint)
                {
                    auto source = Random::NextUInt64(randomGen64, Ranges::Range{1, linksCount});
                    auto target = Random::NextUInt64(randomGen64, Ranges::Range{1, linksCount}); //-V3086
                    auto resultLink = storage.GetOrCreate(source, target);
                    if (resultLink > linksCount)
                    {
                        created++;
                    }
                }
                else
                {
                    storage.Create();
                    created++;
                }
            }
            auto count = storage.Count();
            for (auto i = 0; i < count; i++)
            {
                auto link = i + 1;
                {
                    storage.Update(link, 0, 0);
                    storage.Delete(link);
                    deleted++;
                }
            }
        }
    }

    template<typename TLinkAddress>
    TLinkAddress SearchOrDefault(const auto& storage, TLinkAddress source, TLinkAddress target)
    {
        auto constants = storage.Constants;
        auto _break = constants.Break;
        TLinkAddress searchedLinkAddress;
        storage.Each(std::array{storage.Constants.Any, source, target}, [&searchedLinkAddress, _break] (auto link) {
            searchedLinkAddress = link[0];
            return _break;
        });
        return searchedLinkAddress;
    }

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


    template<typename TLinkAddress>
    static TLinkAddress Delete(auto&& storage, TLinkAddress linkToDelete, auto&& handler)
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
        auto $break {storage.Constants.Break};
        TLinkAddress firstLink;
        if (0 == storage.Count())
        {
            throw std::runtime_error("No links in the storage..");
        }
        storage.Each(std::array{storage.Constants.Any, storage.Constants.Any, storage.Constants.Any}, [&firstLink, $break] (Interfaces::CArray auto&& link){
            firstLink = link[0];
            return $break;
        });
        if ( 0 == firstLink)
        {
            throw std::runtime_error("No links are found in the storage.");
        }
        return firstLink;
    }

    template<typename TLinkAddress>
    static Interfaces::CList auto SingleOrDefault(auto&& storage, Interfaces::CList auto&& query)
    {
        std::vector<TLinkAddress> result = {};
        auto count = 0;
        auto constants = storage.Constants;
        auto linkHandler { [&result, &count, &constants] (Interfaces::CList auto&& link) {
            if (count == 0)
            {
                result = link;
                count++;
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

    template<typename TLinkAddress>
    static bool CheckPathExistance(auto&& storage, Interfaces::CList auto&& path)
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
            if ((source != next) && (target != next))
            {
                return false;
            }
            current = next;
        }
        return true;
    }

    template<typename TLinkAddress>
    static TLinkAddress GetByKeys(auto&& storage, TLinkAddress root, Interfaces::CList auto&& path)
    {
        storage.EnsureLinkExists(root, "root");
        auto currentLink = root;
        for (auto i { 0 }; i < path.Length; ++i)
        {
            currentLink = storage.GetLink(currentLink)[path[i]];
        }
        return currentLink;
    }

    //    template<typename TLinkAddress>
    //    static TLinkAddress GetSquareMatrixSequenceElementByIndex(auto&& storage, TLinkAddress root, std::uint64_t size, std::uint64_t index)
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
    //            currentLink = storage.GetLink(currentLink)[path[i] ? target : source];
    //        }
    //        return currentLink;
    //    }

    template<typename TLinkAddress>
    static TLinkAddress GetIndex(auto&& storage, Interfaces::CList auto&& link) { return link[storage.Constants.IndexPart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetSource(auto&& storage, TLinkAddress link) { return storage.GetLink(link)[storage.Constants.SourcePart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetSource(auto&& storage, Interfaces::CList auto&& link) { return link[storage.Constants.SourcePart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetTarget(auto&& storage, TLinkAddress link) { return storage.GetLink(link)[storage.Constants.TargetPart]; }

    template<typename TLinkAddress>
    static TLinkAddress GetTarget(auto&& storage, Interfaces::CList auto&& link) { return link[storage.Constants.TargetPart]; }

    //    template<typename TLinkAddress>
    //    static auto&& All(auto&& storage, Interfaces::CList auto&& restriction)
    //    {
    //        using namespace Platform::Collections;
    //        auto allLinks = std::vector<std::vector<TLinkAddress>>();
    //        auto filler = Collections::ListFiller<IList<TLinkAddress>?, TLinkAddress>(allLinks, storage.Constants.Continue);
    //        storage.Each(filler.AddAndReturnConstant, restriction);
    //        return allLinks;
    //    }
    //
    //    static Interfaces::CList auto AllIndices<TLinkAddress>(auto&& storage, Interfaces::CList auto&& restriction)
    //    {
    //        auto allIndices = List<TLinkAddress>();
    //        auto filler = ListFiller<TLinkAddress, TLinkAddress>(allIndices, storage.Constants.Continue);
    //        storage.Each(filler.AddFirstAndReturnConstant, restriction);
    //        return allIndices;
    //    }
    //
    //
    //    template<typename TLinkAddress>
    //    static void EnsureLinkExists(auto&& storage, Interfaces::CList auto&& restriction)
    //    {
    //        for (auto i { 0 }; i < restriction.Count(); ++i)
    //        {
    //            if (!storage.Exists(restriction[i]))
    //            {
    //                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
    //            }
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureInnerReferenceExists(auto&& storage, TLinkAddress reference, std::string argumentName)
    //    {
    //        if (storage.Constants.IsInternalReference(reference) && !storage.Exists(reference))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(reference, argumentName);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureInnerReferenceExists(auto&& storage, Interfaces::CList auto&& restriction, std::string argumentName)
    //    {
    //        for (std::int32_t i = 0; i < restriction.Count(); ++i)
    //        {
    //            storage.EnsureInnerReferenceExists(restriction[i], argumentName);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureLinkIsAnyOrExists(auto&& storage, Interfaces::CList auto&& restriction)
    //    {
    //        auto any = storage.Constants.Any;
    //        for (auto i { 0 }; i < restriction.Count(); ++i)
    //        {
    //            if ((any != restriction[i]) && !storage.Exists(restriction[i]))
    //            {
    //                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
    //            }
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureLinkIsAnyOrExists(auto&& storage, TLinkAddress link, std::string argumentName)
    //    {
    //        if ((storage.Constants.Any != link) && !storage.Exists(link))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureLinkIsItselfOrExists(auto&& storage, TLinkAddress link, std::string argumentName)
    //    {
    //        if ((storage.Constants.Itself != link) && !storage.Exists(link))
    //        {
    //            throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureDoesNotExists(auto&& storage, TLinkAddress source, TLinkAddress target)
    //    {
    //        if (storage.Exists(source, target))
    //        {
    //            throw LinkWithSameValueAlreadyExistsException();
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureNoUsages(auto&& storage, TLinkAddress link)
    //    {
    //        if (storage.HasUsages(link))
    //        {
    //            throw ArgumentLinkHasDependenciesException<TLinkAddress>(link);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureCreated(auto&& storage, Interfaces::CList auto&& addresses) { storage.EnsureCreated(storage.Create, addresses); }
    //
    //    template<typename TLinkAddress>
    //    static void EnsurePointsCreated(auto&& storage, Interfaces::CList auto&& addresses) { storage.EnsureCreated(storage.CreatePoint, addresses); }
    //
    //    template<typename TLinkAddress>
    //    static void EnsureCreated(auto&& storage, std::function<TLinkAddress()> creator, Interfaces::CList auto&& addresses)
    //    {
    //        auto nonExistentAddresses = HashSet<TLinkAddress>(addresses.Where(x => !storage.Exists(x)));
    //        if (nonExistentAddresses.Count() > 0)
    //        {
    //            auto max = nonExistentAddresses.Max();
    //            max = System::Math::Min(max), storage.Constants.InternalReferencesRange.Maximum
    //            auto createdLinks = List<TLinkAddress>();
    //            TLinkAddress createdLink = creator();
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
    //    template<typename TLinkAddress>
    //    static TLinkAddress CountUsages(auto&& storage, TLinkAddress link)
    //    {
    //        auto constants = storage.Constants;
    //        auto values = storage.GetLink(link);
    //        TLinkAddress usagesAsSource = storage.Count()(Link<TLinkAddress>(constants.Any, link, constants.Any));
    //        if ( link == storage.GetSource(values))
    //        {
    //            usagesAsSource = usagesAsSource - 1;
    //        }
    //        TLinkAddress usagesAsTarget = storage.Count()(Link<TLinkAddress>(constants.Any, constants.Any, link));
    //        if ( link == storage.GetTarget(values))
    //        {
    //            usagesAsTarget = usagesAsTarget - 1;
    //        }
    //        return usagesAsSource + usagesAsTarget;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static bool HasUsages(auto&& storage, TLinkAddress link) { return Comparer<TLinkAddress>.Default.Compare(storage.Count()Usages(link), 0) > 0; }
    //
    //    template<typename TLinkAddress>
    //    static bool operator ==(const auto&& storage, TLinkAddress link, TLinkAddress source, TLinkAddress &target) const
    //    {
    //        auto constants = storage.Constants;
    //        auto values = storage.GetLink(link);
    //        return  source == storage.GetSource(values) &&  target == storage.GetTarget(values);
    //    }
    //
    //    template<typename TLinkAddress>
    //    static TLinkAddress SearchOrDefault(auto&& storage, TLinkAddress source, TLinkAddress target)
    //    {
    //        auto contants = storage.Constants;
    //        auto setter = Setter<TLinkAddress, TLinkAddress>(contants.Continue, contants.Break, 0);
    //        storage.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
    //        return setter.Result;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static TLinkAddress CreatePoint(auto&& storage)
    //    {
    //        auto constants = storage.Constants;
    //        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
    //        storage.CreatePoint(setter.SetFirstFromSecondListAndReturnTrue);
    //        return setter.Result;
    //    }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress CreatePoint(auto&& storage, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
    //        TLinkAddress link = 0;
    //        TLinkAddress HandlerWrapper = [&]()Address {
    //            link = storage.GetIndex(after);
    //            return handlerState.Handle(before, after);;
    //        };
    //        handlerState.Apply(storage.Create({}, HandlerWrapper));
    //        handlerState.Apply(storage.Update(link, link, link, HandlerWrapper));
    //        return handlerState.Result;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static TLinkAddress CreateAndUpdate(auto&& storage, TLinkAddress source, TLinkAddress target)
    //    {
    //        auto constants = storage.Constants;
    //        auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
    //        storage.CreateAndUpdate(source, target, setter.SetFirstFromSecondListAndReturnTrue);
    //        return setter.Result;
    //    }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress CreateAndUpdate(auto&& storage, TLinkAddress source, TLinkAddress target, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        TLinkAddress createdLink = 0;
    //        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
    //        handlerState.Apply(storage.Create({}, (before, after) =>
    //        {
    //            createdLink = storage.GetIndex(after);
    //            return handlerState.Handle(before, after);;
    //        }));
    //        handlerState.Apply(storage.Update(createdLink, source, target, handler));
    //        return handlerState.Result;
    //    }
    //
        template<typename TLinkAddress>
        TLinkAddress Update(auto&& storage, Interfaces::CArray auto&& restriction, Interfaces::CArray auto&& substitution)
        {
            auto _continue {storage.Constants.Continue};
            TLinkAddress updatedLinkAddress;
            storage.Update(restriction, substitution, [&updatedLinkAddress, _continue] (Interfaces::CArray auto&& before, Interfaces::CArray auto&& after) {
                updatedLinkAddress = after[0];
                return _continue;
            });
            return updatedLinkAddress;
        }

        template<typename TLinkAddress>
        static TLinkAddress Update(auto&& storage, TLinkAddress linkAddress, TLinkAddress newSource, TLinkAddress newTarget)
        {
            std::array restriction {linkAddress};
            std::array substitution {linkAddress, newSource, newTarget};
            return Update<TLinkAddress>(storage, restriction, substitution);
        }

        template<typename TLinkAddress>
        static TLinkAddress Update(auto&& storage, Interfaces::CList auto&& restriction, auto&& handler)
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

        template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
        static TLinkAddress Update(auto&& storage, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget, Handler handler)
        {
            return storage.Update(Link{link, newSource, newTarget}, handler);
        }

    template<typename TLinkAddress>
    static TLinkAddress UpdateOrCreateOrGet(auto&& storage, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget)
    {
        auto constants = storage.Constants;
        auto _continue = constants.Continue;
        TLinkAddress resultLink;
        UpdateOrCreateOrGet(storage, source, target, newSource, newTarget, [&resultLink, _continue](Interfaces::CArray auto&& before, Interfaces::CArray auto&& after) {
            resultLink = after[0];
            return _continue;
        });
        return resultLink;
    }

    template<typename TLinkAddress>
    static TLinkAddress UpdateOrCreateOrGet(auto&& storage, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget, auto&& handler)
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

        template<typename TLinkAddress>
        static Interfaces::CList auto ResolveConstantAsSelfReference(auto&& storage, TLinkAddress constant, Interfaces::CList auto&& restriction, Interfaces::CList auto&& substitution)
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
            return Link(substitutionIndex, source, target);
        }

    template<typename TLinkAddress>
    TLinkAddress CreateAndUpdate(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        return Update(storage, Create<TLinkAddress>(storage), source, target);
    }

    template<typename TLinkAddress>
    TLinkAddress GetOrCreate(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto constants = storage.Constants;
        auto link = SearchOrDefault(storage, source, target);
        if (link == constants.Null)
        {
            link = CreateAndUpdate(storage, source, target);
        }
        return link;
    }
    //    template<typename TLinkAddress>
    //    static TLinkAddress GetOrCreate(auto&& storage, TLinkAddress source, TLinkAddress target)
    //    {
    //        auto link = SearchOrDefault(storage, source, target);
    //        if (link == 0)
    //        {
    //            link = CreateAndUpdate(storage, source, target);
    //        }
    //        return link;
    //    }
    //


    template<typename TLinkAddress>
    auto DeleteAll(auto&& storage)
    {
        for (auto count = storage.Count(); count != 0; count = storage.Count())
        {
            storage.Delete(count);
        }
    }

    template<typename TLinkAddress>
    TLinkAddress DeleteIfExists(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        auto constants = storage.Constants;
        auto link = storage.SearchOrDefault(source, target);
        if (link != constants.Null)
        {
            return storage.Delete(link);
        }
        return constants.Null;
    }

    template<typename TLinkAddress>
    auto DeleteByQuery(auto&& storage,  Interfaces::CArray auto&& query)
    {
        auto count = storage.Count(query);
        auto toDelete = std::vector<TLinkAddress>(count);

        storage.Each([&](auto link) {
            toDelete.push_back(link[0]);
            return storage.Constants.Continue;
        }, query);

        for (auto link : toDelete | std::views::reverse)
        {
            storage.Delete(link);
        }
    }

    template<typename TLinkAddress>
    auto ResetValues(auto&& storage,  TLinkAddress link)
    {
        storage.Update(link, 0, 0);
    }

    template<typename TLinkAddress>
    TLinkAddress CreatePoint(auto&& storage)
    {
        auto point = storage.Create();
        return storage.Update(point, point, point);
    }

    template<typename TLinkAddress>
    TLinkAddress GetSource(const auto&& storage, TLinkAddress index) {
        GetLink(index).Source;
    }

    template<typename TLinkAddress>
    TLinkAddress GetTarget(const auto&& storage, TLinkAddress index) {
        GetLink(index).Target;
    }

    template<typename TLinkAddress>
    bool Exists(const auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        return storage.Count(storage.Constants.Any, source, target) != 0;
    }

    template<typename TLinkAddress>
    static bool Exists(auto&& storage, TLinkAddress source, TLinkAddress target)
    {
        return storage.Count(Link{storage.Constants.Any, source, target}) > 0;
    }

    template<typename TLinkAddress>
    TLinkAddress CountUsages(const auto&& storage, TLinkAddress link)
    {
        auto constants = storage.Constants;
        auto values = storage.GetLink(link);

        TLinkAddress usages = 0;
        usages += storage.Count(constants.Any, link, constants.Any) - bool(values.Source == link);
        usages += storage.Count(constants.Any, constants.Any, link) - bool(values.Target == link);
        return usages;
    }

    template<typename TLinkAddress>
    TLinkAddress HasUsages(const auto&& storage, TLinkAddress link)
    {
        return storage.CountUsages(link) != 0;
    }

    template<typename TLinkAddress>
    std::string Format(const auto&& storage, TLinkAddress link)
    {
        auto values = storage.GetLink(link);
        return Format(values);
    }

    template<typename TLinkAddress>
    std::string Format(const auto&& storage, Link<TLinkAddress> link)
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
    //    template<typename TLinkAddress>
    //    static TLinkAddress DeleteIfExists(auto&& storage, TLinkAddress source, TLinkAddress target)
    //    {
    //        auto link = SearchOrDefault(storage, source, target);
    //        if (link != 0)
    //        {
    //            storage.Delete(link);
    //            return link;
    //        }
    //        return 0;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void DeleteMany(auto&& storage, Interfaces::CList auto&& deletedLinks)
    //    {
    //        for (std::int32_t i = 0; i < deletedLinks.Count(); ++i)
    //        {
    //            storage.Delete(deletedLinks[i]);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void DeleteAllUsages(auto&& storage, TLinkAddress linkIndex) { storage.DeleteAllUsages(linkIndex, {}); }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress DeleteAllUsages(auto&& storage, TLinkAddress linkIndex, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        auto any = constants.Any;
    //        auto usagesAsSourceQuery = Link<TLinkAddress>(any, linkIndex, any);
    //        auto usagesAsTargetQuery = Link<TLinkAddress>(any, any, linkIndex);
    //        auto usages = List<IList<TLinkAddress>?>();
    //        auto usagesFiller = ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, constants.Continue);
    //        storage.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
    //        storage.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
    //        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
    //        foreach (auto usage in usages)
    //        {
    //            if ( (linkIndex == storage.GetIndex(usage)) || (!storage.Exists(storage.GetIndex(usage))) )
    //            {
    //                continue;
    //            }
    //            handlerState.Apply(storage.Delete(storage.GetIndex(usage), handlerState.Handler));
    //        }
    //        return handlerState.Result;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void DeleteByQuery(auto&& storage, Link<TLinkAddress> query)
    //    {
    //        auto queryResult = List<TLinkAddress>();
    //        auto queryResultFiller = ListFiller<TLinkAddress, TLinkAddress>(queryResult, storage.Constants.Continue);
    //        storage.Each(queryResultFiller.AddFirstAndReturnConstant, query);
    //        foreach (auto link in queryResult)
    //        {
    //            storage.Delete(link);
    //        }
    //    }
    //
    //    template<typename TLinkAddress>
    //    static bool AreValuesReset(auto&& storage, TLinkAddress linkIndex)
    //    {
    //        auto nullConstant = storage.Constants.Null;
    //        auto link = storage.GetLink(linkIndex);
    //        for (std::int32_t i = 1; i < link.Count(); ++i)
    //        {
    //            if ( nullConstant != link[i])
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void ResetValues(auto&& storage, TLinkAddress linkIndex) { storage.ResetValues(linkIndex, {}); }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress ResetValues(auto&& storage, TLinkAddress linkIndex, Handler handler)
    //    {
    //        auto nullConstant = storage.Constants.Null;
    //        auto updateRequest = Link<TLinkAddress>(linkIndex, nullConstant, nullConstant);
    //        return storage.Update(updateRequest, handler);
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void EnforceResetValues(auto&& storage, TLinkAddress linkIndex) { storage.EnforceResetValues(linkIndex, {}); }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress EnforceResetValues(auto&& storage, TLinkAddress linkIndex, Handler handler)
    //    {
    //        if (!storage.AreValuesReset(linkIndex))
    //        {
    //            return storage.ResetValues(linkIndex, handler);
    //        }
    //        return storage.Constants.Continue;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static void MergeUsages(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex) { storage.MergeUsages(oldLinkIndex, newLinkIndex, {}); }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress MergeUsages(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, Handler handler)
    //    {
    //        if ( newLinkIndex == oldLinkIndex)
    //        {
    //            return newLinkIndex;
    //        }
    //        auto constants = storage.Constants;
    //        auto usagesAsSource = storage.All(Link<TLinkAddress>(constants.Any, oldLinkIndex, constants.Any));
    //        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
    //        for (auto i { 0 }; i < usagesAsSource.Count(); ++i)
    //        {
    //            auto usageAsSource = usagesAsSource[i];
    //            if ( oldLinkIndex == storage.GetIndex(usageAsSource))
    //            {
    //                continue;
    //            }
    //            auto restriction = LinkAddress{storage.GetIndex(usageAsSource});
    //            auto substitution = Link<TLinkAddress>(newLinkIndex, storage.GetTarget(usageAsSource));
    //            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
    //        }
    //        auto usagesAsTarget = storage.All(Link<TLinkAddress>(constants.Any, constants.Any, oldLinkIndex));
    //        for (auto i { 0 }; i < usagesAsTarget.Count(); ++i)
    //        {
    //            auto usageAsTarget = usagesAsTarget[i];
    //            if ( oldLinkIndex == storage.GetIndex(usageAsTarget))
    //            {
    //                continue;
    //            }
    //            auto restriction = storage.GetLink(storage.GetIndex(usageAsTarget));
    //            auto substitution = Link<TLinkAddress>(storage.GetTarget(usageAsTarget), newLinkIndex);
    //            handlerState.Apply(storage.Update(restriction, substitution, handlerState.Handler));
    //        }
    //        return handlerState.Result;
    //    }
    //
    //    template<typename TLinkAddress>
    //    static TLinkAddress MergeAndDelete(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex)
    //    {
    //        if ( newLinkIndex != oldLinkIndex)
    //        {
    //            storage.MergeUsages(oldLinkIndex, newLinkIndex);
    //            storage.Delete(oldLinkIndex);
    //        }
    //        return newLinkIndex;
    //    }
    //
    //    template<typename TLinkAddress, typename Handler, typename TList1, typename TList2>
    //    requires std::invocable<Handler&, Interfaces::CList<TLinkAddress> auto, Interfaces::CList<TLinkAddress> auto>
    //    static TLinkAddress MergeAndDelete(auto&& storage, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, Handler handler)
    //    {
    //        auto constants = storage.Constants;
    //        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
    //        if ( newLinkIndex != oldLinkIndex)
    //        {
    //            handlerState.Apply(storage.MergeUsages(oldLinkIndex, newLinkIndex, handlerState.Handler));
    //            handlerState.Apply(storage.Delete(oldLinkIndex, handlerState.Handler));
    //        }
    //        return handlerState.Result;
    //    }
    //
//        template<typename TLinkAddress>
//        static auto&& DecorateWithAutomaticUniquenessAndUsagesResolution(auto&& storage)
//        {
//            using namespace Platform::Data::Doublets::Decorators;
//            storage = LinksCascadeUsagesResolver<TLinkAddress>(storage);
//            storage = NonNullContentsLinkDeletionResolver<TLinkAddress>(storage);
//            storage = LinksCascadeUniquenessAndUsagesResolver<TLinkAddress>(storage);
//            return storage;
//        }
    //
    //    template<typename TLinkAddress>
    //    static std::string Format(auto&& storage, Interfaces::CList auto&& link)
    //    {
    //        auto constants = storage.Constants;
    //        return "({storage.GetIndex(link)}: {storage.GetSource(link)} {storage.GetTarget(link)})";
    //    }
    //
    //    template<typename TLinkAddress>
    //    static std::string Format(auto&& storage, TLinkAddress link) { return storage.Format(storage.GetLink(link)); }
}

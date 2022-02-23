namespace Platform::Data::Doublets
{
    class ILinksExtensions
    {
    public:
        template<typename TLinkAddress>
        static void RunRandomCreations(ILinks<TLinkAddress> &links, std::uint64_t amountOfCreations) 
        {
            using namespace Platform::Random;
            using namespace Platform::Ranges;
            auto randomGenerator64 = RandomHelpers.Default;
            for (auto i { 0UL }; i < amountOfCreations; ++i)
            {
                Range<std::uint64_t> linksAddressRange { 0, links.Count() };
                auto source = Random::NextUInt64(randomGenerator64, linksAddressRange)
                auto target = Random::NextUInt64(randomGenerator64, linksAddressRange)
                links.GetOrCreate(source, target);
            }
        }

        template<typename TLinkAddress>
        static void RunRandomSearches(ILinks<TLinkAddress> &links, std::uint64_t amountOfSearches) 
        {
            auto random = RandomHelpers.Default;
            for (auto i { 0UL }; i < amountOfSearches; ++i)
            {
                auto linksAddressRange = Range<std::uint64_t>(0, links.Count());
                auto source = Random::NextUInt64(randomGenerator64, linksAddressRange)
                auto target = Random::NextUInt64(randomGenerator64, linksAddressRange)
                links.SearchOrDefault(source, target);
            }
        }

        template<typename TLinkAddress>
        static void RunRandomDeletions(ILinks<TLinkAddress> &links, std::uint64_t amountOfDeletions) 
        {
            auto random = RandomHelpers.Default;
            auto linksCount = links.Count();
            auto min = amountOfDeletions > linksCount ? 0UL : linksCount - amountOfDeletions;
            for (auto i { 0UL }; i < amountOfDeletions; ++i)
            {
                linksCount = links.Count();
                if (linksCount <= min)
                {
                    break;
                }
                auto linksAddressRange = Range<std::uint64_t>(min, linksCount);
                auto link = Random::NextUInt64(randomGenerator64, linksAddressRange)
                links.Delete(link);
            }
        }

        template<typename TLinkAddress>
        static TLinkAddress Delete(ILinks<TLinkAddress> &links, TLinkAddress linkToDelete, WriteHandler<TLinkAddress>? handler) 
        {
            if (links.Exists(linkToDelete))
            {
                links.EnforceResetValues(linkToDelete, handler);
            }
            return links.Delete(LinkAddress<TLinkAddress>(linkToDelete), handler);
        }

        template<typename TLinkAddress>
        static void DeleteAll(ILinks<TLinkAddress> &links) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto comparer = Comparer<TLinkAddress>.Default;
            for (auto i = links.Count(); comparer.Compare(i, 0) > 0; i = i - 1)
            {
                links.Delete(i);
                if (!equalityComparer.Equals(links.Count(), i - 1))
                {
                    i = links.Count();
                }
            }
        }

        template<typename TLinkAddress>
        static TLinkAddress First(ILinks<TLinkAddress> &links) 
        {
            TLinkAddress firstLink = 0;
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (equalityComparer.Equals(links.Count(), 0))
            {
                throw std::runtime_error("В хранилище нет связей.");
            }
            links.Each(Link<TLinkAddress>(links.Constants.Any, links.Constants.Any, links.Constants.Any), link =>
            {
                firstLink = link[links.Constants.IndexPart];
                return links.Constants.Break;
            });
            if (equalityComparer.Equals(firstLink, 0))
            {
                throw std::runtime_error("В процессе поиска по хранилищу не было найдено связей.");
            }
            return firstLink;
        }

        static IList<TLinkAddress>? SingleOrDefault<TLinkAddress>(ILinks<TLinkAddress> &links, IList<TLinkAddress>? query) 
        {
            IList<TLinkAddress>? result = {};
            auto count = 0;
            auto constants = links.Constants;
            auto continue = constants.Continue;
            auto break = constants.Break;
            links.Each(query, linkHandler);
            return result;
            
            TLinkAddress linkHandler(IList<TLinkAddress>? link)
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
        static bool CheckPathExistance(ILinks<TLinkAddress> &links, params TLinkAddress path[]) 
        {
            auto current = path[0];
            if (!links.Exists(current))
            {
                return false;
            }
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto constants = links.Constants;
            for (auto i = 1; i < path.Length; ++i)
            {
                auto next = path[i];
                auto values = links.GetLink(current);
                auto source = links.GetSource(values);
                auto target = links.GetTarget(values);
                if (equalityComparer.Equals(source, target) && equalityComparer.Equals(source, next))
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
        static TLinkAddress GetByKeys(ILinks<TLinkAddress> &links, TLinkAddress root, params std::int32_t path[]) 
        {
            links.EnsureLinkExists(root, "root");
            auto currentLink = root;
            for (auto i = 0; i < path.Length; ++i)
            {
                currentLink = links.GetLink(currentLink)[path[i]];
            }
            return currentLink;
        }

        template<typename TLinkAddress>
        static TLinkAddress GetSquareMatrixSequenceElementByIndex(ILinks<TLinkAddress> &links, TLinkAddress root, std::uint64_t size, std::uint64_t index) 
        {
            auto constants = links.Constants;
            auto source = constants.SourcePart;
            auto target = constants.TargetPart;
            if (!Platform.Numbers.Math.IsPowerOfTwo(size))
            {
                throw std::invalid_argument("size", "Sequences with sizes other than powers of two are not supported.");
            }
            auto path = BitArray(BitConverter.GetBytes(index));
            auto length = Bit.GetLowestPosition(size);
            links.EnsureLinkExists(root, "root");
            auto currentLink = root;
            for (auto i = length - 1; i >= 0; i--)
            {
                currentLink = links.GetLink(currentLink)[path[i] ? target : source];
            }
            return currentLink;
        }

        template<typename TLinkAddress>
        static TLinkAddress GetIndex(ILinks<TLinkAddress> &links, IList<TLinkAddress>? link) { return link[links.Constants.IndexPart]; }

        template<typename TLinkAddress>
        static TLinkAddress GetSource(ILinks<TLinkAddress> &links, TLinkAddress link) { return links.GetLink(link)[links.Constants.SourcePart]; }

        template<typename TLinkAddress>
        static TLinkAddress GetSource(ILinks<TLinkAddress> &links, IList<TLinkAddress>? link) { return link[links.Constants.SourcePart]; }

        template<typename TLinkAddress>
        static TLinkAddress GetTarget(ILinks<TLinkAddress> &links, TLinkAddress link) { return links.GetLink(link)[links.Constants.TargetPart]; }

        template<typename TLinkAddress>
        static TLinkAddress GetTarget(ILinks<TLinkAddress> &links, IList<TLinkAddress>? link) { return link[links.Constants.TargetPart]; }

        static IList<IList<TLinkAddress>?> All<TLinkAddress>(ILinks<TLinkAddress> &links, params TLinkAddress restriction[]) 
        {
            auto allLinks = List<IList<TLinkAddress>?>();
            auto filler = ListFiller<IList<TLinkAddress>?, TLinkAddress>(allLinks, links.Constants.Continue);
            links.Each(filler.AddAndReturnConstant, restriction);
            return allLinks;
        }

        static IList<TLinkAddress>? AllIndices<TLinkAddress>(ILinks<TLinkAddress> &links, params TLinkAddress restriction[]) 
        {
            auto allIndices = List<TLinkAddress>();
            auto filler = ListFiller<TLinkAddress, TLinkAddress>(allIndices, links.Constants.Continue);
            links.Each(filler.AddFirstAndReturnConstant, restriction);
            return allIndices;
        }

        template<typename TLinkAddress>
        static bool Exists(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) { return Comparer<TLinkAddress>.Default.Compare(links.Count()(links.Constants.Any, source, target), 0) > 0; }

        template<typename TLinkAddress>
        static void EnsureLinkExists(ILinks<TLinkAddress> &links, IList<TLinkAddress>? restriction) 
        {
            for (auto i = 0; i < restriction.Count(); ++i)
            {
                if (!links.Exists(restriction[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        template<typename TLinkAddress>
        static void EnsureInnerReferenceExists(ILinks<TLinkAddress> &links, TLinkAddress reference, std::string argumentName) 
        {
            if (links.Constants.IsInternalReference(reference) && !links.Exists(reference))
            {
                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(reference, argumentName);
            }
        }

        template<typename TLinkAddress>
        static void EnsureInnerReferenceExists(ILinks<TLinkAddress> &links, IList<TLinkAddress>? restriction, std::string argumentName) 
        {
            for (std::int32_t i = 0; i < restriction.Count(); ++i)
            {
                links.EnsureInnerReferenceExists(restriction[i], argumentName);
            }
        }

        template<typename TLinkAddress>
        static void EnsureLinkIsAnyOrExists(ILinks<TLinkAddress> &links, IList<TLinkAddress>? restriction) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto any = links.Constants.Any;
            for (auto i = 0; i < restriction.Count(); ++i)
            {
                if (!equalityComparer.Equals(restriction[i], any) && !links.Exists(restriction[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<TLinkAddress>(restriction[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        template<typename TLinkAddress>
        static void EnsureLinkIsAnyOrExists(ILinks<TLinkAddress> &links, TLinkAddress link, std::string argumentName) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Any) && !links.Exists(link))
            {
                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        template<typename TLinkAddress>
        static void EnsureLinkIsItselfOrExists(ILinks<TLinkAddress> &links, TLinkAddress link, std::string argumentName) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Itself) && !links.Exists(link))
            {
                throw ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        template<typename TLinkAddress>
        static void EnsureDoesNotExists(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) 
        {
            if (links.Exists(source, target))
            {
                throw LinkWithSameValueAlreadyExistsException();
            }
        }

        template<typename TLinkAddress>
        static void EnsureNoUsages(ILinks<TLinkAddress> &links, TLinkAddress link) 
        {
            if (links.HasUsages(link))
            {
                throw ArgumentLinkHasDependenciesException<TLinkAddress>(link);
            }
        }

        template<typename TLinkAddress>
        static void EnsureCreated(ILinks<TLinkAddress> &links, params TLinkAddress addresses[]) { links.EnsureCreated(links.Create, addresses); }

        template<typename TLinkAddress>
        static void EnsurePointsCreated(ILinks<TLinkAddress> &links, params TLinkAddress addresses[]) { links.EnsureCreated(links.CreatePoint, addresses); }

        template<typename TLinkAddress>
        static void EnsureCreated(ILinks<TLinkAddress> &links, std::function<TLinkAddress()> creator, params TLinkAddress addresses[]) 
        {
            auto nonExistentAddresses = HashSet<TLinkAddress>(addresses.Where(x => !links.Exists(x)));
            if (nonExistentAddresses.Count() > 0)
            {
                auto max = nonExistentAddresses.Max();
                max = System::Math::Min(max), addressToUInt64Converter.Convert(links.Constants.InternalReferencesRange.Maximum)
                auto createdLinks = List<TLinkAddress>();
                auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
                TLinkAddress createdLink = creator();
                while (!equalityComparer.Equals(createdLink, max))
                {
                    createdLinks.Add(createdLink);
                }
                for (auto i = 0; i < createdLinks.Count(); ++i)
                {
                    if (!nonExistentAddresses.Contains(createdLinks[i]))
                    {
                        links.Delete(createdLinks[i]);
                    }
                }
            }
        }

        template<typename TLinkAddress>
        static TLinkAddress CountUsages(ILinks<TLinkAddress> &links, TLinkAddress link) 
        {
            auto constants = links.Constants;
            auto values = links.GetLink(link);
            TLinkAddress usagesAsSource = links.Count()(Link<TLinkAddress>(constants.Any, link, constants.Any));
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (equalityComparer.Equals(links.GetSource(values), link))
            {
                usagesAsSource = Arithmetic<TLinkAddress>.Decrement(usagesAsSource);
            }
            TLinkAddress usagesAsTarget = links.Count()(Link<TLinkAddress>(constants.Any, constants.Any, link));
            if (equalityComparer.Equals(links.GetTarget(values), link))
            {
                usagesAsTarget = Arithmetic<TLinkAddress>.Decrement(usagesAsTarget);
            }
            return Arithmetic<TLinkAddress>.Add(usagesAsSource, usagesAsTarget);
        }

        template<typename TLinkAddress>
        static bool HasUsages(ILinks<TLinkAddress> &links, TLinkAddress link) { return Comparer<TLinkAddress>.Default.Compare(links.Count()Usages(link), 0) > 0; }

        template<typename TLinkAddress>
        static bool operator ==(const ILinks<TLinkAddress> &links, TLinkAddress link, TLinkAddress source, TLinkAddress &target) const 
        {
            auto constants = links.Constants;
            auto values = links.GetLink(link);
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            return equalityComparer.Equals(links.GetSource(values), source) && equalityComparer.Equals(links.GetTarget(values), target);
        }

        template<typename TLinkAddress>
        static TLinkAddress SearchOrDefault(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) 
        {
            auto contants = links.Constants;
            auto setter = Setter<TLinkAddress, TLinkAddress>(contants.Continue, contants.Break, 0);
            links.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
            return setter.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress CreatePoint(ILinks<TLinkAddress> &links) 
        {
            auto constants = links.Constants;
            auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreatePoint(setter.SetFirstFromSecondListAndReturnTrue);
            return setter.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress CreatePoint(ILinks<TLinkAddress> &links, WriteHandler<TLinkAddress>? handler) 
        {
            auto constants = links.Constants;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            TLinkAddress link = 0;
            auto HandlerWrapper = [&]() -> TLinkAddress {
                link = links.GetIndex(after);
                return handlerState.Handle(before, after);;
            };
            handlerState.Apply(links.Create({}, HandlerWrapper));
            handlerState.Apply(links.Update(link, link, link, HandlerWrapper));
            return handlerState.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress CreateAndUpdate(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) 
        {
            auto constants = links.Constants;
            auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.CreateAndUpdate(source, target, setter.SetFirstFromSecondListAndReturnTrue);
            return setter.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress CreateAndUpdate(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target, WriteHandler<TLinkAddress>? handler) 
        {
            auto constants = links.Constants;
            TLinkAddress createdLink = 0;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            handlerState.Apply(links.Create({}, (before, after) =>
            {
                createdLink = links.GetIndex(after);
                return handlerState.Handle(before, after);;
            }));
            handlerState.Apply(links.Update(createdLink, source, target, handler));
            return handlerState.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget) { return links.Update(LinkAddress<TLinkAddress>(link), Link<TLinkAddress>(link, newSource, newTarget)); }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, params TLinkAddress restriction[]) { return links.Update((IList<TLinkAddress>)restriction); }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, WriteHandler<TLinkAddress>? handler, params TLinkAddress restriction[]) { return links.Update(restriction, handler); }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, IList<TLinkAddress>? restriction) 
        {
            auto constants = links.Constants;
            auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.Update(restriction, setter.SetFirstFromSecondListAndReturnTrue);
            return setter.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler) 
        {
            return restriction.Count() switch
            {
                2 => links.MergeAndDelete(restriction[0], restriction[1], handler),
                4 => links.UpdateOrCreateOrGet(restriction[0], restriction[1], restriction[2], restriction[3], handler),
                _ => links.Update(restriction[0], restriction[1], restriction[2], handler)
            };
        }

        template<typename TLinkAddress>
        static TLinkAddress Update(ILinks<TLinkAddress> &links, TLinkAddress link, TLinkAddress newSource, TLinkAddress newTarget, WriteHandler<TLinkAddress>? handler) { return links.Update(LinkAddress<TLinkAddress>(link), Link<TLinkAddress>(link, newSource, newTarget), handler); }

        static IList<TLinkAddress>? ResolveConstantAsSelfReference<TLinkAddress>(ILinks<TLinkAddress> &links, TLinkAddress constant, IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto constants = links.Constants;
            auto restrictionIndex = links.GetIndex(restriction);
            auto substitutionIndex = links.GetIndex(substitution);
            if (equalityComparer.Equals(substitutionIndex, 0))
            {
                substitutionIndex = restrictionIndex;
            }
            auto source = links.GetSource(substitution);
            auto target = links.GetTarget(substitution);
            source = equalityComparer.Equals(source, constant) ? substitutionIndex : source;
            target = equalityComparer.Equals(target, constant) ? substitutionIndex : target;
            return Link<TLinkAddress>(substitutionIndex, source, target);
        }

        template<typename TLinkAddress>
        static TLinkAddress GetOrCreate(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) 
        {
            auto link = links.SearchOrDefault(source, target);
            if (link == 0)
            {
                link = links.CreateAndUpdate(source, target);
            }
            return link;
        }

        template<typename TLinkAddress>
        static TLinkAddress UpdateOrCreateOrGet(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget) 
        {
            auto constants = links.Constants;
            auto setter = Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break);
            links.UpdateOrCreateOrGet(source, target, newSource, newTarget, setter.SetFirstFromSecondListAndReturnTrue);
            return setter.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress UpdateOrCreateOrGet(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target, TLinkAddress newSource, TLinkAddress newTarget, WriteHandler<TLinkAddress>? handler) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto link = links.SearchOrDefault(source, target);
            if (equalityComparer.Equals(link, 0))
            {
                return links.CreateAndUpdate(newSource, newTarget, handler);
            }
            if (equalityComparer.Equals(newSource, source) && equalityComparer.Equals(newTarget, target))
            {
                auto linkStruct = Link<TLinkAddress>(link, source, target);
                return link;
            }
            return links.Update(link, newSource, newTarget, handler);
        }

        template<typename TLinkAddress>
        static TLinkAddress DeleteIfExists(ILinks<TLinkAddress> &links, TLinkAddress source, TLinkAddress target) 
        {
            auto link = links.SearchOrDefault(source, target);
            if (link != 0)
            {
                links.Delete(link);
                return link;
            }
            return 0;
        }

        template<typename TLinkAddress>
        static void DeleteMany(ILinks<TLinkAddress> &links, IList<TLinkAddress>? deletedLinks) 
        {
            for (std::int32_t i = 0; i < deletedLinks.Count(); ++i)
            {
                links.Delete(deletedLinks[i]);
            }
        }

        template<typename TLinkAddress>
        static void DeleteAllUsages(ILinks<TLinkAddress> &links, TLinkAddress linkIndex) { links.DeleteAllUsages(linkIndex, {}); }

        template<typename TLinkAddress>
        static TLinkAddress DeleteAllUsages(ILinks<TLinkAddress> &links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler) 
        {
            auto constants = links.Constants;
            auto any = constants.Any;
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto usagesAsSourceQuery = Link<TLinkAddress>(any, linkIndex, any);
            auto usagesAsTargetQuery = Link<TLinkAddress>(any, any, linkIndex);
            auto usages = List<IList<TLinkAddress>?>();
            auto usagesFiller = ListFiller<IList<TLinkAddress>?, TLinkAddress>(usages, constants.Continue);
            links.Each(usagesFiller.AddAndReturnConstant, usagesAsSourceQuery);
            links.Each(usagesFiller.AddAndReturnConstant, usagesAsTargetQuery);
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            foreach (auto usage in usages)
            {
                if (equalityComparer.Equals(links.GetIndex(usage), linkIndex) || !links.Exists(links.GetIndex(usage)))
                {
                    continue;
                }
                handlerState.Apply(links.Delete(links.GetIndex(usage), handlerState.Handler));
            }
            return handlerState.Result;
        }

        template<typename TLinkAddress>
        static void DeleteByQuery(ILinks<TLinkAddress> &links, Link<TLinkAddress> query) 
        {
            auto queryResult = List<TLinkAddress>();
            auto queryResultFiller = ListFiller<TLinkAddress, TLinkAddress>(queryResult, links.Constants.Continue);
            links.Each(queryResultFiller.AddFirstAndReturnConstant, query);
            foreach (auto link in queryResult)
            {
                links.Delete(link);
            }
        }

        template<typename TLinkAddress>
        static bool AreValuesReset(ILinks<TLinkAddress> &links, TLinkAddress linkIndex) 
        {
            auto nullConstant = links.Constants.Null;
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto link = links.GetLink(linkIndex);
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
        static void ResetValues(ILinks<TLinkAddress> &links, TLinkAddress linkIndex) { links.ResetValues(linkIndex, {}); }

        template<typename TLinkAddress>
        static TLinkAddress ResetValues(ILinks<TLinkAddress> &links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler) 
        {
            auto nullConstant = links.Constants.Null;
            auto updateRequest = Link<TLinkAddress>(linkIndex, nullConstant, nullConstant);
            return links.Update(updateRequest, handler);
        }

        template<typename TLinkAddress>
        static void EnforceResetValues(ILinks<TLinkAddress> &links, TLinkAddress linkIndex) { links.EnforceResetValues(linkIndex, {}); }

        template<typename TLinkAddress>
        static TLinkAddress EnforceResetValues(ILinks<TLinkAddress> &links, TLinkAddress linkIndex, WriteHandler<TLinkAddress>? handler) 
        {
            if (!links.AreValuesReset(linkIndex))
            {
                return links.ResetValues(linkIndex, handler);
            }
            return links.Constants.Continue;
        }

        template<typename TLinkAddress>
        static void MergeUsages(ILinks<TLinkAddress> &links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex) { links.MergeUsages(oldLinkIndex, newLinkIndex, {}); }

        template<typename TLinkAddress>
        static TLinkAddress MergeUsages(ILinks<TLinkAddress> &links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, WriteHandler<TLinkAddress>? handler) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                return newLinkIndex;
            }
            auto constants = links.Constants;
            auto usagesAsSource = links.All(Link<TLinkAddress>(constants.Any, oldLinkIndex, constants.Any));
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            for (auto i = 0; i < usagesAsSource.Count(); ++i)
            {
                auto usageAsSource = usagesAsSource[i];
                if (equalityComparer.Equals(links.GetIndex(usageAsSource), oldLinkIndex))
                {
                    continue;
                }
                auto restriction = LinkAddress<TLinkAddress>(links.GetIndex(usageAsSource));
                auto substitution = Link<TLinkAddress>(newLinkIndex, links.GetTarget(usageAsSource));
                handlerState.Apply(links.Update(restriction, substitution, handlerState.Handler));
            }
            auto usagesAsTarget = links.All(Link<TLinkAddress>(constants.Any, constants.Any, oldLinkIndex));
            for (auto i = 0; i < usagesAsTarget.Count(); ++i)
            {
                auto usageAsTarget = usagesAsTarget[i];
                if (equalityComparer.Equals(links.GetIndex(usageAsTarget), oldLinkIndex))
                {
                    continue;
                }
                auto restriction = links.GetLink(links.GetIndex(usageAsTarget));
                auto substitution = Link<TLinkAddress>(links.GetTarget(usageAsTarget), newLinkIndex);
                handlerState.Apply(links.Update(restriction, substitution, handlerState.Handler));
            }
            return handlerState.Result;
        }

        template<typename TLinkAddress>
        static TLinkAddress MergeAndDelete(ILinks<TLinkAddress> &links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                links.MergeUsages(oldLinkIndex, newLinkIndex);
                links.Delete(oldLinkIndex);
            }
            return newLinkIndex;
        }

        template<typename TLinkAddress>
        static TLinkAddress MergeAndDelete(ILinks<TLinkAddress> &links, TLinkAddress oldLinkIndex, TLinkAddress newLinkIndex, WriteHandler<TLinkAddress>? handler) 
        {
            auto equalityComparer = EqualityComparer<TLinkAddress>.Default;
            auto constants = links.Constants;
            WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                handlerState.Apply(links.MergeUsages(oldLinkIndex, newLinkIndex, handlerState.Handler));
                handlerState.Apply(links.Delete(oldLinkIndex, handlerState.Handler));
            }
            return handlerState.Result;
        }

        static ILinks<TLinkAddress> DecorateWithAutomaticUniquenessAndUsagesResolution<TLinkAddress>(ILinks<TLinkAddress> &links) 
        {
            links = LinksCascadeUsagesResolver<TLinkAddress>(links);
            links = NonNullContentsLinkDeletionResolver<TLinkAddress>(links);
            links = LinksCascadeUniquenessAndUsagesResolver<TLinkAddress>(links);
            return links;
        }

        template<typename TLinkAddress>
        static std::string Format(ILinks<TLinkAddress> &links, IList<TLinkAddress>? link) 
        {
            auto constants = links.Constants;
            return "({links.GetIndex(link)}: {links.GetSource(link)} {links.GetTarget(link)})";
        }

        template<typename TLinkAddress>
        static std::string Format(ILinks<TLinkAddress> &links, TLinkAddress link) { return links.Format(links.GetLink(link)); }
    };
}

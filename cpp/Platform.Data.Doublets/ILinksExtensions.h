namespace Platform::Data::Doublets
{
    class ILinksExtensions
    {
        public: template <typename TLink> static void RunRandomCreations(ILinks<TLink> &links, std::uint64_t amountOfCreations)
        {
            auto random = RandomHelpers.Default;
            auto addressToUInt64Converter = UncheckedConverter<TLink, std::uint64_t>.Default;
            auto uInt64ToAddressConverter = UncheckedConverter<std::uint64_t, TLink>.Default;
            for (auto i = 0UL; i < amountOfCreations; i++)
            {
                auto linksAddressRange = Range<std::uint64_t>(0, addressToUInt64Converter.Convert(links.Count()()));
                auto source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                auto target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.GetOrCreate(source, target);
            }
        }

        public: template <typename TLink> static void RunRandomSearches(ILinks<TLink> &links, std::uint64_t amountOfSearches)
        {
            auto random = RandomHelpers.Default;
            auto addressToUInt64Converter = UncheckedConverter<TLink, std::uint64_t>.Default;
            auto uInt64ToAddressConverter = UncheckedConverter<std::uint64_t, TLink>.Default;
            for (auto i = 0UL; i < amountOfSearches; i++)
            {
                auto linksAddressRange = Range<std::uint64_t>(0, addressToUInt64Converter.Convert(links.Count()()));
                auto source = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                auto target = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.SearchOrDefault(source, target);
            }
        }

        public: template <typename TLink> static void RunRandomDeletions(ILinks<TLink> &links, std::uint64_t amountOfDeletions)
        {
            auto random = RandomHelpers.Default;
            auto addressToUInt64Converter = UncheckedConverter<TLink, std::uint64_t>.Default;
            auto uInt64ToAddressConverter = UncheckedConverter<std::uint64_t, TLink>.Default;
            auto linksCount = addressToUInt64Converter.Convert(links.Count()());
            auto min = amountOfDeletions > linksCount ? 0UL : linksCount - amountOfDeletions;
            for (auto i = 0UL; i < amountOfDeletions; i++)
            {
                linksCount = addressToUInt64Converter.Convert(links.Count()());
                if (linksCount <= min)
                {
                    break;
                }
                auto linksAddressRange = Range<std::uint64_t>(min, linksCount);
                auto link = uInt64ToAddressConverter.Convert(random.NextUInt64(linksAddressRange));
                links.Delete(link);
            }
        }

        public: template <typename TLink> static void Delete(ILinks<TLink> &links, TLink linkToDelete) { links.Delete(LinkAddress<TLink>(linkToDelete)); }

        public: template <typename TLink> static void DeleteAll(ILinks<TLink> &links)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto comparer = Comparer<TLink>.Default;
            for (auto i = links.Count()(); comparer.Compare(i, 0) > 0; i = i - 1)
            {
                links.Delete(i);
                if (!equalityComparer.Equals(links.Count()(), i - 1))
                {
                    i = links.Count()();
                }
            }
        }

        public: template <typename TLink> static TLink First(ILinks<TLink> &links)
        {
            TLink firstLink = 0;
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (equalityComparer.Equals(links.Count()(), 0))
            {
                throw std::runtime_error("В хранилище нет связей.");
            }
            links.Each(links.Constants.Any, links.Constants.Any, link =>
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

        public: static IList<TLink> SingleOrDefault<TLink>(ILinks<TLink> &links, IList<TLink> &query)
        {
            IList<TLink> *result = {};
            auto count = 0;
            auto constants = links.Constants;
            auto continue = constants.Continue;
            auto break = constants.Break;
            links.Each(linkHandler, query);
            return result;
            
            TLink linkHandler(IList<TLink> &link)
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

        public: template <typename TLink> static bool CheckPathExistence(ILinks<TLink> &links, params TLink path[])
        {
            auto current = path[0];
            if (!links.Exists(current))
            {
                return false;
            }
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto constants = links.Constants;
            for (auto i = 1; i < path.Length; i++)
            {
                auto next = path[i];
                auto values = links.GetLink(current);
                auto source = values[constants.SourcePart];
                auto target = values[constants.TargetPart];
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

        public: template <typename TLink> static TLink GetByKeys(ILinks<TLink> &links, TLink root, params std::int32_t path[])
        {
            links.EnsureLinkExists(root, "root");
            auto currentLink = root;
            for (auto i = 0; i < path.Length; i++)
            {
                currentLink = links.GetLink(currentLink)[path[i]];
            }
            return currentLink;
        }

        public: template <typename TLink> static TLink GetSquareMatrixSequenceElementByIndex(ILinks<TLink> &links, TLink root, std::uint64_t size, std::uint64_t index)
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

        public: template <typename TLink> static TLink GetIndex(ILinks<TLink> &links, IList<TLink> &link) { return link[links.Constants.IndexPart]; }

        public: template <typename TLink> static TLink GetSource(ILinks<TLink> &links, TLink link) { return links.GetLink(link)[links.Constants.SourcePart]; }

        public: template <typename TLink> static TLink GetSource(ILinks<TLink> &links, IList<TLink> &link) { return link[links.Constants.SourcePart]; }

        public: template <typename TLink> static TLink GetTarget(ILinks<TLink> &links, TLink link) { return links.GetLink(link)[links.Constants.TargetPart]; }

        public: template <typename TLink> static TLink GetTarget(ILinks<TLink> &links, IList<TLink> &link) { return link[links.Constants.TargetPart]; }

        public: template <typename TLink> static bool Each(ILinks<TLink> &links, Func<IList<TLink>, TLink> handler, params TLink restrictions[]) { return (links.Each(handler) == restrictions, links.Constants.Continue); }

        public: template <typename TLink> static bool Each(ILinks<TLink> &links, TLink source, TLink target, Func<TLink, bool> handler)
        {
            auto constants = links.Constants;
            return links.Each(link => handler(link[constants.IndexPart]) ? constants.Continue : constants.Break, constants.Any, source, target);
        }

        public: template <typename TLink> static bool Each(ILinks<TLink> &links, TLink source, TLink target, Func<IList<TLink>, TLink> handler) { return links.Each(handler, links.Constants.Any, source, target); }

        public: static IList<IList<TLink>> All<TLink>(ILinks<TLink> &links, params TLink restrictions[])
        {
            auto arraySize = CheckedConverter<TLink, std::uint64_t>.Default.Convert(links.Count()(restrictions));
            if (arraySize > 0)
            {
                auto array = IList<TLink>[arraySize];
                auto filler = ArrayFiller<IList<TLink>, TLink>(array, links.Constants.Continue);
                links.Each(filler.AddAndReturnConstant, restrictions);
                return array;
            }
            else
            {
                return Array.Empty<IList<TLink>>();
            }
        }

        public: static IList<TLink> AllIndices<TLink>(ILinks<TLink> &links, params TLink restrictions[])
        {
            auto arraySize = CheckedConverter<TLink, std::uint64_t>.Default.Convert(links.Count()(restrictions));
            if (arraySize > 0)
            {
                TLink array[arraySize] = { {0} };
                auto filler = ArrayFiller<TLink, TLink>(array, links.Constants.Continue);
                links.Each(filler.AddFirstAndReturnConstant, restrictions);
                return array;
            }
            else
            {
                return Array.Empty<TLink>();
            }
        }

        public: template <typename TLink> static bool Exists(ILinks<TLink> &links, TLink source, TLink target) { return Comparer<TLink>.Default.Compare(links.Count()(links.Constants.Any, source, target), 0) > 0; }
        // CountUsages

        public: template <typename TLink> static void EnsureLinkExists(ILinks<TLink> &links, IList<TLink> &restrictions)
        {
            for (auto i = 0; i < restrictions.Count(); i++)
            {
                if (!links.Exists(restrictions[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<TLink>(restrictions[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        public: template <typename TLink> static void EnsureInnerReferenceExists(ILinks<TLink> &links, TLink reference, std::string argumentName)
        {
            if (links.Constants.IsInternalReference(reference) && !links.Exists(reference))
            {
                throw ArgumentLinkDoesNotExistsException<TLink>(reference, argumentName);
            }
        }

        public: template <typename TLink> static void EnsureInnerReferenceExists(ILinks<TLink> &links, IList<TLink> &restrictions, std::string argumentName)
        {
            for (std::int32_t i = 0; i < restrictions.Count(); i++)
            {
                links.EnsureInnerReferenceExists(restrictions[i], argumentName);
            }
        }

        public: template <typename TLink> static void EnsureLinkIsAnyOrExists(ILinks<TLink> &links, IList<TLink> &restrictions)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto any = links.Constants.Any;
            for (auto i = 0; i < restrictions.Count(); i++)
            {
                if (!equalityComparer.Equals(restrictions[i], any) && !links.Exists(restrictions[i]))
                {
                    throw ArgumentLinkDoesNotExistsException<TLink>(restrictions[i], std::string("sequence[").append(Platform::Converters::To<std::string>(i)).append(1, ']'));
                }
            }
        }

        public: template <typename TLink> static void EnsureLinkIsAnyOrExists(ILinks<TLink> &links, TLink link, std::string argumentName)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Any) && !links.Exists(link))
            {
                throw ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
            }
        }

        public: template <typename TLink> static void EnsureLinkIsItselfOrExists(ILinks<TLink> &links, TLink link, std::string argumentName)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(link, links.Constants.Itself) && !links.Exists(link))
            {
                throw ArgumentLinkDoesNotExistsException<TLink>(link, argumentName);
            }
        }

        public: template <typename TLink> static void EnsureDoesNotExists(ILinks<TLink> &links, TLink source, TLink target)
        {
            if (links.Exists(source, target))
            {
                throw LinkWithSameValueAlreadyExistsException();
            }
        }

        public: template <typename TLink> static void EnsureNoUsages(ILinks<TLink> &links, TLink link)
        {
            if (links.HasUsages(link))
            {
                throw ArgumentLinkHasDependenciesException<TLink>(link);
            }
        }

        public: template <typename TLink> static void EnsureCreated(ILinks<TLink> &links, params TLink addresses[]) { links.EnsureCreated(links.Create, addresses); }

        public: template <typename TLink> static void EnsurePointsCreated(ILinks<TLink> &links, params TLink addresses[]) { links.EnsureCreated(links.CreatePoint, addresses); }

        public: template <typename TLink> static void EnsureCreated(ILinks<TLink> &links, std::function<TLink()> creator, params TLink addresses[])
        {
            auto addressToUInt64Converter = CheckedConverter<TLink, std::uint64_t>.Default;
            auto uInt64ToAddressConverter = CheckedConverter<std::uint64_t, TLink>.Default;
            auto nonExistentAddresses = HashSet<TLink>(addresses.Where(x => !links.Exists(x)));
            if (nonExistentAddresses.Count() > 0)
            {
                auto max = nonExistentAddresses.Max();
                max = uInt64ToAddressConverter.Convert(System::Math::Min(addressToUInt64Converter.Convert(max), addressToUInt64Converter.Convert(links.Constants.InternalReferencesRange.Maximum)));
                auto createdLinks = List<TLink>();
                auto equalityComparer = EqualityComparer<TLink>.Default;
                TLink createdLink = creator();
                while (!equalityComparer.Equals(createdLink, max))
                {
                    createdLinks.Add(createdLink);
                }
                for (auto i = 0; i < createdLinks.Count(); i++)
                {
                    if (!nonExistentAddresses.Contains(createdLinks[i]))
                    {
                        links.Delete(createdLinks[i]);
                    }
                }
            }
        }

        public: template <typename TLink> static TLink CountUsages(ILinks<TLink> &links, TLink link)
        {
            auto constants = links.Constants;
            auto values = links.GetLink(link);
            TLink usagesAsSource = links.Count()(Link<TLink>(constants.Any, link, constants.Any));
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (equalityComparer.Equals(values[constants.SourcePart], link))
            {
                usagesAsSource = Arithmetic<TLink>.Decrement(usagesAsSource);
            }
            TLink usagesAsTarget = links.Count()(Link<TLink>(constants.Any, constants.Any, link));
            if (equalityComparer.Equals(values[constants.TargetPart], link))
            {
                usagesAsTarget = Arithmetic<TLink>.Decrement(usagesAsTarget);
            }
            return Arithmetic<TLink>.Add(usagesAsSource, usagesAsTarget);
        }

        public: template <typename TLink> static bool HasUsages(ILinks<TLink> &links, TLink link) { return Comparer<TLink>.Default.Compare(links.Count()Usages(link), 0) > 0; }

        public: template <typename TLink> static bool operator ==(const ILinks<TLink> &links, TLink link, TLink source, TLink &target) const
        {
            auto constants = links.Constants;
            auto values = links.GetLink(link);
            auto equalityComparer = EqualityComparer<TLink>.Default;
            return equalityComparer.Equals(values[constants.SourcePart], source) && equalityComparer.Equals(values[constants.TargetPart], target);
        }

        public: template <typename TLink> static TLink SearchOrDefault(ILinks<TLink> &links, TLink source, TLink target)
        {
            auto contants = links.Constants;
            auto setter = Setter<TLink, TLink>(contants.Continue, contants.Break, 0);
            links.Each(setter.SetFirstAndReturnFalse, contants.Any, source, target);
            return setter.Result;
        }

        public: template <typename TLink> static TLink Create(ILinks<TLink> &links) { return links.Create({}); }

        public: template <typename TLink> static TLink CreatePoint(ILinks<TLink> &links)
        {
            auto link = links.Create();
            return links.Update(link, link, link);
        }

        public: template <typename TLink> static TLink CreateAndUpdate(ILinks<TLink> &links, TLink source, TLink target) { return links.Update(links.Create(), source, target); }

        public: template <typename TLink> static TLink Update(ILinks<TLink> &links, TLink link, TLink newSource, TLink newTarget) { return links.Update(LinkAddress<TLink>(link), Link<TLink>(link, newSource, newTarget)); }

        public: template <typename TLink> static TLink Update(ILinks<TLink> &links, params TLink restrictions[])
        {
            if (restrictions.Length == 2)
            {
                return links.MergeAndDelete(restrictions[0], restrictions[1]);
            }
            if (restrictions.Length == 4)
            {
                return links.UpdateOrCreateOrGet(restrictions[0], restrictions[1], restrictions[2], restrictions[3]);
            }
            else
            {
                return links.Update(LinkAddress<TLink>(restrictions[0]), restrictions);
            }
        }

        public: static IList<TLink> ResolveConstantAsSelfReference<TLink>(ILinks<TLink> &links, TLink constant, IList<TLink> &restrictions, IList<TLink> &substitution)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto constants = links.Constants;
            auto restrictionsIndex = restrictions[constants.IndexPart];
            auto substitutionIndex = substitution[constants.IndexPart];
            if (equalityComparer.Equals(substitutionIndex, 0))
            {
                substitutionIndex = restrictionsIndex;
            }
            auto source = substitution[constants.SourcePart];
            auto target = substitution[constants.TargetPart];
            source = equalityComparer.Equals(source, constant) ? substitutionIndex : source;
            target = equalityComparer.Equals(target, constant) ? substitutionIndex : target;
            return Link<TLink>(substitutionIndex, source, target);
        }

        public: template <typename TLink> static TLink GetOrCreate(ILinks<TLink> &links, TLink source, TLink target)
        {
            auto link = links.SearchOrDefault(source, target);
            if (link == 0)
            {
                link = links.CreateAndUpdate(source, target);
            }
            return link;
        }

        public: template <typename TLink> static TLink UpdateOrCreateOrGet(ILinks<TLink> &links, TLink source, TLink target, TLink newSource, TLink newTarget)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto link = links.SearchOrDefault(source, target);
            if (equalityComparer.Equals(link, 0))
            {
                return links.CreateAndUpdate(newSource, newTarget);
            }
            if (equalityComparer.Equals(newSource, source) && equalityComparer.Equals(newTarget, target))
            {
                return link;
            }
            return links.Update(link, newSource, newTarget);
        }

        public: template <typename TLink> static TLink DeleteIfExists(ILinks<TLink> &links, TLink source, TLink target)
        {
            auto link = links.SearchOrDefault(source, target);
            if (link != 0)
            {
                links.Delete(link);
                return link;
            }
            return 0;
        }

        public: template <typename TLink> static void DeleteMany(ILinks<TLink> &links, IList<TLink> &deletedLinks)
        {
            for (std::int32_t i = 0; i < deletedLinks.Count(); i++)
            {
                links.Delete(deletedLinks[i]);
            }
        }

        public: template <typename TLink> static void DeleteAllUsages(ILinks<TLink> &links, TLink linkIndex)
        {
            auto anyConstant = links.Constants.Any;
            auto usagesAsSourceQuery = Link<TLink>(anyConstant, linkIndex, anyConstant);
            links.DeleteByQuery(usagesAsSourceQuery);
            auto usagesAsTargetQuery = Link<TLink>(anyConstant, linkIndex, anyConstant);
            links.DeleteByQuery(usagesAsTargetQuery);
        }

        public: template <typename TLink> static void DeleteByQuery(ILinks<TLink> &links, Link<TLink> query)
        {
            auto count = CheckedConverter<TLink, std::int64_t>.Default.Convert(links.Count()(query));
            if (count > 0)
            {
                TLink queryResult[count] = { {0} };
                auto queryResultFiller = ArrayFiller<TLink, TLink>(queryResult, links.Constants.Continue);
                links.Each(queryResultFiller.AddFirstAndReturnConstant, query);
                for (auto i = count - 1; i >= 0; i--)
                {
                    links.Delete(queryResult[i]);
                }
            }
        }

        public: template <typename TLink> static bool AreValuesReset(ILinks<TLink> &links, TLink linkIndex)
        {
            auto nullConstant = links.Constants.Null;
            auto equalityComparer = EqualityComparer<TLink>.Default;
            auto link = links.GetLink(linkIndex);
            for (std::int32_t i = 1; i < link.Count(); i++)
            {
                if (!equalityComparer.Equals(link[i], nullConstant))
                {
                    return false;
                }
            }
            return true;
        }

        public: template <typename TLink> static void ResetValues(ILinks<TLink> &links, TLink linkIndex)
        {
            auto nullConstant = links.Constants.Null;
            auto updateRequest = Link<TLink>(linkIndex, nullConstant, nullConstant);
            links.Update(updateRequest);
        }

        public: template <typename TLink> static void EnforceResetValues(ILinks<TLink> &links, TLink linkIndex)
        {
            if (!links.AreValuesReset(linkIndex))
            {
                links.ResetValues(linkIndex);
            }
        }

        public: template <typename TLink> static TLink MergeUsages(ILinks<TLink> &links, TLink oldLinkIndex, TLink newLinkIndex)
        {
            auto addressToInt64Converter = CheckedConverter<TLink, std::int64_t>.Default;
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                auto constants = links.Constants;
                auto usagesAsSourceQuery = Link<TLink>(constants.Any, oldLinkIndex, constants.Any);
                auto usagesAsSourceCount = addressToInt64Converter.Convert(links.Count()(usagesAsSourceQuery));
                auto usagesAsTargetQuery = Link<TLink>(constants.Any, constants.Any, oldLinkIndex);
                auto usagesAsTargetCount = addressToInt64Converter.Convert(links.Count()(usagesAsTargetQuery));
                auto isStandalonePoint = Point<TLink>.IsFullPoint(links.GetLink(oldLinkIndex)) && usagesAsSourceCount == 1 && usagesAsTargetCount == 1;
                if (!isStandalonePoint)
                {
                    auto totalUsages = usagesAsSourceCount + usagesAsTargetCount;
                    if (totalUsages > 0)
                    {
                        auto usages = ArrayPool.Allocate<TLink>(totalUsages);
                        auto usagesFiller = ArrayFiller<TLink, TLink>(usages, links.Constants.Continue);
                        auto i = 0L;
                        if (usagesAsSourceCount > 0)
                        {
                            links.Each(usagesFiller.AddFirstAndReturnConstant, usagesAsSourceQuery);
                            for (; i < usagesAsSourceCount; i++)
                            {
                                auto usage = usages[i];
                                if (!equalityComparer.Equals(usage, oldLinkIndex))
                                {
                                    links.Update(usage, newLinkIndex, links.GetTarget(usage));
                                }
                            }
                        }
                        if (usagesAsTargetCount > 0)
                        {
                            links.Each(usagesFiller.AddFirstAndReturnConstant, usagesAsTargetQuery);
                            for (; i < usages.Length; i++)
                            {
                                auto usage = usages[i];
                                if (!equalityComparer.Equals(usage, oldLinkIndex))
                                {
                                    links.Update(usage, links.GetSource(usage), newLinkIndex);
                                }
                            }
                        }
                        ArrayPool.Free(usages);
                    }
                }
            }
            return newLinkIndex;
        }

        public: template <typename TLink> static TLink MergeAndDelete(ILinks<TLink> &links, TLink oldLinkIndex, TLink newLinkIndex)
        {
            auto equalityComparer = EqualityComparer<TLink>.Default;
            if (!equalityComparer.Equals(oldLinkIndex, newLinkIndex))
            {
                links.MergeUsages(oldLinkIndex, newLinkIndex);
                links.Delete(oldLinkIndex);
            }
            return newLinkIndex;
        }

        public: static ILinks<TLink> DecorateWithAutomaticUniquenessAndUsagesResolution<TLink>(ILinks<TLink> &links)
        {
            links = LinksCascadeUsagesResolver<TLink>(links);
            links = NonNullContentsLinkDeletionResolver<TLink>(links);
            links = LinksCascadeUniquenessAndUsagesResolver<TLink>(links);
            return links;
        }

        public: template <typename TLink> static std::string Format(ILinks<TLink> &links, IList<TLink> &link)
        {
            auto constants = links.Constants;
            return "({link[constants.IndexPart]}: {link[constants.SourcePart]} {link[constants.TargetPart]})";
        }

        public: template <typename TLink> static std::string Format(ILinks<TLink> &links, TLink link) { return links.Format(links.GetLink(link)); }
    };
}

namespace Platform::Data::Doublets
{
    template<typename Self, typename TLink>
    struct ILinks : public Data::ILinks<Self, TLink, LinksConstants<TLink>>
    {
        using base = Data::ILinks<Self, TLink, LinksConstants<TLink>>;

        public: using base::Exists;

        void TestRandomCreationsAndDeletions(std::size_t maximumOperationsPerCycle)
        {
            using namespace Platform::Random;

            auto&& links = *this;
            for (auto N = 1; N < maximumOperationsPerCycle; N++)
            {
                auto& random = RandomHelpers::Default;
                auto created = 0UL;
                auto deleted = 0UL;
                for (auto i = 0; i < N; i++)
                {
                    auto linksCount = links.Count();
                    auto createPoint = Random::NextBoolean(random);
                    if (linksCount >= 2 && createPoint)
                    {
                        auto source = Random::NextUInt64(random, {1, linksCount});
                        auto target = Random::NextUInt64(random, {1, linksCount}); //-V3086
                        auto resultLink = links.GetOrCreate(source, target);
                        if (resultLink > linksCount)
                        {
                            created++;
                        }
                    }
                    else
                    {
                        links.Create();
                        created++;
                    }
                }
                auto count = links.Count();
                for (auto i = 0; i < count; i++)
                {
                    auto link = i + 1;
                    {
                        links.Update(link, 0, 0);
                        links.Delete(link);
                        deleted++;
                    }
                }
            }
        }

        void RunRandomCreations(std::size_t amountOfCreations)
        {
            using namespace Platform::Random;
            using namespace Platform::Ranges;

            auto& links = *this;
            auto& random = RandomHelpers::Default;
            for (std::size_t i = 0; i < amountOfCreations; i++)
            {
                // TODO: Use ranges/0.1.4 features
                auto linksAddressRange = Range<std::size_t>(0, links.Count());
                auto source = Random::NextUInt64(random, linksAddressRange);
                auto target = Random::NextUInt64(random, linksAddressRange);
                links.GetOrCreate(source, target);
            }
        }

        // TODO: implement const Each
        auto SearchOrDefault(TLink source, TLink target) const -> TLink
        {
            using namespace Platform::Setters;

            auto&& links = *this;
            auto constants = links.Constants;
            TLink result = 0;
            base::Each([&](auto link) {
                result = link[0];
                return links.Constants.Break;
            }, Link{links.Constants.Any, source, target});
            return result;
        }

        auto Create() -> TLink
        {
            constexpr std::array<TLink, 0> empty{};
            auto&& links = *this;
            return base::Create(empty);
        }

        auto GetOrCreate(TLink source, TLink target) -> TLink
        {
            auto& links = *this;
            auto constants = links.Constants;
            auto link = links.SearchOrDefault(source, target);
            if (link == constants.Null)
            {
                link = links.CreateAndUpdate(source, target);
            }
            return link;
        }

        auto Update(TLink link, TLink newSource, TLink newTarget) -> TLink
        {
            auto& links = *this;
            return base::Update(LinkAddress(link), Link{link, newSource, newTarget});
        }

        auto UpdateOrCreateOrGet(TLink source, TLink target, TLink newSource, TLink newTarget) -> TLink
        {
            auto& links = *this;
            auto constants = links.Constants;
            auto link = links.SearchOrDefault(source, target);
            if (link == constants.Null)
            {
                link = links.CreateAndUpdate(newSource, newTarget);
            }
            if (source == newSource && target == newTarget)
            {
                return link;
            }
            return links.Update(link, newSource, newTarget);
        }

        auto Delete(TLink link) // TODO: -> TLink
        {
            auto& links = *this;
            if (links.Exists(link))
            {
                links.ResetValues(link);
                return base::Delete(LinkAddress(link));
            }
        }     

        auto DeleteAll() 
        {
            auto& links = *this;
            for (auto count = links.Count(); count != 0; count = links.Count())
            {
                links.Delete(count);
            }
        } 

        auto DeleteIfExists(TLink source, TLink target) -> TLink
        {
            auto& links = *this;
            auto constants = links.Constants;
            auto link = links.SearchOrDefault(source, target);
            if (link != constants.Null)
            {
                return links.Delete(link);
            }
            return constants.Null;
        }

        auto DeleteByQuery(Interfaces::CArray auto&& query)
        {
            auto& links = *this;
            auto count = links.Count(query);
            auto toDelete = std::vector<TLink>(count);

            links.Each([&](auto link) {
                toDelete.push_back(link[0]);
                return links.Constants.Continue;
            }, query);

            for (auto link : toDelete | std::views::reverse)
            {
                links.Delete(link);
            }
        }

        auto ResetValues(TLink link)
        {
            auto& links = *this;
            links.Update(link, 0, 0);
        }

        auto CreateAndUpdate(TLink source, TLink target) -> TLink
        {
            auto& links = *this;
            return links.Update(links.Create(), source, target);
        }

        auto CreatePoint() -> TLink
        {
            auto& links = *this;
            auto point = links.Create();
            return links.Update(point, point, point);
        }

        auto GetLink(TLink index) const -> Link<TLink> {
            auto store = std::optional<Link<TLink>>{};
            auto any = base::Constants.Any;
            auto _break = base::Constants.Break;
            base::Each([&](auto link) {
                store = std::optional{Link{link[0], link[1], link[2]}};
                return _break;
            }, LinkAddress{index});
            return store.value();
        }

        auto GetSource(TLink index) const -> TLink {
            GetLink(index).Source;
        }

        auto GetTarget(TLink index) const -> TLink {
            GetLink(index).Target;
        }

        auto Exists(TLink source, TLink target) const -> bool {
            auto&& links = *this;
            return links.Count(links.Constants.Any, source, target) != 0;
        }

        auto CountUsages(TLink link) const -> TLink 
        {
            auto&& links = *this;
            auto constants = links.Constants;
            auto values = links.GetLink(link);

            TLink usages = 0;
            usages += links.Count(constants.Any, link, constants.Any) - bool(values.Source == link);
            usages += links.Count(constants.Any, constants.Any, link) - bool(values.Target == link);
            return usages;
        }

        auto HasUsages(TLink link) const -> TLink 
        {
            auto&& links = *this;
            return links.CountUsages(link) != 0;
        }

        auto Format(TLink link) const -> std::string 
        {
            auto&& links = *this;
            auto values = links.GetLink(link);
            return Format(values);
        }

        auto Format(Link<TLink> link) const -> std::string 
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
    };
}

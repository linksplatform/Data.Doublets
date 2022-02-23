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
            using namespace Platform::Ranges;

            auto&& storage = *this;
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

        void RunRandomCreations(std::size_t amountOfCreations)
        {
            using namespace Platform::Random;
            using namespace Platform::Ranges;

            auto& storage = *this;
            auto& randomGen64 = RandomHelpers::Default;
            for (std::size_t i = 0; i < amountOfCreations; i++)
            {
                // TODO: Use ranges/0.1.4 features
                auto linksAddressRange = Range<std::size_t>(0, storage.Count());
                auto source = Random::NextUInt64(randomGen64, linksAddressRange);
                auto target = Random::NextUInt64(randomGen64, linksAddressRange);
                storage.GetOrCreate(source, target);
            }
        }

        // TODO: implement const Each
        auto SearchOrDefault(TLink source, TLink target) const -> TLink
        {
            using namespace Platform::Setters;

            auto&& storage = *this;
            auto constants = storage.Constants;
            TLink result = 0;
            base::Each([&](auto link) {
                result = link[0];
                return storage.Constants.Break;
            }, Link{storage.Constants.Any, source, target});
            return result;
        }

        auto Create() -> TLink
        {
            constexpr std::array<TLink, 0> empty{};
            auto&& storage = *this;
            return base::Create(empty);
        }

        auto GetOrCreate(TLink source, TLink target) -> TLink
        {
            auto& storage = *this;
            auto constants = storage.Constants;
            auto link = storage.SearchOrDefault(source, target);
            if (link == constants.Null)
            {
                link = storage.CreateAndUpdate(source, target);
            }
            return link;
        }

        auto Update(TLink link, TLink newSource, TLink newTarget) -> TLink
        {
            auto& storage = *this;
            return base::Update(LinkAddress(link), Link{link, newSource, newTarget});
        }

        auto UpdateOrCreateOrGet(TLink source, TLink target, TLink newSource, TLink newTarget) -> TLink
        {
            auto& storage = *this;
            auto constants = storage.Constants;
            auto link = storage.SearchOrDefault(source, target);
            if (link == constants.Null)
            {
                link = storage.CreateAndUpdate(newSource, newTarget);
            }
            if (source == newSource && target == newTarget)
            {
                return link;
            }
            return storage.Update(link, newSource, newTarget);
        }

        auto Delete(TLink link) // TODO: -> TLink
        {
            auto& storage = *this;
            if (storage.Exists(link))
            {
                storage.ResetValues(link);
                return base::Delete(LinkAddress(link));
            }
        }     

        auto DeleteAll() 
        {
            auto& storage = *this;
            for (auto count = storage.Count(); count != 0; count = storage.Count())
            {
                storage.Delete(count);
            }
        } 

        auto DeleteIfExists(TLink source, TLink target) -> TLink
        {
            auto& storage = *this;
            auto constants = storage.Constants;
            auto link = storage.SearchOrDefault(source, target);
            if (link != constants.Null)
            {
                return storage.Delete(link);
            }
            return constants.Null;
        }

        auto DeleteByQuery(Interfaces::CArray auto&& query)
        {
            auto& storage = *this;
            auto count = storage.Count(query);
            auto toDelete = std::vector<TLink>(count);

            storage.Each([&](auto link) {
                toDelete.push_back(link[0]);
                return storage.Constants.Continue;
            }, query);

            for (auto link : toDelete | std::views::reverse)
            {
                storage.Delete(link);
            }
        }

        auto ResetValues(TLink link)
        {
            auto& storage = *this;
            storage.Update(link, 0, 0);
        }

        auto CreateAndUpdate(TLink source, TLink target) -> TLink
        {
            auto& storage = *this;
            return storage.Update(storage.Create(), source, target);
        }

        auto CreatePoint() -> TLink
        {
            auto& storage = *this;
            auto point = storage.Create();
            return storage.Update(point, point, point);
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
            auto&& storage = *this;
            return storage.Count(storage.Constants.Any, source, target) != 0;
        }

        auto CountUsages(TLink link) const -> TLink 
        {
            auto&& storage = *this;
            auto constants = storage.Constants;
            auto values = storage.GetLink(link);

            TLink usages = 0;
            usages += storage.Count(constants.Any, link, constants.Any) - bool(values.Source == link);
            usages += storage.Count(constants.Any, constants.Any, link) - bool(values.Target == link);
            return usages;
        }

        auto HasUsages(TLink link) const -> TLink 
        {
            auto&& storage = *this;
            return storage.CountUsages(link) != 0;
        }

        auto Format(TLink link) const -> std::string 
        {
            auto&& storage = *this;
            auto values = storage.GetLink(link);
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

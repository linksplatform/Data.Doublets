namespace Platform::Data::Doublets
{
    template<typename Self, typename TLink>
    struct ILinks : public Data::ILinks<Self, TLink, LinksConstants<TLink>>
    {
        using base = Data::ILinks<Self, TLink, LinksConstants<TLink>>;

        void TestRandomCreationsAndDeletions(std::size_t maximumOperationsPerCycle)
        {
            auto&& links = *this;
            for (auto N = 1; N < maximumOperationsPerCycle; N++)
            {
                auto& random = Platform::Random::RandomHelpers::Default;
                auto created = 0UL;
                auto deleted = 0UL;
                for (auto i = 0; i < N; i++)
                {
                    auto linksCount = links.Count();
                    auto createPoint = Platform::Random::NextBoolean(random);
                    if (linksCount >= 2 && createPoint)
                    {
                        auto source = NextUInt64(random, {1, linksCount});
                        auto target = NextUInt64(random, {1, linksCount}); //-V3086
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
                auto source = NextUInt64(random, linksAddressRange);
                auto target = NextUInt64(random, linksAddressRange);
                links.GetOrCreate(source, target);
            }
        }

        // TODO: implement const Each
        auto SearchOrDefault(TLink source, TLink target) /*const*/ -> TLink
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
            constexpr TLink null[]{};
            auto&& links = *this;
            return base::Create(std::vector<TLink>{});
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

        auto Delete(TLink link)
        {
            auto& links = *this;
            base::Delete(LinkAddress(link));
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

        auto GetLink(TLink index) -> std::optional<Link<TLink>> {
            auto store = std::optional<Link<TLink>>{};
            auto any = base::Constants.Any;
            auto br = base::Constants.Break;
            base::Each([&](auto link) {
                store = std::optional{Link{link[0], link[1], link[2]}};
                return br;
            }, LinkAddress{index});
            return store;
        }

        auto GetSource(TLink index) -> TLink {
            return GetLink(index).value().Source;
        }

        auto GetTarget(TLink index) -> TLink {
            return GetLink(index).value().Target;
        }

        auto Exist(TLink index) -> TLink {
            return base::Count(index) != 0;
        }

        auto IsPartialPoint(TLink index) -> bool {

        }
    };
}

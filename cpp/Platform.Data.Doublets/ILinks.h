namespace Platform::Data::Doublets
{
    template<typename Self, typename TLink>
    struct ILinks : public Data::ILinks<Self, TLink, LinksConstants<TLink>>
    {
        //template <typename TLink>
        //void RunRandomCreations(std::size_t amountOfCreations)
        //{
        //    using namespace Platform::Random;

        //    auto& links = *this;
        //    auto& random = RandomHelpers::Default
        //    for (std::size_t i = 0; i < amountOfCreations; i++)
        //    {
        //        auto linksAddressRange = Range<std::size_t>(0, links.Count());
        //        auto source = Random::NextUInt64(random, linksAddressRange);
        //        auto target = Random::NextUInt64(random, linksAddressRange);
        //        //links.GetOrCreate(source, target);
        //    }
        //}
    };
}

namespace Platform::Data::Doublets
{
    template<typename Self, typename TLink>
    struct ILinks : public Data::ILinks<ILinks<Self, TLink>, TLink, LinksConstants<TLink>>
    {

    };
}

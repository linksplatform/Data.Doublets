namespace Platform::Data::Doublets
{
    template<typename Self, typename TLinkAddress>
    struct ILinks : public Data::ILinks<ILinks<Self, TLinkAddress>, TLinkAddress, LinksConstants<TLinkAddress>>
    {

    };
}

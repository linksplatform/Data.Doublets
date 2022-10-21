namespace Platform::Data::Doublets
{
    template <typename ...> class ISynchronizedLinks;
    template <std::integral TLinkAddress> class ISynchronizedLinks<TLinkAddress> : public ISynchronizedLinks<TLinkAddress, ILinks<TLinkAddress>, LinksConstants<TLinkAddress>>, ILinks<TLinkAddress>
    {
    public:
    };
}

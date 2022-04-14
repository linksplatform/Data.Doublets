namespace Platform::Data::Doublets
{
    template <typename ...> class ISynchronizedLinks;
    template <typename TLinkAddress> class ISynchronizedLinks<TLinkAddress> : public ISynchronizedLinks<TLinkAddress, ILinks<TLinkAddress>, LinksConstants<TLinkAddress>>, ILinks<TLinkAddress>
    {
    public:
    };
}

namespace Platform::Data::Doublets
{
    template <typename ...> class ISynchronizedLinks;
    template <typename TLink> class ISynchronizedLinks<TLink> : public ISynchronizedLinks<TLink, ILinks<TLink>, LinksConstants<TLink>>, ILinks<TLink>
    {
    public:
    };
}

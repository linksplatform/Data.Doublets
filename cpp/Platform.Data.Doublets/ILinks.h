namespace Platform::Data::Doublets
{
    template <typename ...> class ILinks;
    template <typename TLink> class ILinks<TLink> : public ILinks<TLink, LinksConstants<TLink>>
    {
    public:
    };
}

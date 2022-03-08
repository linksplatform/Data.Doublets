namespace Platform::Data::Doublets
{
    template<typename Self, typename TLink>
    struct ILinks : public Data::ILinks<ILinks<Self, TLink>, TLink, LinksConstants<TLink>>
    {
        using base = Data::ILinks<ILinks<Self, TLink>, TLink, LinksConstants<TLink>>;
    public:
        // TODO Move to Platform.Data
    public:
        virtlal TLink Update(TLink link, TLink newSource, TLink newTarget)
        {
            auto& storage = *this;
            return base::Update(LinkAddress(link), Link{link, newSource, newTarget});
        }

        virtlal TLink Delete(TLink link)
        {
            auto& storage = *this;
            if (storage.Exists(link))
            {
                storage.ResetValues(link);
                return base::Delete(LinkAddress(link));
            }
        }
    };
}

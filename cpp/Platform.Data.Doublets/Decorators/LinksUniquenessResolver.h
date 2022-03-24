namespace Platform::Data::Doublets::Decorators
{
    using namespace Platform::Interfaces;
    template <typename TFacade, typename TDecorated>
    struct LinksUniquenessResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
;
        using LinkAddressType = base::LinkAddressType;
        using WriteHandlerType = base::WriteHandlerType;
        using ReadHandlerType = base::ReadHandlerType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksUniquenessResolver, base);

    public: LinkAddressType Update(CArray<LinkAddressType> auto&& restriction, CArray<LinkAddressType> auto&& substitution, const WriteHandlerType& handler)
        {
            auto newLinkAddress = SearchOrDefault(this->decorated(), substitution[Constants.SourcePart], substitution[Constants.TargetPart]);
            if (newLinkAddress == LinkAddressType{})
            {
                return this->decorated().Update(restriction, substitution, handler);
            }
            return this->ResolveAddressChangeConflict(restriction[Constants.IndexPart], newLinkAddress, handler);
        }

        protected: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress, const WriteHandlerType& handler)
        {
            if (oldLinkAddress != newLinkAddress && Exists(this->decorated(), oldLinkAddress))
            {
                this->facade().Delete(LinkType{oldLinkAddress}, handler);
            }
            return Constants.Continue;
        }
    };
}

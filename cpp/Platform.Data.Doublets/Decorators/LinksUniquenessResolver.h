namespace Platform::Data::Doublets::Decorators
{
    using namespace Platform::Interfaces;
    template <typename TFacade, typename TDecorated>
    struct LinksUniquenessResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksUniquenessResolver, base);

    public: LinkAddressType Update(CArray<LinkAddressType> auto&& restrictions, CArray<LinkAddressType> auto&& substitution, auto&& handler)
        {
            auto newLinkAddress = SearchOrDefault(this->decorated(), substitution[Constants.SourcePart], substitution[Constants.TargetPart]);
            if (newLinkAddress == LinkAddressType{})
            {
                return this->decorated().Update(restrictions, substitution, handler);
            }
            return this->ResolveAddressChangeConflict(restrictions[Constants.IndexPart], newLinkAddress, handler);
        }

        protected: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress, auto&& handler)
        {
            if (oldLinkAddress != newLinkAddress && Exists(this->decorated(), oldLinkAddress))
            {
                this->facade().Delete(LinkAddress{oldLinkAddress}, handler);
            }
            return Constants.Continue;
        }
    };
}

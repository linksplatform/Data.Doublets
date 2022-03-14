namespace Platform::Data::Doublets::Decorators
{
    using namespace Platform::Interfaces;
    template <typename TFacade, typename TDecorated>
    class LinksUniquenessResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksUniquenessResolver, base);

    public: LinkAddressType Update(CArray<LinkAddressType> auto&& restrictions, CArray<LinkAddressType> auto&& substitution)
        {
            auto storage = this->decorated();
            auto newLinkAddress = storage.SearchOrDefault(substitution[Constants.SourcePart], substitution[Constants.TargetPart]);
            if (newLinkAddress == LinkAddressType{})
            {
                return storage.Update(restrictions, substitution);
            }
            return this->ResolveAddressChangeConflict(restrictions[Constants.IndexPart], newLinkAddress);
        }

        protected: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress)
        {
            if (oldLinkAddress != newLinkAddress && Exists(this->decorated(), oldLinkAddress))
            {
                this->facade().Delete(oldLinkAddress);
            }
            return this->decorated().Constants.Continue;
        }
    };
}

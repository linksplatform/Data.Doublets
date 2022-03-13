namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    class LinksUniquenessResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksUniquenessResolver, base);

    public: LinkAddressType Update(CArray auto&& restrictions, CArray auto&& substitution)
        {
            auto storage = this->decorated();
            auto newLinkAddress = storage.SearchOrDefault(substitution[Constants.SourcePart], substitution[Constants.TargetPart]);
            if (newLinkAddress == LinkAddressType{})
            {
                return storage.Update(restrictions, substitution);
            }
            return this->ResolveAddressChangeConflict(restrictions[constants.IndexPart], newLinkAddress);
        }

        protected: TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress)
        {
            if (oldLinkAddress != newLinkAddress && Exists(this->decorated(), oldLinkAddress))
            {
                this->facade().Delete(oldLinkAddress);
            }
            return this->decorated().Constants.Continue;
        }
    };
}

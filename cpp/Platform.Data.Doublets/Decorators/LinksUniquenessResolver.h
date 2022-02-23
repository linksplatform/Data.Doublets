namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessResolver;
    template <typename TLink> class LinksUniquenessResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksUniquenessResolver(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto constants = _constants;
            auto storage = _links;
            auto newLinkAddress = storage.SearchOrDefault(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            if (newLinkAddress == 0)
            {
                return storage.Update(restrictions, substitution);
            }
            return this->ResolveAddressChangeConflict(restrictions[constants.IndexPart], newLinkAddress);
        }

        protected: virtual TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress)
        {
            if (!oldLinkAddress == newLinkAddress && _links.Exists(oldLinkAddress))
            {
                _facade.Delete(oldLinkAddress);
            }
            return newLinkAddress;
        }
    };
}

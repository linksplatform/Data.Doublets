namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksCascadeUniquenessAndUsagesResolver;
    template <typename TLink> class LinksCascadeUniquenessAndUsagesResolver<TLink> : public LinksUniquenessResolver<TLink>
    {
        public: LinksCascadeUniquenessAndUsagesResolver(ILinks<TLink> &links) : LinksUniquenessResolver(links) { }

        protected: TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress) override
        {
            _facade.MergeUsages(oldLinkAddress, newLinkAddress);
            return base.ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    };
}

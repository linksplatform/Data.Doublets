namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUniquenessAndUsagesResolver : public LinksUniquenessResolver<TFacade, TDecorated>
    {
    using base = LinksUniquenessResolver<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
        public:
            USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUniquenessAndUsagesResolver, base);

        protected: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress)
        {
            this->facade().MergeUsages(oldLinkAddress, newLinkAddress);
            return ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    };
}

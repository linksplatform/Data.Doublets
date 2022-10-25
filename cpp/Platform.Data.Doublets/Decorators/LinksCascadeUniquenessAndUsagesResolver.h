namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUniquenessAndUsagesResolver : public LinksUniquenessResolver<TFacade, TDecorated>
    {
    public:
    using base = LinksUniquenessResolver<TFacade, TDecorated>;
    using typename base::LinkAddressType;
    using typename base::LinkType;
    using typename base::WriteHandlerType;
    using typename base::ReadHandlerType;
    using base::Constants;
        public:
            USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUniquenessAndUsagesResolver, base);

        public: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress)
        {
            this->facade().TFacade::MergeUsages(oldLinkAddress, newLinkAddress);
            return base::ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    };
}

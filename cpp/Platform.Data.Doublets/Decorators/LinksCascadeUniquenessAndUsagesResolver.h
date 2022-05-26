namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUniquenessAndUsagesResolver : public LinksUniquenessResolver<TFacade, TDecorated>
    {
    using base = LinksUniquenessResolver<TFacade, TDecorated>;
    using typename base::LinkAddressType;
    using typename base::WriteHandlerType;
    using typename base::ReadHandlerType;
        public:
            USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUniquenessAndUsagesResolver, base);

        public: LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress)
        {
            this->facade().MergeUsages(oldLinkAddress, newLinkAddress);
            return ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    };
}

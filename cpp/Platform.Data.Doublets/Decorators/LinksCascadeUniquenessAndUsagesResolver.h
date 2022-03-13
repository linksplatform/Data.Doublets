namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksCascadeUniquenessAndUsagesResolver;

    template <typename TFacade, typename TDecorated>
    class LinksCascadeUniquenessAndUsagesResolver : public LinksUniquenessResolver<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
        public:
            USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUniquenessAndUsagesResolver, base);

        protected: TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress)
        {
            this->facade().MergeUsages(oldLinkAddress, newLinkAddress);
            return base.ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    };
}

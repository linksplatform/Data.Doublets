namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class NonNullContentsLinkDeletionResolver;
    template <typename TLink> class NonNullContentsLinkDeletionResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: NonNullContentsLinkDeletionResolver(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: void Delete(CList auto&&restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto storage = _links;
            storage.EnforceResetValues(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}

namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class NonNullContentsLinkDeletionResolver;
    template <typename TLink> class NonNullContentsLinkDeletionResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: NonNullContentsLinkDeletionResolver(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: void Delete(CList auto&&restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto links = _links;
            links.EnforceResetValues(linkIndex);
            links.Delete(linkIndex);
        }
    };
}

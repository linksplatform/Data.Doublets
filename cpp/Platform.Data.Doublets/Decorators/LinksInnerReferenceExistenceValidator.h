namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLink> class LinksInnerReferenceExistenceValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CList auto&&restrictions) override
        {
            auto storage = _links;
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            return storage.Each(handler, restrictions);
        }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto storage = _links;
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            storage.EnsureInnerReferenceExists(substitution, "substitution");
            return storage.Update(restrictions, substitution);
        }

        public: void Delete(CList auto&&restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto storage = _links;
            storage.EnsureLinkExists(link, "link");
            storage.Delete(link);
        }
    };
}

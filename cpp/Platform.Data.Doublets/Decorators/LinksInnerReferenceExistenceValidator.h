namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLink> class LinksInnerReferenceExistenceValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CList auto&&restrictions) override
        {
            auto storage = this->decorated();
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            return storage.Each(restrictions, handler);
        }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto storage = this->decorated();
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            storage.EnsureInnerReferenceExists(substitution, "substitution");
            return storage.Update(restrictions, substitution);
        }

        public: void Delete(CList auto&&restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto storage = this->decorated();
            storage.EnsureLinkExists(link, "link");
            storage.Delete(link);
        }
    };
}

namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLink> class LinksInnerReferenceExistenceValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CArray<TLinkAddress> auto&& restrictions) override
        {
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            return storage.Each(restrictions, handler);
        }

        public: TLink Update(CArray<TLinkAddress> auto&& restrictions, CArray<TLinkAddress> auto&& substitution) override
        {
            storage.EnsureInnerReferenceExists(restrictions, "restrictions");
            storage.EnsureInnerReferenceExists(substitution, "substitution");
            return storage.Update(restrictions, substitution);
        }

        public: void Delete(CArray<TLinkAddress> auto&& restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            storage.EnsureLinkExists(link, "link");
            storage.Delete(link);
        }
    };
}

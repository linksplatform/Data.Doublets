namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLinkAddress> class LinksInnerReferenceExistenceValidator<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, const  LinkType& restriction)
        {
            storage.EnsureInnerReferenceExists(restriction, "restriction");
            return storage.Each(restriction, handler);
        }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution)
        {
            storage.EnsureInnerReferenceExists(restriction, "restriction");
            storage.EnsureInnerReferenceExists(substitution, "substitution");
            return storage.Update(restriction, substitution);
        }

        public: void Delete(const  LinkType& restriction)
        {
            auto link = restriction[_constants.IndexPart];
            storage.EnsureLinkExists(link, "link");
            storage.Delete(link);
        }
    };
}

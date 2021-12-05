namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLink> class LinksInnerReferenceExistenceValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> &restrictions) override
        {
            auto links = _links;
            links.EnsureInnerReferenceExists(restrictions, "restrictions");
            return links.Each(handler, restrictions);
        }

        public: TLink Update(IList<TLink> &restrictions, IList<TLink> &substitution) override
        {
            auto links = _links;
            links.EnsureInnerReferenceExists(restrictions, "restrictions");
            links.EnsureInnerReferenceExists(substitution, "substitution");
            return links.Update(restrictions, substitution);
        }

        public: void Delete(IList<TLink> &restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto links = _links;
            links.EnsureLinkExists(link, "link");
            links.Delete(link);
        }
    };
}

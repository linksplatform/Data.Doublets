namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksInnerReferenceExistenceValidator;
    template <typename TLink> class LinksInnerReferenceExistenceValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksInnerReferenceExistenceValidator(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CList auto&&restrictions) override
        {
            auto links = _links;
            links.EnsureInnerReferenceExists(restrictions, "restrictions");
            return links.Each(handler, restrictions);
        }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto links = _links;
            links.EnsureInnerReferenceExists(restrictions, "restrictions");
            links.EnsureInnerReferenceExists(substitution, "substitution");
            return links.Update(restrictions, substitution);
        }

        public: void Delete(CList auto&&restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto links = _links;
            links.EnsureLinkExists(link, "link");
            links.Delete(link);
        }
    };
}

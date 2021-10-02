namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUsagesValidator;
    template <typename TLink> class LinksUsagesValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksUsagesValidator(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Update(IList<TLink> &restrictions, IList<TLink> &substitution) override
        {
            auto links = _links;
            links.EnsureNoUsages(restrictions[_constants.IndexPart]);
            return links.Update(restrictions, substitution);
        }

        public: void Delete(IList<TLink> &restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto links = _links;
            links.EnsureNoUsages(link);
            links.Delete(link);
        }
    };
}

namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessValidator;
    template <typename TLink> class LinksUniquenessValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksUniquenessValidator(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto links = _links;
            auto constants = _constants;
            links.EnsureDoesNotExists(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return links.Update(restrictions, substitution);
        }
    };
}

namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNonExistentDependenciesCreator;
    template <typename TLink> class LinksNonExistentDependenciesCreator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksNonExistentDependenciesCreator(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto constants = _constants;
            auto links = _links;
            links.EnsureCreated(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return links.Update(restrictions, substitution);
        }
    };
}

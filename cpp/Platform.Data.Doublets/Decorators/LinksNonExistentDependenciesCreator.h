namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNonExistentDependenciesCreator;
    template <typename TLink> class LinksNonExistentDependenciesCreator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksNonExistentDependenciesCreator(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto constants = _constants;
            auto storage = _links;
            storage.EnsureCreated(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restrictions, substitution);
        }
    };
}

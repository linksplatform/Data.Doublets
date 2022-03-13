namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNonExistentDependenciesCreator;
    template <typename TLink> class LinksNonExistentDependenciesCreator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNonExistentDependenciesCreator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto constants = _constants;
            auto storage = this->decorated();
            storage.EnsureCreated(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restrictions, substitution);
        }
    };
}

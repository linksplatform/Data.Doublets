namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNonExistentDependenciesCreator;
    template <typename TLink> class LinksNonExistentDependenciesCreator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNonExistentDependenciesCreator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(const  LinkType& restriction, CArray<TLinkAddress> auto&& substitution) override
        {
            auto constants = _constants;
            storage.EnsureCreated(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restriction, substitution);
        }
    };
}

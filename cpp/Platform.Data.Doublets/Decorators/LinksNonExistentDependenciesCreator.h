namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNonExistentDependenciesCreator;
    template <typename TLinkAddress> class LinksNonExistentDependenciesCreator<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNonExistentDependenciesCreator(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution)
        {
            auto constants = _constants;
            storage.EnsureCreated(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restriction, substitution);
        }
    };
}

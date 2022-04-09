namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessValidator;
    template <typename TLinkAddress> class LinksUniquenessValidator<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUniquenessValidator(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution) override
        {
            auto constants = _constants;
            storage.EnsureDoesNotExists(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restriction, substitution);
        }
    };
}

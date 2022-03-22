namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessValidator;
    template <typename TLink> class LinksUniquenessValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUniquenessValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(CArray<TLinkAddress> auto&& restriction, CArray<TLinkAddress> auto&& substitution) override
        {
            auto constants = _constants;
            storage.EnsureDoesNotExists(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restriction, substitution);
        }
    };
}

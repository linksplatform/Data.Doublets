namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessValidator;
    template <typename TLink> class LinksUniquenessValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUniquenessValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto storage = this->decorated();
            auto constants = _constants;
            storage.EnsureDoesNotExists(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restrictions, substitution);
        }
    };
}

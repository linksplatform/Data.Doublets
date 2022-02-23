namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUniquenessValidator;
    template <typename TLink> class LinksUniquenessValidator<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksUniquenessValidator(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override
        {
            auto storage = _links;
            auto constants = _constants;
            storage.EnsureDoesNotExists(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            return storage.Update(restrictions, substitution);
        }
    };
}

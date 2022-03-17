namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUsagesValidator;
    template <typename TLink> class LinksUsagesValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUsagesValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(CArray<TLinkAddress> auto&& restrictions, CArray<TLinkAddress> auto&& substitution) override
        {
            auto storage = this->decorated();
            storage.EnsureNoUsages(restrictions[_constants.IndexPart]);
            return storage.Update(restrictions, substitution);
        }

        public: void Delete(CArray<TLinkAddress> auto&& restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto storage = this->decorated();
            storage.EnsureNoUsages(link);
            storage.Delete(link);
        }
    };
}

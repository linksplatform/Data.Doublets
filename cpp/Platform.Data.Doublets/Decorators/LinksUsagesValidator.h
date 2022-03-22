namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUsagesValidator;
    template <typename TLink> class LinksUsagesValidator<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUsagesValidator(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Update(CArray<TLinkAddress> auto&& restriction, CArray<TLinkAddress> auto&& substitution) override
        {
            storage.EnsureNoUsages(restriction[_constants.IndexPart]);
            return storage.Update(restriction, substitution);
        }

        public: void Delete(CArray<TLinkAddress> auto&& restriction) override
        {
            auto link = restriction[_constants.IndexPart];
            storage.EnsureNoUsages(link);
            storage.Delete(link);
        }
    };
}

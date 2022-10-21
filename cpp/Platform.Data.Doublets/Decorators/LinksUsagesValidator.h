namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksUsagesValidator;
    template <std::integral TLinkAddress> class LinksUsagesValidator<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksUsagesValidator(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution)
        {
            storage.EnsureNoUsages(restriction[_constants.IndexPart]);
            return storage.Update(restriction, substitution);
        }

        public: void Delete(const  LinkType& restriction)
        {
            auto link = restriction[_constants.IndexPart];
            storage.EnsureNoUsages(link);
            storage.Delete(link);
        }
    };
}

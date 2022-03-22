namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLink> class LinksNullConstantToSelfReferenceResolver<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Create(CArray<TLinkAddress> auto&& restriction) override { return this->decorated().CreatePoint(); }

        public: TLink Update(CArray<TLinkAddress> auto&& restriction, CArray<TLinkAddress> auto&& substitution) override { return this->decorated().Update(restriction, this->decorated().ResolveConstantAsSelfReference(_constants.Null, restriction, substitution)); }
    };
}

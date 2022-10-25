namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <std::integral TLinkAddress> class LinksNullConstantToSelfReferenceResolver<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Create(const  LinkType& restriction) { return this->decorated().TDecorated::CreatePoint(); }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution) { return this->decorated().TDecorated::Update(restriction, this->decorated().TDecorated::ResolveConstantAsSelfReference(_constants.Null, restriction, substitution)); }
    };
}

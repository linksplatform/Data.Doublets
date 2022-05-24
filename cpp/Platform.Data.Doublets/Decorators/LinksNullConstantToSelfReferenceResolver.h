namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLinkAddress> class LinksNullConstantToSelfReferenceResolver<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Create(const  LinkType& restriction) { return this->decorated().CreatePoint(); }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution) { return this->decorated().Update(restriction, this->decorated().ResolveConstantAsSelfReference(_constants.Null, restriction, substitution)); }
    };
}

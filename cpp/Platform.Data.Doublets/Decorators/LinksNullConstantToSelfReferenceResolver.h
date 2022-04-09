namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLink> class LinksNullConstantToSelfReferenceResolver<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Create(const  LinkType& restriction) override { return this->decorated().CreatePoint(); }

        public: TLink Update(const  LinkType& restriction, const LinkType& substitution) override { return this->decorated().Update(restriction, this->decorated().ResolveConstantAsSelfReference(_constants.Null, restriction, substitution)); }
    };
}

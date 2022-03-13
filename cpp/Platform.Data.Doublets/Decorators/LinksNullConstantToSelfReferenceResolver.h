namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLink> class LinksNullConstantToSelfReferenceResolver<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Create(CList auto&&restrictions) override { return this->decorated().CreatePoint(); }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override { return this->decorated().Update(restrictions, this->decorated().ResolveConstantAsSelfReference(_constants.Null, restrictions, substitution)); }
    };
}

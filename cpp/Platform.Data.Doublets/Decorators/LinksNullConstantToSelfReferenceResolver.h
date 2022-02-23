namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLink> class LinksNullConstantToSelfReferenceResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Create(CList auto&&restrictions) override { return _links.CreatePoint(); }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override { return _links.Update(restrictions, _links.ResolveConstantAsSelfReference(_constants.Null, restrictions, substitution)); }
    };
}

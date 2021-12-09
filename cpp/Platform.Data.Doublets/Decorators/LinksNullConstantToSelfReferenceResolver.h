namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksNullConstantToSelfReferenceResolver;
    template <typename TLink> class LinksNullConstantToSelfReferenceResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksNullConstantToSelfReferenceResolver(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Create(IList<TLink> &restrictions) override { return _links.CreatePoint(); }

        public: TLink Update(IList<TLink> &restrictions, IList<TLink> &substitution) override { return _links.Update(restrictions, _links.ResolveConstantAsSelfReference(_constants.Null, restrictions, substitution)); }
    };
}

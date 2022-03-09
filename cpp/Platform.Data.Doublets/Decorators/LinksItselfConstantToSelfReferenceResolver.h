namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <typename TLink> class LinksItselfConstantToSelfReferenceResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLink> &storage) : LinksDecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CList auto&&restrictions) override
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restrictions.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return _links.Each(restrictions, handler);
        }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override { return _links.Update(restrictions, _links.ResolveConstantAsSelfReference(_constants.Itself, restrictions, substitution)); }
    };
}

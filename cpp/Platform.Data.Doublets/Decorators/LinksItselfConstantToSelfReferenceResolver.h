namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <typename TLink> class LinksItselfConstantToSelfReferenceResolver<TLink> : public LinksDecoratorBase<TLink>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLink> &links) : LinksDecoratorBase(links) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> &restrictions) override
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restrictions.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return _links.Each(handler, restrictions);
        }

        public: TLink Update(IList<TLink> &restrictions, IList<TLink> &substitution) override { return _links.Update(restrictions, _links.ResolveConstantAsSelfReference(_constants.Itself, restrictions, substitution)); }
    };
}

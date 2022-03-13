namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <typename TLink> class LinksItselfConstantToSelfReferenceResolver<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CList auto&&restrictions) override
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restrictions.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return this->decorated().Each(restrictions, handler);
        }

        public: TLink Update(CList auto&&restrictions, CList auto&&substitution) override { return this->decorated().Update(restrictions, this->decorated().ResolveConstantAsSelfReference(_constants.Itself, restrictions, substitution)); }
    };
}

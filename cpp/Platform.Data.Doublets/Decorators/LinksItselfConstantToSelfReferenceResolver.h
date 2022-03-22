namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <typename TLink> class LinksItselfConstantToSelfReferenceResolver<TLink> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLink> &storage) : DecoratorBase(storage) { }

        public: TLink Each(Func<IList<TLink>, TLink> handler, CArray<TLinkAddress> auto&& restriction) override
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restriction.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return this->decorated().Each(restriction, handler);
        }

        public: TLink Update(CArray<TLinkAddress> auto&& restriction, CArray<TLinkAddress> auto&& substitution) override { return this->decorated().Update(restriction, this->decorated().ResolveConstantAsSelfReference(_constants.Itself, restriction, substitution)); }
    };
}

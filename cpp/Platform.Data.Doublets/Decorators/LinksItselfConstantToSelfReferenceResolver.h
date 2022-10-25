namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <std::integral TLinkAddress> class LinksItselfConstantToSelfReferenceResolver<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, const  LinkType& restriction)
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restriction.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return this->decorated().TDecorated::Each(restriction, handler);
        }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution) { return this->decorated().TDecorated::Update(restriction, this->decorated().TDecorated::ResolveConstantAsSelfReference(_constants.Itself, restriction, substitution)); }
    };
}

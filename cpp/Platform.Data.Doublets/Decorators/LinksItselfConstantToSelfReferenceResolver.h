namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksItselfConstantToSelfReferenceResolver;
    template <typename TLinkAddress> class LinksItselfConstantToSelfReferenceResolver<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>
    {
        public: LinksItselfConstantToSelfReferenceResolver(ILinks<TLinkAddress> &storage) : DecoratorBase(storage) { }

        public: TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, const  LinkType& restriction) override
        {
            auto constants = _constants;
            auto itselfConstant = constants.Itself;
            if (!constants.Any == itselfConstant && restriction.Contains(itselfConstant))
            {
                return constants.Continue;
            }
            return this->decorated().Each(restriction, handler);
        }

        public: TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution) override { return this->decorated().Update(restriction, this->decorated().ResolveConstantAsSelfReference(_constants.Itself, restriction, substitution)); }
    };
}

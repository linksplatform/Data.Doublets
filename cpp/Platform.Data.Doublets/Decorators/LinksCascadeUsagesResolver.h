namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUsagesResolver : DecoratorBase<TFacade, TDecorated>
    {
    public:
        using base = DecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
        using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUsagesResolver, base);

        public: LinkAddressType Delete( const LinkType& restriction, const WriteHandlerType& handler)
        {
            auto $continue {Constants.Continue};
            auto linkIndex = restriction[Constants.IndexPart];
            WriteHandlerState<TDecorated> handlerState {Constants.Continue, Constants.Break, handler};
            handlerState.Apply(DeleteAllUsages(this->facade(), linkIndex, handlerState.Handler));
            return handlerState.Apply(this->decorated().TDecorated::Delete(LinkType{linkIndex}, handlerState.Handler));
        }
    };
}

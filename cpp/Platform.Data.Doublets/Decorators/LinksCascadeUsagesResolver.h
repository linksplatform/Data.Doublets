namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUsagesResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
    public: using typename base::HandlerParameterType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUsagesResolver, base);

        public: LinkAddressType Delete(CArray<LinkAddressType> auto&& restrictions, auto&& handler)
        {
            auto $continue {Constants.Continue};
            auto linkIndex = restrictions[Constants.IndexPart];
            WriteHandlerState<LinkAddressType, HandlerParameterType> handlerState {Constants.Continue, Constants.Break, handler};
            handlerState.Apply(DeleteAllUsages(this->facade(), linkIndex, handlerState));
            return handlerState.Apply(this->decorated().Delete(LinkAddress{linkIndex}, handlerState));
        }
    };
}

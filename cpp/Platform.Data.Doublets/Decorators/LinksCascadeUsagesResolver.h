namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksCascadeUsagesResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
;
        using typename base::LinkAddressType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUsagesResolver, base);

        public: LinkAddressType Delete(CArray<LinkAddressType> auto&& restriction, const WriteHandlerType& handler)
        {
            auto $continue {Constants.Continue};
            auto linkIndex = restriction[Constants.IndexPart];
            WriteHandlerState<TDecorated> handlerState {Constants.Continue, Constants.Break, handler};
            handlerState.Apply(DeleteAllUsages(this->facade(), linkIndex, handlerState.Handler));
            return handlerState.Apply(this->decorated().Delete(LinkType{linkIndex}, handlerState.Handler));
        }
    };
}

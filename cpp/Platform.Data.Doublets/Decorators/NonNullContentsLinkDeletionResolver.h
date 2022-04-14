namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct NonNullContentsLinkDeletionResolver : public DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: LinkAddressType Delete( const LinkType& restriction, const WriteHandlerType& handler)
        {
            auto $break = Constants.Break;
            auto linkIndex = restriction[Constants.IndexPart];
            WriteHandlerState<TDecorated> handlerState {Constants.Continue, Constants.Break, handler};
            handlerState.Apply(EnforceResetValues(this->decorated(), linkIndex, handlerState.Handler));
            return handlerState.Apply(this->decorated().Delete(LinkType{linkIndex}, handlerState.Handler));
        }
    };
}

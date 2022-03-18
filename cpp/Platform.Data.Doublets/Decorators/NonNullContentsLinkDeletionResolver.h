namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct NonNullContentsLinkDeletionResolver : public DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: LinkAddressType Delete(CArray<LinkAddressType> auto&& restrictions, auto&& handler)
        {
            auto $break = Constants.Break;
            auto linkIndex = restrictions[Constants.IndexPart];
            WriteHandlerState<LinkAddressType, decltype(handler)> handlerState {Constants.Continue, Constants.Break, handler};
            EnforceResetValues(this->decorated(), linkIndex, handlerState);
            return this->decorated().Delete(LinkAddress{linkIndex}, handlerState);

        }
    };
}

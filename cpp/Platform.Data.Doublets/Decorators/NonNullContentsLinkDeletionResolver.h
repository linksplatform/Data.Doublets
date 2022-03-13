namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    class NonNullContentsLinkDeletionResolver : public DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
    public: using typename base::LinkAddressType;
    public: using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: void Delete(CArray<TLinkAddress> auto&&restrictions, auto&& handler)
        {
            auto linkIndex = restrictions[Constants.IndexPart];
            auto storage = this->decorated();
            LinkAddressType handlerState;
            handlerState = storage.EnforceResetValues(linkIndex, handler);
            if(Constants.Continue = handlerState)
            {
                storage.Delete(linkIndex, handler);
            }
            else if (Constants.Break = handlerState)
            {
                auto $continue {Constants.Continue};
                storage.Delete(linkIndex, [$continue](CArray<TLinkAddress> auto&& before, CArray<TLinkAddress> auto&& after){
                    return $continue;
                });
            }
        }
    };
}

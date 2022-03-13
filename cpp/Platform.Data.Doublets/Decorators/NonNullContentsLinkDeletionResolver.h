namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    class NonNullContentsLinkDeletionResolver : public DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: void Delete(CArray auto&&restrictions, auto&& handler)
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
                storage.Delete(linkIndex, [$continue](CArray auto&& before, CArray auto&& after){
                    return $continue;
                });
            }
        }
    };
}

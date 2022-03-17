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
            auto storage = this->decorated();
            LinkAddressType handlerState;
            handlerState = EnforceResetValues(storage, linkIndex, handler);
            if(Constants.Break == handlerState)
            {
                return storage.Delete(LinkAddress{linkIndex}, [$break](CArray<LinkAddressType> auto&& before, CArray<LinkAddressType> auto&& after)
                {
                    return $break;
                });
            }
            else
            {
                return storage.Delete(LinkAddress{linkIndex}, handler);
            }
        }
    };
}

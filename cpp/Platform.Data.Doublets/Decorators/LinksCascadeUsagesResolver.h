namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    class LinksCascadeUsagesResolver : DecoratorBase<TFacade, TDecorated>
    {
        using base = DecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using base::Constants;
    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksCascadeUsagesResolver, base);

        public: void Delete(CList auto&& restrictions, auto&& handler)
        {
            auto $continue {Constants.Continue};
            auto linkIndex = restrictions[Constants.IndexPart];
            LinkAddressType handlerState {Constants.Continue};
            handlerState = this->facade().DeleteAllUsages(linkIndex, handler);
            if(Constants.Break == handlerState)
            {
                return this->decorated().Delete(linkIndex, [$continue](CArray auto&& before, CArray auto&& after)
                {
                    return $continue;
                });
            }
            else
            {
                return this->decorated().Delete(linkIndex, handler);
            }
        }
    };
}

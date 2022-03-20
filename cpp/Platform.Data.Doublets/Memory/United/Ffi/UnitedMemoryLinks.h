namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress,LinksConstants<TLinkAddress> VConstants, CArray<TLinkAddress> THandlerParameter = Link<TLinkAddress>, typename ...TBase>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, VConstants, THandlerParameter, TBase...>, TLinkAddress, VConstants, THandlerParameter, TBase...>
    {
        public:
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, VConstants, THandlerParameter, TBase...>, TLinkAddress, VConstants, THandlerParameter, TBase...>;
        using base::base;
    };
}

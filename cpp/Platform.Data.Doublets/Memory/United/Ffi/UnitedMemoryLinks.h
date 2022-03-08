namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress,LinksConstants<TLinkAddress> VConstants, typename ...TBase>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, VConstants, TBase...>, TLinkAddress, VConstants, TBase...>
    {
        public:
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, VConstants, TBase...>, TLinkAddress, VConstants, TBase...>;
        using base::base;
    };
}

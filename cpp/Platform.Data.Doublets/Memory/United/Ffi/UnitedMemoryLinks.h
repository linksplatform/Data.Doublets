namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress, typename ...TBase>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, TBase...>, TLinkAddress, TBase...>
    {
        public:
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, TBase...>, TLinkAddress, TBase...>;
        using base::base;
    };
}

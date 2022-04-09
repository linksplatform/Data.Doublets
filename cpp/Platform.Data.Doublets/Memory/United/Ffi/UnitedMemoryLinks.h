namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkOptions, typename ...TBase>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkOptions, TBase...>, TLinkOptions, TBase...>
    {
        public:
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkOptions, TBase...>, TLinkOptions, TBase...>;
        using base::base;
    }; 
}

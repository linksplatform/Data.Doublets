namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress>
    class UnitedMemoryLinks : public Platform::Data::Doublets::Memory::United::Ffi::UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress>, TLinkAddress>
    {
    public:
        using base = Platform::Data::Doublets::Memory::United::Ffi::UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress>, TLinkAddress>;
        using base::base;
//        using base::Create;
    };
}

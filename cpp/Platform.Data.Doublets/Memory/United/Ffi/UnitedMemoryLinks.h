namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress, typename ...TBase>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLink, TBase...>, TLinkAddress, TBase...>
    {

    };
}

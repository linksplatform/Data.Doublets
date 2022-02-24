namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename TLinkAddress>
    class DefaultUnitedMemoryLinks : public UnitedMemoryLinks<DefaultUnitedMemoryLinks<TLinkAddress>, TLinkAddress>
    {

    };
}

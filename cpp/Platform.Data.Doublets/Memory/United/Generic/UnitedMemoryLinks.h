namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<std::integral TLinkAddress, typename TMemory = FileMappedResizableDirectMemory, typename TSourceTreeMethods = LinksSourcesSizeBalancedTreeMethods<TLinkAddress>, typename TTargetTreeMethods = LinksTargetsSizeBalancedTreeMethods<TLinkAddress>, typename TUnusedLinks = UnusedLinksListMethods<TLinkAddress>, typename ...TBase>
    class UnitedMemoryLinks : Interfaces::Polymorph<UnitedMemoryLinks<TLinkAddress, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, TBase...>
    {
    public:
        UnitedMemoryLinks() = delete;
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    using namespace Platform::Memory;
    template<
        typename TLinksOptions,
        typename TMemory = FileMappedResizableDirectMemory,
        typename TInternalSourcesTreeMethods = InternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TInternalTargetsTreeMethods = InternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TExternalSourcesTreeMethods = ExternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TExternalTargetsTreeMethods = ExternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TInternalLinksSourcesLinkedTreeMethods = InternalLinksSourcesLinkedListMethods<TLinksOptions>,
        typename TUnusedLinksListMethods = UnusedLinksListMethods<TLinksOptions>,
        typename... TBase>
    struct SplitMemoryLinks : public SplitMemoryLinksBase<SplitMemoryLinks<TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksListMethods, TBase...>, TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksListMethods, TBase...>
    {
        using base = SplitMemoryLinksBase<SplitMemoryLinks<TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksListMethods, TBase...>, TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksListMethods, TBase...>;
    public:
        using base::base;
    };
}

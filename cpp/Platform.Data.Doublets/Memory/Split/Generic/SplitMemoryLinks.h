namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<
        typename TLinksOptions,
        typename TMemory = FileMappedResizableDirectMemory,
        typename TInternalSourcesTreeMethods = InternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TInternalLinksSourcesLinkedListMethods = InternalLinksSourcesLinkedListMethods<TLinksOptions>,
        typename TInternalTargetsTreeMethods = InternalLinksTargetsSizeBalancedTreeMethods<TLinksOptions>,
        typename TExternalSourcesTreeMethods = ExternalLinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TExternalTargetsTreeMethods = ExternalLinksTargetsSizeBalancedTreeMethods<TLinksOptions>,
        typename TUnusedLinksListMethods = UnusedLinksListMethods<TLinksOptions>,
        typename... TBase>
    struct SplitMemoryLinks : public SplitMemoryLinksBase<SplitMemoryLinks<TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalLinksSourcesLinkedListMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TUnusedLinksListMethods, TBase...>, TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalLinksSourcesLinkedListMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TUnusedLinksListMethods, TBase...>
    {
        using base = SplitMemoryLinksBase<SplitMemoryLinks<TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalLinksSourcesLinkedListMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TUnusedLinksListMethods, TBase...>, TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalLinksSourcesLinkedListMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TUnusedLinksListMethods, TBase...>;
    public:
        using base::base;
    };
}

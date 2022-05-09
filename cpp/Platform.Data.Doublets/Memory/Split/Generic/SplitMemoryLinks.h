namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    template<
        typename TLinksOptions,
        typename TMemory,
        typename TInternalSourcesTreeMethods,
        typename TInternalTargetsTreeMethods,
        typename TExternalSourcesTreeMethods,
        typename TExternalTargetsTreeMethods,
        typename TInternalLinksSourcesLinkedTreeMethods,
        typename TUnusedLinksTreeMethods,
        typename... TBase>
    class SplitMemoryLinks : SplitMemoryLinksBase<SplitMemoryLinks<TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksTreeMethods, TBase...>, TLinksOptions, TMemory, TInternalSourcesTreeMethods, TInternalTargetsTreeMethods, TExternalSourcesTreeMethods, TExternalTargetsTreeMethods, TInternalLinksSourcesLinkedTreeMethods, TUnusedLinksTreeMethods, TBase...>
    {

    };
}

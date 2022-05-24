namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;

    template<
        typename TLinksOptions = Platform::Data::Doublets::LinksOptions<>,
        typename TMemory = FileMappedResizableDirectMemory,
        typename TSourceTreeMethods = LinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TTargetTreeMethods = LinksTargetsSizeBalancedTreeMethods<TLinksOptions>,
        typename TUnusedLinks = UnusedLinksListMethods<typename TLinksOptions::LinkAddressType>,
        typename ...TBase>
    struct UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, public std::enable_shared_from_this<UnitedMemoryLinks<TLinksOptions>>
    {
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>;

        using LinkAddressType = base::LinkAddressType;
        using LinkType = base::LinkType;
        using WriteHandlerType = base::WriteHandlerType;
        using ReadHandlerType = base::ReadHandlerType;
//        using base::Constants;

    public:
        using base::DefaultLinksSizeStep;

    public:
        using base::GetLinkStruct;

    public:
        using base::Constants;



        // private: using base::_memory;

        // TODO: implicit constructor for Constants
    public:
        using base::base;


    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

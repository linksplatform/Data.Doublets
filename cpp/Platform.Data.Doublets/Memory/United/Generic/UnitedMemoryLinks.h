namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;

    template<
        typename TLinksOptions = LinksOptions<>,
        typename TMemory = FileMappedResizableDirectMemory,
        typename TSourceTreeMethods = LinksSourcesSizeBalancedTreeMethods<TLinksOptions>,
        typename TTargetTreeMethods = LinksTargetsSizeBalancedTreeMethods<TLinksOptions>,
        typename TUnusedLinks = UnusedLinksListMethods<typename TLinksOptions::LinkAddressType>,
        typename ...TBase>
    struct UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, public std::enable_shared_from_this<UnitedMemoryLinks<TLinksOptions>>
    {
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>, TLinksOptions, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks, TBase...>;

        using LinkAddressType = base::LinkAddressType;
        using WriteHandlerType = base::WriteHandlerType;
        using ReadHandlerType = base::ReadHandlerType;
//        using base::Constants;

    public:
        using base::DefaultLinksSizeStep;

    public:
        using base::GetLinkStruct;

    public:
        using base::Constants;

    private:
        std::function<void(std::unique_ptr<ILinksTreeMethods<LinkAddressType>>)> _createSourceTreeMethods;

    private:
        std::function<void(std::unique_ptr<ILinksTreeMethods<LinkAddressType>>)> _createTargetTreeMethods;

    private:
        std::byte* _header;

    private:
        std::byte* _links;

        // private: using base::_memory;

        // TODO: implicit constructor for Constants
    public:
        UnitedMemoryLinks(TMemory&& memory, std::size_t memoryReservationStep = DefaultLinksSizeStep) :
            base(std::move(memory), memoryReservationStep)
        {
            //if (indexTreeType == IndexTreeType.SizedAndThreadedAVLBalancedTree)
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesAvlBalancedTreeMethods<LinkAddressType>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsAvlBalancedTreeMethods<LinkAddressType>(Constants, _links, _header);
            //}
            //else
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesSizeBalancedTreeMethods<LinkAddressType>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsSizeBalancedTreeMethods<LinkAddressType>(Constants, _links, _header);
            //}
            base::Init(this->_memory, memoryReservationStep);
        }

    public:
        void SetPointers(TMemory& memory)
        {
            _links = static_cast<std::byte*>(memory.Pointer());
            _header = _links;
            base::_SourcesTreeMethods = new TSourceTreeMethods(_links, _header);
            base::_TargetsTreeMethods = new TTargetTreeMethods(_links, _header);
            base::_UnusedLinksListMethods = new TUnusedLinks(_links, _header);
        }

    public:
        auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header);
        }

    public:
        auto&& GetLinkReference(LinkAddressType linkIndex) const
        {
            return *(reinterpret_cast<RawLink<LinkAddressType>*>(_links) + linkIndex);
        }
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

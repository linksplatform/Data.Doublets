namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;

    template<typename TLinkAddress, typename TMemory = FileMappedResizableDirectMemory, typename TSourceTreeMethods = LinksSourcesSizeBalancedTreeMethods<TLinkAddress>, typename TTargetTreeMethods = LinksTargetsSizeBalancedTreeMethods<TLinkAddress>, typename TUnusedLinks = UnusedLinksListMethods<TLinkAddress>>
    class UnitedMemoryLinks : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLinkAddress, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks>, TLinkAddress, TMemory, TSourceTreeMethods, TTargetTreeMethods, TUnusedLinks> /*, public std::enable_shared_from_this<UnitedMemoryLinks<TLinkAddress>>*/
    {
        using base = UnitedMemoryLinksBase<
            UnitedMemoryLinks<
                TLinkAddress,
                TMemory,
                TSourceTreeMethods,
                TTargetTreeMethods,
                TUnusedLinks>,
            TLinkAddress,
            TMemory,
            TSourceTreeMethods,
            TTargetTreeMethods,
            TUnusedLinks>;

    public:
        using base::DefaultLinksSizeStep;

    public:
        using base::GetLinkStruct;

    public:
        using base::Constants;

    private:
        std::function<void(std::unique_ptr<ILinksTreeMethods<TLinkAddress>>)> _createSourceTreeMethods;

    private:
        std::function<void(std::unique_ptr<ILinksTreeMethods<TLinkAddress>>)> _createTargetTreeMethods;

    private:
        std::byte* _header;

    private:
        std::byte* _links;

        // private: using base::_memory;

        // TODO: implicit constructor for Constants
    public:
        UnitedMemoryLinks(TMemory memory, std::size_t memoryReservationStep = DefaultLinksSizeStep, LinksConstants<TLinkAddress> constants = LinksConstants<TLinkAddress>{} /*, IndexTreeType indexTreeType*/) :
            base(std::move(memory), memoryReservationStep, constants)
        {
            //if (indexTreeType == IndexTreeType.SizedAndThreadedAVLBalancedTree)
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesAvlBalancedTreeMethods<TLinkAddress>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsAvlBalancedTreeMethods<TLinkAddress>(Constants, _links, _header);
            //}
            //else
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesSizeBalancedTreeMethods<TLinkAddress>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsSizeBalancedTreeMethods<TLinkAddress>(Constants, _links, _header);
            //}
            base::Init(this->_memory, memoryReservationStep);
        }

    public:
        void SetPointers(TMemory& memory)
        {
            std::cout << memory.Pointer() << std::endl;
            _links = static_cast<std::byte*>(memory.Pointer());
            _header = _links;
            base::_SourcesTreeMethods = new TSourceTreeMethods(Constants, _links, _header);
            base::_TargetsTreeMethods = new TTargetTreeMethods(Constants, _links, _header);
            base::_UnusedLinksListMethods = new TUnusedLinks(_links, _header);
        }

    public:
        auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<TLinkAddress>*>(_header);
        }

    public:
        auto&& GetLinkReference(TLinkAddress linkIndex) const
        {
            return *(reinterpret_cast<RawLink<TLinkAddress>*>(_links) + linkIndex);
        }

        bool Exists(TLinkAddress link)
        {
            if (IsExternalReference(Constants, link))
            {
                return false;
            }
            return GreaterOrEqualThan(link, Constants.InternalReferencesRange.Minimum) && LessOrEqualThan(link, GetHeaderReference().AllocatedLinks) && !IsUnusedLink(link);
        }

        bool IsUnusedLink(TLinkAddress linkIndex)
        {
            if ((linkIndex != GetHeaderReference().FirstFreeLink) // May be this check is not needed
            {
                auto& link = GetLinkReference(linkIndex);
                return TLinkAddress{} == link.SizeAsSource && TLinkAddress{} != link.Source;
            }
            else
            {
                return true;
            }
        }
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

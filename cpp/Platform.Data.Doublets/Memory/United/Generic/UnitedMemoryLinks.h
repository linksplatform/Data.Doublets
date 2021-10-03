namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;

    template<
        typename TLink,
        typename TMemory = FileMappedResizableDirectMemory,
        typename TSourceTreeMethods = LinksSourcesSizeBalancedTreeMethods<TLink>,
        typename TTargetTreeMethods = LinksTargetsSizeBalancedTreeMethods<TLink>,
        typename TUnusedLinks = UnusedLinksListMethods<TLink>
    >
    class UnitedMemoryLinks
        : public UnitedMemoryLinksBase<
            UnitedMemoryLinks<
                TLink,
                TMemory,
                TSourceTreeMethods,
                TTargetTreeMethods,
                TUnusedLinks
            >, 
            TLink,
            TMemory,
            TSourceTreeMethods,
            TTargetTreeMethods,
            TUnusedLinks
        >,

          public std::enable_shared_from_this<UnitedMemoryLinks<TLink>>
    {
        using base = UnitedMemoryLinksBase<
            UnitedMemoryLinks<
                TLink,
                TMemory,
                TSourceTreeMethods,
                TTargetTreeMethods,
                TUnusedLinks
            >, 
            TLink,
            TMemory,
            TSourceTreeMethods,
            TTargetTreeMethods,
            TUnusedLinks
        >;
        public: using base::DefaultLinksSizeStep;
        //public: using base::GetLinkReference;
        public: using base::GetLinkStruct;
        public: using base::Constants;

        private: std::function<void(std::unique_ptr<ILinksTreeMethods<TLink>>)> _createSourceTreeMethods;

        private: std::function<void(std::unique_ptr<ILinksTreeMethods<TLink>>)> _createTargetTreeMethods;

        private: std::byte* _header;

        private: std::byte* _links;

        //public: UnitedMemoryLinks(const std::string& path, std::size_t memoryReservationStep = DefaultLinksSizeStep)
        //    : UnitedMemoryLinks(std::make_unique<FileMappedResizableDirectMemory>(path, memoryReservationStep), memoryReservationStep)
        //{
        //}

        // TODO: implicit constructor for Constants
        public: UnitedMemoryLinks(TMemory& memory, std::size_t memoryReservationStep = DefaultLinksSizeStep, LinksConstants<TLink> constants = LinksConstants<TLink>{}/*, IndexTreeType indexTreeType*/)
            : base(memory, memoryReservationStep, constants)
        {
            //if (indexTreeType == IndexTreeType.SizedAndThreadedAVLBalancedTree)
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesAvlBalancedTreeMethods<TLink>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsAvlBalancedTreeMethods<TLink>(Constants, _links, _header);
            //}
            //else
            //{
            //    _createSourceTreeMethods = () => new LinksSourcesSizeBalancedTreeMethods<TLink>(Constants, _links, _header);
            //    _createTargetTreeMethods = () => new LinksTargetsSizeBalancedTreeMethods<TLink>(Constants, _links, _header);
            //}
            base::Init(memory, memoryReservationStep);
        }

        public: void SetPointers(TMemory& memory)
        {
            _links = static_cast<std::byte*>(memory.Pointer());
            _header = _links;
            base::_SourcesTreeMethods = new TSourceTreeMethods(Constants, _links, _header);
            base::_TargetsTreeMethods = new TTargetTreeMethods(Constants, _links, _header);
            base::_UnusedLinksListMethods = new TUnusedLinks(_links, _header);

            //base::_SourcesTreeMethods = std::make_unique<LinksSourcesSizeBalancedTreeMethods<TLink>>(Constants, _links, _header);
            //base::_TargetsTreeMethods = std::make_unique<LinksTargetsSizeBalancedTreeMethods<TLink>>(Constants, _links, _header);
            //base::_UnusedLinksListMethods = std::make_unique<UnusedLinksListMethods<TLink>>(_links, _header);
        }

        public: auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<TLink>*>(_header);
        }

        public: auto&& GetLinkReference(TLink linkIndex)const
        {
            return *(reinterpret_cast<RawLink<TLink>*>(_links) + linkIndex);
        }
    };
}

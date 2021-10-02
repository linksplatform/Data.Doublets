namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;

    template<typename TLink>
    class UnitedMemoryLinks
        : public UnitedMemoryLinksBase<UnitedMemoryLinks<TLink>, TLink>,
          public std::enable_shared_from_this<UnitedMemoryLinks<TLink>>
    {
        using base = UnitedMemoryLinksBase<UnitedMemoryLinks<TLink>, TLink>;
        public: using base::DefaultLinksSizeStep;
        //public: using base::GetLinkReference;
        public: using base::GetLinkStruct;
        public: using base::Constants;

        private: std::function<void(std::unique_ptr<ILinksTreeMethods<TLink>>)> _createSourceTreeMethods;

        private: std::function<void(std::unique_ptr<ILinksTreeMethods<TLink>>)> _createTargetTreeMethods;

        private: std::byte* _header;

        private: std::byte* _links;

        public: UnitedMemoryLinks(const std::string& path, std::size_t memoryReservationStep = DefaultLinksSizeStep)
            : UnitedMemoryLinks(std::make_unique<FileMappedResizableDirectMemory>(path, memoryReservationStep), memoryReservationStep)
        {
        }

        // TODO: implicit constructor for Constants
        public: UnitedMemoryLinks(IResizableDirectMemory& memory, std::size_t memoryReservationStep = DefaultLinksSizeStep, LinksConstants<TLink> constants = LinksConstants<TLink>{}/*, IndexTreeType indexTreeType*/)
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

        protected: void SetPointers(IResizableDirectMemory& memory) override
        {
            _links = static_cast<std::byte*>(memory.Pointer());
            _header = _links;
            base::_SourcesTreeMethods = std::unique_ptr<ILinksTreeMethods<TLink>>(new LinksSourcesSizeBalancedTreeMethods<TLink>(Constants, _links, _header));
            base::_TargetsTreeMethods = std::unique_ptr<ILinksTreeMethods<TLink>>(new LinksTargetsSizeBalancedTreeMethods<TLink>(Constants, _links, _header));
            base::_UnusedLinksListMethods = std::unique_ptr<ILinksListMethods<TLink>>(new UnusedLinksListMethods<TLink>(_links, _header));

            //base::_SourcesTreeMethods = std::make_unique<LinksSourcesSizeBalancedTreeMethods<TLink>>(Constants, _links, _header);
            //base::_TargetsTreeMethods = std::make_unique<LinksTargetsSizeBalancedTreeMethods<TLink>>(Constants, _links, _header);
            //base::_UnusedLinksListMethods = std::make_unique<UnusedLinksListMethods<TLink>>(_links, _header);
        }

        protected: void ResetPointers()
        {
            base::ResetPointers();
            _links = nullptr;
            _header = nullptr;
        }

        public: LinksHeader<TLink>& GetHeaderReference()
        {
            return *reinterpret_cast<LinksHeader<TLink>*>(_header);
        }

        public: RawLink<TLink>& GetLinkReference(TLink linkIndex)
        {
            return *(reinterpret_cast<RawLink<TLink>*>(_links) + linkIndex);
        }
    };
}

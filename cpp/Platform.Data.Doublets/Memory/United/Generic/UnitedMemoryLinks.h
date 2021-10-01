

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class UnitedMemoryLinks<TLink> : public UnitedMemoryLinksBase<TLink>
    {
        private: Func<ILinksTreeMethods<TLink>> _createSourceTreeMethods;
        private: Func<ILinksTreeMethods<TLink>> _createTargetTreeMethods;
        private: std::uint8_t* _header;
        private: std::uint8_t* _links;

        public: UnitedMemoryLinks(std::string address) : this(address, DefaultLinksSizeStep) { }

        public: UnitedMemoryLinks(std::string address, std::int64_t memoryReservationStep) : this(FileMappedResizableDirectMemory(address, memoryReservationStep), memoryReservationStep) { }

        public: UnitedMemoryLinks(IResizableDirectMemory &memory) : this(memory, DefaultLinksSizeStep) { }

        public: UnitedMemoryLinks(IResizableDirectMemory &memory, std::int64_t memoryReservationStep) : this(memory, memoryReservationStep, Default<LinksConstants<TLink>>.Instance, IndexTreeType.Default) { }

        public: UnitedMemoryLinks(IResizableDirectMemory &memory, std::int64_t memoryReservationStep, LinksConstants<TLink> constants, IndexTreeType indexTreeType) : base(memory, memoryReservationStep, constants)
        {
            if (indexTreeType == IndexTreeType.SizedAndThreadedAVLBalancedTree)
            {
                _createSourceTreeMethods = () { return LinksSourcesAvlBalancedTreeMethods<TLink>(Constants, _links, _header); }
                _createTargetTreeMethods = () { return LinksTargetsAvlBalancedTreeMethods<TLink>(Constants, _links, _header); }
            }
            else
            {
                _createSourceTreeMethods = () { return LinksSourcesSizeBalancedTreeMethods<TLink>(Constants, _links, _header); }
                _createTargetTreeMethods = () { return LinksTargetsSizeBalancedTreeMethods<TLink>(Constants, _links, _header); }
            }
            Init(memory, memoryReservationStep);
        }

        protected: void SetPointers(IResizableDirectMemory &memory) override
        {
            _links = (std::uint8_t*)memory.Pointer;
            _header = _links;
            SourcesTreeMethods = _createSourceTreeMethods();
            TargetsTreeMethods = _createTargetTreeMethods();
            UnusedLinksListMethods = UnusedLinksListMethods<TLink>(_links, _header);
        }

        protected: override void ResetPointers()
        {
            base.ResetPointers();
            _links = {};
            _header = {};
        }

        protected: override ref LinksHeader<TLink> GetHeaderReference() { return ref AsRef<LinksHeader<TLink>>(_header); }

        protected: override ref RawLink<TLink> GetLinkReference(TLink linkIndex) { return ref AsRef<RawLink<TLink>>(_links + (LinkSizeInBytes * ConvertToInt64(linkIndex))); }
    };
}
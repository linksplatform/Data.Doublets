

using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64UnusedLinksListMethods : public UnusedLinksListMethods<TLinkAddress>
    {
        private: readonly RawLinkDataPart<std::uint64_t>* _links;
        private: readonly LinksHeader<std::uint64_t>* _header;

        public: UInt64UnusedLinksListMethods(RawLinkDataPart<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header)
            : base((std::uint8_t*)storage, (std::uint8_t*)header)
        {
            _links = storage;
            _header = header;
        }

        protected: override ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return &_links[link]; }

        protected: override ref LinksHeader<TLinkAddress> GetHeaderReference() { return ref *_header; }
    };
}

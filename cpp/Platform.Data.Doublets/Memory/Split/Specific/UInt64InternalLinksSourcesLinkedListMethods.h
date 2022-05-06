
using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class UInt64InternalLinksSourcesLinkedListMethods : public InternalLinksSourcesLinkedListMethods<TLinkAddress>
    {
        private: RawLinkDataPart<TLinkAddress>* _linksDataParts;
        private: RawLinkIndexPart<TLinkAddress>* _linksIndexParts;

        public: UInt64InternalLinksSourcesLinkedListMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts)
            : base(constants, (std::uint8_t*)linksDataParts, (std::uint8_t*)linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
        }

        protected: override ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return &_linksDataParts[link]; }

        protected: override ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link) { return &_linksIndexParts[link]; }
    };
}

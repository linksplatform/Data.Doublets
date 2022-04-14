
using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class UInt32InternalLinksSourcesLinkedListMethods : public InternalLinksSourcesLinkedListMethods<TLinkAddress>
    {
        private: readonly RawLinkDataPart<TLinkAddress>* _linksDataParts;
        private: readonly RawLinkIndexPart<TLinkAddress>* _linksIndexParts;

        public: UInt32InternalLinksSourcesLinkedListMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts)
            : base(constants, (std::uint8_t*)linksDataParts, (std::uint8_t*)linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
        }

        protected: override ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return &_linksDataParts[link]; }

        protected: override ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link) { return &_linksIndexParts[link]; }
    };
}

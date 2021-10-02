
using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class UInt64InternalLinksSourcesLinkedListMethods : public InternalLinksSourcesLinkedListMethods<TLink>
    {
        private: readonly RawLinkDataPart<TLink>* _linksDataParts;
        private: readonly RawLinkIndexPart<TLink>* _linksIndexParts;

        public: UInt64InternalLinksSourcesLinkedListMethods(LinksConstants<TLink> constants, RawLinkDataPart<TLink>* linksDataParts, RawLinkIndexPart<TLink>* linksIndexParts)
            : base(constants, (std::uint8_t*)linksDataParts, (std::uint8_t*)linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
        }

        protected: override ref RawLinkDataPart<TLink> GetLinkDataPartReference(TLink link) { return &_linksDataParts[link]; }

        protected: override ref RawLinkIndexPart<TLink> GetLinkIndexPartReference(TLink link) { return &_linksIndexParts[link]; }
    };
}

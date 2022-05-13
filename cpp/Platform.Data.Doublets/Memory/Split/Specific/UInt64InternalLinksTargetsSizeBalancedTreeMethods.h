
using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64InternalLinksTargetsSizeBalancedTreeMethods : public UInt64InternalLinksSizeBalancedTreeMethodsBase
    {
        public: UInt64InternalLinksTargetsSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        public: ref std::uint64_t GetLeftReference(std::uint64_t node) { return ref LinksIndexParts[node].LeftAsTarget; }

        public: ref std::uint64_t GetRightReference(std::uint64_t node) { return ref LinksIndexParts[node].RightAsTarget; }

        public: TLinkAddress GetLeft(TLinkAddress node) { return LinksIndexParts[node].LeftAsTarget; }

        public: TLinkAddress GetRight(TLinkAddress node) { return LinksIndexParts[node].RightAsTarget; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) { LinksIndexParts[node].LeftAsTarget = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) { LinksIndexParts[node].RightAsTarget = right; }

        public: TLinkAddress GetSize(TLinkAddress node) { return LinksIndexParts[node].SizeAsTarget; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) { LinksIndexParts[node].SizeAsTarget = size; }

        public: TLinkAddress GetTreeRoot(TLinkAddress node) { return LinksIndexParts[node].RootAsTarget; }

        public: TLinkAddress GetBasePartValue(TLinkAddress node) { return LinksDataParts[node].Target; }

        public: TLinkAddress GetKeyPartValue(TLinkAddress node) { return LinksDataParts[node].Source; }

        public: void ClearNode(TLinkAddress node)
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) { return this->SearchCore(this->GetTreeRoot(target), source); }
    };
}


using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt32ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods : public UInt32ExternalLinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) { return ref LinksIndexParts[node].LeftAsTarget; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) { return ref LinksIndexParts[node].RightAsTarget; }

        public: TLinkAddress GetLeft(TLinkAddress node) { return LinksIndexParts[node].LeftAsTarget; }

        public: TLinkAddress GetRight(TLinkAddress node) { return LinksIndexParts[node].RightAsTarget; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) { LinksIndexParts[node].LeftAsTarget = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) { LinksIndexParts[node].RightAsTarget = right; }

        public: TLinkAddress GetSize(TLinkAddress node) { return LinksIndexParts[node].SizeAsTarget; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) { LinksIndexParts[node].SizeAsTarget = size; }

        public: TLinkAddress GetTreeRoot() { return Header->RootAsTarget; }

        public: TLinkAddress GetBasePartValue(TLinkAddress node) { return LinksDataParts[node].Target; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return firstTarget < secondTarget || firstTarget == secondTarget && firstSource < secondSource; }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return firstTarget > secondTarget || firstTarget == secondTarget && firstSource > secondSource; }

        public: void ClearNode(TLinkAddress node)
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

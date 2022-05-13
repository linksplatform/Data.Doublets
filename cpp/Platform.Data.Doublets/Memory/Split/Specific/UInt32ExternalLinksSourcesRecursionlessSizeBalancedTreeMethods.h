
using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt32ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods : public UInt32ExternalLinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) { return ref LinksIndexParts[node].LeftAsSource; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) { return ref LinksIndexParts[node].RightAsSource; }

        public: TLinkAddress GetLeft(TLinkAddress node) { return LinksIndexParts[node].LeftAsSource; }

        public: TLinkAddress GetRight(TLinkAddress node) { return LinksIndexParts[node].RightAsSource; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) { LinksIndexParts[node].LeftAsSource = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) { LinksIndexParts[node].RightAsSource = right; }

        public: TLinkAddress GetSize(TLinkAddress node) { return LinksIndexParts[node].SizeAsSource; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) { LinksIndexParts[node].SizeAsSource = size; }

        public: TLinkAddress GetTreeRoot() { return Header->RootAsSource; }

        public: TLinkAddress GetBasePartValue(TLinkAddress node) { return LinksDataParts[node].Source; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return firstSource < secondSource || firstSource == secondSource && firstTarget < secondTarget; }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return firstSource > secondSource || firstSource == secondSource && firstTarget > secondTarget; }

        public: void ClearNode(TLinkAddress node)
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

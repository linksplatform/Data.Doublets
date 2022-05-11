
using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt32ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods : public UInt32ExternalLinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return ref LinksIndexParts[node].LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return ref LinksIndexParts[node].RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return LinksIndexParts[node].LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return LinksIndexParts[node].RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { LinksIndexParts[node].LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { LinksIndexParts[node].RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return LinksIndexParts[node].SizeAsSource; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { LinksIndexParts[node].SizeAsSource = size; }

        protected: override TLinkAddress GetTreeRoot() { return Header->RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress node) override { return LinksDataParts[node].Source; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstSource < secondSource || firstSource == secondSource && firstTarget < secondTarget; }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstSource > secondSource || firstSource == secondSource && firstTarget > secondTarget; }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = LinksIndexParts[node];
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

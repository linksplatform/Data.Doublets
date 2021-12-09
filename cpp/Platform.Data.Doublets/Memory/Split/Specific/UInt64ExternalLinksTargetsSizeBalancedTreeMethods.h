
using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64ExternalLinksTargetsSizeBalancedTreeMethods : public UInt64ExternalLinksSizeBalancedTreeMethodsBase
    {
        public: UInt64ExternalLinksTargetsSizeBalancedTreeMethods(LinksConstants<TLink> constants, RawLinkDataPart<TLink>* linksDataParts, RawLinkIndexPart<TLink>* linksIndexParts, LinksHeader<TLink>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return ref LinksIndexParts[node].LeftAsTarget; }

        protected: TLink* GetRightReference(TLink node) override { return ref LinksIndexParts[node].RightAsTarget; }

        protected: TLink GetLeft(TLink node) override { return LinksIndexParts[node].LeftAsTarget; }

        protected: TLink GetRight(TLink node) override { return LinksIndexParts[node].RightAsTarget; }

        protected: void SetLeft(TLink node, TLink left) override { LinksIndexParts[node].LeftAsTarget = left; }

        protected: void SetRight(TLink node, TLink right) override { LinksIndexParts[node].RightAsTarget = right; }

        protected: TLink GetSize(TLink node) override { return LinksIndexParts[node].SizeAsTarget; }

        protected: void SetSize(TLink node, TLink size) override { LinksIndexParts[node].SizeAsTarget = size; }

        protected: override TLink GetTreeRoot() { return Header->RootAsTarget; }

        protected: TLink GetBasePartValue(TLink node) override { return LinksDataParts[node].Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstTarget < secondTarget || firstTarget == secondTarget && firstSource < secondSource; }

        protected: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstTarget > secondTarget || firstTarget == secondTarget && firstSource > secondSource; }

        protected: void ClearNode(TLink node) override
        {
            auto* link = LinksIndexParts[node];
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}
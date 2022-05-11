
using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64InternalLinksTargetsRecursionlessSizeBalancedTreeMethods : public UInt64InternalLinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt64InternalLinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: ref std::uint64_t GetLeftReference(std::uint64_t node) override { return ref LinksIndexParts[node].LeftAsTarget; }

        protected: ref std::uint64_t GetRightReference(std::uint64_t node) override { return ref LinksIndexParts[node].RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return LinksIndexParts[node].LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return LinksIndexParts[node].RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { LinksIndexParts[node].LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { LinksIndexParts[node].RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return LinksIndexParts[node].SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { LinksIndexParts[node].SizeAsTarget = size; }

        protected: TLinkAddress GetTreeRoot(TLinkAddress node) override { return LinksIndexParts[node].RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress node) override { return LinksDataParts[node].Target; }

        protected: TLinkAddress GetKeyPartValue(TLinkAddress node) override { return LinksDataParts[node].Source; }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) override { return this->SearchCore(this->GetTreeRoot(target), source); }
    };
}

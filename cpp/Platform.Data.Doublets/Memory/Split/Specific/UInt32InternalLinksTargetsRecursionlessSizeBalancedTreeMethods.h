
using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt32InternalLinksTargetsRecursionlessSizeBalancedTreeMethods : public UInt32InternalLinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32InternalLinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) override { return ref LinksIndexParts[node].LeftAsTarget; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) override { return ref LinksIndexParts[node].RightAsTarget; }

        public: TLinkAddress GetLeft(TLinkAddress node) override { return LinksIndexParts[node].LeftAsTarget; }

        public: TLinkAddress GetRight(TLinkAddress node) override { return LinksIndexParts[node].RightAsTarget; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) override { LinksIndexParts[node].LeftAsTarget = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) override { LinksIndexParts[node].RightAsTarget = right; }

        public: TLinkAddress GetSize(TLinkAddress node) override { return LinksIndexParts[node].SizeAsTarget; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) override { LinksIndexParts[node].SizeAsTarget = size; }

        public: TLinkAddress GetTreeRoot(TLinkAddress node) override { return LinksIndexParts[node].RootAsTarget; }

        public: TLinkAddress GetBasePartValue(TLinkAddress node) override { return LinksDataParts[node].Target; }

        public: TLinkAddress GetKeyPartValue(TLinkAddress node) override { return LinksDataParts[node].Source; }

        public: void ClearNode(TLinkAddress node) override
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) override { return this->SearchCore(this->GetTreeRoot(target), source); }
    };
}

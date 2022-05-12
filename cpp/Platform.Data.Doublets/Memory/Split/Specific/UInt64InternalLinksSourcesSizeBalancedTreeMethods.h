
using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64InternalLinksSourcesSizeBalancedTreeMethods : public UInt64InternalLinksSizeBalancedTreeMethodsBase
    {
        public: UInt64InternalLinksSourcesSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, RawLinkDataPart<TLinkAddress>* linksDataParts, RawLinkIndexPart<TLinkAddress>* linksIndexParts, LinksHeader<TLinkAddress>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) override { return ref LinksIndexParts[node].LeftAsSource; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) override { return ref LinksIndexParts[node].RightAsSource; }

        public: TLinkAddress GetLeft(TLinkAddress node) override { return LinksIndexParts[node].LeftAsSource; }

        public: TLinkAddress GetRight(TLinkAddress node) override { return LinksIndexParts[node].RightAsSource; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) override { LinksIndexParts[node].LeftAsSource = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) override { LinksIndexParts[node].RightAsSource = right; }

        public: TLinkAddress GetSize(TLinkAddress node) override { return LinksIndexParts[node].SizeAsSource; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) override { LinksIndexParts[node].SizeAsSource = size; }

        public: TLinkAddress GetTreeRoot(TLinkAddress node) override { return LinksIndexParts[node].RootAsSource; }

        public: TLinkAddress GetBasePartValue(TLinkAddress node) override { return LinksDataParts[node].Source; }

        public: TLinkAddress GetKeyPartValue(TLinkAddress node) override { return LinksDataParts[node].Target; }

        public: void ClearNode(TLinkAddress node) override
        {
            auto& link = LinksIndexParts[node];
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) override { return this->SearchCore(this->GetTreeRoot(source), target); }
    };
}

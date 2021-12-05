
using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64InternalLinksSourcesSizeBalancedTreeMethods : public UInt64InternalLinksSizeBalancedTreeMethodsBase
    {
        public: UInt64InternalLinksSourcesSizeBalancedTreeMethods(LinksConstants<TLink> constants, RawLinkDataPart<TLink>* linksDataParts, RawLinkIndexPart<TLink>* linksIndexParts, LinksHeader<TLink>* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return ref LinksIndexParts[node].LeftAsSource; }

        protected: TLink* GetRightReference(TLink node) override { return ref LinksIndexParts[node].RightAsSource; }

        protected: TLink GetLeft(TLink node) override { return LinksIndexParts[node].LeftAsSource; }

        protected: TLink GetRight(TLink node) override { return LinksIndexParts[node].RightAsSource; }

        protected: void SetLeft(TLink node, TLink left) override { LinksIndexParts[node].LeftAsSource = left; }

        protected: void SetRight(TLink node, TLink right) override { LinksIndexParts[node].RightAsSource = right; }

        protected: TLink GetSize(TLink node) override { return LinksIndexParts[node].SizeAsSource; }

        protected: void SetSize(TLink node, TLink size) override { LinksIndexParts[node].SizeAsSource = size; }

        protected: TLink GetTreeRoot(TLink node) override { return LinksIndexParts[node].RootAsSource; }

        protected: TLink GetBasePartValue(TLink node) override { return LinksDataParts[node].Source; }

        protected: TLink GetKeyPartValue(TLink node) override { return LinksDataParts[node].Target; }

        protected: void ClearNode(TLink node) override
        {
            auto* link = LinksIndexParts[node];
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: TLink Search(TLink source, TLink target) override { return this->SearchCore(this->GetTreeRoot(source), target); }
    };
}
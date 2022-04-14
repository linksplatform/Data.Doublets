namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksTargetsSizeBalancedTreeMethods<TLinkAddress> : public InternalLinksSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: InternalLinksTargetsSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkIndexPartReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkIndexPartReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkIndexPartReference(node)->SizeAsTarget = size; }

        protected: TLinkAddress GetTreeRoot(TLinkAddress link) override { return this->GetLinkIndexPartReference(link)->RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Target; }

        protected: TLinkAddress GetKeyPartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Source; }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) override { return this->SearchCore(this->GetTreeRoot(target), source); }
    };
}

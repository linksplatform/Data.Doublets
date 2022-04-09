namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public InternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: InternalLinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected: TLinkAddress GetTreeRoot(TLinkAddress link) override { return this->GetLinkIndexPartReference(link)->RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Source; }

        protected: TLinkAddress GetKeyPartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Target; }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target) override { return this->SearchCore(this->GetTreeRoot(source), target); }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksSourcesRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public LinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkReference(node)->LeftAsSource; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkReference(node)->RightAsSource; }

        public: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkReference(node)->LeftAsSource; }

        public: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkReference(node)->RightAsSource; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkReference(node)->LeftAsSource = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkReference(node)->RightAsSource = right; }

        public: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkReference(node)->SizeAsSource; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkReference(node)->SizeAsSource = size; }

        public: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        public: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkReference(link)->Source; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(TLinkAddress node) override
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksSourcesAvlBalancedTreeMethods<TLinkAddress> : public LinksAvlBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksSourcesAvlBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkReference(node)->LeftAsSource; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkReference(node)->RightAsSource; }

        public: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkReference(node).LeftAsSource; }

        public: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkReference(node).RightAsSource; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkReference(node).LeftAsSource = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkReference(node).RightAsSource = right; }

        public: TLinkAddress GetSize(TLinkAddress node) override { return this->GetSizeValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetSize(TLinkAddress node, TLinkAddress size) override { this->SetSizeValue(this->GetLinkReference(node).SizeAsSource, size); }

        public: bool GetLeftIsChild(TLinkAddress node) override { return this->GetLeftIsChildValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetLeftIsChild(TLinkAddress node, bool value) override { this->SetLeftIsChildValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: bool GetRightIsChild(TLinkAddress node) override { return this->GetRightIsChildValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetRightIsChild(TLinkAddress node, bool value) override { this->SetRightIsChildValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: std::uint8_t GetBalance(TLinkAddress node) override { return this->GetBalanceValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetBalance(TLinkAddress node, std::uint8_t value) override { this->SetBalanceValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        public: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkReference(link).Source; }

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

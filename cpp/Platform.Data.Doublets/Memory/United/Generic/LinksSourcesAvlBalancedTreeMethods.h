namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksSourcesAvlBalancedTreeMethods<TLinkAddress> : public LinksAvlBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksSourcesAvlBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* storage, std::uint8_t* header) : base(constants, storage, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkReference(node)->LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkReference(node)->RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkReference(node)->LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkReference(node)->RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkReference(node)->RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetSizeValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->SetSizeValue(this->GetLinkReference(node)->SizeAsSource, size); }

        protected: bool GetLeftIsChild(TLinkAddress node) override { return this->GetLeftIsChildValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetLeftIsChild(TLinkAddress node, bool value) override { this->SetLeftIsChildValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: bool GetRightIsChild(TLinkAddress node) override { return this->GetRightIsChildValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetRightIsChild(TLinkAddress node, bool value) override { this->SetRightIsChildValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: std::uint8_t GetBalance(TLinkAddress node) override { return this->GetBalanceValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetBalance(TLinkAddress node, std::uint8_t value) override { this->SetBalanceValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkReference(link)->Source; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsAvlBalancedTreeMethods<TLinkAddress> : public LinksAvlBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksTargetsAvlBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* storage, std::uint8_t* header) : base(constants, storage, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetSizeValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->SetSizeValue(this->GetLinkReference(node)->SizeAsTarget, size); }

        protected: bool GetLeftIsChild(TLinkAddress node) override { return this->GetLeftIsChildValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetLeftIsChild(TLinkAddress node, bool value) override { this->SetLeftIsChildValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: bool GetRightIsChild(TLinkAddress node) override { return this->GetRightIsChildValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetRightIsChild(TLinkAddress node, bool value) override { this->SetRightIsChildValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: std::uint8_t GetBalance(TLinkAddress node) override { return this->GetBalanceValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetBalance(TLinkAddress node, std::uint8_t value) override { this->SetBalanceValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkReference(link)->Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

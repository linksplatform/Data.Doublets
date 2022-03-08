namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsAvlBalancedTreeMethods<TLink> : public LinksAvlBalancedTreeMethodsBase<TLink>
    {
        public: LinksTargetsAvlBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* storage, std::uint8_t* header) : base(constants, storage, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkReference(node)->LeftAsTarget; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkReference(node)->RightAsTarget; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkReference(node)->LeftAsTarget; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkReference(node)->RightAsTarget; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkReference(node)->RightAsTarget = right; }

        protected: TLink GetSize(TLink node) override { return this->GetSizeValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetSize(TLink node, TLink size) override { this->SetSizeValue(this->GetLinkReference(node)->SizeAsTarget, size); }

        protected: bool GetLeftIsChild(TLink node) override { return this->GetLeftIsChildValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetLeftIsChild(TLink node, bool value) override { this->SetLeftIsChildValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: bool GetRightIsChild(TLink node) override { return this->GetRightIsChildValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetRightIsChild(TLink node, bool value) override { this->SetRightIsChildValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: std::int8_t GetBalance(TLink node) override { return this->GetBalanceValue(this->GetLinkReference(node)->SizeAsTarget); }

        protected: void SetBalance(TLink node, std::int8_t value) override { this->SetBalanceValue(this->GetLinkReference(node)->SizeAsTarget, value); }

        protected: override TLink GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        protected: TLink GetBasePartValue(TLink link) override { return this->GetLinkReference(link)->Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        protected: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(TLink node) override
        {
            auto* link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

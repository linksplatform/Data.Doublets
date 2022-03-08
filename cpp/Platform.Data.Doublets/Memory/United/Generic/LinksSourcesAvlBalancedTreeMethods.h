namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksSourcesAvlBalancedTreeMethods<TLink> : public LinksAvlBalancedTreeMethodsBase<TLink>
    {
        public: LinksSourcesAvlBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* storage, std::uint8_t* header) : base(constants, storage, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkReference(node)->LeftAsSource; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkReference(node)->RightAsSource; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkReference(node)->LeftAsSource; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkReference(node)->RightAsSource; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkReference(node)->RightAsSource = right; }

        protected: TLink GetSize(TLink node) override { return this->GetSizeValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetSize(TLink node, TLink size) override { this->SetSizeValue(this->GetLinkReference(node)->SizeAsSource, size); }

        protected: bool GetLeftIsChild(TLink node) override { return this->GetLeftIsChildValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetLeftIsChild(TLink node, bool value) override { this->SetLeftIsChildValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: bool GetRightIsChild(TLink node) override { return this->GetRightIsChildValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetRightIsChild(TLink node, bool value) override { this->SetRightIsChildValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: std::int8_t GetBalance(TLink node) override { return this->GetBalanceValue(this->GetLinkReference(node)->SizeAsSource); }

        protected: void SetBalance(TLink node, std::int8_t value) override { this->SetBalanceValue(this->GetLinkReference(node)->SizeAsSource, value); }

        protected: override TLink GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        protected: TLink GetBasePartValue(TLink link) override { return this->GetLinkReference(link)->Source; }

        protected: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        protected: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(TLink node) override
        {
            auto* link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

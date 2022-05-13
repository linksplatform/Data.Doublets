namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsAvlBalancedTreeMethods<TLinkAddress> : public LinksAvlBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksTargetsAvlBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) { return &GetLinkReference(node)->LeftAsTarget; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) { return &GetLinkReference(node)->RightAsTarget; }

        public: TLinkAddress GetLeft(TLinkAddress node) { return this->GetLinkReference(node).LeftAsTarget; }

        public: TLinkAddress GetRight(TLinkAddress node) { return this->GetLinkReference(node).RightAsTarget; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) { this->GetLinkReference(node).LeftAsTarget = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) { this->GetLinkReference(node).RightAsTarget = right; }

        public: TLinkAddress GetSize(TLinkAddress node) { return this->GetSizeValue(this->GetLinkReference(node).SizeAsTarget); }

        public: void SetSize(TLinkAddress node, TLinkAddress size) { this->SetSizeValue(this->GetLinkReference(node).SizeAsTarget, size); }

        public: bool GetLeftIsChild(TLinkAddress node) { return this->GetLeftIsChildValue(this->GetLinkReference(node).SizeAsTarget); }

        public: void SetLeftIsChild(TLinkAddress node, bool value) { this->SetLeftIsChildValue(this->GetLinkReference(node).SizeAsTarget, value); }

        public: bool GetRightIsChild(TLinkAddress node) { return this->GetRightIsChildValue(this->GetLinkReference(node).SizeAsTarget); }

        public: void SetRightIsChild(TLinkAddress node, bool value) { this->SetRightIsChildValue(this->GetLinkReference(node).SizeAsTarget, value); }

        public: std::uint8_t GetBalance(TLinkAddress node) { return this->GetBalanceValue(this->GetLinkReference(node).SizeAsTarget); }

        public: void SetBalance(TLinkAddress node, std::uint8_t value) { this->SetBalanceValue(this->GetLinkReference(node).SizeAsTarget, value); }

        public: TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        public: TLinkAddress GetBasePartValue(TLinkAddress link) { return this->GetLinkReference(link).Target; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        public: void ClearNode(TLinkAddress node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

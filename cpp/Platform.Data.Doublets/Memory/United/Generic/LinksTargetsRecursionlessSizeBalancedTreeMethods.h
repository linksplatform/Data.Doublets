namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public LinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkReference(node)->SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkReference(node)->SizeAsTarget = size; }

        protected: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkReference(link)->Target; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

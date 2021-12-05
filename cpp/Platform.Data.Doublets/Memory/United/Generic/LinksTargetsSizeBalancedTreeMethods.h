namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsSizeBalancedTreeMethods<TLink> : public LinksSizeBalancedTreeMethodsBase<TLink>
    {
        public: LinksTargetsSizeBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* links, std::uint8_t* header) : base(constants, links, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkReference(node)->LeftAsTarget; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkReference(node)->RightAsTarget; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkReference(node)->LeftAsTarget; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkReference(node)->RightAsTarget; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkReference(node)->RightAsTarget = right; }

        protected: TLink GetSize(TLink node) override { return this->GetLinkReference(node)->SizeAsTarget; }

        protected: void SetSize(TLink node, TLink size) override { this->GetLinkReference(node)->SizeAsTarget = size; }

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
namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLink> : public ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLink>
    {
        public: ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkIndexPartReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkIndexPartReference(node)->RightAsTarget = right; }

        protected: TLink GetSize(TLink node) override { return this->GetLinkIndexPartReference(node)->SizeAsTarget; }

        protected: void SetSize(TLink node, TLink size) override { this->GetLinkIndexPartReference(node)->SizeAsTarget = size; }

        protected: override TLink GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        protected: TLink GetBasePartValue(TLink link) override { return this->GetLinkDataPartReference(link)->Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        protected: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(TLink node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}
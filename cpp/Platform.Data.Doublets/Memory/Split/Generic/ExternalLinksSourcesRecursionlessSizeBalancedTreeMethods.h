namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLink> : public ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLink>
    {
        public: ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: TLink GetSize(TLink node) override { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(TLink node, TLink size) override { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected: override TLink GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        protected: TLink GetBasePartValue(TLink link) override { return this->GetLinkDataPartReference(link)->Source; }

        protected: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        protected: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(TLink node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}
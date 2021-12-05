namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksSourcesSizeBalancedTreeMethods<TLink> : public LinksSizeBalancedTreeMethodsBase<TLink>
    {
        public: LinksSourcesSizeBalancedTreeMethods(LinksConstants<TLink> constants, std::uint8_t* links, std::uint8_t* header) : base(constants, links, header) { }

        protected: TLink* GetLeftReference(TLink node) override { return &GetLinkReference(node)->LeftAsSource; }

        protected: TLink* GetRightReference(TLink node) override { return &GetLinkReference(node)->RightAsSource; }

        protected: TLink GetLeft(TLink node) override { return this->GetLinkReference(node)->LeftAsSource; }

        protected: TLink GetRight(TLink node) override { return this->GetLinkReference(node)->RightAsSource; }

        protected: void SetLeft(TLink node, TLink left) override { this->GetLinkReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLink node, TLink right) override { this->GetLinkReference(node)->RightAsSource = right; }

        protected: TLink GetSize(TLink node) override { return this->GetLinkReference(node)->SizeAsSource; }

        protected: void SetSize(TLink node, TLink size) override { this->GetLinkReference(node)->SizeAsSource = size; }

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
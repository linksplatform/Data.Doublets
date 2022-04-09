namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Source; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

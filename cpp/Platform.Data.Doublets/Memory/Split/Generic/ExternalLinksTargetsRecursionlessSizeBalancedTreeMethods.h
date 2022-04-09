namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node) override { return &GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left) override { this->GetLinkIndexPartReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right) override { this->GetLinkIndexPartReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node) override { return this->GetLinkIndexPartReference(node)->SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size) override { this->GetLinkIndexPartReference(node)->SizeAsTarget = size; }

        protected: override TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link) override { return this->GetLinkDataPartReference(link)->Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return this->LessThan(firstTarget, secondTarget) || (firstTarget == secondTarget && this->LessThan(firstSource, secondSource)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(TLinkAddress node) override
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

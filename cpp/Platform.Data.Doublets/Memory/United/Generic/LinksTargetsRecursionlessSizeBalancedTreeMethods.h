namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksTargetsRecursionlessSizeBalancedTreeMethods<TLinkAddress> : public LinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: LinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: TLinkAddress* GetLeftReference(TLinkAddress node) { return &GetLinkReference(node)->LeftAsTarget; }

        public: TLinkAddress* GetRightReference(TLinkAddress node) { return &GetLinkReference(node)->RightAsTarget; }

        public: TLinkAddress GetLeft(TLinkAddress node) { return this->GetLinkReference(node).LeftAsTarget; }

        public: TLinkAddress GetRight(TLinkAddress node) { return this->GetLinkReference(node).RightAsTarget; }

        public: void SetLeft(TLinkAddress node, TLinkAddress left) { this->GetLinkReference(node).LeftAsTarget = left; }

        public: void SetRight(TLinkAddress node, TLinkAddress right) { this->GetLinkReference(node).RightAsTarget = right; }

        public: TLinkAddress GetSize(TLinkAddress node) { return this->GetLinkReference(node).SizeAsTarget; }

        public: void SetSize(TLinkAddress node, TLinkAddress size) { this->GetLinkReference(node).SizeAsTarget = size; }

        public: TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        public: TLinkAddress GetBasePartValue(TLinkAddress link) { return this->GetLinkReference(link).Target; }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget) { return (firstTarget < secondTarget) || (firstTarget == secondTarget && (firstSource < secondSource)); }

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

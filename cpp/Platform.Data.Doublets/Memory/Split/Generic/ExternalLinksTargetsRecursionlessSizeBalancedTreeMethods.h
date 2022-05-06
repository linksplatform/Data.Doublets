namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods : public ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinkOptions>
    {
    public: using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        public: static constexpr auto Constants = LinksOptionsType::Constants;
        public: ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left)  { this->GetLinkIndexPartReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right)  { this->GetLinkIndexPartReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size)  { this->GetLinkIndexPartReference(node)->SizeAsTarget = size; }

        protected:  TLinkAddress GetTreeRoot() { return this->GetHeaderReference().RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Target; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)  { return (firstTarget < secondTarget) || (firstTarget == secondTarget && (firstSource < secondSource)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)  { return (firstTarget > secondTarget) || (firstTarget == secondTarget && (firstSource > secondSource)); }

        protected: void ClearNode(TLinkAddress node)
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

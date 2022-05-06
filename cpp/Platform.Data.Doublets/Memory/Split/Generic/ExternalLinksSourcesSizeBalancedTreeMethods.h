namespace Platform::Data::Doublets::Memory::Split::Generic
{
    class ExternalLinksSourcesSizeBalancedTreeMethods : public ExternalLinksSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: ExternalLinksSourcesSizeBalancedTreeMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left)  { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right)  { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size)  { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected:  TLinkAddress GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Source; }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)  { return this->LessThan(firstSource, secondSource) || (firstSource == secondSource && this->LessThan(firstTarget, secondTarget)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress firstSource, TLinkAddress firstTarget, TLinkAddress secondSource, TLinkAddress secondTarget)  { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(TLinkAddress node)
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

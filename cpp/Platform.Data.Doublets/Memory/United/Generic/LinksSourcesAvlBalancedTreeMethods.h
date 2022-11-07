namespace Platform::Data::Doublets::Memory::United::Generic
{
template<typename TLinksOptions>
class LinksSourcesAvlBalancedTreeMethods : public LinksAvlBalancedTreeMethodsBase<LinkAddressType>
    {
        public: LinksSourcesAvlBalancedTreeMethods(LinksConstants<LinkAddressType> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: LinkAddressType* GetLeftReference(LinkAddressType node) { return &GetLinkReference(node)->LeftAsSource; }

        public: LinkAddressType* GetRightReference(LinkAddressType node) { return &GetLinkReference(node)->RightAsSource; }

        public: LinkAddressType GetLeft(LinkAddressType node) { return this->GetLinkReference(node).LeftAsSource; }

        public: LinkAddressType GetRight(LinkAddressType node) { return this->GetLinkReference(node).RightAsSource; }

        public: void SetLeft(LinkAddressType node, LinkAddressType left) { this->GetLinkReference(node).LeftAsSource = left; }

        public: void SetRight(LinkAddressType node, LinkAddressType right) { this->GetLinkReference(node).RightAsSource = right; }

        public: LinkAddressType GetSize(LinkAddressType node) { return this->GetSizeValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetSize(LinkAddressType node, LinkAddressType size) { this->SetSizeValue(this->GetLinkReference(node).SizeAsSource, size); }

        public: bool GetLeftIsChild(LinkAddressType node) { return this->GetLeftIsChildValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetLeftIsChild(LinkAddressType node, bool value) { this->SetLeftIsChildValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: bool GetRightIsChild(LinkAddressType node) { return this->GetRightIsChildValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetRightIsChild(LinkAddressType node, bool value) { this->SetRightIsChildValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: std::uint8_t GetBalance(LinkAddressType node) { return this->GetBalanceValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetBalance(LinkAddressType node, std::uint8_t value) { this->SetBalanceValue(this->GetLinkReference(node).SizeAsSource, value); }

        public: LinkAddressType GetTreeRoot() { return GetHeaderReference().RootAsSource; }

        public: LinkAddressType GetBasePartValue(LinkAddressType link) { return this->GetLinkReference(link).Source; }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return (firstSource < secondSource) || (firstSource == secondSource && (firstTarget < secondTarget)); }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(LinkAddressType node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }
    };
}

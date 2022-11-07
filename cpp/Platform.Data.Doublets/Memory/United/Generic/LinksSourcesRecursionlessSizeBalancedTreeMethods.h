namespace Platform::Data::Doublets::Memory::United::Generic
{
template<typename TLinksOptions>
class LinksSourcesRecursionlessSizeBalancedTreeMethods: public LinksRecursionlessSizeBalancedTreeMethodsBase<LinkAddressType>
    {
        public: LinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<LinkAddressType> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: LinkAddressType* GetLeftReference(LinkAddressType node) { return &GetLinkReference(node)->LeftAsSource; }

        public: LinkAddressType* GetRightReference(LinkAddressType node) { return &GetLinkReference(node)->RightAsSource; }

        public: LinkAddressType GetLeft(LinkAddressType node) { return this->GetLinkReference(node).LeftAsSource; }

        public: LinkAddressType GetRight(LinkAddressType node) { return this->GetLinkReference(node).RightAsSource; }

        public: void SetLeft(LinkAddressType node, LinkAddressType left) { this->GetLinkReference(node).LeftAsSource = left; }

        public: void SetRight(LinkAddressType node, LinkAddressType right) { this->GetLinkReference(node).RightAsSource = right; }

        public: LinkAddressType GetSize(LinkAddressType node) { return this->GetLinkReference(node).SizeAsSource; }

        public: void SetSize(LinkAddressType node, LinkAddressType size) { this->GetLinkReference(node).SizeAsSource = size; }

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

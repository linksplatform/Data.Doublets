namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<typename TLinksOptions>
    class LinksTargetsSizeBalancedTreeMethods
        : public LinksSizeBalancedTreeMethodsBase<LinksTargetsSizeBalancedTreeMethods<TLinksOptions>, TLinksOptions>
    {
        using base = LinksSizeBalancedTreeMethodsBase<LinksTargetsSizeBalancedTreeMethods<TLinksOptions>, TLinksOptions>;
        using typename base::LinkAddressType;
        using typename base::ReadHandlerType;

        public: LinksTargetsSizeBalancedTreeMethods(std::byte* storage, std::byte* header) : base(storage, header) { }

        public: LinkAddressType* GetLeftReference(LinkAddressType node) { return &this->GetLinkReference(node).LeftAsTarget; }

        public: LinkAddressType* GetRightReference(LinkAddressType node) { return &this->GetLinkReference(node).RightAsTarget; }

        public: LinkAddressType GetLeft(LinkAddressType node) { return this->GetLinkReference(node).LeftAsTarget; }

        public: LinkAddressType GetRight(LinkAddressType node) { return this->GetLinkReference(node).RightAsTarget; }

        public: void SetLeft(LinkAddressType node, LinkAddressType left) { this->GetLinkReference(node).LeftAsTarget = left; }

        public: void SetRight(LinkAddressType node, LinkAddressType right) { this->GetLinkReference(node).RightAsTarget = right; }

        public: LinkAddressType GetSize(LinkAddressType node) { return this->GetLinkReference(node).SizeAsTarget; }

        public: void SetSize(LinkAddressType node, LinkAddressType size) { this->GetLinkReference(node).SizeAsTarget = size; }

        public: LinkAddressType GetTreeRoot() { return this->GetHeaderReference().RootAsTarget; }

        public: LinkAddressType GetBasePartValue(LinkAddressType linkAddress) { return this->GetLinkReference(linkAddress).Target; }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return (firstTarget < secondTarget) || (firstTarget == secondTarget && firstSource < secondSource); }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return (firstTarget > secondTarget) || (firstTarget == secondTarget && firstSource > secondSource); }

        public: void ClearNode(LinkAddressType node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

    public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

    public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

    public:

        LinkAddressType CountUsages(LinkAddressType root) { return base::CountUsages(root); }

        LinkAddressType Search(LinkAddressType source, LinkAddressType target) { return base::Search(source, target); }

        LinkAddressType EachUsage(LinkAddressType root, const std::function<LinkAddressType(const LinkType&)>& handler) { return base::EachUsage(root, handler); }

        void Detach(LinkAddressType& root, LinkAddressType linkIndex) { base::methods::Detach(&root, linkIndex); }

        void Attach(LinkAddressType& root, LinkAddressType linkIndex) { base::methods::Attach(&root, linkIndex); }
    };
}

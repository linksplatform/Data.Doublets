﻿namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<typename TLinksOptions>
    struct LinksSourcesAvlBalancedTreeMethods : public LinksAvlBalancedTreeMethodsBase<LinksSourcesAvlBalancedTreeMethods<TLinksOptions>, TLinksOptions>
    {
        using base = LinksAvlBalancedTreeMethodsBase<LinksSourcesAvlBalancedTreeMethods<TLinksOptions>, TLinksOptions>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::ReadHandlerType;
        using base::FirstIsToTheLeftOfSecond;
        using base::FirstIsToTheRightOfSecond;

        public: LinksSourcesAvlBalancedTreeMethods(std::byte* storage, std::byte* header) : base(storage, header) { }

        public: LinkAddressType* GetLeftReference(LinkAddressType node) { return &this->GetLinkReference(node).LeftAsSource; }

        public: LinkAddressType* GetRightReference(LinkAddressType node) { return &this->GetLinkReference(node).RightAsSource; }

        public: LinkAddressType GetLeft(LinkAddressType node) { return this->GetLinkReference(node).LeftAsSource; }

        public: LinkAddressType GetRight(LinkAddressType node) { return this->GetLinkReference(node).RightAsSource; }

        public: void SetLeft(LinkAddressType node, LinkAddressType left) { this->GetLinkReference(node).LeftAsSource = left; }

        public: void SetRight(LinkAddressType node, LinkAddressType right) { this->GetLinkReference(node).RightAsSource = right; }

        public: LinkAddressType GetSize(LinkAddressType node) { return this->GetSizeValue(this->GetLinkReference(node).SizeAsSource); }

        public: void SetSize(LinkAddressType node, LinkAddressType size) { this->SetSizeValue(&this->GetLinkReference(node).SizeAsSource, size); }

        public: bool GetLeftIsChild(LinkAddressType node) {
            return this->GetLeftIsChildValue(this->GetLinkReference(node).SizeAsSource);
        }

        public: void SetLeftIsChild(LinkAddressType node, bool value) {
            this->SetLeftIsChildValue(&this->GetLinkReference(node).SizeAsSource, value);
        }

        public: bool GetRightIsChild(LinkAddressType node) {
            return this->GetRightIsChildValue(this->GetLinkReference(node).SizeAsSource);
        }
        
        public: void SetRightIsChild(LinkAddressType node, bool value) {
            this->SetRightIsChildValue(&this->GetLinkReference(node).SizeAsSource, value);
        }

        public: int8_t GetBalance(LinkAddressType node) { 
            return this->GetBalanceValue(this->GetLinkReference(node).SizeAsSource);
        }

        public: void SetBalance(LinkAddressType node, int8_t value) {
            this->SetBalanceValue(&this->GetLinkReference(node).SizeAsSource, value);
        }

        public: LinkAddressType GetTreeRoot() { return this->GetHeaderReference().RootAsSource; }

        public: LinkAddressType GetBasePartValue(LinkAddressType linkAddress) { return this->GetLinkReference(linkAddress).Source; }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return (firstSource < secondSource) || (firstSource == secondSource && firstTarget < secondTarget); }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(LinkAddressType node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        // public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        // {
        //     auto& firstLink = this->GetLinkReference(first);
        //     auto& secondLink = this->GetLinkReference(second);
        //     return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        // }

        // public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        // {
        //     auto& firstLink = this->GetLinkReference(first);
        //     auto& secondLink = this->GetLinkReference(second);
        //     return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        // }

    public:

        LinkAddressType CountUsages(LinkAddressType root) { return base::CountUsages(root); }

        LinkAddressType Search(LinkAddressType source, LinkAddressType target) { return base::Search(source, target); }

        LinkAddressType EachUsage(LinkAddressType root, const std::function<LinkAddressType(const LinkType&)>& handler) { return base::EachUsage(root, handler); }

        void Detach(LinkAddressType& root, LinkAddressType linkIndex) { base::methods::Detach(&root, linkIndex); }

        void Attach(LinkAddressType& root, LinkAddressType linkIndex) { base::methods::Attach(&root, linkIndex); }
    };
}
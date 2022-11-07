﻿namespace Platform::Data::Doublets::Memory::United::Generic
{
template<typename TLinksOptions>
class LinksTargetsRecursionlessSizeBalancedTreeMethods<LinkAddressType> : public LinksRecursionlessSizeBalancedTreeMethodsBase<LinkAddressType>
    {
        public: LinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<LinkAddressType> constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: LinkAddressType* GetLeftReference(LinkAddressType node) { return &GetLinkReference(node)->LeftAsTarget; }

        public: LinkAddressType* GetRightReference(LinkAddressType node) { return &GetLinkReference(node)->RightAsTarget; }

        public: LinkAddressType GetLeft(LinkAddressType node) { return this->GetLinkReference(node).LeftAsTarget; }

        public: LinkAddressType GetRight(LinkAddressType node) { return this->GetLinkReference(node).RightAsTarget; }

        public: void SetLeft(LinkAddressType node, LinkAddressType left) { this->GetLinkReference(node).LeftAsTarget = left; }

        public: void SetRight(LinkAddressType node, LinkAddressType right) { this->GetLinkReference(node).RightAsTarget = right; }

        public: LinkAddressType GetSize(LinkAddressType node) { return this->GetLinkReference(node).SizeAsTarget; }

        public: void SetSize(LinkAddressType node, LinkAddressType size) { this->GetLinkReference(node).SizeAsTarget = size; }

        public: LinkAddressType GetTreeRoot() { return GetHeaderReference().RootAsTarget; }

        public: LinkAddressType GetBasePartValue(LinkAddressType link) { return this->GetLinkReference(link).Target; }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return (firstTarget < secondTarget) || (firstTarget == secondTarget && (firstSource < secondSource)); }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType firstSource, LinkAddressType firstTarget, LinkAddressType secondSource, LinkAddressType secondTarget) { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        public: void ClearNode(LinkAddressType node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }
    };
}

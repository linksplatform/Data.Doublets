﻿namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class InternalLinksSourcesRecursionlessSizeBalancedTreeMethods : public InternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinksOptions>
    {
    public:
        using LinksOptionsType = TLinksOptions;
                using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using WriteHandlerType = LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
        static constexpr auto Constants = LinksOptionsType::Constants;
        using base = InternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinksOptions>;
        public: InternalLinksSourcesRecursionlessSizeBalancedTreeMethods(std::byte* linksDataParts, std::byte* linksIndexParts, std::byte* header) : base(linksDataParts, linksIndexParts, header) { }

        protected: LinkAddressType* GetLeftReference(LinkAddressType node)  { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: LinkAddressType* GetRightReference(LinkAddressType node)  { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: LinkAddressType GetLeft(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: LinkAddressType GetRight(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(LinkAddressType node, LinkAddressType left)  { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(LinkAddressType node, LinkAddressType right)  { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: LinkAddressType GetSize(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(LinkAddressType node, LinkAddressType size)  { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        public: LinkAddressType GetTreeRoot(LinkAddressType link)  { return this->GetLinkIndexPartReference(link)->RootAsSource; }

        public: LinkAddressType GetBasePartValue(LinkAddressType link)  { return this->GetLinkDataPartReference(link)->Source; }

        protected: LinkAddressType GetKeyPartValue(LinkAddressType link)  { return this->GetLinkDataPartReference(link)->Target; }

        protected: void ClearNode(LinkAddressType node)
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)  { return this->SearchCore(this->GetTreeRoot(source), target); }
    };
}

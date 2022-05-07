namespace Platform::Data::Doublets::Memory::Split::Generic
{
    class InternalLinksSourcesSizeBalancedTreeMethods : public InternalLinksSizeBalancedTreeMethodsBase<LinkAddressType>
    {
        using base = InternalLinksSizeBalancedTreeMethodsBase<LinkAddressType>;
        public: InternalLinksSourcesSizeBalancedTreeMethods(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(linksDataParts, linksIndexParts, header) { }

        protected: LinkAddressType* GetLeftReference(LinkAddressType node)  { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: LinkAddressType* GetRightReference(LinkAddressType node)  { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: LinkAddressType GetLeft(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: LinkAddressType GetRight(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(LinkAddressType node, LinkAddressType left)  { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(LinkAddressType node, LinkAddressType right)  { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: LinkAddressType GetSize(LinkAddressType node)  { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(LinkAddressType node, LinkAddressType size)  { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected: LinkAddressType GetTreeRoot(LinkAddressType link)  { return this->GetLinkIndexPartReference(link)->RootAsSource; }

        protected: LinkAddressType GetBasePartValue(LinkAddressType link)  { return this->GetLinkDataPartReference(link)->Source; }

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

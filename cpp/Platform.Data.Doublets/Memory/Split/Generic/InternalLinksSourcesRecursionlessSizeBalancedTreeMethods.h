namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class InternalLinksSourcesRecursionlessSizeBalancedTreeMethods : public InternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinksOptions>
    {
    public:
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using Constants = LinkOptionsType.Constants;
        public: InternalLinksSourcesRecursionlessSizeBalancedTreeMethods(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(constants, linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->RightAsSource; }

        protected: TLinkAddress GetLeft(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->LeftAsSource; }

        protected: TLinkAddress GetRight(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->RightAsSource; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left)  { this->GetLinkIndexPartReference(node)->LeftAsSource = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right)  { this->GetLinkIndexPartReference(node)->RightAsSource = right; }

        protected: TLinkAddress GetSize(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->SizeAsSource; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size)  { this->GetLinkIndexPartReference(node)->SizeAsSource = size; }

        protected: TLinkAddress GetTreeRoot(TLinkAddress link)  { return this->GetLinkIndexPartReference(link)->RootAsSource; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Source; }

        protected: TLinkAddress GetKeyPartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Target; }

        protected: void ClearNode(TLinkAddress node)
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target)  { return this->SearchCore(this->GetTreeRoot(source), target); }
    };
}

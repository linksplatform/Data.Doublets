namespace Platform::Data::Doublets::Memory::Split::Generic
{
    class InternalLinksTargetsSizeBalancedTreeMethods : public InternalLinksSizeBalancedTreeMethodsBase<TLinkAddress>
    {
        public: InternalLinksTargetsSizeBalancedTreeMethods(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header) : base(linksDataParts, linksIndexParts, header) { }

        protected: TLinkAddress* GetLeftReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress* GetRightReference(TLinkAddress node)  { return &GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: TLinkAddress GetLeft(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->LeftAsTarget; }

        protected: TLinkAddress GetRight(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->RightAsTarget; }

        protected: void SetLeft(TLinkAddress node, TLinkAddress left)  { this->GetLinkIndexPartReference(node)->LeftAsTarget = left; }

        protected: void SetRight(TLinkAddress node, TLinkAddress right)  { this->GetLinkIndexPartReference(node)->RightAsTarget = right; }

        protected: TLinkAddress GetSize(TLinkAddress node)  { return this->GetLinkIndexPartReference(node)->SizeAsTarget; }

        protected: void SetSize(TLinkAddress node, TLinkAddress size)  { this->GetLinkIndexPartReference(node)->SizeAsTarget = size; }

        protected: TLinkAddress GetTreeRoot(TLinkAddress link)  { return this->GetLinkIndexPartReference(link)->RootAsTarget; }

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Target; }

        protected: TLinkAddress GetKeyPartValue(TLinkAddress link)  { return this->GetLinkDataPartReference(link)->Source; }

        protected: void ClearNode(TLinkAddress node)
        {
            auto* link = this->GetLinkIndexPartReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target)  { return this->SearchCore(this->GetTreeRoot(target), source); }
    };
}

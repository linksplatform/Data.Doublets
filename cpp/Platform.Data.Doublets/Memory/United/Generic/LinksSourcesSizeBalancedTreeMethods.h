namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<typename TLink>
    class LinksSourcesSizeBalancedTreeMethods
        : public LinksSizeBalancedTreeMethodsBase<LinksSourcesSizeBalancedTreeMethods<TLink>, TLink>
    {
        using base = LinksSizeBalancedTreeMethodsBase<LinksSourcesSizeBalancedTreeMethods<TLink>, TLink>;

        public: LinksSourcesSizeBalancedTreeMethods(LinksConstants<TLink> constants, std::byte* links, std::byte* header) : base(constants, links, header) { }

        public: TLink* GetLeftReference(TLink node) { return &this->GetLinkReference(node).LeftAsSource; }

        public: TLink* GetRightReference(TLink node) { return &this->GetLinkReference(node).RightAsSource; }

        public: TLink GetLeft(TLink node) { return this->GetLinkReference(node).LeftAsSource; }

        public: TLink GetRight(TLink node) { return this->GetLinkReference(node).RightAsSource; }

        public: void SetLeft(TLink node, TLink left) { this->GetLinkReference(node).LeftAsSource = left; }

        public: void SetRight(TLink node, TLink right) { this->GetLinkReference(node).RightAsSource = right; }

        public: TLink GetSize(TLink node) { return this->GetLinkReference(node).SizeAsSource; }

        public: void SetSize(TLink node, TLink size) { this->GetLinkReference(node).SizeAsSource = size; }

        public: TLink GetTreeRoot() { return this->GetHeaderReference().RootAsSource; }

        public: TLink GetBasePartValue(TLink link) { return this->GetLinkReference(link).Source; }

        public: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) { return (firstSource < secondSource) || (firstSource == secondSource && firstTarget < secondTarget); }

        public: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(TLink node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

        public: bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(TLink first, TLink second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

    public:

        TLink CountUsages(TLink root) { return base::CountUsages(root); }

        TLink Search(TLink source, TLink target) { return base::Search(source, target); }

        TLink EachUsage(TLink root, const std::function<TLink(const std::vector<TLink>&)>& handler) { return base::EachUsage(root, handler); }

        void Detach(TLink& root, TLink linkIndex) { base::methods::Detach(&root, linkIndex); }

        void Attach(TLink& root, TLink linkIndex) { base::methods::Attach(&root, linkIndex); }
    };
}
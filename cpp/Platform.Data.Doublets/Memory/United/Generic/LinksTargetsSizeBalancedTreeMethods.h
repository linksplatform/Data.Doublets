namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<typename TLink>
    class LinksTargetsSizeBalancedTreeMethods
        : public LinksSizeBalancedTreeMethodsBase<LinksTargetsSizeBalancedTreeMethods<TLink>, TLink>
    {
        using base = LinksSizeBalancedTreeMethodsBase<LinksTargetsSizeBalancedTreeMethods<TLink>, TLink>;

        public: LinksTargetsSizeBalancedTreeMethods(const LinksConstants<TLink>& constants, std::byte* storage, std::byte* header) : base(constants, storage, header) { }

        public: TLink* GetLeftReference(TLink node) { return &this->GetLinkReference(node).LeftAsTarget; }

        public: TLink* GetRightReference(TLink node) { return &this->GetLinkReference(node).RightAsTarget; }

        public: TLink GetLeft(TLink node) { return this->GetLinkReference(node).LeftAsTarget; }

        public: TLink GetRight(TLink node) { return this->GetLinkReference(node).RightAsTarget; }

        public: void SetLeft(TLink node, TLink left) { this->GetLinkReference(node).LeftAsTarget = left; }

        public: void SetRight(TLink node, TLink right) { this->GetLinkReference(node).RightAsTarget = right; }

        public: TLink GetSize(TLink node) { return this->GetLinkReference(node).SizeAsTarget; }

        public: void SetSize(TLink node, TLink size) { this->GetLinkReference(node).SizeAsTarget = size; }

        public: TLink GetTreeRoot() { return this->GetHeaderReference().RootAsTarget; }

        public: TLink GetBasePartValue(TLink link) { return this->GetLinkReference(link).Target; }

        public: bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) { return (firstTarget < secondTarget) || (firstTarget == secondTarget && firstSource < secondSource); }

        public: bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) { return (firstTarget > secondTarget) || (firstTarget == secondTarget && firstSource > secondSource); }

        public: void ClearNode(TLink node)
        {
            auto& link = this->GetLinkReference(node);
            link.LeftAsTarget = 0;
            link.RightAsTarget = 0;
            link.SizeAsTarget = 0;
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

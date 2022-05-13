namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64LinksSourcesAvlBalancedTreeMethods : public UInt64LinksAvlBalancedTreeMethodsBase
    {
        public: UInt64LinksSourcesAvlBalancedTreeMethods(LinksConstants<std::uint64_t> constants, RawLink<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header) : base(constants, storage, header) { }

        public: ref std::uint64_t GetLeftReference(std::uint64_t node) { return ref Links[node].LeftAsSource; }

        public: ref std::uint64_t GetRightReference(std::uint64_t node) { return ref Links[node].RightAsSource; }

        public: std::uint64_t GetLeft(std::uint64_t node) { return Links[node].LeftAsSource; }

        public: std::uint64_t GetRight(std::uint64_t node) { return Links[node].RightAsSource; }

        public: void SetLeft(std::uint64_t node, std::uint64_t left) { Links[node].LeftAsSource = left; }

        public: void SetRight(std::uint64_t node, std::uint64_t right) { Links[node].RightAsSource = right; }

        public: std::uint64_t GetSize(std::uint64_t node) { return this->GetSizeValue(Links[node].SizeAsSource); }

        public: void SetSize(std::uint64_t node, std::uint64_t size) { this->SetSizeValue(ref Links[node].SizeAsSource, size); }

        public: bool GetLeftIsChild(std::uint64_t node) { return this->GetLeftIsChildValue(Links[node].SizeAsSource); }

        public: void SetLeftIsChild(std::uint64_t node, bool value) { this->SetLeftIsChildValue(ref Links[node].SizeAsSource, value); }

        public: bool GetRightIsChild(std::uint64_t node) { return this->GetRightIsChildValue(Links[node].SizeAsSource); }

        public: void SetRightIsChild(std::uint64_t node, bool value) { this->SetRightIsChildValue(ref Links[node].SizeAsSource, value); }

        public: std::uint8_t GetBalance(std::uint64_t node) { return this->GetBalanceValue(Links[node].SizeAsSource); }

        public: void SetBalance(std::uint64_t node, std::uint8_t value) { this->SetBalanceValue(ref Links[node].SizeAsSource, value); }

        public: std::uint64_t GetTreeRoot() { return Header->RootAsSource; }

        public: std::uint64_t GetBasePartValue(std::uint64_t link) { return Links[link].Source; }

        public: bool FirstIsToTheLeftOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) { return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget); }

        public: bool FirstIsToTheRightOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(std::uint64_t node)
        {
            auto& link = Links[node];
            link.LeftAsSource = 0UL;
            link.RightAsSource = 0UL;
            link.SizeAsSource = 0UL;
        }
    };
}

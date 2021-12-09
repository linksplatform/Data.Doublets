namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64LinksSourcesAvlBalancedTreeMethods : public UInt64LinksAvlBalancedTreeMethodsBase
    {
        public: UInt64LinksSourcesAvlBalancedTreeMethods(LinksConstants<std::uint64_t> constants, RawLink<std::uint64_t>* links, LinksHeader<std::uint64_t>* header) : base(constants, links, header) { }

        protected: ref std::uint64_t GetLeftReference(std::uint64_t node) override { return ref Links[node].LeftAsSource; }

        protected: ref std::uint64_t GetRightReference(std::uint64_t node) override { return ref Links[node].RightAsSource; }

        protected: std::uint64_t GetLeft(std::uint64_t node) override { return Links[node].LeftAsSource; }

        protected: std::uint64_t GetRight(std::uint64_t node) override { return Links[node].RightAsSource; }

        protected: void SetLeft(std::uint64_t node, std::uint64_t left) override { Links[node].LeftAsSource = left; }

        protected: void SetRight(std::uint64_t node, std::uint64_t right) override { Links[node].RightAsSource = right; }

        protected: std::uint64_t GetSize(std::uint64_t node) override { return this->GetSizeValue(Links[node].SizeAsSource); }

        protected: void SetSize(std::uint64_t node, std::uint64_t size) override { this->SetSizeValue(ref Links[node].SizeAsSource, size); }

        protected: bool GetLeftIsChild(std::uint64_t node) override { return this->GetLeftIsChildValue(Links[node].SizeAsSource); }

        protected: void SetLeftIsChild(std::uint64_t node, bool value) override { this->SetLeftIsChildValue(ref Links[node].SizeAsSource, value); }

        protected: bool GetRightIsChild(std::uint64_t node) override { return this->GetRightIsChildValue(Links[node].SizeAsSource); }

        protected: void SetRightIsChild(std::uint64_t node, bool value) override { this->SetRightIsChildValue(ref Links[node].SizeAsSource, value); }

        protected: std::int8_t GetBalance(std::uint64_t node) override { return this->GetBalanceValue(Links[node].SizeAsSource); }

        protected: void SetBalance(std::uint64_t node, std::int8_t value) override { this->SetBalanceValue(ref Links[node].SizeAsSource, value); }

        protected: override std::uint64_t GetTreeRoot() { return Header->RootAsSource; }

        protected: std::uint64_t GetBasePartValue(std::uint64_t link) override { return Links[link].Source; }

        protected: bool FirstIsToTheLeftOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget); }

        protected: bool FirstIsToTheRightOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(std::uint64_t node) override
        {
            auto* link = Links[node];
            link.LeftAsSource = 0UL;
            link.RightAsSource = 0UL;
            link.SizeAsSource = 0UL;
        }
    };
}
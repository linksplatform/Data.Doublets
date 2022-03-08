namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64LinksTargetsAvlBalancedTreeMethods : public UInt64LinksAvlBalancedTreeMethodsBase
    {
        public: UInt64LinksTargetsAvlBalancedTreeMethods(LinksConstants<std::uint64_t> constants, RawLink<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header) : base(constants, storage, header) { }

        protected: ref std::uint64_t GetLeftReference(std::uint64_t node) override { return ref Links[node].LeftAsTarget; }

        protected: ref std::uint64_t GetRightReference(std::uint64_t node) override { return ref Links[node].RightAsTarget; }

        protected: std::uint64_t GetLeft(std::uint64_t node) override { return Links[node].LeftAsTarget; }

        protected: std::uint64_t GetRight(std::uint64_t node) override { return Links[node].RightAsTarget; }

        protected: void SetLeft(std::uint64_t node, std::uint64_t left) override { Links[node].LeftAsTarget = left; }

        protected: void SetRight(std::uint64_t node, std::uint64_t right) override { Links[node].RightAsTarget = right; }

        protected: std::uint64_t GetSize(std::uint64_t node) override { return this->GetSizeValue(Links[node].SizeAsTarget); }

        protected: void SetSize(std::uint64_t node, std::uint64_t size) override { this->SetSizeValue(ref Links[node].SizeAsTarget, size); }

        protected: bool GetLeftIsChild(std::uint64_t node) override { return this->GetLeftIsChildValue(Links[node].SizeAsTarget); }

        protected: void SetLeftIsChild(std::uint64_t node, bool value) override { this->SetLeftIsChildValue(ref Links[node].SizeAsTarget, value); }

        protected: bool GetRightIsChild(std::uint64_t node) override { return this->GetRightIsChildValue(Links[node].SizeAsTarget); }

        protected: void SetRightIsChild(std::uint64_t node, bool value) override { this->SetRightIsChildValue(ref Links[node].SizeAsTarget, value); }

        protected: std::int8_t GetBalance(std::uint64_t node) override { return this->GetBalanceValue(Links[node].SizeAsTarget); }

        protected: void SetBalance(std::uint64_t node, std::int8_t value) override { this->SetBalanceValue(ref Links[node].SizeAsTarget, value); }

        protected: override std::uint64_t GetTreeRoot() { return Header->RootAsTarget; }

        protected: std::uint64_t GetBasePartValue(std::uint64_t link) override { return Links[link].Target; }

        protected: bool FirstIsToTheLeftOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstTarget < secondTarget || (firstTarget == secondTarget && firstSource < secondSource); }

        protected: bool FirstIsToTheRightOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(std::uint64_t node) override
        {
            auto* link = Links[node];
            link.LeftAsTarget = 0UL;
            link.RightAsTarget = 0UL;
            link.SizeAsTarget = 0UL;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64LinksTargetsSizeBalancedTreeMethods : public UInt64LinksSizeBalancedTreeMethodsBase
    {
        public: UInt64LinksTargetsSizeBalancedTreeMethods(LinksConstants<std::uint64_t> constants, RawLink<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header) : base(constants, storage, header) { }

        public: ref std::uint64_t GetLeftReference(std::uint64_t node) override { return ref Links[node].LeftAsTarget; }

        public: ref std::uint64_t GetRightReference(std::uint64_t node) override { return ref Links[node].RightAsTarget; }

        public: std::uint64_t GetLeft(std::uint64_t node) override { return Links[node].LeftAsTarget; }

        public: std::uint64_t GetRight(std::uint64_t node) override { return Links[node].RightAsTarget; }

        public: void SetLeft(std::uint64_t node, std::uint64_t left) override { Links[node].LeftAsTarget = left; }

        public: void SetRight(std::uint64_t node, std::uint64_t right) override { Links[node].RightAsTarget = right; }

        public: std::uint64_t GetSize(std::uint64_t node) override { return Links[node].SizeAsTarget; }

        public: void SetSize(std::uint64_t node, std::uint64_t size) override { Links[node].SizeAsTarget = size; }

        public: override std::uint64_t GetTreeRoot() { return Header->RootAsTarget; }

        public: std::uint64_t GetBasePartValue(std::uint64_t link) override { return Links[link].Target; }

        public: bool FirstIsToTheLeftOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstTarget < secondTarget || (firstTarget == secondTarget && firstSource < secondSource); }

        public: bool FirstIsToTheRightOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        public: void ClearNode(std::uint64_t node) override
        {
            auto& link = Links[node];
            link.LeftAsTarget = 0UL;
            link.RightAsTarget = 0UL;
            link.SizeAsTarget = 0UL;
        }
    };
}

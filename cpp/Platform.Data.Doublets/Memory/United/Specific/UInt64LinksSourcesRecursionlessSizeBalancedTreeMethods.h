namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64LinksSourcesRecursionlessSizeBalancedTreeMethods : public UInt64LinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt64LinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<std::uint64_t> constants, RawLink<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header) : base(constants, storage, header) { }

        public: ref std::uint64_t GetLeftReference(std::uint64_t node) override { return ref Links[node].LeftAsSource; }

        public: ref std::uint64_t GetRightReference(std::uint64_t node) override { return ref Links[node].RightAsSource; }

        public: std::uint64_t GetLeft(std::uint64_t node) override { return Links[node].LeftAsSource; }

        public: std::uint64_t GetRight(std::uint64_t node) override { return Links[node].RightAsSource; }

        public: void SetLeft(std::uint64_t node, std::uint64_t left) override { Links[node].LeftAsSource = left; }

        public: void SetRight(std::uint64_t node, std::uint64_t right) override { Links[node].RightAsSource = right; }

        public: std::uint64_t GetSize(std::uint64_t node) override { return Links[node].SizeAsSource; }

        public: void SetSize(std::uint64_t node, std::uint64_t size) override { Links[node].SizeAsSource = size; }

        public: override std::uint64_t GetTreeRoot() { return Header->RootAsSource; }

        public: std::uint64_t GetBasePartValue(std::uint64_t link) override { return Links[link].Source; }

        public: bool FirstIsToTheLeftOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget); }

        public: bool FirstIsToTheRightOfSecond(std::uint64_t firstSource, std::uint64_t firstTarget, std::uint64_t secondSource, std::uint64_t secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(std::uint64_t node) override
        {
            auto& link = Links[node];
            link.LeftAsSource = 0UL;
            link.RightAsSource = 0UL;
            link.SizeAsSource = 0UL;
        }
    };
}

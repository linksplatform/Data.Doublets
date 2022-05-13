namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt32LinksSourcesRecursionlessSizeBalancedTreeMethods : public UInt32LinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32LinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<std::uint32_t> constants, RawLink<std::uint32_t>* storage, LinksHeader<std::uint32_t>* header) : base(constants, storage, header) { }

        public: ref std::uint32_t GetLeftReference(std::uint32_t node) { return ref Links[node].LeftAsSource; }

        public: ref std::uint32_t GetRightReference(std::uint32_t node) { return ref Links[node].RightAsSource; }

        public: std::uint32_t GetLeft(std::uint32_t node) { return Links[node].LeftAsSource; }

        public: std::uint32_t GetRight(std::uint32_t node) { return Links[node].RightAsSource; }

        public: void SetLeft(std::uint32_t node, std::uint32_t left) { Links[node].LeftAsSource = left; }

        public: void SetRight(std::uint32_t node, std::uint32_t right) { Links[node].RightAsSource = right; }

        public: std::uint32_t GetSize(std::uint32_t node) { return Links[node].SizeAsSource; }

        public: void SetSize(std::uint32_t node, std::uint32_t size) { Links[node].SizeAsSource = size; }

        public: std::uint32_t GetTreeRoot() { return Header->RootAsSource; }

        public: std::uint32_t GetBasePartValue(std::uint32_t link) { return Links[link].Source; }

        public: bool FirstIsToTheLeftOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) { return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget); }

        public: bool FirstIsToTheRightOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        public: void ClearNode(std::uint32_t node)
        {
            auto& link = Links[node];
            link.LeftAsSource = 0U;
            link.RightAsSource = 0U;
            link.SizeAsSource = 0U;
        }
    };
}

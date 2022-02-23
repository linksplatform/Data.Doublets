namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt32LinksSourcesRecursionlessSizeBalancedTreeMethods : public UInt32LinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32LinksSourcesRecursionlessSizeBalancedTreeMethods(LinksConstants<std::uint32_t> constants, RawLink<std::uint32_t>* storage, LinksHeader<std::uint32_t>* header) : base(constants, storage, header) { }

        protected: ref std::uint32_t GetLeftReference(std::uint32_t node) override { return ref Links[node].LeftAsSource; }

        protected: ref std::uint32_t GetRightReference(std::uint32_t node) override { return ref Links[node].RightAsSource; }

        protected: std::uint32_t GetLeft(std::uint32_t node) override { return Links[node].LeftAsSource; }

        protected: std::uint32_t GetRight(std::uint32_t node) override { return Links[node].RightAsSource; }

        protected: void SetLeft(std::uint32_t node, std::uint32_t left) override { Links[node].LeftAsSource = left; }

        protected: void SetRight(std::uint32_t node, std::uint32_t right) override { Links[node].RightAsSource = right; }

        protected: std::uint32_t GetSize(std::uint32_t node) override { return Links[node].SizeAsSource; }

        protected: void SetSize(std::uint32_t node, std::uint32_t size) override { Links[node].SizeAsSource = size; }

        protected: override std::uint32_t GetTreeRoot() { return Header->RootAsSource; }

        protected: std::uint32_t GetBasePartValue(std::uint32_t link) override { return Links[link].Source; }

        protected: bool FirstIsToTheLeftOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) override { return firstSource < secondSource || (firstSource == secondSource && firstTarget < secondTarget); }

        protected: bool FirstIsToTheRightOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) override { return firstSource > secondSource || (firstSource == secondSource && firstTarget > secondTarget); }

        protected: void ClearNode(std::uint32_t node) override
        {
            auto* link = Links[node];
            link.LeftAsSource = 0U;
            link.RightAsSource = 0U;
            link.SizeAsSource = 0U;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt32LinksTargetsRecursionlessSizeBalancedTreeMethods : public UInt32LinksRecursionlessSizeBalancedTreeMethodsBase
    {
        public: UInt32LinksTargetsRecursionlessSizeBalancedTreeMethods(LinksConstants<std::uint32_t> constants, RawLink<std::uint32_t>* storage, LinksHeader<std::uint32_t>* header) : base(constants, storage, header) { }

        protected: ref std::uint32_t GetLeftReference(std::uint32_t node) override { return ref Links[node].LeftAsTarget; }

        protected: ref std::uint32_t GetRightReference(std::uint32_t node) override { return ref Links[node].RightAsTarget; }

        protected: std::uint32_t GetLeft(std::uint32_t node) override { return Links[node].LeftAsTarget; }

        protected: std::uint32_t GetRight(std::uint32_t node) override { return Links[node].RightAsTarget; }

        protected: void SetLeft(std::uint32_t node, std::uint32_t left) override { Links[node].LeftAsTarget = left; }

        protected: void SetRight(std::uint32_t node, std::uint32_t right) override { Links[node].RightAsTarget = right; }

        protected: std::uint32_t GetSize(std::uint32_t node) override { return Links[node].SizeAsTarget; }

        protected: void SetSize(std::uint32_t node, std::uint32_t size) override { Links[node].SizeAsTarget = size; }

        protected: override std::uint32_t GetTreeRoot() { return Header->RootAsTarget; }

        protected: std::uint32_t GetBasePartValue(std::uint32_t link) override { return Links[link].Target; }

        public: bool FirstIsToTheLeftOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) override { return firstTarget < secondTarget || (firstTarget == secondTarget && firstSource < secondSource); }

        public: bool FirstIsToTheRightOfSecond(std::uint32_t firstSource, std::uint32_t firstTarget, std::uint32_t secondSource, std::uint32_t secondTarget) override { return firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource); }

        protected: void ClearNode(std::uint32_t node) override
        {
            auto& link = Links[node];
            link.LeftAsTarget = 0U;
            link.RightAsTarget = 0U;
            link.SizeAsTarget = 0U;
        }
    };
}

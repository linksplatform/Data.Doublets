namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Collections::Methods;

    template<typename Self, typename TLinkAddress>
    class LinksSizeBalancedTreeMethodsBase
        :
          //public Trees::SizeBalancedTreeMethods<Self, TLinkAddress>,
          public Trees::RecursionlessSizeBalancedTreeMethods<Self, TLinkAddress>,
          public ILinksTreeMethods<TLinkAddress>,
          public Interfaces::Polymorph<Self>
    {

        public: using methods = Trees::RecursionlessSizeBalancedTreeMethods<Self, TLinkAddress>;

        public: TLinkAddress Break = 0;
        public: TLinkAddress Continue = 0;
        public: std::byte* const Links;
        public: std::byte* const Header;

        public: LinksSizeBalancedTreeMethodsBase(const LinksConstants<TLinkAddress>& constants, std::byte* storage, std::byte* header)
            : Links(storage), Header(header), Break(constants.Break), Continue(constants.Continue) {}

        public: TLinkAddress GetTreeRoot() { return this->object().GetTreeRoot(); }

        public: TLinkAddress GetBasePartValue(TLinkAddress link) { return this->object().GetBasePartValue(link); }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) { return this->object().FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget); }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) { return this->object().FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget); }

        public: auto& GetHeaderReference() { return *reinterpret_cast<LinksHeader<TLinkAddress>*>(Header); }

        public: auto& GetLinkReference(TLinkAddress link) { return *(reinterpret_cast<RawLink<TLinkAddress>*>(Links) + link); }

        public: auto GetLinkValues(TLinkAddress linkIndex)
        {
            auto& link = GetLinkReference(linkIndex);
            return Link{linkIndex, link.Source, link.Target};
        }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        auto operator[](std::size_t index)
        {
            auto root = GetTreeRoot();
            if (index >= GetSize(root))
            {
                return 0;
            }
            while (root != 0)
            {
                auto left = GetLeftOrDefault(root);
                auto leftSize = GetSizeOrZero(left);
                if (index < leftSize)
                {
                    root = left;
                    continue;
                }
                if (index == leftSize)
                {
                    return root;
                }
                root = GetRightOrDefault(root);
                index = index - (leftSize + 1);
            }
            return 0;
        }

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target)
        {
            auto root = this->GetTreeRoot();
            while (root != 0)
            {
                auto& rootLink = this->GetLinkReference(root);
                auto rootSource = rootLink.Source;
                auto rootTarget = rootLink.Target;
                if (this->FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget))
                {
                    root = this->GetLeftOrDefault(root);
                }
                else if (this->FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget))
                {
                    root = this->GetRightOrDefault(root);
                }
                else
                {
                    return root;
                }
            }
            return 0;
        }

        public: TLinkAddress CountUsages(TLinkAddress link)
        {
            auto root = this->GetTreeRoot();
            auto total = this->GetSize(root);
            auto totalRightIgnore = 0;
            while (root != 0)
            {
                auto base = this->GetBasePartValue(root);
                if (base <= link)
                {
                    root = this->GetRightOrDefault(root);
                }
                else
                {
                    totalRightIgnore = totalRightIgnore + (this->GetRightSize(root) + 1);
                    root = this->GetLeftOrDefault(root);
                }
            }
            root = this->GetTreeRoot();
            auto totalLeftIgnore = 0;
            while (root != 0)
            {
                auto base = this->GetBasePartValue(root);
                if (base >= link)
                {
                    root = this->GetLeftOrDefault(root);
                }
                else
                {
                    totalLeftIgnore = totalLeftIgnore + (this->GetLeftSize(root) + 1);
                    root = this->GetRightOrDefault(root);
                }
            }
            return total - totalRightIgnore - totalLeftIgnore;
        }

        public: TLinkAddress EachUsage(TLinkAddress base, auto&& handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: TLinkAddress EachUsageCore(TLinkAddress base, TLinkAddress link, auto&& handler)
        {
            auto $continue = Continue;
            if (link == 0)
            {
                return $continue;
            }
            auto linkBasePart = this->GetBasePartValue(link);
            auto $break = Break;
            if (linkBasePart > (base))
            {
                if ((this->EachUsageCore(base, this->GetLeftOrDefault(link), handler) == $break))
                {
                    return $break;
                }
            }
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base, this->GetRightOrDefault(link), handler) == $break))
                {
                    return $break;
                }
            }
            else
            {
                auto values = this->GetLinkValues(link);
                if (handler(std::vector(values.begin(), values.end())) == ($break))
                {
                    return $break;
                }
                if ((this->EachUsageCore(base, this->GetLeftOrDefault(link), handler) == $break))
                {
                    return $break;
                }
                if ((this->EachUsageCore(base, this->GetRightOrDefault(link), handler) == $break))
                {
                    return $break;
                }
            }
            return $continue;
        }
    };
}

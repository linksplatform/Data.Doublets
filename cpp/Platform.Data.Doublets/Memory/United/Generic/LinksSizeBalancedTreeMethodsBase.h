namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Collections::Methods;

    template<typename Self, typename TLink>
    class LinksSizeBalancedTreeMethodsBase
        :
          //public Trees::SizeBalancedTreeMethods<Self, TLink>,
          public Trees::RecursionlessSizeBalancedTreeMethods<Self, TLink>,
          public ILinksTreeMethods<TLink>,
          public Interfaces::Polymorph<Self>
    {
        using Interfaces::Polymorph<Self>::self;

        public: using methods = Trees::RecursionlessSizeBalancedTreeMethods<Self, TLink>;

        public: TLink Break = 0;
        public: TLink Continue = 0;
        public: std::byte* const Links;
        public: std::byte* const Header;

        public: LinksSizeBalancedTreeMethodsBase(const LinksConstants<TLink>& constants, std::byte* storage, std::byte* header)
            : Links(storage), Header(header), Break(constants.Break), Continue(constants.Continue) {}

        public: TLink GetTreeRoot() { return self().GetTreeRoot(); }

        public: TLink GetBasePartValue(TLink link) { return self().GetBasePartValue(link); }

        public: bool FirstIsToTheRightOfSecond(TLink source, TLink target, TLink rootSource, TLink rootTarget) { return self().FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget); }

        public: bool FirstIsToTheLeftOfSecond(TLink source, TLink target, TLink rootSource, TLink rootTarget) { return self().FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget); }

        public: auto& GetHeaderReference() { return *reinterpret_cast<LinksHeader<TLink>*>(Header); }

        public: auto& GetLinkReference(TLink link) { return *(reinterpret_cast<RawLink<TLink>*>(Links) + link); }

        public: auto GetLinkValues(TLink linkIndex)
        {
            auto& link = GetLinkReference(linkIndex);
            return Link{linkIndex, link.Source, link.Target};
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

        public: TLink Search(TLink source, TLink target)
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

        public: TLink CountUsages(TLink link)
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

        public: TLink EachUsage(TLink base, auto&& handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: TLink EachUsageCore(TLink base, TLink link, auto&& handler)
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

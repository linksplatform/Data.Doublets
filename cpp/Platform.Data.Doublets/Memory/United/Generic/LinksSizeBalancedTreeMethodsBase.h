namespace Platform::Data::Doublets::Memory::United::Generic
{

    template<typename Self, typename TLinksOptions>
    struct LinksSizeBalancedTreeMethodsBase : public Platform::Collections::Methods::Trees::SizeBalancedTreeMethods<Self, typename TLinksOptions::LinkAddressType>
    {
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
        static constexpr LinksConstants<LinkAddressType> Constants = LinksOptionsType::Constants;


        public: using methods = Platform::Collections::Methods::Trees::SizeBalancedTreeMethods<Self, LinkAddressType>;

        public: std::byte* const Links;
        public: std::byte* const Header;

        public: LinksSizeBalancedTreeMethodsBase(std::byte* storage, std::byte* header)
            : Links(storage), Header(header) {}

        public: auto& GetHeaderReference() { return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header); }

        public: auto& GetLinkReference(LinkAddressType linkAddress) { return *(reinterpret_cast<RawLink<LinkAddressType>*>(Links) + linkAddress); }

        public: LinkType GetLinkValues(LinkAddressType linkIndex)
        {
            auto& link = GetLinkReference(linkIndex);
            return LinkType{linkIndex, link.Source, link.Target};
        }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->object().FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->object().FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        auto operator[](std::size_t index)
        {
            auto root = this->object().GetTreeRoot();
            if (index >= this->object().GetSize(root))
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

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
        {
            auto root = this->object().GetTreeRoot();
            while (root != 0)
            {
                auto& rootLink = this->GetLinkReference(root);
                auto rootSource = rootLink.Source;
                auto rootTarget = rootLink.Target;
                if (this->object().FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget))
                {
                    root = this->GetLeftOrDefault(root);
                }
                else if (this->object().FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget))
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

        public: LinkAddressType CountUsages(LinkAddressType linkAddress)
        {
            auto root = this->object().GetTreeRoot();
            auto total = this->object().GetSize(root);
            auto totalRightIgnore = 0;
            while (root != 0)
            {
                auto base = this->object().GetBasePartValue(root);
                if (base <= linkAddress)
                {
                    root = this->GetRightOrDefault(root);
                }
                else
                {
                    totalRightIgnore = totalRightIgnore + (this->GetRightSize(root) + 1);
                    root = this->GetLeftOrDefault(root);
                }
            }
            root = this->object().GetTreeRoot();
            auto totalLeftIgnore = 0;
            while (root != 0)
            {
                auto base = this->object().GetBasePartValue(root);
                if (base >= linkAddress)
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

        public: LinkAddressType EachUsage(LinkAddressType base, const ReadHandlerType& handler) { return this->EachUsageCore(base, this->object().GetTreeRoot(), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType linkAddress, const ReadHandlerType& handler)
        {
            auto $continue = Constants.Continue;
            if (linkAddress == 0)
            {
                return $continue;
            }
            auto linkBasePart = this->object().GetBasePartValue(linkAddress);
            auto $break = Constants.Break;
            if (linkBasePart > (base))
            {
                if ((this->EachUsageCore(base, this->GetLeftOrDefault(linkAddress), handler) == $break))
                {
                    return $break;
                }
            }
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base, this->GetRightOrDefault(linkAddress), handler) == $break))
                {
                    return $break;
                }
            }
            else
            {
                auto link = this->GetLinkValues(linkAddress);
                if (handler(link) == ($break))
                {
                    return $break;
                }
                if ((this->EachUsageCore(base, this->GetLeftOrDefault(linkAddress), handler) == $break))
                {
                    return $break;
                }
                if ((this->EachUsageCore(base, this->GetRightOrDefault(linkAddress), handler) == $break))
                {
                    return $break;
                }
            }
            return $continue;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
template<typename Self, typename TLinksOptions>
    struct LinksRecursionlessSizeBalancedTreeMethodsBase : public Platform::Collections::Methods::Trees::RecursionlessSizeBalancedTreeMethods<Self, typename TLinksOptions::LinkAddressType>/*, ILinksTreeMethods<LinkAddressType>*/
    {
      using LinksOptionsType = TLinksOptions;
      using LinkAddressType = typename LinksOptionsType::LinkAddressType;
      using LinkType = typename LinksOptionsType::LinkType;
      using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
      static constexpr LinksConstants<LinkAddressType> Constants = LinksOptionsType::Constants;


    public: using methods = Platform::Collections::Methods::Trees::RecursionlessSizeBalancedTreeMethods<Self, LinkAddressType>;

    public: std::byte* const Links;
    public: std::byte* const Header;
        public: static constexpr Constants = VConstants;
        public: static constexpr LinkAddressType Break = Constants.Break;
        public: static constexpr LinkAddressType Continue = Constants.Continue;
        public: std::byte* Storage;
        public: std::byte* Header;

        public: LinksRecursionlessSizeBalancedTreeMethodsBase(std::byte* storage, std::byte* header)
        {
            Storage = storage;
            Header = header;
        }

        public: LinkAddressType GetTreeRoot()
                {
                    return thls->object()->GetTreeRoot();
                };

        public: LinkAddressType GetBasePartValue(LinkAddressType link)
                {
                    return thls->object()->GetBasePartValue(link);
                };

        public: bool FirstIsToTheRightOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return thls->object()->FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
                };

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return thls->object()->FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
                };

        public:  auto& GetHeaderReference() { return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header); }

        public: auto& GetLinkReference(LinkAddressType linkAddress) { return *(reinterpret_cast<RawLink<LinkAddressType>*>(Links) + linkAddress); }

        public: Link<LinkAddressType> GetLinkValues(LinkAddressType linkIndex)
            {
                auto& link = GetLinkReference(linkIndex);
                return Link{linkIndex, link.Source, link.Target};
            }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: auto operator[](std::size_t index)
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
                if (index <= leftSize)
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

        public: LinkAddressType CountUsages(LinkAddressType link)
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
            return (total - totalRightIgnore) - totalLeftIgnore;
        }

        public: LinkAddressType EachUsage(LinkAddressType base, Func<LinkType, LinkAddressType> handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType link, Func<LinkType, LinkAddressType> handler)
        {
            auto continue = Continue;
            if (link == 0)
            {
                return continue;
            }
            auto linkBasePart = this->GetBasePartValue(link);
            auto break = Break;
            if (linkBasePart > (base))
            {
                if ((this->EachUsageCore(base,this->GetLeftOrDefault(link), handler) == Break))
                {
                    return break;
                }
            }
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base,this->GetRightOrDefault(link), handler) == Break))
                {
                    return break;
                }
            }
            else
            {
                if (handler(this->GetLinkValues(link)) == (break))
                {
                    return break;
                }
                if ((this->EachUsageCore(base,this->GetLeftOrDefault(link), handler) == Break))
                {
                    return break;
                }
                if ((this->EachUsageCore(base,this->GetRightOrDefault(link), handler) == Break))
                {
                    return break;
                }
            }
            return continue;
        }
        };
}

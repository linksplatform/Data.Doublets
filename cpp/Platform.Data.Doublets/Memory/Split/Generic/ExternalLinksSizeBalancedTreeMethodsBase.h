namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TSelf, typename TLinksOptions>
    class ExternalLinksSizeBalancedTreeMethodsBase : public Platform::Collections::Methods::Trees::SizeBalancedTreeMethods<TSelf, typename TLinksOptions::LinkAddressType> /* public ILinksTreeMethods<TLinksOptions>, */
    {
    public:
        using Polymorph<TSelf>::object;
        using LinksOptionsType = TLinksOptions;
        static constexpr auto Constants = LinksOptionsType::Constants;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using WriteHandlerType = typename LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
        static constexpr LinkAddressType Break = Constants.Break;
        static constexpr LinkAddressType Continue = Constants.Continue;
        std::byte* LinksDataParts;
        std::byte* LinksIndexParts;
        std::byte* Header;

        public: ExternalLinksSizeBalancedTreeMethodsBase(std::byte* linksDataParts, std::byte* linksIndexParts, std::byte* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
        }

        public: LinkAddressType GetTreeRoot()
        {
            return this->object().GetTreeRoot();
        };

        public: LinkAddressType GetBasePartValue(LinkAddressType link)
        {
            return this->object().GetBasePartValue(link);
        };

        public: bool FirstIsToTheRightOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
        {
            return this->object().FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
        };

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
        {
            return this->object().FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
        };

        public: auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header);
        }

        public: const RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) const
        {
            return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link)));
        }

        public: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link)
        {
            return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link)));
        }

        public: const RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link) const
        {
            return *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * (link)));
        }


        public: RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link)
        {
            return *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * (link)));
        }

        public: auto GetLinkValues(LinkAddressType linkIndex)
        {
            auto& link = GetLinkDataPartReference(linkIndex);
            return LinkType{linkIndex, link.Source, link.Target};
        }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkDataPartReference(first);
            auto& secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkDataPartReference(first);
            auto& secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: LinkAddressType operator[](LinkAddressType index)
        {
            auto root = GetTreeRoot();
            if ((index >= this->object().GetSize(root)))
            {
                return 0;
            }
            while (root != 0)
            {
                auto left = GetLeftOrDefault(root);
                auto leftSize = GetSizeOrZero(left);
                if ((index < leftSize))
                {
                    root = left;
                    continue;
                }
                if ((index == leftSize))
                {
                    return root;
                }
                root = GetRightOrDefault(root);
                index = (index - (leftSize + 1));
            }
            return 0;

        }

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
        {
            auto root = this->GetTreeRoot();
            while (root != 0)
            {
                auto& rootLink = this->GetLinkDataPartReference(root);
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
            auto total = this->object().GetSize(root);
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

        public: LinkAddressType EachUsage(LinkAddressType base, const ReadHandlerType& handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType link, const ReadHandlerType& handler)
        {
            if (link == 0)
            {
                return Continue;
            }
            auto linkBasePart = this->GetBasePartValue(link);
            if (linkBasePart > (base))
            {
                if ((this->EachUsageCore(base, this->GetLeftOrDefault(link), handler) == Break))
                {
                    return Break;
                }
            }
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base, this->GetRightOrDefault(link), handler) == Break))
                {
                    return Break;
                }
            }
            else
            {
                if (handler(this->GetLinkValues(link)) == (Break))
                {
                    return Break;
                }
                if ((this->EachUsageCore(base,this->GetLeftOrDefault(link), handler) == Break))
                {
                    return Break;
                }
                if ((this->EachUsageCore(base,this->GetRightOrDefault(link), handler) == Break))
                {
                    return Break;
                }
            }
            return Continue;
        }
        };
}

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class ExternalLinksSizeBalancedTreeMethodsBase : public SizeBalancedTreeMethods<ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinksOptions>, typename TLinksOptions::LinkAddressType>, ILinksTreeMethods<TLinksOptions>
    {
    public:
        using LinksOptionsType = TLinksOptions;
        static constexpr auto Constants = LinksOptionsType::Constants;
                using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using WriteHandlerType = LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
        protected: static constexpr LinkAddressType Break = Constants.Break;
        protected: static constexpr LinkAddressType Continue = Constants.Continue;
        protected: std::uint8_t* LinksDataParts;
        protected: std::uint8_t* LinksIndexParts;
        protected: std::uint8_t* Header;

        protected: ExternalLinksSizeBalancedTreeMethodsBase(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
            Break = Constants.Break;
            Continue = Constants.Continue;
        }

        protected: LinkAddressType GetTreeRoot()
                {
                    return this->object()->GetTreeRoot();
                };

        protected: LinkAddressType GetBasePartValue(LinkAddressType link)
                {
                    return this->object()->GetBasePartValue(link);
                };

        protected: bool FirstIsToTheRightOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return this->object()->FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
                };

        protected: bool FirstIsToTheLeftOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return this->object()->FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
                };

        protected:         auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header);
        }

        protected: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) { return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link))); }

        protected: RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link) { *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * (link))); }

        protected: auto GetLinkValues(LinkAddressType linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return LinkType(linkIndex, link.Source, link.Target);
        }

        protected: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto* firstLink = this->GetLinkDataPartReference(first);
            auto* secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        protected: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto* firstLink = this->GetLinkDataPartReference(first);
            auto* secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: LinkAddressType operator[](LinkAddressType index)
        {
            auto root = GetTreeRoot();
            if (GreaterOrEqualThan(index, GetSize(root)))
            {
                return 0;
            }
            while (!EqualToZero(root))
            {
                auto left = GetLeftOrDefault(root);
                auto leftSize = GetSizeOrZero(left);
                if (LessThan(index, leftSize))
                {
                    root = left;
                    continue;
                }
                if (AreEqual(index, leftSize))
                {
                    return root;
                }
                root = GetRightOrDefault(root);
                index = Subtract(index, Increment(leftSize));
            }
            return 0;

        }

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
        {
            auto root = this->GetTreeRoot();
            while (root != 0)
            {
                auto* rootLink = this->GetLinkDataPartReference(root);
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
                if (this->base >= link)
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

        public: LinkAddressType EachUsage(LinkAddressType base, auto&& handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType link, auto&& handler)
        {
            if (link == 0)
            {
                return Continue;
            }
            auto linkBasePart = this->GetBasePartValue(link);
            if (linkBasePart > (base))
            {
                if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), Break))
                {
                    return Break;
                }
            }
            else if (this->LessThan(linkBasePart, base))
            {
                if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), Break))
                {
                    return Break;
                }
            }
            else
            {
                if (this->handler(this->GetLinkValues(link)) == (Break))
                {
                    return Break;
                }
                if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), Break))
                {
                    return Break;
                }
                if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), Break))
                {
                    return Break;
                }
            }
            return Continue;
        }
        };
}

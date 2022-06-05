namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TSelf, typename TLinksOptions>
    class InternalLinksSizeBalancedTreeMethodsBase : public SizeBalancedTreeMethods<TSelf, typename TLinksOptions::LinkAddressType> /* public ILinksTreeMethods<TLinksOptions>, */
    {
        using LinksOptionsType = TLinksOptions;
    public: static constexpr auto Constants = LinksOptionsType::Constants;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using WriteHandlerType = typename LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
        public: static constexpr LinkAddressType Break = Constants.Break;
        public: static constexpr LinkAddressType Continue = Constants.Continue;
        public: std::byte* LinksDataParts;
        public: std::byte* LinksIndexParts;
        public: std::byte* Header;

        public: InternalLinksSizeBalancedTreeMethodsBase(std::byte* linksDataParts, std::byte* linksIndexParts, std::byte* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
        }

        public: LinkAddressType GetTreeRoot(LinkAddressType link)
                {
                    return this->object().GetTreeRoot(link);
                };

        public: LinkAddressType GetBasePartValue(LinkAddressType link)
                {
                    return this->object().GetBasePartValue(link);
                };

        public: LinkAddressType GetKeyPartValue(LinkAddressType link)
                {
                    return this->object().GetKeyPartValue(link);
                };

            public: const RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) const
                {
                    return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * link));
                }

            public: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link)
                {
                    return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * link));
                }

            public: const RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link) const
                {
                    return *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * link));
                }

            public: RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link)
                {
                    return *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * link));
                }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)  { return this->GetKeyPartValue(first) < this->GetKeyPartValue(second); }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)  { return this->GetKeyPartValue(first) > this->GetKeyPartValue(second); }

        public: auto GetLinkValues(LinkAddressType linkIndex)
        {
            auto& link = GetLinkDataPartReference(linkIndex);
            return LinkType{linkIndex, link.Source, link.Target};
        }

//        public: LinkAddressType operator[](LinkAddressType link, LinkAddressType index)
//        {
//            auto root = GetTreeRoot(*link);
//            if (index >= GetSize(root))
//            {
//                return 0;
//            }
//            while (root != 0)
//            {
//                auto left = GetLeftOrDefault(root);
//                auto leftSize = GetSizeOrZero(left);
//                if (index < leftSize)
//                {
//                    root = left;
//                    continue;
//                }
//                if (index == leftSize)
//                {
//                    return root;
//                }
//                root = GetRightOrDefault(root);
//                index = index - (leftbSize + 1);
//            }
//            return 0;
//        }

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
                         {
                             return this->object().Search(source, target);
                         };

        public: LinkAddressType SearchCore(LinkAddressType root, LinkAddressType key)
        {
            while (root != 0)
            {
                auto rootKey = this->GetKeyPartValue(root);
                if (key < rootKey)
                {
                    root = this->GetLeftOrDefault(root);
                }
                else if (key > rootKey)
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

        public: LinkAddressType CountUsages(LinkAddressType link) { return this->GetSizeOrZero(this->GetTreeRoot(link)); }

        public: LinkAddressType EachUsage(LinkAddressType base, const ReadHandlerType& handler) { return this->EachUsageCore(base, this->GetTreeRoot(base), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType link, const ReadHandlerType& handler)
        {
            if (link == 0)
            {
                return Continue;
            }
            if ((this->EachUsageCore(base,this->GetLeftOrDefault(link), handler) == Break))
            {
                return Break;
            }
            if (handler(this->GetLinkValues(link)) == (Break))
            {
                return Break;
            }
            if ((this->EachUsageCore(base,this->GetRightOrDefault(link), handler) == Break))
            {
                return Break;
            }
            return Continue;
        }
        };
}

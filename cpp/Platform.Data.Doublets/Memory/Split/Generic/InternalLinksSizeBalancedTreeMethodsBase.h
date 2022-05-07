namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class InternalLinksSizeBalancedTreeMethodsBase : public SizeBalancedTreeMethods<InternalLinksSizeBalancedTreeMethodsBase<typename TLinksOptions::LinkAddressType, TLinksOptions::Constants>, LinkAddressType>, ILinksTreeMethods<TLinksOptions>
    {
        using LinksOptionsType = TLinksOptions;
    public: static constexpr auto Constants = LinksOptionsType::Constants;
        protected: static constexpr LinkAddressType Break = Constants.Break;
        protected: static constexpr LinkAddressType Continue = Constants.Continue;
        protected: std::uint8_t* LinksDataParts;
        protected: std::uint8_t* LinksIndexParts;
        protected: std::uint8_t* Header;

        protected: InternalLinksSizeBalancedTreeMethodsBase(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
        }

        protected: LinkAddressType GetTreeRoot(LinkAddressType link)
                {
                    return this->object()->GetTreeRoot(link);
                };

        protected: LinkAddressType GetBasePartValue(LinkAddressType link)
                {
                    return this->object()->GetBasePartValue(link);
                };

        protected: LinkAddressType GetKeyPartValue(LinkAddressType link)
                {
                    return this->object()->GetKeyPartValue(link);
                };

            protected: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) { return RawLinkDataPart<LinkAddressType>(LinksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * link)); }

            protected: RawLinkIndexPart<LinkAddressType>& GetLinkIndexPartReference(LinkAddressType link) { return RawLinkIndexPart<LinkAddressType>(LinksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * link)); }

        protected: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)  { return this->GetKeyPartValue(first) < this->GetKeyPartValue(second); }

        protected: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)  { return this->GetKeyPartValue(first) > this->GetKeyPartValue(second); }

        protected: auto GetLinkValues(LinkAddressType linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return LinkType(linkIndex, link.Source, link.Target);
        }

        public: LinkAddressType operator[](LinkAddressType link, LinkAddressType index)
        {
            auto root = GetTreeRoot(*link);
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
                index = index - (leftbSize + 1);
            }
            return 0;
        }

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
                         {
                             return this->object()->Search(source, target);
                         };

        protected: LinkAddressType SearchCore(LinkAddressType root, LinkAddressType key)
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

        public: LinkAddressType EachUsage(LinkAddressType base, ReadHandlerType handler) { return this->EachUsageCore(base, this->GetTreeRoot(base), handler); }

        private: LinkAddressType EachUsageCore(LinkAddressType base, LinkAddressType link, ReadHandlerType handler)
        {
            if (link == 0)
            {
                return Continue;
            }
            if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), Break))
            {
                return Break;
            }
            if (this->handler(this->GetLinkValues(link)) == (Break))
            {
                return Break;
            }
            if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), Break))
            {
                return Break;
            }
            return Continue;
        }
        };
}

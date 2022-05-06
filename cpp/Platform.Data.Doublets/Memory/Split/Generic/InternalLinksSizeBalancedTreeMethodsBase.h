namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinkOptions>
    class InternalLinksSizeBalancedTreeMethodsBase : public SizeBalancedTreeMethods<InternalLinksSizeBalancedTreeMethodsBase<typename TLinksOptions::LinkAddressType, TLinksOptions::Constants>, TLinkAddress>, ILinksTreeMethods<TLinksOptions>
    {
        using OptionsType = TLinkOptions;
    public: static constexpr Constants = OptionsType::Constants;
        protected: static constexpr TLinkAddress Break = Constants.Break;
        protected: static constexpr TLinkAddress Continue = Constants.Continue;
        protected: std::uint8_t* LinksDataParts;
        protected: std::uint8_t* LinksIndexParts;
        protected: std::uint8_t* Header;

        protected: InternalLinksSizeBalancedTreeMethodsBase(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
        }

        protected: TLinkAddress GetTreeRoot(TLinkAddress link)
                {
                    return this->object()->GetTreeRoot(link);
                };

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)
                {
                    return this->object()->GetBasePartValue(link);
                };

        protected: TLinkAddress GetKeyPartValue(TLinkAddress link)
                {
                    return this->object()->GetKeyPartValue(link);
                };

            protected: RawLinkDataPart<TLinkAddress>& GetLinkDataPartReference(TLinkAddress link) { return RawLinkDataPart<TLinkAddress>(LinksDataParts + (RawLinkDataPart<TLinkAddress>::SizeInBytes * link)); }

            protected: RawLinkIndexPart<TLinkAddress>& GetLinkIndexPartReference(TLinkAddress link) { return RawLinkIndexPart<TLinkAddress>(LinksIndexParts + (RawLinkIndexPart<TLinkAddress>::SizeInBytes * link)); }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second)  { return this->GetKeyPartValue(first) < this->GetKeyPartValue(second); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second)  { return this->GetKeyPartValue(first) > this->GetKeyPartValue(second); }

        protected: auto GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return LinkType(linkIndex, link.Source, link.Target);
        }

        public: TLinkAddress this[TLinkAddress link, TLinkAddress index]
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

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target)
                         {
                             return this->object()->Search(source, target);
                         };

        protected: TLinkAddress SearchCore(TLinkAddress root, TLinkAddress key)
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

        public: TLinkAddress CountUsages(TLinkAddress link) { return this->GetSizeOrZero(this->GetTreeRoot(link)); }

        public: TLinkAddress EachUsage(TLinkAddress base, auto&& handler) { return this->EachUsageCore(base, this->GetTreeRoot(base), handler); }

        private: TLinkAddress EachUsageCore(TLinkAddress base, TLinkAddress link, auto&& handler)
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

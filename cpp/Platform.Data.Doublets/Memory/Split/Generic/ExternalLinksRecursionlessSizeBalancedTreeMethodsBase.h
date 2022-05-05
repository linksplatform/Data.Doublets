namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Collections::Methods::Trees;
    template<typename TLinkAddress, LinksConstants<TLinkAddress> VConstants>
    class ExternalLinksRecursionlessSizeBalancedTreeMethodsBase : public RecursionlessSizeBalancedTreeMethods<ExternalLinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress, VConstants>, TLinkAddress>, ILinksTreeMethods<TLinkAddress>
    {
        public: static constexpr TLinkAddress Constants = VConstants;
        protected: static constexpr TLinkAddress Break = Constants.Break;
        protected: static constexpr TLinkAddress Continue = Constants.Continue;
        protected: std::uint8_t* LinksDataParts;
        protected: std::uint8_t* LinksIndexParts;
        protected: std::uint8_t* Header;

        protected: ExternalLinksRecursionlessSizeBalancedTreeMethodsBase(std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
        }

        protected: TLinkAddress GetTreeRoot()
                {
                    return this->object()->GetTreeRoot();
                };

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)
                {
                    return this->object()->GetBasePartValue(link);
                };

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget)
                {
                    return this->object()->FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
                };

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget)
                {
                    return this->object()->FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
                };

        protected: auto& GetHeaderReference()
            {
                return *reinterpret_cast<LinksHeader<TLinkAddress>*>(Header);
            }

        protected: RawLinkDataPart<TLinkAddress>& GetLinkDataPartReference(TLinkAddress link) { return RawLinkDataPart<TLinkAddress>(LinksDataParts + (RawLinkDataPart<TLinkAddress>::SizeInBytes * link)); }

        protected: RawLinkIndexPart<TLinkAddress>& GetLinkIndexPartReference(TLinkAddress link) { return RawLinkIndexPart<TLinkAddress>(LinksIndexParts + (RawLinkIndexPart<TLinkAddress>::SizeInBytes * link)); }

        protected: auto GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return LinkType(linkIndex, link.Source, link.Target);
        }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second)
        {
            auto* firstLink = this->GetLinkDataPartReference(first);
            auto* secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second)
        {
            auto* firstLink = this->GetLinkDataPartReference(first);
            auto* secondLink = this->GetLinkDataPartReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: TLinkAddress operator[](TLinkAddress index)
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
                    if (AreEqual(index, leftSize))
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
            return (total - totalRightIgnore) - totalLeftIgnore;
        }

        public: TLinkAddress EachUsage(TLinkAddress base, Func<IList<TLinkAddress>, TLinkAddress> handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: TLinkAddress EachUsageCore(TLinkAddress base, TLinkAddress link, auto&& handler)
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
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), Break))
                {
                    return Break;
                }
            }
            else
            {
                if (this->handler(this->GetLinkValues(link)) == Break)
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

namespace Platform::Data::Doublets::Memory::United::Generic
{
    template<typename TLinkAddress, LinksConstants<TLinkAddress> VConstants>
    class LinksRecursionlessSizeBalancedTreeMethodsBase : public RecursionlessSizeBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress>
    {
        public: static constexpr Constants = VConstants;
        protected: static constexpr TLinkAddress Break = Constants.Break;
        protected: static constexpr TLinkAddress Continue = Constants.Continue;
        protected: std::byte* Storage;
        protected: std::byte* Header;

        protected: LinksRecursionlessSizeBalancedTreeMethodsBase(std::byte* storage, std::byte* header)
        {
            Storage = storage;
            Header = header;
        }

        protected: TLinkAddress GetTreeRoot()
                {
                    return thls->object()->GetTreeRoot();
                };

        protected: TLinkAddress GetBasePartValue(TLinkAddress link)
                {
                    return thls->object()->GetBasePartValue(link);
                };

        public: bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget)
                {
                    return thls->object()->FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
                };

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget)
                {
                    return thls->object()->FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
                };

        protected:  auto& GetHeaderReference() { return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header); }

        protected: auto& GetLinkReference(LinkAddressType linkAddress) { return *(reinterpret_cast<RawLink<LinkAddressType>*>(Links) + linkAddress); }

        protected: Link<LinkAddressType> GetLinkValues(LinkAddressType linkIndex)
            {
                auto& link = GetLinkReference(linkIndex);
                return Link{linkIndex, link.Source, link.Target};
            }

        public: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: TLinkAddress this[TLinkAddress index]
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

        public: TLinkAddress Search(TLinkAddress source, TLinkAddress target)
        {
            auto root = this->GetTreeRoot();
            while (root != 0)
            {
                auto* rootLink = this->GetLinkReference(root);
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
                if (this->LessOrEqualThan(base, link))
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

        private: TLinkAddress EachUsageCore(TLinkAddress base, TLinkAddress link, Func<IList<TLinkAddress>, TLinkAddress> handler)
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
                if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), break))
                {
                    return break;
                }
            }
            else if (linkBasePart < base)
            {
                if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), break))
                {
                    return break;
                }
            }
            else
            {
                if (this->handler(this->GetLinkValues(link)) == (break))
                {
                    return break;
                }
                if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), break))
                {
                    return break;
                }
                if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), break))
                {
                    return break;
                }
            }
            return continue;
        }
        };
}

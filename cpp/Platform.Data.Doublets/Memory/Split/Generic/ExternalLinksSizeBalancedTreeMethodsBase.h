namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public class ExternalLinksSizeBalancedTreeMethodsBase : public SizeBalancedTreeMethods, ILinksTreeMethods
    {
    public:
        protected: TLinkAddress Break = Constants.Break;
        protected: TLinkAddress Continue = Constants.Continue;
        protected: readonly std::uint8_t* LinksDataParts;
        protected: readonly std::uint8_t* LinksIndexParts;
        protected: readonly std::uint8_t* Header;

        protected: ExternalLinksSizeBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual TLinkAddress GetTreeRoot() = 0;

        protected: virtual TLinkAddress GetBasePartValue(TLinkAddress link) = 0;

        protected: virtual bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) = 0;

        protected: virtual bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) = 0;

        protected: virtual ref LinksHeader<TLinkAddress> GetHeaderReference() { return ref AsRef<LinksHeader<TLinkAddress>>(Header); }

        protected: virtual ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return ref AsRef<RawLinkDataPart<TLinkAddress>>(LinksDataParts + (RawLinkDataPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link) { return ref AsRef<RawLinkIndexPart<TLinkAddress>>(LinksIndexParts + (RawLinkIndexPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual IList<TLinkAddress> GetLinkValues(TLinkAddress linkIndex)
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

        public: TLinkAddress this[TLinkAddress index]
        {
            get
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
                if (this->GreaterOrEqualThan(base, link))
                {
                    root = this->GetLeftOrDefault(root);
                }
                else
                {
                    totalLeftIgnore = totalLeftIgnore + (this->GetLeftSize(root) + 1);
                    root = this->GetRightOrDefault(root);
                }
            }
            return this->Subtract(this->Subtract(total, totalRightIgnore), totalLeftIgnore);
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
            else if (this->LessThan(linkBasePart, base))
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

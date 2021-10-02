

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksRecursionlessSizeBalancedTreeMethodsBase<TLink> : public RecursionlessSizeBalancedTreeMethods<TLink>, ILinksTreeMethods<TLink>
    {
        private: static readonly UncheckedConverter<TLink, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLink, std::int64_t>.Default;

        protected: TLink Break = 0;
        protected: TLink Continue = 0;
        protected: readonly std::uint8_t* Links;
        protected: readonly std::uint8_t* Header;

        protected: LinksRecursionlessSizeBalancedTreeMethodsBase(LinksConstants<TLink> constants, std::uint8_t* links, std::uint8_t* header)
        {
            Links = links;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual TLink GetTreeRoot() = 0;

        protected: virtual TLink GetBasePartValue(TLink link) = 0;

        protected: virtual bool FirstIsToTheRightOfSecond(TLink source, TLink target, TLink rootSource, TLink rootTarget) = 0;

        protected: virtual bool FirstIsToTheLeftOfSecond(TLink source, TLink target, TLink rootSource, TLink rootTarget) = 0;

        protected: virtual ref LinksHeader<TLink> GetHeaderReference() { return ref AsRef<LinksHeader<TLink>>(Header); }

        protected: virtual ref RawLink<TLink> GetLinkReference(TLink link) { return ref AsRef<RawLink<TLink>>(Links + (RawLink<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual IList<TLink> GetLinkValues(TLink linkIndex)
        {
            auto* link = GetLinkReference(linkIndex);
            return Link<TLink>(linkIndex, link.Source, link.Target);
        }

        protected: bool FirstIsToTheLeftOfSecond(TLink first, TLink second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        protected: bool FirstIsToTheRightOfSecond(TLink first, TLink second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: TLink this[TLink index]
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

        public: TLink Search(TLink source, TLink target)
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

        public: TLink CountUsages(TLink link)
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

        public: TLink EachUsage(TLink base, Func<IList<TLink>, TLink> handler) { return this->EachUsageCore(base, this->GetTreeRoot(), handler); }

        private: TLink EachUsageCore(TLink base, TLink link, Func<IList<TLink>, TLink> handler)
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
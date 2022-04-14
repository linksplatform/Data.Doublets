

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksRecursionlessSizeBalancedTreeMethodsBase<TLinkAddress> : public RecursionlessSizeBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress>
    {
        private: static readonly UncheckedConverter<TLinkAddress, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLinkAddress, std::int64_t>.Default;

        protected: TLinkAddress Break = 0;
        protected: TLinkAddress Continue = 0;
        protected: readonly std::uint8_t* Links;
        protected: readonly std::uint8_t* Header;

        protected: LinksRecursionlessSizeBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, std::uint8_t* storage, std::uint8_t* header)
        {
            Links = storage;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual TLinkAddress GetTreeRoot() = 0;

        protected: virtual TLinkAddress GetBasePartValue(TLinkAddress link) = 0;

        protected: virtual bool FirstIsToTheRightOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) = 0;

        protected: virtual bool FirstIsToTheLeftOfSecond(TLinkAddress source, TLinkAddress target, TLinkAddress rootSource, TLinkAddress rootTarget) = 0;

        protected: virtual ref LinksHeader<TLinkAddress> GetHeaderReference() { return ref AsRef<LinksHeader<TLinkAddress>>(Header); }

        protected: virtual ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress link) { return ref AsRef<RawLink<TLinkAddress>>(Links + (RawLink<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual IList<TLinkAddress> GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkReference(linkIndex);
            return Link<TLinkAddress>(linkIndex, link.Source, link.Target);
        }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second) override
        {
            auto* firstLink = this->GetLinkReference(first);
            auto* secondLink = this->GetLinkReference(second);
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

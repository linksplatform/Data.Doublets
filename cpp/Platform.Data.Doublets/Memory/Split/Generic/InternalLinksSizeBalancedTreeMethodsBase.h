

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksSizeBalancedTreeMethodsBase<TLinkAddress> : public SizeBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress>
    {
        private: static readonly UncheckedConverter<TLinkAddress, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLinkAddress, std::int64_t>.Default;

        protected: TLinkAddress Break = 0;
        protected: TLinkAddress Continue = 0;
        protected: readonly std::uint8_t* LinksDataParts;
        protected: readonly std::uint8_t* LinksIndexParts;
        protected: readonly std::uint8_t* Header;

        protected: InternalLinksSizeBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual TLinkAddress GetTreeRoot(TLinkAddress link) = 0;

        protected: virtual TLinkAddress GetBasePartValue(TLinkAddress link) = 0;

        protected: virtual TLinkAddress GetKeyPartValue(TLinkAddress link) = 0;

        protected: virtual ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return ref AsRef<RawLinkDataPart<TLinkAddress>>(LinksDataParts + (RawLinkDataPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link) { return ref AsRef<RawLinkIndexPart<TLinkAddress>>(LinksIndexParts + (RawLinkIndexPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: bool FirstIsToTheLeftOfSecond(TLinkAddress first, TLinkAddress second) override { return this->LessThan(this->GetKeyPartValue(first), this->GetKeyPartValue(second)); }

        protected: bool FirstIsToTheRightOfSecond(TLinkAddress first, TLinkAddress second) override { return this->GetKeyPartValue(first) > this->GetKeyPartValue(second); }

        protected: virtual IList<TLinkAddress> GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link<TLinkAddress>(linkIndex, link.Source, link.Target);
        }

        public: TLinkAddress this[TLinkAddress link, TLinkAddress index]
        {
            get
            {
                auto root = GetTreeRoot(*link);
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

        public: virtual TLinkAddress Search(TLinkAddress source, TLinkAddress target) = 0;

        protected: TLinkAddress SearchCore(TLinkAddress root, TLinkAddress key)
        {
            while (root != 0)
            {
                auto rootKey = this->GetKeyPartValue(root);
                if (this->LessThan(key, rootKey))
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

        public: TLinkAddress EachUsage(TLinkAddress base, Func<IList<TLinkAddress>, TLinkAddress> handler) { return this->EachUsageCore(base, this->GetTreeRoot(base), handler); }

        private: TLinkAddress EachUsageCore(TLinkAddress base, TLinkAddress link, Func<IList<TLinkAddress>, TLinkAddress> handler)
        {
            auto continue = Continue;
            if (link == 0)
            {
                return continue;
            }
            auto break = Break;
            if ((this->EachUsageCore(base) == (this->GetLeftOrDefault(link), handler), break))
            {
                return break;
            }
            if (this->handler(this->GetLinkValues(link)) == (break))
            {
                return break;
            }
            if ((this->EachUsageCore(base) == (this->GetRightOrDefault(link), handler), break))
            {
                return break;
            }
            return continue;
        }
        };
}

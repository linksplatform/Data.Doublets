

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksSizeBalancedTreeMethodsBase<TLink> : public SizeBalancedTreeMethods<TLink>, ILinksTreeMethods<TLink>
    {
        private: static readonly UncheckedConverter<TLink, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLink, std::int64_t>.Default;

        protected: TLink Break = 0;
        protected: TLink Continue = 0;
        protected: readonly std::uint8_t* LinksDataParts;
        protected: readonly std::uint8_t* LinksIndexParts;
        protected: readonly std::uint8_t* Header;

        protected: InternalLinksSizeBalancedTreeMethodsBase(LinksConstants<TLink> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts, std::uint8_t* header)
        {
            LinksDataParts = linksDataParts;
            LinksIndexParts = linksIndexParts;
            Header = header;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual TLink GetTreeRoot(TLink link) = 0;

        protected: virtual TLink GetBasePartValue(TLink link) = 0;

        protected: virtual TLink GetKeyPartValue(TLink link) = 0;

        protected: virtual ref RawLinkDataPart<TLink> GetLinkDataPartReference(TLink link) { return ref AsRef<RawLinkDataPart<TLink>>(LinksDataParts + (RawLinkDataPart<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual ref RawLinkIndexPart<TLink> GetLinkIndexPartReference(TLink link) { return ref AsRef<RawLinkIndexPart<TLink>>(LinksIndexParts + (RawLinkIndexPart<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: bool FirstIsToTheLeftOfSecond(TLink first, TLink second) override { return this->LessThan(this->GetKeyPartValue(first), this->GetKeyPartValue(second)); }

        protected: bool FirstIsToTheRightOfSecond(TLink first, TLink second) override { return this->GetKeyPartValue(first) > this->GetKeyPartValue(second); }

        protected: virtual IList<TLink> GetLinkValues(TLink linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link<TLink>(linkIndex, link.Source, link.Target);
        }

        public: TLink this[TLink link, TLink index]
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

        public: virtual TLink Search(TLink source, TLink target) = 0;

        protected: TLink SearchCore(TLink root, TLink key)
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

        public: TLink CountUsages(TLink link) { return this->GetSizeOrZero(this->GetTreeRoot(link)); }

        public: TLink EachUsage(TLink base, Func<IList<TLink>, TLink> handler) { return this->EachUsageCore(base, this->GetTreeRoot(base), handler); }

        private: TLink EachUsageCore(TLink base, TLink link, Func<IList<TLink>, TLink> handler)
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
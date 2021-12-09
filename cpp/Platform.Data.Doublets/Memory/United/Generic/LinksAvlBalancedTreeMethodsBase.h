

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksAvlBalancedTreeMethodsBase<TLink> : public SizedAndThreadedAVLBalancedTreeMethods<TLink>, ILinksTreeMethods<TLink>
    {
        private: static readonly UncheckedConverter<TLink, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLink, std::int64_t>.Default;
        private: static readonly UncheckedConverter<TLink, std::int32_t> _addressToInt32Converter = UncheckedConverter<TLink, std::int32_t>.Default;
        private: static readonly UncheckedConverter<bool, TLink> _boolToAddressConverter = UncheckedConverter<bool, TLink>.Default;
        private: static readonly UncheckedConverter<TLink, bool> _addressToBoolConverter = UncheckedConverter<TLink, bool>.Default;
        private: static readonly UncheckedConverter<std::int32_t, TLink> _int32ToAddressConverter = UncheckedConverter<std::int32_t, TLink>.Default;

        protected: TLink Break = 0;
        protected: TLink Continue = 0;
        protected: readonly std::uint8_t* Links;
        protected: readonly std::uint8_t* Header;

        protected: LinksAvlBalancedTreeMethodsBase(LinksConstants<TLink> constants, std::uint8_t* links, std::uint8_t* header)
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

        protected: virtual TLink GetSizeValue(TLink value) { return Bit<TLink>.PartialRead(value, 5, -5); }

        protected: virtual void SetSizeValue(TLink* storedValue, TLink size) { return storedValue = Bit<TLink>.PartialWrite(storedValue, size, 5, -5); }

        protected: virtual bool GetLeftIsChildValue(TLink value)
        {
            unchecked
            {
                return _addressToBoolConverter.Convert(Bit<TLink>.PartialRead(value, 4, 1));
            }
        }

        protected: virtual void SetLeftIsChildValue(TLink* storedValue, bool value)
        {
            unchecked
            {
                auto previousValue = *storedValue;
                auto modified = Bit<TLink>.PartialWrite(previousValue, _boolToAddressConverter.Convert(value), 4, 1);
                *storedValue = modified;
            }
        }

        protected: virtual bool GetRightIsChildValue(TLink value)
        {
            unchecked
            {
                return _addressToBoolConverter.Convert(Bit<TLink>.PartialRead(value, 3, 1));
            }
        }

        protected: virtual void SetRightIsChildValue(TLink* storedValue, bool value)
        {
            unchecked
            {
                auto previousValue = *storedValue;
                auto modified = Bit<TLink>.PartialWrite(previousValue, _boolToAddressConverter.Convert(value), 3, 1);
                *storedValue = modified;
            }
        }

        protected: bool IsChild(TLink parent, TLink possibleChild)
        {
            auto parentSize = this->GetSize(parent);
            auto childSize = this->GetSizeOrZero(possibleChild);
            return childSize > 0 && this->LessOrEqualThan(childSize, parentSize);
        }

        protected: virtual std::int8_t GetBalanceValue(TLink storedValue)
        {
            unchecked
            {
                auto value = _addressToInt32Converter.Convert(Bit<TLink>.PartialRead(storedValue, 0, 3));
                value |= 0xF8 * ((value & 4) >> 2);
                return (std::int8_t)value;
            }
        }

        protected: virtual void SetBalanceValue(TLink* storedValue, std::int8_t value)
        {
            unchecked
            {
                auto packagedValue = _int32ToAddressConverter.Convert((std::uint8_t)value >> 5 & 4 | value & 3);
                auto modified = Bit<TLink>.PartialWrite(*storedValue, packagedValue, 0, 3);
                *storedValue = modified;
            }
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

        public: TLink EachUsage(TLink link, Func<IList<TLink>, TLink> handler)
        {
            auto root = this->GetTreeRoot();
            if (root == 0)
            {
                return Continue;
            }
            TLink first = 0, current = root;
            while (current != 0)
            {
                auto base = this->GetBasePartValue(current);
                if (this->GreaterOrEqualThan(base, link))
                {
                    if ((base) == link)
                    {
                        first = current;
                    }
                    current = this->GetLeftOrDefault(current);
                }
                else
                {
                    current = this->GetRightOrDefault(current);
                }
            }
            if (first != 0)
            {
                current = first;
                while (true)
                {
                    if (this->handler(this->GetLinkValues(current)) == Break)
                    {
                        return Break;
                    }
                    current = this->GetNext(current);
                    if (current == 0 || this->GetBasePartValue(current) != link)
                    {
                        break;
                    }
                }
            }
            return Continue;
        }
        };
}
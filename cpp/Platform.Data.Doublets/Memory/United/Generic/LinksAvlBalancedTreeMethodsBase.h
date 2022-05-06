

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class LinksAvlBalancedTreeMethodsBase<TLinkAddress> : public SizedAndThreadedAVLBalancedTreeMethods<TLinkAddress>, ILinksTreeMethods<TLinkAddress>
    {
        private: static UncheckedConverter<TLinkAddress, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLinkAddress, std::int64_t>.Default;
        private: static UncheckedConverter<TLinkAddress, std::int32_t> _addressToInt32Converter = UncheckedConverter<TLinkAddress, std::int32_t>.Default;
        private: static UncheckedConverter<bool, TLinkAddress> _boolToAddressConverter = UncheckedConverter<bool, TLinkAddress>.Default;
        private: static UncheckedConverter<TLinkAddress, bool> _addressToBoolConverter = UncheckedConverter<TLinkAddress, bool>.Default;
        private: static UncheckedConverter<std::int32_t, TLinkAddress> _int32ToAddressConverter = UncheckedConverter<std::int32_t, TLinkAddress>.Default;

        protected: static constexpr TLinkAddress Break = 0;
        protected: static constexpr TLinkAddress Continue = 0;
        protected: std::uint8_t* Links;
        protected: std::uint8_t* Header;

        protected: LinksAvlBalancedTreeMethodsBase(LinksConstants<TLinkAddress> constants, std::uint8_t* storage, std::uint8_t* header)
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

        protected:         auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header);
        }

        protected: RawLink<TLinkAddress>& GetLinkReference(TLinkAddress link) { *reinterpret_cast<RawLink<TLinkAddress>*>(Links + (RawLink<TLinkAddress>::SizeInBytes * (link))); }

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

        protected: virtual TLinkAddress GetSizeValue(TLinkAddress value) { return Bit<TLinkAddress>.PartialRead(value, 5, -5); }

        protected: virtual void SetSizeValue(TLinkAddress* storedValue, TLinkAddress size) { return storedValue = Bit<TLinkAddress>.PartialWrite(storedValue, size, 5, -5); }

        protected: virtual bool GetLeftIsChildValue(TLinkAddress value)
        {
            {
                return _addressToBoolConverter.Convert(Bit<TLinkAddress>.PartialRead(value, 4, 1));
            }
        }

        protected: virtual void SetLeftIsChildValue(TLinkAddress* storedValue, bool value)
        {
            {
                auto previousValue = *storedValue;
                auto modified = Bit<TLinkAddress>.PartialWrite(previousValue, _boolToAddressConverter.Convert(value), 4, 1);
                *storedValue = modified;
            }
        }

        protected: virtual bool GetRightIsChildValue(TLinkAddress value)
        {
            {
                return _addressToBoolConverter.Convert(Bit<TLinkAddress>.PartialRead(value, 3, 1));
            }
        }

        protected: virtual void SetRightIsChildValue(TLinkAddress* storedValue, bool value)
        {
            {
                auto previousValue = *storedValue;
                auto modified = Bit<TLinkAddress>.PartialWrite(previousValue, _boolToAddressConverter.Convert(value), 3, 1);
                *storedValue = modified;
            }
        }

        protected: bool IsChild(TLinkAddress parent, TLinkAddress possibleChild)
        {
            auto parentSize = this->GetSize(parent);
            auto childSize = this->GetSizeOrZero(possibleChild);
            return childSize > 0 && this->LessOrEqualThan(childSize, parentSize);
        }

        protected: virtual std::int8_t GetBalanceValue(TLinkAddress storedValue)
        {
            {
                auto value = _addressToInt32Converter.Convert(Bit<TLinkAddress>.PartialRead(storedValue, 0, 3));
                value |= 0xF8 * ((value & 4) >> 2);
                return (std::int8_t)value;
            }
        }

        protected: virtual void SetBalanceValue(TLinkAddress* storedValue, std::int8_t value)
        {
            {
                auto packagedValue = _int32ToAddressConverter.Convert((std::uint8_t)value >> 5 & 4 | value & 3);
                auto modified = Bit<TLinkAddress>.PartialWrite(*storedValue, packagedValue, 0, 3);
                *storedValue = modified;
            }
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

        public: TLinkAddress EachUsage(TLinkAddress link, Func<IList<TLinkAddress>, TLinkAddress> handler)
        {
            auto root = this->GetTreeRoot();
            if (root == 0)
            {
                return Continue;
            }
            TLinkAddress first = 0, current = root;
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

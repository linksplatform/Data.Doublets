namespace Platform::Data::Doublets::Memory::United::Generic
{
template<typename Self, typename TLinksOptions>
struct LinksAvlBalancedTreeMethodsBase : public Platform::Collections::Methods::Trees::SizedAndThreadedAVLBalancedTreeMethods<Self, typename TLinksOptions::LinkAddressType>/*, ILinksTreeMethods<TLinksOptions>*/
    {
  using LinksOptionsType = TLinksOptions;
  using LinkAddressType = typename LinksOptionsType::LinkAddressType;
  using LinkType = typename LinksOptionsType::LinkType;
  using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
  static constexpr LinksConstants<LinkAddressType> Constants = LinksOptionsType::Constants;


public: using methods = Platform::Collections::Methods::Trees::SizedAndThreadedAVLBalancedTreeMethods<Self, LinkAddressType>;

        public: static constexpr LinkAddressType Break = Constants.Break;
        public: static constexpr LinkAddressType Continue = Constants.Continue;
        public: std::byte* Links;
        public: std::byte* Header;

        public: LinksAvlBalancedTreeMethodsBase(std::byte* storage, std::byte* header) : Links(storage), Header(header) {}

        public: LinkAddressType GetTreeRoot()
                {
                    return this->object().GetTreeRoot();
                };

        public: LinkAddressType GetBasePartValue(LinkAddressType link)
                {
                    return this->object().GetBasePartValue(link);
                };

        public: bool FirstIsToTheRightOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return this->object().FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget);
                };

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType source, LinkAddressType target, LinkAddressType rootSource, LinkAddressType rootTarget)
                {
                    return this->object().FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget);
                };

        public: auto& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(Header);
        }

        public: auto& GetLinkReference(LinkAddressType linkAddress) { return *(reinterpret_cast<RawLink<LinkAddressType>*>(Links) + linkAddress); }

        public: LinkType GetLinkValues(LinkAddressType linkIndex)
        {
            auto& link = GetLinkReference(linkIndex);
            return LinkType{linkIndex, link.Source, link.Target};
        }

        public: bool FirstIsToTheLeftOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        public: bool FirstIsToTheRightOfSecond(LinkAddressType first, LinkAddressType second)
        {
            auto& firstLink = this->GetLinkReference(first);
            auto& secondLink = this->GetLinkReference(second);
            return this->FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

      public: LinkAddressType GetSizeValue(LinkAddressType value) { return Platform::Numbers::Bit::PartialRead<LinkAddressType>(value, 5, -5); }

        public: void SetSizeValue(LinkAddressType* storedValue, LinkAddressType size) { return storedValue = Platform::Numbers::Bit::PartialWrite<LinkAddressType>(storedValue, size, 5, -5); }

        public: bool GetLeftIsChildValue(LinkAddressType value)
        {
            {
                return (bool)Platform::Numbers::Bit::PartialRead<LinkAddressType>(value, 4, 1);
            }
        }

        public: void SetLeftIsChildValue(LinkAddressType* storedValue, bool value)
        {
            {
                auto previousValue = *storedValue;
                auto modified = Platform::Numbers::Bit::PartialWrite<LinkAddressType>(previousValue, (LinkAddressType)value, 4, 1);
                *storedValue = modified;
            }
        }

        public: bool GetRightIsChildValue(LinkAddressType value)
        {
            {
                return (bool)Platform::Numbers::Bit::PartialRead<LinkAddressType>(value, 3, 1);
            }
        }

        public: void SetRightIsChildValue(LinkAddressType* storedValue, bool value)
        {
            {
                auto previousValue = *storedValue;
                auto modified = Platform::Numbers::Bit::PartialWrite<LinkAddressType>(previousValue, (LinkAddressType)value, 3, 1);
                *storedValue = modified;
            }
        }

        public: bool IsChild(LinkAddressType parent, LinkAddressType possibleChild)
        {
            auto parentSize = this->GetSize(parent);
            auto childSize = this->GetSizeOrZero(possibleChild);
            return childSize > 0 && (childSize <= parentSize);
        }

        public: std::int8_t GetBalanceValue(LinkAddressType storedValue)
        {
            {
                auto value = (std::int32_t)Platform::Numbers::Bit::PartialRead<LinkAddressType>(storedValue, 0, 3);
                value |= 0xF8 * ((value & 4) >> 2);
                return (std::int8_t)value;
            }
        }

        public: void SetBalanceValue(LinkAddressType* storedValue, std::int8_t value)
        {
            {
                auto packagedValue = (LinkAddressType)((std::uint8_t)value >> 5 & 4 | value & 3);
                auto modified = Platform::Numbers::Bit::PartialWrite<LinkAddressType>(*storedValue, packagedValue, 0, 3);
                *storedValue = modified;
            }
        }

        public: auto operator[](std::size_t index)
        {
                auto root = GetTreeRoot();
                if (index >= GetSize(root))
                {
                    return 0;
                }
                while (!EqualToZero(root))
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

        public: LinkAddressType Search(LinkAddressType source, LinkAddressType target)
        {
            auto root = this->GetTreeRoot();
            while (root != 0)
            {
                auto& rootLink = this->GetLinkReference(root);
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

        public: LinkAddressType CountUsages(LinkAddressType link)
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

      public: LinkAddressType EachUsage(LinkAddressType link, std::function<LinkAddressType(LinkType)> handler)
        {
            auto root = this->GetTreeRoot();
            if (root == 0)
            {
                return Continue;
            }
            LinkAddressType first = 0, current = root;
            while (current != 0)
            {
                auto base = this->GetBasePartValue(current);
                if (base >= link)
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
                    if (handler(this->GetLinkValues(current)) == Break)
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
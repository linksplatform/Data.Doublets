namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    using namespace Platform::Data;
    template<
        typename TSelf,
        typename TLinksOptions,
        typename TMemory,
        typename TInternalSourcesListMethods,
        typename TInternalSourcesTreeMethods,
        typename TInternalTargetsTreeMethods,
        typename TExternalSourcesTreeMethods,
        typename TExternalTargetsTreeMethods,
        typename TInternalLinksSourcesLinkedTreeMethods,
        typename TUnusedLinksTreeMethods,
        typename... TBase>
    struct SplitMemoryLinksBase : public Interfaces::Polymorph<TSelf, TBase...>
    {
    public:
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using WriteHandlerType = LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
        static constexpr LinksConstants<LinkAddressType> Constants = LinksOptionsType::Constants;
        static constexpr bool UseLinkedList = false;
    protected:
        TMemory _dataMemory;
        TMemory _indexMemory;
        std::uint64_t _dataMemoryReservationStepInBytesInBytes;
        std::uint64_t _indexDataMemoryReservationStepInBytesInBytes;
        TInternalSourcesListMethods* InternalSourcesListMethods;
        TInternalSourcesTreeMethods* InternalSourcesTreeMethods;
        TInternalTargetsTreeMethods* InternalTargetsTreeMethods;
        TExternalSourcesTreeMethods* ExternalSourcesTreeMethods;
        TExternalTargetsTreeMethods* ExternalTargetsTreeMethods;
        TUnusedLinksTreeMethods* UnusedLinksTreeMethods;
        std::byte* _header;
        std::byte* _linksDataParts;
        std::byte* _linksIndexParts;

    public:
        const RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType linkIndex) const
        {
            return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_linksDataParts + (LinkDataPartSizeInBytes * linkIndex));
        }

        RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType linkIndex)
        {
            return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_linksDataParts + (LinkDataPartSizeInBytes * linkIndex));
        }

        auto&& GetLinkIndexPartReference(LinkAddressType linkIndex)
        {
            return *reinterpret_cast<RawLinkIndexPart<LinkAddressType>*>(_linksIndexParts + (LinkIndexPartSizeInBytes * linkIndex));
        }

    public:
        auto&& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(this->_header);
        }

        LinkAddressType Total() const
        {
            const auto& header = this->GetHeaderReference();
            return header.AllocatedLinks - header.FreeLinks;
        }

    public:
        static constexpr std::uint64_t LinkDataPartSizeInBytes = sizeof(RawLinkDataPart<LinkAddressType>);

        static constexpr std::uint64_t LinkIndexPartSizeInBytes = sizeof(RawLinkIndexPart<LinkAddressType>);

        static constexpr std::uint64_t LinkHeaderSizeInBytes = sizeof(LinksHeader<LinkAddressType>);

        static constexpr std::uint64_t DefaultLinksSizeStep = 1 * 1024 * 1024;

        SplitMemoryLinksBase(TMemory dataMemory, TMemory indexMemory) : SplitMemoryLinksBase(dataMemory, indexMemory, DefaultLinksSizeStep)
        {
        }

        SplitMemoryLinksBase(TMemory dataMemory, TMemory indexMemory, std::uint64_t dataMemoryReservationStepInBytes) : _dataMemory{ dataMemory }, _indexMemory{ indexMemory }, _dataMemoryReservationStepInBytesInBytes{ dataMemoryReservationStepInBytes }
        {
            if (UseLinkedList)
            {
//                InternalSourcesTreeMethods = new TInternalLinksSourcesLinkedTreeMethods(_linksDataParts, _linksIndexParts);
            }
            else
            {
                InternalSourcesTreeMethods = new TInternalSourcesTreeMethods(_linksDataParts, _linksIndexParts, _header);
            }
            ExternalSourcesTreeMethods = new TExternalSourcesTreeMethods(_linksDataParts, _linksIndexParts, _header);
            InternalTargetsTreeMethods = new TInternalTargetsTreeMethods(_linksDataParts, _linksIndexParts, _header);
            ExternalTargetsTreeMethods = new TExternalTargetsTreeMethods(_linksDataParts, _linksIndexParts, _header);
            Init(dataMemory, indexMemory);
        }

        void Init(TMemory dataMemory, TMemory indexMemory)
        {
            if(indexMemory.ReservedCapacity() < LinkHeaderSizeInBytes)
            {
                indexMemory.ReservedCapacity(LinkHeaderSizeInBytes);
            }
            SetPointers(dataMemory, indexMemory);
            auto& header { this->GetHeaderReference() };
            auto allocatedLinks { header.AllocatedLinks };
            // Adjust reserved capacity
            auto minimumDataReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if(minimumDataReservedCapacity < dataMemory.UsedCapacity())
            {
                minimumDataReservedCapacity = dataMemory.UsedCapacity();
            }
            if(minimumDataReservedCapacity < _dataMemoryReservationStepInBytesInBytes)
            {
                minimumDataReservedCapacity = _dataMemoryReservationStepInBytesInBytes;
            }
            auto minimumIndexReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if (minimumIndexReservedCapacity < indexMemory.UsedCapacity)
            {
                minimumIndexReservedCapacity = indexMemory.UsedCapacity;
            }
            if (minimumIndexReservedCapacity < _indexDataMemoryReservationStepInBytesInBytes)
            {
                minimumIndexReservedCapacity = _indexDataMemoryReservationStepInBytesInBytes;
            }
            // Check for alignment
            if (minimumDataReservedCapacity % _dataMemoryReservationStepInBytesInBytes > 0)
            {
                minimumDataReservedCapacity = ((minimumDataReservedCapacity / _dataMemoryReservationStepInBytesInBytes) * _dataMemoryReservationStepInBytesInBytes) + _dataMemoryReservationStepInBytesInBytes;
            }
            if (minimumIndexReservedCapacity % _indexDataMemoryReservationStepInBytesInBytes > 0)
            {
                minimumIndexReservedCapacity = ((minimumIndexReservedCapacity / _indexDataMemoryReservationStepInBytesInBytes) * _indexDataMemoryReservationStepInBytesInBytes) + _indexDataMemoryReservationStepInBytesInBytes;
            }
            if (dataMemory.ReservedCapacity() != minimumDataReservedCapacity)
            {
                dataMemory.ReservedCapacity() = minimumDataReservedCapacity;
            }
            if (indexMemory.ReservedCapacity() != minimumIndexReservedCapacity)
            {
                indexMemory.ReservedCapacity() = minimumIndexReservedCapacity;
            }
            SetPointers(dataMemory, indexMemory);
            header = this->GetHeaderReference();
            // Ensure correctness _memory.UsedCapacity over _header->AllocatedLinks
            dataMemory.UsedCapacity((header.AllocatedLinks * LinkDataPartSizeInBytes) + LinkDataPartSizeInBytes); // First link is read only zero link.
            indexMemory.UsedCapacity((header.AllocatedLinks * LinkIndexPartSizeInBytes) + LinkHeaderSizeInBytes);
            // Ensure correctness _memory.ReservedLinks over _header->ReservedCapacity
            header.ReservedLinks = (dataMemory.ReservedCapacity() - LinkDataPartSizeInBytes) / LinkDataPartSizeInBytes;
        }

        LinkAddressType Count(const LinkType& restriction) const
        {
            if (std::ranges::size(restriction) == 0)
            {
                return this->Total();
            }
            auto index = GetIndex(*this, restriction);
            auto any = Constants.Any;
            if (std::ranges::size(restriction) == 1)
            {
                if (index == any)
                {
                    return this->Total();
                }
                return this->Exists(index) ? LinkAddressType{1} : LinkAddressType{0};
            }
            if (std::ranges::size(restriction) == 2)
            {
                auto value = restriction[1];
                if (index == any)
                {
                    if (value == any)
                    {
                        return this->Total(); // Any - как отсутствие ограничения
                    }
                    auto externalReferencesRange = Constants.ExternalReferencesRange;
                    if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(value))
                    {
                        return ExternalSourcesTreeMethods->CountUsages(value) + ExternalTargetsTreeMethods->CountUsages(value);
                    }
                    else
                    {
                        if (UseLinkedList)
                        {
                            return InternalSourcesListMethods->CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value);
                        }
                        else
                        {
                            return InternalSourcesTreeMethods->CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value);
                        }
                    }
                }
                else
                {
                    if (!this->Exists(index))
                    {
                        return LinkAddressType{0};
                    }
                    if (value == any)
                    {
                        return LinkAddressType{1};
                    }
                    auto& storedLinkValue = this->GetLinkDataPartReference(index);
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return LinkAddressType{1};
                    }
                    return LinkAddressType{0};
                }
            }
            if (std::ranges::size(restriction) == 3)
            {
                auto externalReferencesRange = Constants.ExternalReferencesRange;
                auto source = GetSource(*this, restriction);
                auto target = GetTarget(*this, restriction);
                if (index == any)
                {
                    if ((source == any) && (target == any))
                    {
                        return this->Total();
                    }
                    else if ((source == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(target))
                        {
                            return ExternalTargetsTreeMethods->CountUsages(target);
                        }
                        else
                        {
                            return InternalTargetsTreeMethods->CountUsages(target);
                        }
                    }
                    else if ((target == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(source))
                        {
                            return ExternalSourcesTreeMethods->CountUsages(source);
                        }
                        else
                        {
                            if (UseLinkedList)
                            {
                                return InternalSourcesTreeMethods->CountUsages(source);
                            }
                            else
                            {
                                return InternalSourcesTreeMethods->CountUsages(source);
                            }
                        }
                    }
                    else //if(source != Any && target != Any)
                    {
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                        LinkAddressType linkAddress;
                        if (Constants.IsExternalReferencesRangeEnabled)
                        {
                            if (externalReferencesRange.Contains(source) && externalReferencesRange.Contains(target))
                            {
                                linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(source))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(target))
                            {
                                if (UseLinkedList)
                                {
                                    linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                            else
                            {
                                if (UseLinkedList || InternalSourcesTreeMethods->CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target))
                                {
                                    linkAddress = InternalTargetsTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                        }
                        else
                        {
                            if (UseLinkedList || InternalSourcesTreeMethods->CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else
                            {
                                linkAddress = InternalSourcesTreeMethods->Search(source, target);
                            }
                        }
                        return (linkAddress == Constants.Null) ? LinkAddressType{0} : LinkAddressType{1};
                    }
                }
                else
                {
                    if (!this->Exists(index))
                    {
                        return LinkAddressType{0};
                    }
                    if ((source == any) && (target == any))
                    {
                        return LinkAddressType{1};
                    }
                    auto& storedLinkValue = GetLinkDataPartReference(index);
                    if ((source != any) && (target != any))
                    {
                        if ((storedLinkValue.Source == source) && (storedLinkValue.Target == target))
                        {
                            return LinkAddressType{1};
                        }
                        return LinkAddressType{0};
                    }
                    auto value = LinkAddressType{};
                    if ((source == any))
                    {
                        value = target;
                    }
                    if ((target == any))
                    {
                        value = source;
                    }
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return LinkAddressType{1};
                    }
                    return LinkAddressType{0};
                }
            }
            throw Platform::Exceptions::NotSupportedException();
        }
//
        void SetPointers(TMemory dataMemory, TMemory indexMemory)
        {
            _linksDataParts = static_cast<std::byte*>(dataMemory.Pointer());
            _linksIndexParts = static_cast<std::byte*>(indexMemory.Pointer());
            _header = _linksIndexParts;

            ExternalSourcesTreeMethods = new TExternalSourcesTreeMethods(_linksDataParts, _linksIndexParts, _header);
            InternalTargetsTreeMethods = new TInternalTargetsTreeMethods(_linksDataParts, _linksIndexParts, _header);
            ExternalTargetsTreeMethods = new TExternalTargetsTreeMethods(_linksDataParts, _linksIndexParts, _header);
            UnusedLinksTreeMethods = new TUnusedLinksTreeMethods(static_cast<std::byte*>(_dataMemory.Pointer()), _header);
        }

        LinkAddressType Count(const LinkType& restriction)
        {
            auto length = std::ranges::size(restriction);
            // Если нет ограничений, тогда возвращаем общее число связей находящихся в хранилище.
            if (length == 0)
            {
                return Total();
            }
            auto constants = Constants;
            auto any = constants.Any;
            auto index = GetIndex(*this, restriction);
            if (length == 1)
            {
                if (index == any)
                {
                    return Total();
                }
                return Exists(index) ? LinkAddressType{1} : LinkAddressType{0};
            }
            if (length == 2)
            {
                auto value = restriction[1];
                if ((index == any))
                {
                    if ((value == any))
                    {
                        return Total(); // Any - как отсутствие ограничения
                    }
                    auto externalReferencesRange = constants.ExternalReferencesRange;
                    if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(value))
                    {
                        return (ExternalSourcesTreeMethods->CountUsages(value) + ExternalTargetsTreeMethods->CountUsages(value));
                    }
                    else
                    {
                        if (_useLinkedList)
                        {
                            return (InternalSourcesListMethods->CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value));
                        }
                        else
                        {
                            return (InternalSourcesTreeMethods->CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value));
                        }
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return LinkAddressType{0};
                    }
                    if ((value == any))
                    {
                        return LinkAddressType{1};
                    }
                    auto& storedLinkValue = GetLinkDataPartReference(index);
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return LinkAddressType{1};
                    }
                    return LinkAddressType{0};
                }
            }
            if (length == 3)
            {
                auto externalReferencesRange = constants.ExternalReferencesRange;
                auto source = GetSource(*this, restriction);
                auto target = GetTarget(*this, restriction);
                if ((index == any))
                {
                    if ((source == any) && (target == any))
                    {
                        return Total();
                    }
                    else if ((source == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(target))
                        {
                            return ExternalTargetsTreeMethods->CountUsages(target);
                        }
                        else
                        {
                            return InternalTargetsTreeMethods->CountUsages(target);
                        }
                    }
                    else if ((target == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(source))
                        {
                            return ExternalSourcesTreeMethods->CountUsages(source);
                        }
                        else
                        {
                            if (_useLinkedList)
                            {
                                return InternalSourcesListMethods->CountUsages(source);
                            }
                            else
                            {
                                return InternalSourcesTreeMethods->CountUsages(source);
                            }
                        }
                    }
                    else //if(source != Any && target != Any)
                    {
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                        LinkAddressType linkAddress;
                        if (Constants.IsExternalReferencesRangeEnabled)
                        {
                            if (externalReferencesRange.Contains(source) && externalReferencesRange.Contains(target))
                            {
                                linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(source))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(target))
                            {
                                if (_useLinkedList)
                                {
                                    linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                            else
                            {
                                if (_useLinkedList || (InternalSourcesTreeMethods->CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target)))
                                {
                                    linkAddress = InternalTargetsTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                        }
                        else
                        {
                            if (_useLinkedList || (InternalSourcesTreeMethods->CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target)))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else
                            {
                                linkAddress = InternalSourcesTreeMethods->Search(source, target);
                            }
                        }
                        return (linkAddress == constants.Null) ? LinkAddressType{0} : LinkAddressType{1};
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return LinkAddressType{0};
                    }
                    if ((source == any) && (target == any))
                    {
                        return LinkAddressType{1};
                    }
                    auto& storedLinkValue = GetLinkDataPartReference(index);
                    if ((source != any) && (target != any))
                    {
                        if ((storedLinkValue.Source == source) && (storedLinkValue.Target == target))
                        {
                            return LinkAddressType{1};
                        }
                        return LinkAddressType{0};
                    }
                    auto value = LinkAddressType{0};
                    if ((source == any))
                    {
                        value = target;
                    }
                    if ((target == any))
                    {
                        value = source;
                    }
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return LinkAddressType{1};
                    }
                    return LinkAddressType{0};
                }
            }
            throw NotSupportedException("Другие размеры и способы ограничений не поддерживаются.");
        }


         LinkAddressType Each(const LinkType& restriction, const ReadHandlerType& handler)
        {
            auto constants = Constants;
            auto $break = constants.Break;
            if (length == 0)
            {
                for (auto linkAddress = LinkAddressType{1}; (linkAddress <= GetHeaderReference().AllocatedLinks); ++linkAddress)
                {
                    if (Exists(linkAddress) && (handler(GetLinkStruct(linkAddress)) == $break))
                    {
                        return $break;
                    }
                }
                return $break;
            }
            auto $continue = constants.Continue;
            auto any = constants.Any;
            auto index = GetIndex(*this, restriction);
            if (length == 1)
            {
                if ((index == any))
                {
                    return Each(Array.Empty<LinkAddressType>(), handler);
                }
                if (!Exists(index))
                {
                    return $continue;
                }
                return handler(GetLinkStruct(index));
            }
            if (length == 2)
            {
                auto value = restriction[1];
                if (index == any)
                {
                    if (value == any)
                    {
                        return Each(Array.Empty<LinkAddressType>(), handler);
                    }
                    if (Each(LinkType(index, value, any), handler) == $break)
                    {
                        return $break;
                    }
                    return Each(LinkType(index, any, value), handler);
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
                    }
                    if ((value == any))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    auto& storedLinkValue = GetLinkDataPartReference(index);
                    if ((storedLinkValue.Source == value) ||
                        (storedLinkValue.Target == value))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return $continue;
                }
            }
            if (length == 3)
            {
                auto externalReferencesRange = constants.ExternalReferencesRange;
                auto source = GetSource(*this, restriction);
                auto target = GetTarget(*this, restriction);
                if ((index == any))
                {
                    if ((source == any) && (target == any))
                    {
                        return Each(Array.Empty<LinkAddressType>(), handler);
                    }
                    else if ((source == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(target))
                        {
                            return ExternalTargetsTreeMethods->EachUsage(target, handler);
                        }
                        else
                        {
                            return InternalTargetsTreeMethods->EachUsage(target, handler);
                        }
                    }
                    else if ((target == any))
                    {
                        if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(source))
                        {
                            return ExternalSourcesTreeMethods->EachUsage(source, handler);
                        }
                        else
                        {
                            if (_useLinkedList)
                            {
                                return InternalSourcesListMethods->EachUsage(source, handler);
                            }
                            else
                            {
                                return InternalSourcesTreeMethods->EachUsage(source, handler);
                            }
                        }
                    }
                    else //if(source != Any && target != Any)
                    {
                        LinkAddressType linkAddress;
                        if (Constants.IsExternalReferencesRangeEnabled)
                        {
                            if (externalReferencesRange.Contains(source) && externalReferencesRange.Contains(target))
                            {
                                linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(source))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(target))
                            {
                                if (_useLinkedList)
                                {
                                    linkAddress = ExternalSourcesTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                            else
                            {
                                if (_useLinkedList || (InternalSourcesTreeMethods->CountUsages(source) == InternalTargetsTreeMethods->CountUsages(target)))
                                {
                                    linkAddress = InternalTargetsTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    linkAddress = InternalSourcesTreeMethods->Search(source, target);
                                }
                            }
                        }
                        else
                        {
                            if (_useLinkedList || (InternalSourcesTreeMethods->CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target)))
                            {
                                linkAddress = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else
                            {
                                linkAddress = InternalSourcesTreeMethods->Search(source, target);
                            }
                        }
                        return (linkAddress == constants.Null) ? $continue : handler(GetLinkStruct(linkAddress));
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
                    }
                    if ((source == any) && (target == any))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    auto& storedLinkValue = GetLinkDataPartReference(index);
                    if ((source != any) && (target != any))
                    {
                        if ((storedLinkValue.Source == source) &&
                            (storedLinkValue.Target == target))
                        {
                            return handler(GetLinkStruct(index));
                        }
                        return $continue;
                    }
                    auto value = LinkAddressType{0};
                    if (source == any)
                    {
                        value = target;
                    }
                    if (target == any)
                    {
                        value = source;
                    }
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return $continue;
                }
            }
            throw NotSupportedException();
        }


         LinkAddressType Update(const LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler)
        {
            auto constants = Constants;
            auto $null = constants.Null;
            auto externalReferencesRange = constants.ExternalReferencesRange;
            auto linkIndex = GetIndex(*this, restriction);
            auto before = this->GetLinkStruct(linkIndex);
            auto& link = this->GetLinkDataPartReference(linkIndex);
            auto source = link.Source;
            auto target = link.Target;
            auto& header = this->GetHeaderReference();
            auto& rootAsSource = header.RootAsSource;
            auto& rootAsTarget = header.RootAsTarget;
            // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
            if (source != $null)
            {
                if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(source))
                {
                    ExternalSourcesTreeMethods->Detach(rootAsSource, linkIndex);
                }
                else
                {
                    if (_useLinkedList)
                    {
                        InternalSourcesListMethods->Detach(source, linkIndex);
                    }
                    else
                    {
                        InternalSourcesTreeMethods->Detach(GetLinkIndexPartReference(source).RootAsSource, linkIndex);
                    }
                }
            }
            if (target != $null)
            {
                if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(target))
                {
                    ExternalTargetsTreeMethods->Detach(rootAsTarget, linkIndex);
                }
                else
                {
                    InternalTargetsTreeMethods->Detach(GetLinkIndexPartReference(target).RootAsTarget, linkIndex);
                }
            }
            source = link.Source = GetSource(*this, substitution);
            target = link.Target = GetTarget(*this, substitution);
            if (source != $null)
            {
                if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(source))
                {
                    ExternalSourcesTreeMethods->Attach(rootAsSource, linkIndex);
                }
                else
                {
                    if (_useLinkedList)
                    {
                        InternalSourcesListMethods->AttachAsLast(source, linkIndex);
                    }
                    else
                    {
                        InternalSourcesTreeMethods->Attach(GetLinkIndexPartReference(source).RootAsSource, linkIndex);
                    }
                }
            }
            if (target != $null)
            {
                if (Constants.IsExternalReferencesRangeEnabled && externalReferencesRange.Contains(target))
                {
                    ExternalTargetsTreeMethods->Attach(rootAsTarget, linkIndex);
                }
                else
                {
                    InternalTargetsTreeMethods->Attach(GetLinkIndexPartReference(target).RootAsTarget, linkIndex);
                }
            }
            return handler ? handler(before, LinkType(linkIndex, source, target)) : Constants.Continue;
        }


         LinkAddressType Create(const LinkType& substitution, const WriteHandlerType& handler)
        {
            using namespace Platform::Exceptions;
            auto& header = GetHeaderReference();
            auto freeLink = header.FirstFreeLink;
            if (freeLink != Constants.Null)
            {
                UnusedLinksListMethods.Detach(freeLink);
            }
            else
            {
                auto maximumPossibleInnerReference = Constants.InternalReferencesRange.Maximum;
                if (header.AllocatedLinks > maximumPossibleInnerReference)
                {
                    throw LinksLimitReachedException();
                }
                if (header.AllocatedLinks >= (header.ReservedLinks - 1))
                {
                    _dataMemory.ReservedCapacity() += _dataMemoryReservationStepInBytes;
                    _indexMemory.ReservedCapacity() += _indexMemoryReservationStepInBytes;
                    SetPointers(_dataMemory, _indexMemory);
                    header = GetHeaderReference();
                    header.ReservedLinks = _dataMemory.ReservedCapacity() / LinkDataPartSizeInBytes;
                }
                freeLink = ++header.AllocatedLinks;
                _dataMemory.UsedCapacity(_dataMemory.UsedCapacity() + LinkDataPartSizeInBytes);
                _indexMemory.UsedCapacity(_indexMemory.UsedCapacity() + LinkIndexPartSizeInBytes);
            }
            return handler ? handler(null, GetLinkStruct(freeLink)) : Constants.Continue;
        }


        LinkAddressType Delete(const LinkType& restriction, auto&& handler)
        {
            auto& header = this->GetHeaderReference();
            auto linkAddress = GetIndex(*this, restriction);
            auto before = GetLinkStruct(linkAddress);
            if (linkAddress < header.AllocatedLinks)
            {
                UnusedLinksListMethods.AttachAsFirst(linkAddress);
            }
            else if (linkAddress == header.AllocatedLinks)
            {
                --header.AllocatedLinks;
                _dataMemory.UsedCapacity(_dataMemory.UsedCapacity() - LinkDataPartSizeInBytes);
                _indexMemory.UsedCapacity(_indexMemory.UsedCapacity() - LinkIndexPartSizeInBytes);
                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while ((header.AllocatedLinks > LinkAddressType{0};) && this->IsUnusedLink(header.AllocatedLinks))
                {
                    UnusedLinksListMethods.Detach(header.AllocatedLinks);
                    --header.AllocatedLinks;
                    _dataMemory.UsedCapacity() -= LinkDataPartSizeInBytes;
                    _indexMemory.UsedCapacity() -= LinkIndexPartSizeInBytes;
                }
            }
            return handler ? handler(before, null) : Constants.Continue;
        }

    protected:
        bool IsUnusedLink(LinkAddressType linkIndex) const
        {
            if (GetHeaderReference().FirstFreeLink != linkIndex) // May be this check is not needed
            {
                // TODO: Reduce access to memory in different location (should be enough to use just linkIndexPart)
                const auto& linkDataPart = this->GetLinkDataPartReference(linkIndex);
                const auto& linkIndexPart = this->GetLinkIndexPartReference(linkIndex);
                return (linkIndexPart.SizeAsTarget == LinkAddressType{0}) && (linkDataPart.Source != LinkAddressType{0});
            }
            else
            {
                return true;
            }
        }

        bool Exists(LinkAddressType linkAddress) const
        {
            (linkAddress >= Constants.InternalReferencesRange.Minimum)
                && (linkAddress <= GetHeaderReference().AllocatedLinks)
                && !IsUnusedLink(linkAddress);
        }

    public:
        LinkType GetLinkStruct(LinkAddressType linkIndex) const
        {
            const auto& link = this->GetLinkDataPartReference(linkIndex);
            return LinkType(linkIndex, link.Source, link.Target);
        }

    };
}

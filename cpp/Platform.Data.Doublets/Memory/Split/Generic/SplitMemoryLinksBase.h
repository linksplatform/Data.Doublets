namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    using namespace Platform::Data;
    template<
        typename TSelf,
        typename TLinksOptions,
        typename TMemory,
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
        std::int64_t _dataMemoryReservationStepInBytesInBytes;
        std::int64_t _indexDataMemoryReservationStepInBytesInBytes;
        TInternalSourcesTreeMethods* InternalSourcesTreeMethods;
        TInternalTargetsTreeMethods* InternalTargetsTreeMethods;
        TExternalSourcesTreeMethods* ExternalSourcesTreeMethods;
        TExternalTargetsTreeMethods* ExternalTargetsTreeMethods;
        TUnusedLinksTreeMethods* UnusedLinksTreeMethods;
        std::int8_t* _header;
        std::int8_t* _linksDataParts;
        std::int8_t* _linksIndexParts;

    public:
        TLinkAddress Total() const
        {
            auto& header = this->GetHeaderReference();
            return header.AllocatedLinks - header.FreeLinks;
        }

    public:
        static std::int64_t LinkDataPartSizeInBytes()
        {
            return RawLinkDataPart<LinkAddressType>::SizeInBytes();
        }

        static std::int64_t LinkIndexPartSizeInBytes()
        {
            return RawLinkIndexPart<LinkAddressType>::SizeInBytes();
        }

        static std::int64_t LinkHeaderSizeInBytes()
        {
            return LinksHeader<LinkAddressType>::SizeInBytes();
        }

        static std::int64_t DefaultLinksSizeStep()
        {
            return 1 * 1024 * 1024;
        }

        SplitMemoryLinksBase(TMemory dataMemory, TMemory indexMemory) : SplitMemoryLinksBase(dataMemory, indexMemory, DefaultLinksSizeStep())
        {
        }

        SplitMemoryLinksBase(TMemory dataMemory, TMemory indexMemory, std::int64_t dataMemoryReservationStepInBytes) : _dataMemory{ dataMemory }, _indexMemory{ indexMemory }, _dataMemoryReservationStepInBytesInBytes{ dataMemoryReservationStepInBytes }
        {
            if (UseLinkedList)
            {
                InternalSourcesTreeMethods = TInternalLinksSourcesLinkedTreeMethods(Constants, _linksDataParts, _linksIndexParts);
            }
            else
            {
                InternalSourcesTreeMethods = new TInternalSourcesTreeMethods(Constants, _linksDataParts, _linksIndexParts, _header);
            }
            ExternalSourcesTreeMethods = new TExternalSourcesTreeMethods(Constants, _linksDataParts, _linksIndexParts, _header);
            InternalTargetsTreeMethods = new TInternalTargetsTreeMethods(Constants, _linksDataParts, _linksIndexParts, _header);
            ExternalTargetsTreeMethods = new TExternalTargetsTreeMethods(Constants, _linksDataParts, _linksIndexParts, _header);
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
            auto allocatedLinks { header.AllocatedLinks() };
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
            if (dataMemory.ReservedCapacity != minimumDataReservedCapacity)
            {
                dataMemory.ReservedCapacity = minimumDataReservedCapacity;
            }
            if (indexMemory.ReservedCapacity != minimumIndexReservedCapacity)
            {
                indexMemory.ReservedCapacity = minimumIndexReservedCapacity;
            }
            SetPointers(dataMemory, indexMemory);
            header = this->GetHeaderReference();
            // Ensure correctness _memory.UsedCapacity over _header->AllocatedLinks
            dataMemory.UsedCapacity((header.AllocatedLinks * LinkDataPartSizeInBytes) + LinkDataPartSizeInBytes); // First link is read only zero link.
            indexMemory.UsedCapacity((header.AllocatedLinks * LinkIndexPartSizeInBytes) + LinkHeaderSizeInBytes);
            // Ensure correctness _memory.ReservedLinks over _header->ReservedCapacity
            header.ReservedLinks = (dataMemory.ReservedCapacity - LinkDataPartSizeInBytes) / LinkDataPartSizeInBytes;
        }

        LinkAddressType Count(const LinkType& restriction) const
        {
            if (std::ranges::size(restriction) == 0)
            {
                return Total();
            }
            auto index = GetIndex(*this, restriction);
            auto any = Constants.Any;
            if (std::ranges::size(restriction) == 1)
            {
                if (index == any)
                {
                    return Total();
                }
                return Exists(this, index) ? LinkAddressType{1} : LinkAddressType{0};
            }
            if (std::ranges::size(restriction) == 2)
            {
                auto value = restriction[1];
                if (index == any)
                {
                    if (value == any)
                    {
                        return Total(); // Any - как отсутствие ограничения
                    }
                    auto externalReferencesRange = Constants.ExternalReferencesRange;
                    if (Constants.IsExternalReferencesRangesEnabled && externalReferencesRange.Contains(value))
                    {
                        return ExternalSourcesTreeMethods.CountUsages(value) + ExternalTargetsTreeMethods.CountUsages(value);
                    }
                    else
                    {
                        if (UseLinkedList)
                        {
                            return InternalSourcesTreeMethods.CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value);
                        }
                        else
                        {
                            return InternalSourcesTreeMethods.CountUsages(value) + InternalTargetsTreeMethods->CountUsages(value);
                        }
                    }
                }
                else
                {
                    if (!Exists(this, index))
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
                        return Total();
                    }
                    else if ((source == any))
                    {
                        if (Constants.IsExternalReferencesRangesEnabled && externalReferencesRange.Contains(target))
                        {
                            return ExternalTargetsTreeMethods.CountUsages(target);
                        }
                        else
                        {
                            return InternalTargetsTreeMethods->CountUsages(target);
                        }
                    }
                    else if ((target == any))
                    {
                        if (Constants.IsExternalReferencesRangesEnabled && externalReferencesRange.Contains(source))
                        {
                            return ExternalSourcesTreeMethods.CountUsages(source);
                        }
                        else
                        {
                            if (UseLinkedList)
                            {
                                return InternalSourcesTreeMethods.CountUsages(source);
                            }
                            else
                            {
                                return InternalSourcesTreeMethods.CountUsages(source);
                            }
                        }
                    }
                    else //if(source != Any && target != Any)
                    {
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                        LinkAddressType link;
                        if (Constants.IsExternalReferencesRangesEnabled)
                        {
                            if (externalReferencesRange.Contains(source) && externalReferencesRange.Contains(target))
                            {
                                link = ExternalSourcesTreeMethods.Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(source))
                            {
                                link = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else if (externalReferencesRange.Contains(target))
                            {
                                if (UseLinkedList)
                                {
                                    link = ExternalSourcesTreeMethods.Search(source, target);
                                }
                                else
                                {
                                    link = InternalSourcesTreeMethods.Search(source, target);
                                }
                            }
                            else
                            {
                                if (UseLinkedList || InternalSourcesTreeMethods.CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target))
                                {
                                    link = InternalTargetsTreeMethods->Search(source, target);
                                }
                                else
                                {
                                    link = InternalSourcesTreeMethods.Search(source, target);
                                }
                            }
                        }
                        else
                        {
                            if (UseLinkedList || InternalSourcesTreeMethods.CountUsages(source) > InternalTargetsTreeMethods->CountUsages(target))
                            {
                                link = InternalTargetsTreeMethods->Search(source, target);
                            }
                            else
                            {
                                link = InternalSourcesTreeMethods.Search(source, target);
                            }
                        }
                        return (link == Constants.Null) ? LinkAddressType{0} : LinkAddressType{1};
                    }
                }
                else
                {
                    if (!Exists(this, index))
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
            _linksDataParts = (std::int8_t*)dataMemory.Pointer;
            _linksIndexParts = (std::int8_t*)indexMemory.Pointer;
            _header = _linksIndexParts;

            ExternalSourcesTreeMethods = new TExternalSourcesTreeMethods();
            InternalTargetsTreeMethods = new TInternalTargetsTreeMethods();
            ExternalTargetsTreeMethods = new TExternalTargetsTreeMethods();
            UnusedLinksTreeMethods = new TUnusedLinksTreeMethods(_linksDataParts, _header);
        }
    };
}

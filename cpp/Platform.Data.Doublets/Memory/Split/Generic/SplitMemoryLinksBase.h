namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    using Platform::Memory::IResizableDirectMemory;
    template<
        typename TSelf,
        typename TLinksOptions,
        typename TMemory,
        typename TSourceTreeMethods,
        bool VUseLinkedList,
        typename TTargetTreeMethods,
        typename TUnusedLinks,
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
        static constexpr bool UseLinkedList = VUseLinkedList;
    private:
        std::int64_t _datadataMemoryReservationStepInBytesInBytes;
    protected:
        IResizableDirectMemory _dataMemory;
        IResizableDirectMemory _indexMemory;
        std::int64_t _datadataMemoryReservationStepInBytesInBytes;
        std::int64t _indexdataMemoryReservationStepInBytesInBytes;

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

        SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, std::int64_t dataMemoryReservationStepInBytes) : this(dataMemory, indexMemory, dataMemoryReservationStepInBytes, LinksConstants<LinkAddressType>{}, true)
        {

        }

        SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, std::int64_t dataMemoryReservationStepInBytes, LinksConstants<LinkAddressType> constants, IndexTreeType indexTreeType) : _dataMemory{ dataMemory }, _indexMemory{ indexMemory }, _datadataMemoryReservationStepInBytesInBytes{ dataMemoryReservationStepInBytes }, _indexTreeType{ indexTreeType } }
        {
            if (UseLinkedList)
            {
                InternalSourcesListMethods = InternalLinksSourcesLinkedListMethods<LinkAddressType>(Constants, _linksDataParts, _linksIndexParts);
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

        void Init(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            if(indexMemory.ReservedCapacity() < LinkHeaderSizeInBytes)
            {
                indexMemory.ReservedCapacity(LinkHeaderSizeInBytes);
            }
            SetPointers(dataMemory, indexMemory);
            auto& header { GetHeaderReference() };
            auto allocatedLinks { header.AllocatedLinks() };
            // Adjust reserved capacity
            auto minimumDataReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if(minimumDataReservedCapacity < dataMemory.UsedCapacity())
            {
                minimumDataReservedCapacity = dataMemory.UsedCapacity();
            }
            if(minimumDataReservedCapacity < _datadataMemoryReservationStepInBytesInBytes)
            {
                minimumDataReservedCapacity = _datadataMemoryReservationStepInBytesInBytes;
            }
            auto minimumIndexReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if (minimumIndexReservedCapacity < indexMemory.UsedCapacity)
            {
                minimumIndexReservedCapacity = indexMemory.UsedCapacity;
            }
            if (minimumIndexReservedCapacity < _indexdataMemoryReservationStepInBytesInBytes)
            {
                minimumIndexReservedCapacity = _indexdataMemoryReservationStepInBytesInBytes;
            }
            // Check for alignment
            if (minimumDataReservedCapacity % _datadataMemoryReservationStepInBytesInBytes > 0)
            {
                minimumDataReservedCapacity = ((minimumDataReservedCapacity / _datadataMemoryReservationStepInBytesInBytes) * _datadataMemoryReservationStepInBytesInBytes) + _datadataMemoryReservationStepInBytesInBytes;
            }
            if (minimumIndexReservedCapacity % _indexdataMemoryReservationStepInBytesInBytes > 0)
            {
                minimumIndexReservedCapacity = ((minimumIndexReservedCapacity / _indexdataMemoryReservationStepInBytesInBytes) * _indexdataMemoryReservationStepInBytesInBytes) + _indexdataMemoryReservationStepInBytesInBytes;
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
            header = GetHeaderReference();
            // Ensure correctness _memory.UsedCapacity over _header->AllocatedLinks
            dataMemory.UsedCapacity((header.AllocatedLinks * LinkDataPartSizeInBytes) + LinkDataPartSizeInBytes); // First link is read only zero link.
            indexMemory.UsedCapacity((header.AllocatedLinks * LinkIndexPartSizeInBytes) + LinkHeaderSizeInBytes);
            // Ensure correctness _memory.ReservedLinks over _header->ReservedCapacity
            header.ReservedLinks = (dataMemory.ReservedCapacity - LinkDataPartSizeInBytes) / LinkDataPartSizeInBytes;
        }

        LinkAddressType Count(Interfaces::CList<LinkAddressType> auto&& restriction) const
        {
            auto length { std::ranges::size(restriction) };
            if(0 == length)
            {
                return Total;
            }
            auto any = Constants.Any;
            auto index = GetIndex(this, restriction);
            if (1 == length)
            {
                if (any == index))
                {
                    return Total;
                }
                return Exists(this, index) ? 1 : 0;
            }
            if (2 == length)
            {
                auto value = restriction[1];
                if (any == index)
                {
                    if (value == any))
                    {
                        return Total; // Any - как отсутствие ограничения
                    }
                    auto externalReferencesRange = constants.ExternalReferencesRange;
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value))
                    {
                        return ExternalSourcesTreeMethods.CountUsages(value) + ExternalTargetsTreeMethods.CountUsages(value);
                    }
                    else
                    {
                        if (UseLinkedList)
                        {
                            return InternalSourcesListMethods.CountUsages(value) + InternalTargetsTreeMethods.CountUsages(value);
                        }
                        else
                        {
                            return InternalSourcesTreeMethods.CountUsages(value) + InternalTargetsTreeMethods.CountUsages(value);
                        }
                    }
                }
                else
                {
                    if (!Exists(this, index))
                    {
                        return 0;
                    }
                    if (any == value)
                    {
                        return 1;
                    }
                    auto storedLinkValue { GetLinkDataPartReference(index) };
                    if ((values == storedLinkValue.Source) || (value == storedLinkValue.Target))
                    {
                        return 1;
                    }
                    return 0;
                }
            }
            if(3 == length)
            {
                auto source { this->GetSource(this, restriction) };
                auto target { this->GetTarget(this, restriction) };
                if(any == index)
                {
                    if((any == source) && (any == target))
                    {
                        return Total;
                    }
                    else if (any == source)
                    {
                        if(externalReferenceRange)
                    }
                }
            }
        }
//
        void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            _linksDataParts = (byte*)dataMemory.Pointer;
            _linksIndexParts = (byte*)indexMemory.Pointer;
            _header = _linksIndexParts;

            ExternalSourcesTreeMethods = new ExternalSourcesTreeMethods();
            InternalTargetsTreeMethods = new InternalTargetsTreeMethods();
            ExternalTargetsTreeMethods = new ExternalTargetsTreeMethods();
            UnusedLinksListMethods = new UnusedLinksListMethods(_linksDataParts, _header);
        }

        void ResetPointers()
        {
            base.ResetPointers();
            _linksDataParts = null;
            _linksIndexParts = null;
            _header = null;
        }
    };
}

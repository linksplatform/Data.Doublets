namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Interfaces;
    using Platform::Memory::IResizableDirectMemory;
    template<
        typename TSelf,
        typename TLinkOptions,
        typename TMemory,
        typename TSourceTreeMethods,
        typename TTargetTreeMethods,
        typename TUnusedLinks,
        typename... TBase>
    struct SplitMemoryLinksBase : public Interfaces::Polymorph<TSelf, TBase...>
    {
    public:
        using OptionsType = TLinkOptions;
        using LinkAddressType = OptionsType::LinkAddressType;
        using WriteHandlerType = OptionsType::WriteHandlerType;
        using ReadHandlerType = OptionsType::ReadHandlerType;
        static constexpr LinksConstants<LinkAddressType> Constants = OptionsType::Constants;
    private:
        std::int64_t _memoryReservationStep;
        IndexTreeType _indexTreeType;
        std::function<ILinksTreeMethods<LinkAddressType>> _createInternalSourceTreeMethods
    protected:
        IResizableDirectMemory _dataMemory;
        IResizableDirectMemory _indexMemory;

        bool _useLinkedList;
        std::int64_t _dataMemoryReservationStepInBytes;
        std::int64t _indexMemoryReservationStepInBytes;

    public:
        static std::int64_t LinkDataPartSizeInBytes()
        {
            return RawLinkDataPart<LinkAddressType>.SizeInBytes();
        }

        static std::int64_t LinkIndexPartSizeInBytes()
        {
            return RawLinkIndexPart<LinkAddressType>.SizeInBytes();
        }

        static std::int64_t LinkHeaderSizeInBytes()
        {
            return LinksHeader<LinkAddressType>.SizeInBytes();
        }

        static std::int64_t DefaultLinksSizeStep()
        {
            return 1 * 1024 * 1024;
        }

        SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, std::int64_t memoryReservationStep) : this(dataMemory, indexMemory, memoryReservationStep, LinksConstants<LinkAddressType>{}, true)
        {

        }

        SplitMemoryLinks(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, std::int64_t memoryReservationStep, LinksConstants<LinkAddressType> constants, IndexTreeType indexTreeType, bool useLinkedList) : _dataMemory{ dataMemory }, _indexMemory{ indexMemory }, _memoryReservationStep{ memoryReservationStep }, _constants{ constants }, _indexTreeType{ indexTreeType }, _useLinkedList{ useLinkedList }
        {
            if(indexTreeType == IndexTreeType::SizeBalancedTree)
            {
                // TODO
                _createInternalSourceTreeMethods { [](){ return InternalLinksSourcesSizeBalancedTreeMethods<LinkAddressType> } }
            }
            Init(dataMemory, indexMemory);
        }

        void Init(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
        {
            if(indexMemory.ReservedCapacity() < LinkHeaderSizeInBytes)
            {
                indexMemory.ReservedCapacity(LinkHeaderSizeInBytes);
            }
            SetPointers(dataMemory, indexMemory);
            auto header { GetHeaderReference() };
            auto allocatedLinks { static_cast<std::int64_t>(header.AllocatedLinks()) };
            // Adjust reserved capacity
            auto minimumDataReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if(minimumDataReservedCapacity < dataMemory.UsedCapacity())
            {
                minimumDataReservedCapacity = dataMemory.UsedCapacity();
            }
            if(minimumDataReservedCapacity < _dataMemoryReservationStepInBytes)
            {
                minimumDataReservedCapacity = _dataMemoryReservationStepInBytes;
            }
            auto minimumIndexReservedCapacity { allocatedLinks * LinkDataPartSizeInBytes };
            if (minimumIndexReservedCapacity < indexMemory.UsedCapacity)
            {
                minimumIndexReservedCapacity = indexMemory.UsedCapacity;
            }
            if (minimumIndexReservedCapacity < _indexMemoryReservationStepInBytes)
            {
                minimumIndexReservedCapacity = _indexMemoryReservationStepInBytes;
            }
            // Check for alignment
            if (minimumDataReservedCapacity % _dataMemoryReservationStepInBytes > 0)
            {
                minimumDataReservedCapacity = ((minimumDataReservedCapacity / _dataMemoryReservationStepInBytes) * _dataMemoryReservationStepInBytes) + _dataMemoryReservationStepInBytes;
            }
            if (minimumIndexReservedCapacity % _indexMemoryReservationStepInBytes > 0)
            {
                minimumIndexReservedCapacity = ((minimumIndexReservedCapacity / _indexMemoryReservationStepInBytes) * _indexMemoryReservationStepInBytes) + _indexMemoryReservationStepInBytes;
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

        LinkAddressType Count(Interfaces::CList<LinkAddressType> auto&& restriction)
        {
            auto length { std::ranges::size(restriction) };
            if(0 == length)
            {
                return Total;
            }
            var any = Constants.Any;
            var index = GetIndex(thes, restriction);
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
                var value = restriction[1];
                if (any == index)
                {
                    if (value == any))
                    {
                        return Total; // Any - как отсутствие ограничения
                    }
                    var externalReferencesRange = constants.ExternalReferencesRange;
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value))
                    {
                        return Add(ExternalSourcesTreeMethods.CountUsages(value), ExternalTargetsTreeMethods.CountUsages(value));
                    }
                    else
                    {
                        if (_useLinkedList)
                        {
                            return Add(InternalSourcesListMethods.CountUsages(value), InternalTargetsTreeMethods.CountUsages(value));
                        }
                        else
                        {
                            return Add(InternalSourcesTreeMethods.CountUsages(value), InternalTargetsTreeMethods.CountUsages(value));
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
                auto source { GetSource(thes, restriction) };
                auto target { GetTarget(thes, restriction) };
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
//        void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
//        {
//            _linksDataParts = (byte*)dataMemory.Pointer;
//            _linksIndexParts = (byte*)indexMemory.Pointer;
//            _header = _linksIndexParts;
//            if (_useLinkedList)
//            {
//                InternalSourcesListMethods = new InternalLinksSourcesLinkedListMethods<LinkAddressType>(Constants, _linksDataParts, _linksIndexParts);
//            }
//            else
//            {
//                InternalSourcesTreeMethods = _createInternalSourceTreeMethods();
//            }
//            ExternalSourcesTreeMethods = _createExternalSourceTreeMethods();
//            InternalTargetsTreeMethods = _createInternalTargetTreeMethods();
//            ExternalTargetsTreeMethods = _createExternalTargetTreeMethods();
//            UnusedLinksListMethods = new UnusedLinksListMethods<LinkAddressType>(_linksDataParts, _header);
//        }
//
//        void ResetPointers()
//        {
//            base.ResetPointers();
//            _linksDataParts = null;
//            _linksIndexParts = null;
//            _header = null;
//        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;
    using namespace Platform::Exceptions;

    template<typename Self, typename TLinkAddress, typename TMemory, typename TSourceTreeMethods, typename TTargetTreeMethods, typename TUnusedLinks>
    class UnitedMemoryLinksBase : public Interfaces::Polymorph<Self>
    {
    public:
        LinksConstants<TLinkAddress> Constants;

        static constexpr std::size_t LinkSizeInBytes = sizeof(RawLink<TLinkAddress>);

        static constexpr std::size_t LinkHeaderSizeInBytes = sizeof(LinksHeader<TLinkAddress>);

        static constexpr std::size_t DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

        TMemory _memory;

        const std::size_t _memoryReservationStep;

        TTargetTreeMethods* _TargetsTreeMethods;

        TSourceTreeMethods* _SourcesTreeMethods;

        TUnusedLinks* _UnusedLinksListMethods;

        TLinkAddress GetTotal() const
        {
            auto& header = GetHeaderReference();
            return Subtract(header.AllocatedLinks, header.FreeLinks);
        }

    protected:
        UnitedMemoryLinksBase(TMemory memory, std::int64_t memoryReservationStep) :
            _memory{std::move(memory)}, _memoryReservationStep{memoryReservationStep}, Constants{LinksConstants<TLinkAddress>{}}
        {
        }

        UnitedMemoryLinksBase(TMemory memory, std::int64_t memoryReservationStep, LinksConstants<TLinkAddress> Constants) :
            _memory{std::move(memory)}, _memoryReservationStep{memoryReservationStep}, Constants{Constants}
        {
        }

        void Init(TMemory& memory, std::size_t memoryReservationStep)
        {
            if (memory.ReservedCapacity() < memoryReservationStep)
            {
                memory.ReservedCapacity(memoryReservationStep);
            }
            SetPointers(memory);
            auto& header = GetHeaderReference();
            memory.UsedCapacity(header.AllocatedLinks * LinkSizeInBytes) + LinkHeaderSizeInBytes);
            header.ReservedLinks = (memory.ReservedCapacity() - LinkHeaderSizeInBytes) / LinkSizeInBytes;
        }

    public:
        TLinkAddress Count(Interfaces::CArray auto&& restriction) const
        {
            auto restrictionLength{std::ranges::size(restriction)};
            if (0 == restrictionLength)
            {
                return GetTotal();
            }
            auto any = Constants.Any;
            auto index = restriction[Constants.IndexPart];
            if (1 == restrictionLength)
            {
                if (any == index)
                {
                    return GetTotal();
                }
                return Exists(index) ? GetOne() : GetZero();
            }
            if (2 == restrictionLength)
            {
                auto value = restriction[1];
                if (any == index)
                {
                    if (any == value)
                    {
                        return GetTotal();// Any - как отсутствие ограничения
                    }
                    return _SourcesTreeMethods->CountUsages(value) + _TargetsTreeMethods->CountUsages(value);
                }
                else
                {
                    if (!Exists(index))
                    {
                        return GetZero();
                    }
                    if (any == value)
                    {
                        return GetOne();
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (value == storedLinkValue.Source || value == storedLinkValue.Target)
                    {
                        return GetOne();
                    }
                    return GetZero();
                }
            }
            if (3 == restrictionLength)
            {
                auto source = restriction[Constants.SourcePart];
                auto target = restriction[Constants.TargetPart];
                if (any == index)
                {
                    if (any == source && any == target)
                    {
                        return GetTotal();
                    }
                    else if (any == source)
                    {
                        return _TargetsTreeMethods->CountUsages(target);
                    }
                    else if (any == target)
                    {
                        return _SourcesTreeMethods->CountUsages(source);
                    }
                    else// if(source != Any && target != Any)
                    {
                        auto link = _SourcesTreeMethods->Search(source, target);
                        return Constants.Null == link ? GetZero() : GetOne();
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return GetZero();
                    }
                    if (any == source && any == target)
                    {
                        return GetOne();
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if ((any != source && !any == target)
                    {
                        if (source == storedLinkValue.Source && target == storedLinkValue.Target)
                        {
                            return GetOne();
                        }
                        return GetZero();
                    }
                    TLinkAddress value;
                    if (any == source)
                    {
                        value = target;
                    }
                    if (any == target)
                    {
                        value = source;
                    }
                    if (value == storedLinkValue.Source || value == storedLinkValue.Target)
                    {
                        return GetOne();
                    }
                    return GetZero();
                }
            }
            NotSupportedException(/*"Другие размеры и способы ограничений не поддерживаются."*/);
        }

        TLinkAddress Each(auto&& handler, Interfaces::CArray auto&& restriction) const
        {
            auto $break = Constants.Break;
            if (0 == restrictionLength)
            {
                for (auto link = 1; link <= GetHeaderReference().AllocatedLinks; ++link)
                {
                    if (Exists(link) && $break == handler(GetLinkStruct(link)))
                    {
                        return $break;
                    }
                }
                return $break;
            }
            auto any = Constants.Any;
            auto index = restriction[Constants.IndexPart];
            if (1 == restrictionLength)
            {
                if (any == index)
                {
                    return Each(this, handler);
                }
                if (!Exists(index))
                {
                    return $continue;
                }
                return handler(GetLinkStruct(index));
            }
            if (2 == restrictionLength)
            {
                auto value = restriction[1];
                if (any == index)
                {
                    if (any == value)
                    {
                        return Each(this, handler);
                    }
                    if (Link<TLinkAddress>(index, value, any == Each(handler), $break))
                    {
                        return $break;
                    }
                    return Each(std::array(index, any, value), handler);
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
                    }
                    if (any == value)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (value == storedLinkValue.Source || value == storedLinkValue.Target)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return $continue;
                }
            }
            if (3 == restrictionLength)
            {
                auto source = restriction[Constants.SourcePart];
                auto target = restriction[Constants.TargetPart];
                if (any == index)
                {
                    if (any == source && any == target)
                    {
                        return Each(this, handler);
                    }
                    else if (any == source)
                    {
                        return _TargetsTreeMethods->EachUsage(target, handler);
                    }
                    else if (any == target)
                    {
                        return _SourcesTreeMethods->EachUsage(source, handler);
                    }
                    else// if(source != Any && target != Any)
                    {
                        auto link = _SourcesTreeMethods->Search(source, target);
                        return Constants.Null == link ? $continue : handler(GetLinkStruct(link));
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
                    }
                    if (any == source && any == target)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    // TODO: 'ref locals' are not converted by C# to C++ Converter:
                    // ORIGINAL LINE: ref var storedLinkValue = ref GetLinkReference(index);
                    auto& storedLinkValue = GetLinkReference(index);
                    if (source != any && target != any)
                    {
                        if (storedLinkValue.Source == source && storedLinkValue.Target == target)
                        {
                            return handler(GetLinkStruct(index));
                        }
                        return $continue;
                    }
                    auto value = TLinkAddress();
                    if (any == source)
                    {
                        value = target;
                    }
                    if (any == target)
                    {
                        value = source;
                    }
                    if (storedLinkValue.Source == value || storedLinkValue.Target == value)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return $continue;
                }
            }
            NotSupportedException(/*"Другие размеры и способы ограничений не поддерживаются."*/);
        }

        // / <remarks>
        // / TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        // / </remarks>
        // NOTE: The following .NET attribute has no direct equivalent in C++:
        // ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public TLinkAddress Update(IList<TLinkAddress> restriction, IList<TLinkAddress> substitution)
        TLinkAddress Update(Interfaces::CArray auto&& restriction, Interfaces::CArray auto&& substitution)
        {
            auto null = Constants.Null;
            auto linkIndex = restriction[Constants.IndexPart];
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var link = ref GetLinkReference(linkIndex);
            auto& link = GetLinkReference(linkIndex);
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var header = ref GetHeaderReference();
            auto& header = GetHeaderReference();
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var firstAsSource = ref header.RootAsSource;
            auto& firstAsSource = header.RootAsSource;
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var firstAsTarget = ref header.RootAsTarget;
            auto& firstAsTarget = header.RootAsTarget;
            // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
            if ((null != link.Source)
            {
                _SourcesTreeMethods->Detach(firstAsSource, linkIndex);
            }
            if ((null != link.Target)
            {
                _TargetsTreeMethods->Detach(firstAsTarget, linkIndex);
            }
            link.Source = substitution[Constants.SourcePart];
            link.Target = substitution[Constants.TargetPart];
            if ((null != link.Source)
            {
                _SourcesTreeMethods->Attach(firstAsSource, linkIndex);
            }
            if ((null != link.Target)
            {
                _TargetsTreeMethods->Attach(firstAsTarget, linkIndex);
            }
            return linkIndex;
        }

        // TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
        TLinkAddress Create(auto&& restriction)
        {
            auto& header = GetHeaderReference();
            auto freeLink = header.FirstFreeLink;
            if ((Constants.Null != freeLink)
            {
                _UnusedLinksListMethods->Detach(freeLink);
            }
            else
            {
                auto maximumPossibleInnerReference = Constants.InternalReferencesRange.Maximum;
                if (GreaterThan(header.AllocatedLinks, maximumPossibleInnerReference))
                {
                    // TODO: !!!!!
                    // throw std::make_shared<LinksLimitReachedException<TLinkAddress>>(maximumPossibleInnerReference);
                }
                if (GreaterOrEqualThan(header.AllocatedLinks, Decrement(header.ReservedLinks)))
                {
                    _memory.ReservedCapacity(_memory.ReservedCapacity() + _memoryReservationStep);
                    SetPointers(_memory);
                    header = GetHeaderReference();
                    header.ReservedLinks = ConvertToAddress(_memory.ReservedCapacity() / LinkSizeInBytes);
                }
                freeLink = header.AllocatedLinks = Increment(header.AllocatedLinks);
                _memory.UsedCapacity(_memory.UsedCapacity() + LinkSizeInBytes);
            }
            return freeLink;
        }

        auto Delete(auto&& restriction)
        {
            auto& header = GetHeaderReference();
            auto link = restriction[Constants.IndexPart];
            if (LessThan(link, header.AllocatedLinks))
            {
                _UnusedLinksListMethods->AttachAsFirst(link);
            }
            else if (header.AllocatedLinks == link)
            {
                header.AllocatedLinks = header.AllocatedLinks - 1;
                _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                while (GreaterThan(header.AllocatedLinks, GetZero()) && IsUnusedLink(header.AllocatedLinks))
                {
                    _UnusedLinksListMethods->Detach(header.AllocatedLinks);
                    header.AllocatedLinks = header.AllocatedLinks - 1;
                    _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                }
            }
        }

        auto GetLinkStruct(TLinkAddress linkIndex) const
        {
            auto& link = this->object().GetLinkReference(linkIndex);
            return Link{linkIndex, link.Source, link.Target};
        }

        // TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
        //
        // Указатель this.storage может быть в том же месте,
        // так как 0-я связь не используется и имеет такой же размер как Header,
        // поэтому header размещается в том же месте, что и 0-я связь
    public:
        void SetPointers(TMemory& memory)
        {
            this->object().SetPointers(memory);
        }

    protected:
        auto&& GetHeaderReference() const
        {
            return this->object().GetHeaderReference();
        }

    protected:
        auto&& GetLinkReference(std::size_t index) const
        {
            return this->object().GetLinkReference(index);
        }

        bool Exists(TLinkAddress link)
        {
            this->object().Exists(link);
        }

        bool IsUnusedLink(TLinkAddress linkIndex)
        {
            this->object().IsUnusedLink(linkIndex);
        }
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

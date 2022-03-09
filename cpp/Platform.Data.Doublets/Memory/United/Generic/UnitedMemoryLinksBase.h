namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;
    using namespace Platform::Exceptions;

    template<
        typename TSelf,
        typename TLinkAddress,
        typename TMemory,
        typename TSourceTreeMethods,
        typename TTargetTreeMethods,
        typename TUnusedLinks,
        typename... TBase>
    class UnitedMemoryLinksBase : public Interfaces::Polymorph<TSelf, TBase...>
    {
    public:
        LinksConstants<TLinkAddress> Constants;

    public:
        static constexpr std::size_t LinkSizeInBytes = sizeof(RawLink<TLinkAddress>);

        static constexpr std::size_t LinkHeaderSizeInBytes = sizeof(LinksHeader<TLinkAddress>);

        static constexpr std::size_t DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

    public:
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

    public:
    protected:
        UnitedMemoryLinksBase(TMemory memory, std::int64_t memoryReservationStep, LinksConstants<TLinkAddress> constants = {}) :
            _memory(std::move(memory)), _memoryReservationStep(memoryReservationStep), Constants(constants)
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
            memory.UsedCapacity((ConvertToInt64(header.AllocatedLinks) * LinkSizeInBytes) + LinkHeaderSizeInBytes);
            header.ReservedLinks = (memory.ReservedCapacity() - LinkHeaderSizeInBytes) / LinkSizeInBytes;
        }

    public:
        TLinkAddress Count(Interfaces::CArray<TLinkAddress> auto&& restrictions) const
        {
            if (std::ranges::size(restrictions) == 0)
            {
                return GetTotal();
            }
            auto constants = Constants;
            auto any = constants.Any;
            auto index = restrictions[constants.IndexPart];
            if (std::ranges::size(restrictions) == 1)
            {
                if (AreEqual(index, any))
                {
                    return GetTotal();
                }
                return Exists(index) ? GetOne() : GetZero();
            }
            if (std::ranges::size(restrictions) == 2)
            {
                auto value = restrictions[1];
                if (AreEqual(index, any))
                {
                    if (AreEqual(value, any))
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
                    if (value == any)
                    {
                        return GetOne();
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (storedLinkValue.Source == value || storedLinkValue.Target == value)
                    {
                        return GetOne();
                    }
                    return GetZero();
                }
            }
            if (std::ranges::size(restrictions) == 3)
            {
                auto source = restrictions[constants.SourcePart];
                auto target = restrictions[constants.TargetPart];
                if (AreEqual(index, any))
                {
                    if (source == any && target == any)
                    {
                        return GetTotal();
                    }
                    else if (source == any)
                    {
                        return _TargetsTreeMethods->CountUsages(target);
                    }
                    else if (target == any)
                    {
                        return _SourcesTreeMethods->CountUsages(source);
                    }
                    else// if(source != Any && target != Any)
                    {
                        auto link = _SourcesTreeMethods->Search(source, target);
                        return AreEqual(link, constants.Null) ? GetZero() : GetOne();
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return GetZero();
                    }
                    if (source == any && target == any)
                    {
                        return GetOne();
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (!source == any && !target == any)
                    {
                        if (AreEqual(storedLinkValue.Source, source) && AreEqual(storedLinkValue.Target, target))
                        {
                            return GetOne();
                        }
                        return GetZero();
                    }
                    auto value = TLinkAddress();
                    if (source == any)
                    {
                        value = target;
                    }
                    if (target == any)
                    {
                        value = source;
                    }
                    if (AreEqual(storedLinkValue.Source, value) || AreEqual(storedLinkValue.Target, value))
                    {
                        return GetOne();
                    }
                    return GetZero();
                }
            }
            NotSupportedException(/*"Другие размеры и способы ограничений не поддерживаются."*/);
        }

        TLinkAddress Each(Interfaces::CArray<TLinkAddress> auto&& restrictions, auto&& handler) const
        {
            auto constants = Constants;
            auto $break = constants.Break;
            if (std::ranges::size(restrictions) == 0)
            {
                for (auto link = GetOne(); LessOrEqualThan(link, GetHeaderReference().AllocatedLinks); link = Increment(link))
                {
                    if (Exists(link) && AreEqual(handler(GetLinkStruct(link)), $break))
                    {
                        return $break;
                    }
                }
                return $break;
            }
            auto _continue = constants.Continue;
            auto any = constants.Any;
            auto index = restrictions[constants.IndexPart];
            if (std::ranges::size(restrictions) == 1)
            {
                if (AreEqual(index, any))
                {
                    return Each(handler, std::array<TLinkAddress, 0>{});
                }
                if (!Exists(index))
                {
                    return _continue;
                }
                return handler(GetLinkStruct(index));
            }
            if (std::ranges::size(restrictions) == 2)
            {
                auto value = restrictions[1];
                if (AreEqual(index, any))
                {
                    if (AreEqual(value, any))
                    {
                        return Each(handler, std::array<TLinkAddress, 0>{});
                    }
                    if (AreEqual(Each(handler, Link<TLinkAddress>(index, value, any)), $break))
                    {
                        return $break;
                    }
                    return Each(handler, Link<TLinkAddress>(index, any, value));
                }
                else
                {
                    if (!Exists(index))
                    {
                        return _continue;
                    }
                    if (AreEqual(value, any))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (AreEqual(storedLinkValue.Source, value) || AreEqual(storedLinkValue.Target, value))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return _continue;
                }
            }
            if (std::ranges::size(restrictions) == 3)
            {
                auto source = restrictions[constants.SourcePart];
                auto target = restrictions[constants.TargetPart];
                if (AreEqual(index, any))
                {
                    if (source == any && target == any)
                    {
                        return Each(handler, std::array<TLinkAddress, 0>{});
                    }
                    else if (source == any)
                    {
                        return _TargetsTreeMethods->EachUsage(target, handler);
                    }
                    else if (target == any)
                    {
                        return _SourcesTreeMethods->EachUsage(source, handler);
                    }
                    else// if(source != Any && target != Any)
                    {
                        auto link = _SourcesTreeMethods->Search(source, target);
                        return AreEqual(link, constants.Null) ? _continue : handler(GetLinkStruct(link));
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return _continue;
                    }
                    if (source == any && target == any)
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
                        return _continue;
                    }
                    auto value = TLinkAddress();
                    if (source == any)
                    {
                        value = target;
                    }
                    if (target == any)
                    {
                        value = source;
                    }
                    if (storedLinkValue.Source == value || storedLinkValue.Target == value)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return _continue;
                }
            }
            NotSupportedException(/*"Другие размеры и способы ограничений не поддерживаются."*/);
        }

        // / <remarks>
        // / TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        // / </remarks>
        // NOTE: The following .NET attribute has no direct equivalent in C++:
        // ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public TLinkAddress Update(IList<TLinkAddress> restrictions, IList<TLinkAddress> substitution)
        TLinkAddress Update(Interfaces::CArray<TLinkAddress> auto&& restrictions, Interfaces::CArray<TLinkAddress> auto&& substitution, auto&& handler)
        {
            auto constants = Constants;
            auto null = constants.Null;
            auto linkIndex = restrictions[constants.IndexPart];
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var link = ref GetLinkReference(linkIndex);
            auto& link = GetLinkReference(linkIndex);
            auto before = link;
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
            if (!AreEqual(link.Source, null))
            {
                _SourcesTreeMethods->Detach(firstAsSource, linkIndex);
            }
            if (!AreEqual(link.Target, null))
            {
                _TargetsTreeMethods->Detach(firstAsTarget, linkIndex);
            }
            link.Source = substitution[constants.SourcePart];
            link.Target = substitution[constants.TargetPart];
            if (!AreEqual(link.Source, null))
            {
                _SourcesTreeMethods->Attach(firstAsSource, linkIndex);
            }
            if (!AreEqual(link.Target, null))
            {
                _TargetsTreeMethods->Attach(firstAsTarget, linkIndex);
            }
            return handler(before, std::array{link.Index, link.Source, link.Target});
        }

        // TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
        TLinkAddress Create(auto&& restrictions, auto&& handler)
        {
            auto& header = GetHeaderReference();
            auto freeLink = header.FirstFreeLink;
            if (!AreEqual(freeLink, Constants.Null))
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
            return handler(NULL, std::array{freeLink, 0, 0});
        }

        auto Delete(auto&& restrictions, auto&& handler)
        {
            auto& header = GetHeaderReference();
            auto linkAddress = restrictions[Constants.IndexPart];
            auto before = GetLink(this, linkAddress);
            if (LessThan(linkAddress, header.AllocatedLinks))
            {
                _UnusedLinksListMethods->AttachAsFirst(linkAddress);
            }
            else if (AreEqual(linkAddress, header.AllocatedLinks))
            {
                header.AllocatedLinks = Decrement(header.AllocatedLinks);
                _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                while (GreaterThan(header.AllocatedLinks, GetZero()) && IsUnusedLink(header.AllocatedLinks))
                {
                    _UnusedLinksListMethods->Detach(header.AllocatedLinks);
                    header.AllocatedLinks = Decrement(header.AllocatedLinks);
                    _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                }
            }
            return handler(before, NULL);
        }

        auto GetLinkStruct(TLinkAddress linkIndex) const
        {
            auto& link = this->GetLinkReference(linkIndex);
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
            if (IsExternalReference(Constants, link))
            {
                return false;
            }
            return GreaterOrEqualThan(link, Constants.InternalReferencesRange.Minimum) && LessOrEqualThan(link, GetHeaderReference().AllocatedLinks) && !IsUnusedLink(link);
        }

        bool IsUnusedLink(TLinkAddress linkIndex)
        {
            if (!AreEqual(GetHeaderReference().FirstFreeLink, linkIndex))// May be this check is not needed
            {
                auto& link = GetLinkReference(linkIndex);
                return AreEqual(link.SizeAsSource, {}) && !AreEqual(link.Source, {});
            }
            else
            {
                return true;
            }
        }

        static TLinkAddress GetOne()
        {
            return TLinkAddress{1};
        }

        static TLinkAddress GetZero()
        {
            return TLinkAddress{0};
        }

        static bool AreEqual(TLinkAddress first, TLinkAddress second)
        {
            return first == second;
        }

        static bool LessThan(TLinkAddress first, TLinkAddress second)
        {
            return first < second;
        }

        static bool LessOrEqualThan(TLinkAddress first, TLinkAddress second)
        {
            return first <= second;
        }

        static bool GreaterThan(TLinkAddress first, TLinkAddress second)
        {
            return first > second;
        }

        static bool GreaterOrEqualThan(TLinkAddress first, TLinkAddress second)
        {
            return first >= second;
        }

        static std::int64_t ConvertToInt64(TLinkAddress value)
        {
            return value;
        }

        static TLinkAddress ConvertToAddress(std::int64_t value)
        {
            return value;
        }

        static TLinkAddress Add(TLinkAddress first, TLinkAddress second)
        {
            return first + second;
        }

        static TLinkAddress Subtract(TLinkAddress first, TLinkAddress second)
        {
            return first - second;
        }

        static TLinkAddress Increment(TLinkAddress link)
        {
            return link + 1;
        }

        static TLinkAddress Decrement(TLinkAddress link)
        {
            return link - 1;
        }
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

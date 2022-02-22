namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;
    using namespace Platform::Exceptions;

    template<
        typename Self, 
        typename TLink,
        typename TMemory,
        typename TSourceTreeMethods,
        typename TTargetTreeMethods, 
        typename TUnusedLinks
    >
    class UnitedMemoryLinksBase 
        : public ILinks<Self, TLink>,
          public Interfaces::Polymorph<Self>
    {
    public:
        LinksConstants<TLink> Constants;
        public: using Interfaces::Polymorph<Self>::self;

    public:
        static constexpr std::size_t LinkSizeInBytes = sizeof(RawLink<TLink>);

        static constexpr std::size_t LinkHeaderSizeInBytes = sizeof(LinksHeader<TLink>);

        static constexpr std::size_t DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

    public:
        using ILinks<Self, TLink>::Create;
        using ILinks<Self, TLink>::Count;
        using ILinks<Self, TLink>::Update;
        using ILinks<Self, TLink>::Delete;
        using ILinks<Self, TLink>::GetLink;
        using ILinks<Self, TLink>::Exists;

        TMemory _memory;

        const std::size_t _memoryReservationStep;

        TTargetTreeMethods* _TargetsTreeMethods;

        TSourceTreeMethods* _SourcesTreeMethods;

        TUnusedLinks* _UnusedLinksListMethods;

        TLink GetTotal() const
        {
            auto& header = GetHeaderReference();
            return Subtract(header.AllocatedLinks, header.FreeLinks);
        }

    public:

    protected:
        UnitedMemoryLinksBase(TMemory memory, std::int64_t memoryReservationStep, LinksConstants<TLink> constants = {})
            : _memory(std::move(memory)), _memoryReservationStep(memoryReservationStep), Constants(constants) {}

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
        TLink Count(Interfaces::CArray auto&& restrictions) const
        {
            if (std::ranges::size(restrictions)  == 0)
            {
                return GetTotal();
            }
            auto constants = Constants;
            auto any = constants.Any;
            auto index = restrictions[constants.IndexPart];
            if (std::ranges::size(restrictions)  == 1)
            {
                if (AreEqual(index, any))
                {
                    return GetTotal();
                }
                return Exists(index) ? GetOne() : GetZero();
            }
            if (std::ranges::size(restrictions)  == 2)
            {
                auto value = restrictions[1];
                if (AreEqual(index, any))
                {
                    if (AreEqual(value, any))
                    {
                        return GetTotal(); // Any - как отсутствие ограничения
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
                    else // if(source != Any && target != Any)
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
                    auto value = TLink();
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


        TLink Each(auto&& handler, Interfaces::CArray auto&& restrictions) const
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
            auto $continue = constants.Continue;
            auto any = constants.Any;
            auto index = restrictions[constants.IndexPart];
            if (std::ranges::size(restrictions) == 1)
            {
                if (AreEqual(index, any))
                {
                    return Each(handler, std::array<TLink, 0>{});
                }
                if (!Exists(index))
                {
                    return $continue;
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
                        return Each(handler, std::array<TLink, 0>{});
                    }
                    if (AreEqual(Each(handler, Link<TLink>(index, value, any)), $break))
                    {
                        return $break;
                    }
                    return Each(handler, Link<TLink>(index, any, value));
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
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
                    return $continue;
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
                        return Each(handler, std::array<TLink, 0>{});
                    }
                    else if (source == any)
                    {
                        return _TargetsTreeMethods->EachUsage(target, handler);
                    }
                    else if (target == any)
                    {
                        return _SourcesTreeMethods->EachUsage(source, handler);
                    }
                    else // if(source != Any && target != Any)
                    {
                        auto link = _SourcesTreeMethods->Search(source, target);
                        return AreEqual(link, constants.Null) ? $continue : handler(GetLinkStruct(link));
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return $continue;
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
                        return $continue;
                    }
                    auto value = TLink();
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
                    return $continue;
                }
            }
            NotSupportedException(/*"Другие размеры и способы ограничений не поддерживаются."*/);
        }

        // / <remarks>
        // / TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        // / </remarks>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public TLink Update(IList<TLink> restrictions, IList<TLink> substitution)
        TLink Update(Interfaces::CArray auto&& restrictions, Interfaces::CArray auto&& substitution)
        {
            auto constants = Constants;
            auto null = constants.Null;
            auto linkIndex = restrictions[constants.IndexPart];
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
            return linkIndex;
        }

        // TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
        TLink Create(auto&& restrictions)
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
                    // throw std::make_shared<LinksLimitReachedException<TLink>>(maximumPossibleInnerReference);
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

        auto Delete(auto&& restrictions)
        {
            auto& header = GetHeaderReference();
            auto link = restrictions[Constants.IndexPart];
            if (LessThan(link, header.AllocatedLinks))
            {
                _UnusedLinksListMethods->AttachAsFirst(link);
            }
            else if (AreEqual(link, header.AllocatedLinks))
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
        }

        auto GetLinkStruct(TLink linkIndex) const
        {
            auto& link = this->GetLinkReference(linkIndex);
            return Link { linkIndex, link.Source, link.Target };
        }

        // TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
        // 
        // Указатель this.links может быть в том же месте, 
        // так как 0-я связь не используется и имеет такой же размер как Header,
        // поэтому header размещается в том же месте, что и 0-я связь
    public:
        void SetPointers(TMemory& memory) { self().SetPointers(memory); }

        protected: auto&& GetHeaderReference() const { return self().GetHeaderReference(); }

        protected: auto&& GetLinkReference(std::size_t index) const { return self().GetLinkReference(index); }

        bool Exists(TLink link)
        {
            if (IsExternalReference(Constants, link)) {
                return false;
            }
            return GreaterOrEqualThan(link, Constants.InternalReferencesRange.Minimum) && LessOrEqualThan(link, GetHeaderReference().AllocatedLinks) && !IsUnusedLink(link);
        }

        bool IsUnusedLink(TLink linkIndex)
        {
            if (!AreEqual(GetHeaderReference().FirstFreeLink, linkIndex)) // May be this check is not needed
            {
                auto& link = GetLinkReference(linkIndex);
                return AreEqual(link.SizeAsSource, {}) && !AreEqual(link.Source, {});
            }
            else
            {
                return true;
            }
        }

        static TLink GetOne()
        {
            return TLink{1};
        }

        static TLink GetZero()
        {
            return TLink{0};
        }

        static bool AreEqual(TLink first, TLink second)
        {
            return first == second;
        }

        static bool LessThan(TLink first, TLink second)
        {
            return first < second;
        }

        static bool LessOrEqualThan(TLink first, TLink second)
        {
            return first <= second;
        }

        static bool GreaterThan(TLink first, TLink second)
        {
            return first > second;
        }

        static bool GreaterOrEqualThan(TLink first, TLink second)
        {
            return first >= second;
        }

        static std::int64_t ConvertToInt64(TLink value)
        {
            return value;
        }

        static TLink ConvertToAddress(std::int64_t value)
        {
            return value;
        }

        static TLink Add(TLink first, TLink second)
        {
            return first + second;
        }

        static TLink Subtract(TLink first, TLink second)
        {
            return first - second;
        }

        static TLink Increment(TLink link)
        {
            return link + 1;
        }

        static TLink Decrement(TLink link)
        {
            return link - 1;
        }
    };
}

namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Interfaces;
    using namespace Platform::Memory;
    using namespace Platform::Exceptions;

    template<
        typename TSelf,
        typename TLinkAddress,
        typename TMemory,
        LinksConstants<TLinkAddress> VConstants,
        CArray<TLinkAddress> THandlerParameter,
        typename TSourceTreeMethods,
        typename TTargetTreeMethods,
        typename TUnusedLinks,
        typename... TBase>
    struct UnitedMemoryLinksBase : public Interfaces::Polymorph<TSelf, TBase...>
    {
    private:
        static constexpr auto null = Link<TLinkAddress>::Null;
    public:
        typedef TLinkAddress LinkAddressType;
        typedef THandlerParameter HandlerParameterType;
        static constexpr LinksConstants<TLinkAddress> Constants = VConstants;

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
            return header.AllocatedLinks - header.FreeLinks;
        }

    public:
    protected:
        UnitedMemoryLinksBase(TMemory memory, std::int64_t memoryReservationStep) :
            _memory(std::move(memory)), _memoryReservationStep(memoryReservationStep)
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
            memory.UsedCapacity((header.AllocatedLinks * LinkSizeInBytes) + LinkHeaderSizeInBytes);
            header.ReservedLinks = (memory.ReservedCapacity() - LinkHeaderSizeInBytes) / LinkSizeInBytes;
        }

    public:
        TLinkAddress Count(const  LinkType& restriction) const
        {
            if (std::ranges::size(restriction) == 0)
            {
                return GetTotal();
            }
            auto constants = Constants;
            auto any = constants.Any;
            auto index = restriction[constants.IndexPart];
            if (std::ranges::size(restriction) == 1)
            {
                if (index == any)
                {
                    return GetTotal();
                }
                return Exists(index) ? TLinkAddress {1} : TLinkAddress {};
            }
            if (std::ranges::size(restriction) == 2)
            {
                auto value = restriction[1];
                if (index == any)
                {
                    if (value == any)
                    {
                        return GetTotal();// Any - как отсутствие ограничения
                    }
                    return _SourcesTreeMethods->CountUsages(value) + _TargetsTreeMethods->CountUsages(value);
                }
                else
                {
                    if (!Exists(index))
                    {
                        return TLinkAddress {};
                    }
                    if (value == any)
                    {
                        return TLinkAddress {1};
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (storedLinkValue.Source == value || storedLinkValue.Target == value)
                    {
                        return TLinkAddress {1};
                    }
                    return TLinkAddress {};
                }
            }
            if (std::ranges::size(restriction) == 3)
            {
                auto source = restriction[constants.SourcePart];
                auto target = restriction[constants.TargetPart];
                if (index == any)
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
                        return link == constants.Null ? TLinkAddress {} : TLinkAddress {1};
                    }
                }
                else
                {
                    if (!Exists(index))
                    {
                        return TLinkAddress {};
                    }
                    if (source == any && target == any)
                    {
                        return TLinkAddress {1};
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if (!source == any && !target == any)
                    {
                        if ((storedLinkValue.Source == source) && (storedLinkValue.Target == target))
                        {
                            return TLinkAddress {1};
                        }
                        return TLinkAddress {};
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
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return TLinkAddress {1};
                    }
                    return TLinkAddress {};
                }
            }
            throw std::logic_error("Not supported exception.");
        }

    public: TLinkAddress Each(const  LinkType& restriction, const ReadHandlerType& handler) const
        {
            auto constants = Constants;
            auto $break = constants.Break;
            if (std::ranges::size(restriction) == 0)
            {
                for (auto link = TLinkAddress {1}; link <= GetHeaderReference().AllocatedLinks; ++link)
                {
                    if (Exists(link) && (handler(GetLinkStruct(link)) == $break))
                    {
                        return $break;
                    }
                }
                return $break;
            }
            auto _continue = constants.Continue;
            auto any = constants.Any;
            auto index = restriction[constants.IndexPart];
            if (std::ranges::size(restriction) == 1)
            {
                if (index == any)
                {
                    return Data::Each(*this, handler);
                }
                if (!Exists(index))
                {
                    return _continue;
                }
                return handler(GetLinkStruct(index));
            }
            if (std::ranges::size(restriction) == 2)
            {
                auto value = restriction[1];
                if (index == any)
                {
                    if (value == any)
                    {
                        return Data::Each(*this, handler);
                    }
                    if (Each(std::array{index, value, any}, handler) == $break)
                    {
                        return $break;
                    }
                    return Each(std::array{index, any, value}, handler);
                }
                else
                {
                    if (!Exists(index))
                    {
                        return _continue;
                    }
                    if (value == any)
                    {
                        return handler(GetLinkStruct(index));
                    }
                    auto& storedLinkValue = GetLinkReference(index);
                    if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
                    {
                        return handler(GetLinkStruct(index));
                    }
                    return _continue;
                }
            }
            if (std::ranges::size(restriction) == 3)
            {
                auto source = restriction[constants.SourcePart];
                auto target = restriction[constants.TargetPart];
                if (index == any)
                {
                    if (source == any && target == any)
                    {
                        return Data::Each(*this, handler);
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
                        return (link == constants.Null) ? _continue : handler(GetLinkStruct(link));
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
            throw std::logic_error("Not supported exception.");
        }

        // / <remarks>
        // / TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в менеджере памяти, а в логике Links)
        // / </remarks>
        // NOTE: The following .NET attribute has no direct equivalent in C++:
        // ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public TLinkAddress Update(IList<TLinkAddress> restriction, IList<TLinkAddress> substitution)
    public:
        TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler)
        {
            auto constants = Constants;
            auto null = constants.Null;
            auto linkIndex = restriction[constants.IndexPart];
            // TODO: 'ref locals' are not converted by C# to C++ Converter:
            // ORIGINAL LINE: ref var link = ref GetLinkReference(linkIndex);
            auto& link = GetLinkReference(linkIndex);
            auto before = std::array{linkIndex, link.Source, link.Target};
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
            if (link.Source != null)
            {
                _SourcesTreeMethods->Detach(firstAsSource, linkIndex);
            }
            if (link.Target != null)
            {
                _TargetsTreeMethods->Detach(firstAsTarget, linkIndex);
            }
            link.Source = substitution[constants.SourcePart];
            link.Target = substitution[constants.TargetPart];
            if (link.Source != null)
            {
                _SourcesTreeMethods->Attach(firstAsSource, linkIndex);
            }
            if (link.Target != null)
            {
                _TargetsTreeMethods->Attach(firstAsTarget, linkIndex);
            }
            return handler(before, Link{linkIndex, link.Source, link.Target});
        }

        // TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
    public:
        TLinkAddress Create(auto&& restriction, const WriteHandlerType& handler)
        {
            auto& header = GetHeaderReference();
            auto freeLink = header.FirstFreeLink;
            if (freeLink != Constants.Null)
            {
                _UnusedLinksListMethods->Detach(freeLink);
            }
            else
            {
                auto maximumPossibleInnerReference = Constants.InternalReferencesRange.Maximum;
                if (header.AllocatedLinks > maximumPossibleInnerReference)
                {
                    // TODO: !!!!!
                    // throw std::make_shared<LinksLimitReachedException<TLinkAddress>>(maximumPossibleInnerReference);
                }
                if (header.AllocatedLinks >= (header.ReservedLinks - 1))
                {
                    _memory.ReservedCapacity(_memory.ReservedCapacity() + _memoryReservationStep);
                    SetPointers(_memory);
                    header = GetHeaderReference();
                    header.ReservedLinks = _memory.ReservedCapacity() / LinkSizeInBytes;
                }
                ++header.AllocatedLinks;
                freeLink = header.AllocatedLinks;
                _memory.UsedCapacity(_memory.UsedCapacity() + LinkSizeInBytes);
            }
            return handler(null, Link{freeLink, TLinkAddress{}, TLinkAddress{}});
        }

    public:
        TLinkAddress Delete(const  LinkType& restriction, const WriteHandlerType& handler)
        {
            auto& header = GetHeaderReference();
            auto linkAddress = restriction[Constants.IndexPart];
            auto before = GetLinkStruct(linkAddress);
            if (linkAddress < header.AllocatedLinks)
            {
                _UnusedLinksListMethods->AttachAsFirst(linkAddress);
            }
            else if ((linkAddress == header.AllocatedLinks))
            {
                auto allLinksBeforeDecrementAllocatedLinks = All(*this);
                --header.AllocatedLinks;
                _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                while ((header.AllocatedLinks > TLinkAddress {}) && IsUnusedLink(header.AllocatedLinks))
                {
                    auto allLinksBeforeDetachAllocatedLinks = All(*this);
                    _UnusedLinksListMethods->Detach(header.AllocatedLinks);
                    --header.AllocatedLinks;
                    _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                    auto allLinksBeforeAfterAllocatedLinks = All(*this);
                }
            }
            return handler(before, null);
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

        bool Exists(TLinkAddress link) const
        {
            if (IsExternalReference(Constants, link))
            {
                return false;
            }
            return (link >= Constants.InternalReferencesRange.Minimum) && (link <= GetHeaderReference().AllocatedLinks) && !IsUnusedLink(link);
        }

        bool IsUnusedLink(TLinkAddress linkIndex) const
        {
            if (GetHeaderReference().FirstFreeLink != linkIndex)// May be this check is not needed
            {
                auto& link = GetLinkReference(linkIndex);
                return (link.SizeAsSource == TLinkAddress {}) && (link.Source != TLinkAddress {});
            }
            else
            {
                return true;
            }
        }
    };
}// namespace Platform::Data::Doublets::Memory::United::Generic

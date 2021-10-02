namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Memory;
    using namespace Platform::Exceptions;

    template<typename Self, typename TLink>
    class UnitedMemoryLinksBase : public ILinks<Self, TLink>, public Interfaces::Polymorph<Self>
    {
        public: using ILinks<Self, TLink>::Constants;
        public: using Interfaces::Polymorph<Self>::self;

    public:
        static constexpr std::size_t LinkSizeInBytes = sizeof(RawLink<TLink>);

        static constexpr std::size_t LinkHeaderSizeInBytes = sizeof(LinksHeader<TLink>);

        static constexpr std::size_t DefaultLinksSizeStep = LinkSizeInBytes * 1024 * 1024;

    protected:
        IResizableDirectMemory& _memory;

        const std::size_t _memoryReservationStep;

        std::unique_ptr<ILinksTreeMethods<TLink>> _TargetsTreeMethods;

        std::unique_ptr<ILinksTreeMethods<TLink>> _SourcesTreeMethods;

        std::unique_ptr<ILinksListMethods<TLink>> _UnusedLinksListMethods;

        virtual TLink GetTotal()
        {
            auto& header = GetHeaderReference();
            return Subtract(header.AllocatedLinks, header.FreeLinks);
        }

    public:

    protected:
        UnitedMemoryLinksBase(IResizableDirectMemory& memory, std::int64_t memoryReservationStep, LinksConstants<TLink> constants = {})
            : _memory(memory), _memoryReservationStep(memoryReservationStep){}

        void Init(IResizableDirectMemory& memory, std::size_t memoryReservationStep)
        {
            if (memory.ReservedCapacity() < memoryReservationStep)
            {
                memory.ReservedCapacity(memoryReservationStep);
            }
            SetPointers(memory);

            auto& header = GetHeaderReference();
            // Гарантия корректности _memory.UsedCapacity относительно _header.AllocatedLinks
            memory.UsedCapacity((ConvertToInt64(header.AllocatedLinks) * LinkSizeInBytes) + LinkHeaderSizeInBytes);
            // Гарантия корректности _header.ReservedLinks относительно _memory.ReservedCapacity
            header.ReservedLinks = (memory.ReservedCapacity() - LinkHeaderSizeInBytes) / LinkSizeInBytes;
        }

        // / <summary>
        // / <para>
        // / Counts the restrictions.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="restrictions">
        // / <para>The restrictions.</para>
        // / <para></para>
        // / </param>
        // / <exception cref="NotSupportedException">
        // / <para>Другие размеры и способы ограничений не поддерживаются.</para>
        // / <para></para>
        // / </exception>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
    public:
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public virtual TLink Count(IList<TLink> restrictions)
        TLink Count(auto&& restrictions)
        {
            // Если нет ограничений, тогда возвращаем общее число связей находящихся в хранилище.
            if (restrictions.size() == 0)
            {
                return GetTotal();
            }
            auto constants = Constants;
            auto any = constants.Any;
            auto index = restrictions[constants.IndexPart];
            if (restrictions.size() == 1)
            {
                if (AreEqual(index, any))
                {
                    return GetTotal();
                }
                return Exists(index) ? GetOne() : GetZero();
            }
            if (restrictions.size() == 2)
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
// TODO: 'ref locals' are not converted by C# to C++ Converter:
// ORIGINAL LINE: ref var storedLinkValue = ref GetLinkReference(index);
                    auto& storedLinkValue = GetLinkReference(index);
                    if (storedLinkValue.Source == value || storedLinkValue.Target == value)
                    {
                        return GetOne();
                    }
                    return GetZero();
                }
            }
            if (restrictions.size() == 3)
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
                        // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
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
// TODO: 'ref locals' are not converted by C# to C++ Converter:
// ORIGINAL LINE: ref var storedLinkValue = ref GetLinkReference(index);
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

        // / <summary>
        // / <para>
        // / Eaches the handler.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="handler">
        // / <para>The handler.</para>
        // / <para></para>
        // / </param>
        // / <param name="restrictions">
        // / <para>The restrictions.</para>
        // / <para></para>
        // / </param>
        // / <exception cref="NotSupportedException">
        // / <para>Другие размеры и способы ограничений не поддерживаются.</para>
        // / <para></para>
        // / </exception>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public virtual TLink Each(System.Func<IList<TLink>, TLink> handler, IList<TLink> restrictions)
        TLink Each(auto&& handler, auto&& restrictions)
        {
            auto constants = Constants;
            auto $break = constants.Break;
            if (restrictions.size() == 0)
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
            if (restrictions.size() == 1)
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
            if (restrictions.size() == 2)
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
            if (restrictions.size() == 3)
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
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public virtual TLink Update(IList<TLink> restrictions, IList<TLink> substitution)
        TLink Update(auto&& restrictions, auto&& substitution)
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

        // / <summary>
        // / <para>
        // / Deletes the restrictions.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="restrictions">
        // / <para>The restrictions.</para>
        // / <para></para>
        // / </param>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public virtual void Delete(IList<TLink> restrictions)
        void Delete(std::vector<TLink>& restrictions)
        {
// TODO: 'ref locals' are not converted by C# to C++ Converter:
// ORIGINAL LINE: ref var header = ref GetHeaderReference();
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
                // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
                // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
                while (GreaterThan(header.AllocatedLinks, GetZero()) && IsUnusedLink(header.AllocatedLinks))
                {
                    _UnusedLinksListMethods->Detach(header.AllocatedLinks);
                    header.AllocatedLinks = Decrement(header.AllocatedLinks);
                    _memory.UsedCapacity(_memory.UsedCapacity() - LinkSizeInBytes);
                }
            }
        }

        // / <summary>
        // / <para>
        // / Gets the link struct using the specified link index.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="linkIndex">
        // / <para>The link index.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>A list of t link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] public IList<TLink> GetLinkStruct(TLink linkIndex)
        auto GetLinkStruct(TLink linkIndex)
        {
            auto& link = this->GetLinkReference(linkIndex);
            return Link<TLink>(linkIndex, link.Source, link.Target);
        }

        // / <remarks>
        // / TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
        // /
        // / Указатель this.links может быть в том же месте, 
        // / так как 0-я связь не используется и имеет такой же размер как Header,
        // / поэтому header размещается в том же месте, что и 0-я связь
        // / </remarks>
    protected:
        virtual void SetPointers(IResizableDirectMemory& memory) = 0;

        virtual void ResetPointers()
        {
            _SourcesTreeMethods = nullptr;
            _TargetsTreeMethods = nullptr;
            _UnusedLinksListMethods = nullptr;
        }

        protected: LinksHeader<TLink>& GetHeaderReference() { return self().GetHeaderReference(); }

        protected: RawLink<TLink>& GetLinkReference(std::size_t index) { return self().GetLinkReference(index); }

        virtual bool Exists(TLink link)
        {
            return GreaterOrEqualThan(link, Constants.InternalReferencesRange.Minimum) && LessOrEqualThan(link, GetHeaderReference().AllocatedLinks) && !IsUnusedLink(link);
        }

        virtual bool IsUnusedLink(TLink linkIndex)
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

        virtual TLink GetOne()
        {
            return TLink{1};
        }

        virtual TLink GetZero()
        {
            return TLink{0};
        }

        virtual bool AreEqual(TLink first, TLink second)
        {
            return first == second;
        }

        virtual bool LessThan(TLink first, TLink second)
        {
            return first < second;
        }

        virtual bool LessOrEqualThan(TLink first, TLink second)
        {
            return first <= second;
        }

        // / <summary>
        // / <para>
        // / Determines whether this instance greater than.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="first">
        // / <para>The first.</para>
        // / <para></para>
        // / </param>
        // / <param name="second">
        // / <para>The second.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The bool</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual bool GreaterThan(TLink first, TLink second)
        virtual bool GreaterThan(TLink first, TLink second)
        {
            return first > second;
        }

        // / <summary>
        // / <para>
        // / Determines whether this instance greater or equal than.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="first">
        // / <para>The first.</para>
        // / <para></para>
        // / </param>
        // / <param name="second">
        // / <para>The second.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The bool</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual bool GreaterOrEqualThan(TLink first, TLink second)
        virtual bool GreaterOrEqualThan(TLink first, TLink second)
        {
            return first >= second;
        }

        // / <summary>
        // / <para>
        // / Converts the to int 64 using the specified value.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="value">
        // / <para>The value.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The long</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual long ConvertToInt64(TLink value)
        virtual std::int64_t ConvertToInt64(TLink value)
        {
            return value;
        }

        // / <summary>
        // / <para>
        // / Converts the to address using the specified value.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="value">
        // / <para>The value.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual TLink ConvertToAddress(long value)
        virtual TLink ConvertToAddress(std::int64_t value)
        {
            return value;
        }

        // / <summary>
        // / <para>
        // / Adds the first.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="first">
        // / <para>The first.</para>
        // / <para></para>
        // / </param>
        // / <param name="second">
        // / <para>The second.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual TLink Add(TLink first, TLink second)
        virtual TLink Add(TLink first, TLink second)
        {
            return first + second;
        }

        // / <summary>
        // / <para>
        // / Subtracts the first.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="first">
        // / <para>The first.</para>
        // / <para></para>
        // / </param>
        // / <param name="second">
        // / <para>The second.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual TLink Subtract(TLink first, TLink second)
        virtual TLink Subtract(TLink first, TLink second)
        {
            return first - second;
        }

        // / <summary>
        // / <para>
        // / Increments the link.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="link">
        // / <para>The link.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual TLink Increment(TLink link)
        virtual TLink Increment(TLink link)
        {
            return link + 1;
        }

        // / <summary>
        // / <para>
        // / Decrements the link.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="link">
        // / <para>The link.</para>
        // / <para></para>
        // / </param>
        // / <returns>
        // / <para>The link</para>
        // / <para></para>
        // / </returns>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected virtual TLink Decrement(TLink link)
        virtual TLink Decrement(TLink link)
        {
            return link - 1;
        }

//        #region Disposable

        // / <summary>
        // / <para>
        // / Gets the allow multiple dispose calls value.
        // / </para>
        // / <para></para>
        // / </summary>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] get => true;
            //[&] (std::any get)
            //{
            //    return true;
            //};

        // / <summary>
        // / <para>
        // / Disposes the manual.
        // / </para>
        // / <para></para>
        // / </summary>
        // / <param name="manual">
        // / <para>The manual.</para>
        // / <para></para>
        // / </param>
        // / <param name="wasDisposed">
        // / <para>The was disposed.</para>
        // / <para></para>
        // / </param>
// NOTE: The following .NET attribute has no direct equivalent in C++:
// ORIGINAL LINE: [MethodImpl(MethodImplOptions.AggressiveInlining)] protected  void Dispose(bool manual, bool wasDisposed)

//        #endregion

        protected: ~UnitedMemoryLinksBase()
        {
            ResetPointers();
        }
    };
}

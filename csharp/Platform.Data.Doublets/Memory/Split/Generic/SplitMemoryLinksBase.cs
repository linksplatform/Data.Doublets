using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Converters;
using Platform.Data.Exceptions;
using Platform.Delegates;
using Platform.Disposables;
using Platform.Memory;
using Platform.Singletons;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic;

/// <summary>
///     <para>
///         Represents the split memory links base.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="DisposableBase" />
/// <seealso cref="ILinks{TLinkAddress}" />
public abstract class SplitMemoryLinksBase<TLinkAddress> : DisposableBase, ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private static readonly Comparer<TLinkAddress> _comparer = Comparer<TLinkAddress>.Default;
    private static readonly TLinkAddress _zero;
    private static readonly TLinkAddress _one = ++_zero;

    /// <summary>Возвращает размер одной связи в байтах.</summary>
    /// <remarks>
    ///     Используется только во вне класса, не рекомедуется использовать внутри.
    ///     Так как во вне не обязательно будет доступен unsafe С#.
    /// </remarks>
    public static readonly long LinkDataPartSizeInBytes = RawLinkDataPart<TLinkAddress>.SizeInBytes;

    /// <summary>
    ///     <para>
    ///         The size in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static readonly long LinkIndexPartSizeInBytes = RawLinkIndexPart<TLinkAddress>.SizeInBytes;

    /// <summary>
    ///     <para>
    ///         The size in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static readonly long LinkHeaderSizeInBytes = LinksHeader<TLinkAddress>.SizeInBytes;

    /// <summary>
    ///     <para>
    ///         The default links size step.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public static readonly long DefaultLinksSizeStep = 1 * 1024 * 1024;

    /// <summary>
    ///     <para>
    ///         The data memory.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly IResizableDirectMemory _dataMemory;
    /// <summary>
    ///     <para>
    ///         The data memory reservation step in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly long _dataMemoryReservationStepInBytes;
    /// <summary>
    ///     <para>
    ///         The index memory.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly IResizableDirectMemory _indexMemory;
    /// <summary>
    ///     <para>
    ///         The index memory reservation step in bytes.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly long _indexMemoryReservationStepInBytes;
    /// <summary>
    ///     <para>
    ///         The use linked list.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly bool _useLinkedList;
    /// <summary>
    ///     <para>
    ///         The external sources tree methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinksTreeMethods<TLinkAddress> ExternalSourcesTreeMethods;
    /// <summary>
    ///     <para>
    ///         The external targets tree methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinksTreeMethods<TLinkAddress> ExternalTargetsTreeMethods;

    /// <summary>
    ///     <para>
    ///         The internal sources list methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected InternalLinksSourcesLinkedListMethods<TLinkAddress> InternalSourcesListMethods;
    /// <summary>
    ///     <para>
    ///         The internal sources tree methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinksTreeMethods<TLinkAddress> InternalSourcesTreeMethods;
    /// <summary>
    ///     <para>
    ///         The internal targets tree methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinksTreeMethods<TLinkAddress> InternalTargetsTreeMethods;
    // TODO: Возможно чтобы гарантированно проверять на то, является ли связь удалённой, нужно использовать не список а дерево, так как так можно быстрее проверить на наличие связи внутри
    /// <summary>
    ///     <para>
    ///         The unused links list methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinksListMethods<TLinkAddress> UnusedLinksListMethods;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinksBase" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>A data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>A index memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="memoryReservationStep">
    ///     <para>A memory reservation step.</para>
    ///     <para></para>
    /// </param>
    /// <param name="constants">
    ///     <para>A constants.</para>
    ///     <para></para>
    /// </param>
    /// <param name="useLinkedList">
    ///     <para>A use linked list.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected SplitMemoryLinksBase(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep, LinksConstants<TLinkAddress> constants, bool useLinkedList)
    {
        _dataMemory = dataMemory;
        _indexMemory = indexMemory;
        _dataMemoryReservationStepInBytes = memoryReservationStep * LinkDataPartSizeInBytes;
        _indexMemoryReservationStepInBytes = memoryReservationStep * LinkIndexPartSizeInBytes;
        _useLinkedList = useLinkedList;
        Constants = constants;
    }

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SplitMemoryLinksBase" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>A data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>A index memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="memoryReservationStep">
    ///     <para>A memory reservation step.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected SplitMemoryLinksBase(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory, long memoryReservationStep) : this(dataMemory: dataMemory, indexMemory: indexMemory, memoryReservationStep: memoryReservationStep, constants: Default<LinksConstants<TLinkAddress>>.Instance, useLinkedList: true) { }

    /// <summary>
    ///     Возвращает общее число связей находящихся в хранилище.
    /// </summary>
    protected virtual TLinkAddress Total
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get
        {
            ref var header = ref GetHeaderReference();
            return (header.AllocatedLinks) - (header.FreeLinks);
        }
    }

    /// <summary>
    ///     <para>
    ///         Gets the constants value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public virtual LinksConstants<TLinkAddress> Constants
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     <para>
    ///         Counts the substitution.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    /// <exception cref="NotSupportedException">
    ///     <para>Другие размеры и способы ограничений не поддерживаются.</para>
    ///     <para></para>
    /// </exception>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        // Если нет ограничений, тогда возвращаем общее число связей находящихся в хранилище.
        if (restriction.Count == 0)
        {
            return Total;
        }
        var constants = Constants;
        var any = constants.Any;
        var index = this.GetIndex(link: restriction);
        if (restriction.Count == 1)
        {
            if ((index == any))
            {
                return Total;
            }
            return Exists(link: index) ? GetOne() : GetZero();
        }
        if (restriction.Count == 2)
        {
            var value = restriction[index: 1];
            if ((index == any))
            {
                if ((value == any))
                {
                    return Total; // Any - как отсутствие ограничения
                }
                var externalReferencesRange = constants.ExternalReferencesRange;
                if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: value))
                {
                    return (ExternalSourcesTreeMethods.CountUsages(root: value) + ExternalTargetsTreeMethods.CountUsages(root: value));
                }
                if (_useLinkedList)
                {
                    return (InternalSourcesListMethods.CountUsages(head: value) + InternalTargetsTreeMethods.CountUsages(root: value));
                }
                return (InternalSourcesTreeMethods.CountUsages(root: value) + InternalTargetsTreeMethods.CountUsages(root: value));
            }
            if (!Exists(link: index))
            {
                return GetZero();
            }
            if ((value == any))
            {
                return GetOne();
            }
            ref var storedLinkValue = ref GetLinkDataPartReference(linkIndex: index);
            if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
            {
                return GetOne();
            }
            return GetZero();
        }
        if (restriction.Count == 3)
        {
            var externalReferencesRange = constants.ExternalReferencesRange;
            var source = this.GetSource(link: restriction);
            var target = this.GetTarget(link: restriction);
            if ((index == any))
            {
                if ((source == any) && (target == any))
                {
                    return Total;
                }
                if ((source == any))
                {
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: target))
                    {
                        return ExternalTargetsTreeMethods.CountUsages(root: target);
                    }
                    return InternalTargetsTreeMethods.CountUsages(root: target);
                }
                if ((target == any))
                {
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: source))
                    {
                        return ExternalSourcesTreeMethods.CountUsages(root: source);
                    }
                    if (_useLinkedList)
                    {
                        return InternalSourcesListMethods.CountUsages(head: source);
                    }
                    return InternalSourcesTreeMethods.CountUsages(root: source);
                }
                //if(source != Any && target != Any)
                // Эквивалент Exists(source, target) => Count(Any, source, target) > 0
                TLinkAddress link;
                if (externalReferencesRange.HasValue)
                {
                    if (externalReferencesRange.Value.Contains(value: source) && externalReferencesRange.Value.Contains(value: target))
                    {
                        link = ExternalSourcesTreeMethods.Search(source: source, target: target);
                    }
                    else if (externalReferencesRange.Value.Contains(value: source))
                    {
                        link = InternalTargetsTreeMethods.Search(source: source, target: target);
                    }
                    else if (externalReferencesRange.Value.Contains(value: target))
                    {
                        if (_useLinkedList)
                        {
                            link = ExternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                        else
                        {
                            link = InternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                    }
                    else
                    {
                        if (_useLinkedList || (InternalSourcesTreeMethods.CountUsages(root: source) > InternalTargetsTreeMethods.CountUsages(root: target)))
                        {
                            link = InternalTargetsTreeMethods.Search(source: source, target: target);
                        }
                        else
                        {
                            link = InternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                    }
                }
                else
                {
                    if (_useLinkedList || (InternalSourcesTreeMethods.CountUsages(root: source) > InternalTargetsTreeMethods.CountUsages(root: target)))
                    {
                        link = InternalTargetsTreeMethods.Search(source: source, target: target);
                    }
                    else
                    {
                        link = InternalSourcesTreeMethods.Search(source: source, target: target);
                    }
                }
                return (link == constants.Null) ? GetZero() : GetOne();
            }
            if (!Exists(link: index))
            {
                return GetZero();
            }
            if ((source == any) && (target == any))
            {
                return GetOne();
            }
            ref var storedLinkValue = ref GetLinkDataPartReference(linkIndex: index);
            if ((source != any) && (target != any))
            {
                if ((storedLinkValue.Source == source) && (storedLinkValue.Target == target))
                {
                    return GetOne();
                }
                return GetZero();
            }
            var value = default(TLinkAddress);
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
                return GetOne();
            }
            return GetZero();
        }
        throw new NotSupportedException(message: "Другие размеры и способы ограничений не поддерживаются.");
    }

    /// <summary>
    ///     <para>
    ///         Eaches the handler.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="handler">
    ///     <para>The handler.</para>
    ///     <para></para>
    /// </param>
    /// <param name="restriction">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    /// <exception cref="NotSupportedException">
    ///     <para>Другие размеры и способы ограничений не поддерживаются.</para>
    ///     <para></para>
    /// </exception>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        var constants = Constants;
        var @break = constants.Break;
        if (restriction.Count == 0)
        {
            for (var link = GetOne(); (link <= GetHeaderReference().AllocatedLinks); link = link + TLinkAddress.One)
            {
                if (Exists(link: link) && (handler(link: GetLinkStruct(linkIndex: link)) == @break))
                {
                    return @break;
                }
            }
            return @break;
        }
        var @continue = constants.Continue;
        var any = constants.Any;
        var index = this.GetIndex(link: restriction);
        if (restriction.Count == 1)
        {
            if ((index == any))
            {
                return Each(restriction: Array.Empty<TLinkAddress>(), handler: handler);
            }
            if (!Exists(link: index))
            {
                return @continue;
            }
            return handler(link: GetLinkStruct(linkIndex: index));
        }
        if (restriction.Count == 2)
        {
            var value = restriction[index: 1];
            if ((index == any))
            {
                if ((value == any))
                {
                    return Each(restriction: Array.Empty<TLinkAddress>(), handler: handler);
                }
                if ((Each(restriction: new Link<TLinkAddress>(index: index, source: value, target: any), handler: handler) == @break))
                {
                    return @break;
                }
                return Each(restriction: new Link<TLinkAddress>(index: index, source: any, target: value), handler: handler);
            }
            if (!Exists(link: index))
            {
                return @continue;
            }
            if ((value == any))
            {
                return handler(link: GetLinkStruct(linkIndex: index));
            }
            ref var storedLinkValue = ref GetLinkDataPartReference(linkIndex: index);
            if ((storedLinkValue.Source == value) || (storedLinkValue.Target == value))
            {
                return handler(link: GetLinkStruct(linkIndex: index));
            }
            return @continue;
        }
        if (restriction.Count == 3)
        {
            var externalReferencesRange = constants.ExternalReferencesRange;
            var source = this.GetSource(link: restriction);
            var target = this.GetTarget(link: restriction);
            if ((index == any))
            {
                if ((source == any) && (target == any))
                {
                    return Each(restriction: Array.Empty<TLinkAddress>(), handler: handler);
                }
                if ((source == any))
                {
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: target))
                    {
                        return ExternalTargetsTreeMethods.EachUsage(root: target, handler: handler);
                    }
                    return InternalTargetsTreeMethods.EachUsage(root: target, handler: handler);
                }
                if ((target == any))
                {
                    if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: source))
                    {
                        return ExternalSourcesTreeMethods.EachUsage(root: source, handler: handler);
                    }
                    if (_useLinkedList)
                    {
                        return InternalSourcesListMethods.EachUsage(source: source, handler: handler);
                    }
                    return InternalSourcesTreeMethods.EachUsage(root: source, handler: handler);
                }
                //if(source != Any && target != Any)
                TLinkAddress link;
                if (externalReferencesRange.HasValue)
                {
                    if (externalReferencesRange.Value.Contains(value: source) && externalReferencesRange.Value.Contains(value: target))
                    {
                        link = ExternalSourcesTreeMethods.Search(source: source, target: target);
                    }
                    else if (externalReferencesRange.Value.Contains(value: source))
                    {
                        link = InternalTargetsTreeMethods.Search(source: source, target: target);
                    }
                    else if (externalReferencesRange.Value.Contains(value: target))
                    {
                        if (_useLinkedList)
                        {
                            link = ExternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                        else
                        {
                            link = InternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                    }
                    else
                    {
                        if (_useLinkedList || (InternalSourcesTreeMethods.CountUsages(root: source) > InternalTargetsTreeMethods.CountUsages(root: target)))
                        {
                            link = InternalTargetsTreeMethods.Search(source: source, target: target);
                        }
                        else
                        {
                            link = InternalSourcesTreeMethods.Search(source: source, target: target);
                        }
                    }
                }
                else
                {
                    if (_useLinkedList || (InternalSourcesTreeMethods.CountUsages(root: source) > InternalTargetsTreeMethods.CountUsages(root: target)))
                    {
                        link = InternalTargetsTreeMethods.Search(source: source, target: target);
                    }
                    else
                    {
                        link = InternalSourcesTreeMethods.Search(source: source, target: target);
                    }
                }
                return (link == constants.Null) ? @continue : handler(link: GetLinkStruct(linkIndex: link));
            }
            if (!Exists(link: index))
            {
                return @continue;
            }
            if ((source == any) && (target == any))
            {
                return handler(link: GetLinkStruct(linkIndex: index));
            }
            ref var storedLinkValue = ref GetLinkDataPartReference(linkIndex: index);
            if ((source != any) && (target != any))
            {
                if ((storedLinkValue.Source == source) && (storedLinkValue.Target == target))
                {
                    return handler(link: GetLinkStruct(linkIndex: index));
                }
                return @continue;
            }
            var value = default(TLinkAddress);
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
                return handler(link: GetLinkStruct(linkIndex: index));
            }
            return @continue;
        }
        throw new NotSupportedException(message: "Другие размеры и способы ограничений не поддерживаются.");
    }

    /// <remarks>
    ///     TODO: Возможно можно перемещать значения, если указан индекс, но значение существует в другом месте (но не в
    ///     менеджере памяти, а в логике Links)
    /// </remarks>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        var constants = Constants;
        var @null = constants.Null;
        var externalReferencesRange = constants.ExternalReferencesRange;
        var linkIndex = this.GetIndex(link: restriction);
        var before = GetLinkStruct(linkIndex: linkIndex);
        ref var link = ref GetLinkDataPartReference(linkIndex: linkIndex);
        var source = link.Source;
        var target = link.Target;
        ref var header = ref GetHeaderReference();
        ref var rootAsSource = ref header.RootAsSource;
        ref var rootAsTarget = ref header.RootAsTarget;
        // Будет корректно работать только в том случае, если пространство выделенной связи предварительно заполнено нулями
        if ((source != @null))
        {
            if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: source))
            {
                ExternalSourcesTreeMethods.Detach(root: ref rootAsSource, linkIndex: linkIndex);
            }
            else
            {
                if (_useLinkedList)
                {
                    InternalSourcesListMethods.Detach(headElement: source, element: linkIndex);
                }
                else
                {
                    InternalSourcesTreeMethods.Detach(root: ref GetLinkIndexPartReference(linkIndex: source).RootAsSource, linkIndex: linkIndex);
                }
            }
        }
        if ((target != @null))
        {
            if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: target))
            {
                ExternalTargetsTreeMethods.Detach(root: ref rootAsTarget, linkIndex: linkIndex);
            }
            else
            {
                InternalTargetsTreeMethods.Detach(root: ref GetLinkIndexPartReference(linkIndex: target).RootAsTarget, linkIndex: linkIndex);
            }
        }
        source = link.Source = this.GetSource(link: substitution);
        target = link.Target = this.GetTarget(link: substitution);
        if ((source != @null))
        {
            if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: source))
            {
                ExternalSourcesTreeMethods.Attach(root: ref rootAsSource, linkIndex: linkIndex);
            }
            else
            {
                if (_useLinkedList)
                {
                    InternalSourcesListMethods.AttachAsLast(headElement: source, element: linkIndex);
                }
                else
                {
                    InternalSourcesTreeMethods.Attach(root: ref GetLinkIndexPartReference(linkIndex: source).RootAsSource, linkIndex: linkIndex);
                }
            }
        }
        if ((target != @null))
        {
            if (externalReferencesRange.HasValue && externalReferencesRange.Value.Contains(value: target))
            {
                ExternalTargetsTreeMethods.Attach(root: ref rootAsTarget, linkIndex: linkIndex);
            }
            else
            {
                InternalTargetsTreeMethods.Attach(root: ref GetLinkIndexPartReference(linkIndex: target).RootAsTarget, linkIndex: linkIndex);
            }
        }
        return handler != null ? handler(before: before, after: new Link<TLinkAddress>(index: linkIndex, source: source, target: target)) : Constants.Continue;
    }

    /// <remarks>
    ///     TODO: Возможно нужно будет заполнение нулями, если внешнее API ими не заполняет пространство
    /// </remarks>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        ref var header = ref GetHeaderReference();
        var freeLink = header.FirstFreeLink;
        if ((freeLink != Constants.Null))
        {
            UnusedLinksListMethods.Detach(freeLink: freeLink);
        }
        else
        {
            var maximumPossibleInnerReference = Constants.InternalReferencesRange.Maximum;
            if ((header.AllocatedLinks > maximumPossibleInnerReference))
            {
                throw new LinksLimitReachedException<TLinkAddress>(limit: maximumPossibleInnerReference);
            }
            if ((header.AllocatedLinks >= header.ReservedLinks - TLinkAddress.One))
            {
                _dataMemory.ReservedCapacity += _dataMemoryReservationStepInBytes;
                _indexMemory.ReservedCapacity += _indexMemoryReservationStepInBytes;
                SetPointers(dataMemory: _dataMemory, indexMemory: _indexMemory);
                header = ref GetHeaderReference();
                header.ReservedLinks = TLinkAddress.CreateTruncating(value: _dataMemory.ReservedCapacity / LinkDataPartSizeInBytes);
            }
            freeLink = header.AllocatedLinks = header.AllocatedLinks + TLinkAddress.One;
            _dataMemory.UsedCapacity += LinkDataPartSizeInBytes;
            _indexMemory.UsedCapacity += LinkIndexPartSizeInBytes;
        }
        return handler != null ? handler(before: null, after: GetLinkStruct(linkIndex: freeLink)) : Constants.Continue;
    }

    /// <summary>
    ///     <para>
    ///         Deletes the substitution.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        ref var header = ref GetHeaderReference();
        var link = restriction[index: Constants.IndexPart];
        var before = GetLinkStruct(linkIndex: link);
        if ((link < header.AllocatedLinks))
        {
            UnusedLinksListMethods.AttachAsFirst(link: link);
        }
        else if ((link == header.AllocatedLinks))
        {
            header.AllocatedLinks = header.AllocatedLinks - TLinkAddress.One;
            _dataMemory.UsedCapacity -= LinkDataPartSizeInBytes;
            _indexMemory.UsedCapacity -= LinkIndexPartSizeInBytes;
            // Убираем все связи, находящиеся в списке свободных в конце файла, до тех пор, пока не дойдём до первой существующей связи
            // Позволяет оптимизировать количество выделенных связей (AllocatedLinks)
            while ((header.AllocatedLinks > GetZero()) && IsUnusedLink(linkIndex: header.AllocatedLinks))
            {
                UnusedLinksListMethods.Detach(freeLink: header.AllocatedLinks);
                header.AllocatedLinks = header.AllocatedLinks - TLinkAddress.One;
                _dataMemory.UsedCapacity -= LinkDataPartSizeInBytes;
                _indexMemory.UsedCapacity -= LinkIndexPartSizeInBytes;
            }
        }
        return handler != null ? handler(before: before, after: null) : Constants.Continue;
    }

    /// <summary>
    ///     <para>
    ///         Inits the data memory.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="dataMemory">
    ///     <para>The data memory.</para>
    ///     <para></para>
    /// </param>
    /// <param name="indexMemory">
    ///     <para>The index memory.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void Init(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory)
    {
        // Read allocated links from header
        if (indexMemory.ReservedCapacity < LinkHeaderSizeInBytes)
        {
            indexMemory.ReservedCapacity = LinkHeaderSizeInBytes;
        }
        SetPointers(dataMemory: dataMemory, indexMemory: indexMemory);
        ref var header = ref GetHeaderReference();
        var allocatedLinks = long.CreateTruncating(value: header.AllocatedLinks);
        // Adjust reserved capacity
        var minimumDataReservedCapacity = allocatedLinks * LinkDataPartSizeInBytes;
        if (minimumDataReservedCapacity < dataMemory.UsedCapacity)
        {
            minimumDataReservedCapacity = dataMemory.UsedCapacity;
        }
        if (minimumDataReservedCapacity < _dataMemoryReservationStepInBytes)
        {
            minimumDataReservedCapacity = _dataMemoryReservationStepInBytes;
        }
        var minimumIndexReservedCapacity = allocatedLinks * LinkDataPartSizeInBytes;
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
            minimumDataReservedCapacity = minimumDataReservedCapacity / _dataMemoryReservationStepInBytes * _dataMemoryReservationStepInBytes + _dataMemoryReservationStepInBytes;
        }
        if (minimumIndexReservedCapacity % _indexMemoryReservationStepInBytes > 0)
        {
            minimumIndexReservedCapacity = minimumIndexReservedCapacity / _indexMemoryReservationStepInBytes * _indexMemoryReservationStepInBytes + _indexMemoryReservationStepInBytes;
        }
        if (dataMemory.ReservedCapacity != minimumDataReservedCapacity)
        {
            dataMemory.ReservedCapacity = minimumDataReservedCapacity;
        }
        if (indexMemory.ReservedCapacity != minimumIndexReservedCapacity)
        {
            indexMemory.ReservedCapacity = minimumIndexReservedCapacity;
        }
        SetPointers(dataMemory: dataMemory, indexMemory: indexMemory);
        header = ref GetHeaderReference();
        // Ensure correctness _memory.UsedCapacity over _header->AllocatedLinks
        // Гарантия корректности _memory.UsedCapacity относительно _header->AllocatedLinks
        dataMemory.UsedCapacity = ConvertToInt64(value: header.AllocatedLinks) * LinkDataPartSizeInBytes + LinkDataPartSizeInBytes; // First link is read only zero link.
        indexMemory.UsedCapacity = ConvertToInt64(value: header.AllocatedLinks) * LinkIndexPartSizeInBytes + LinkHeaderSizeInBytes;
        // Ensure correctness _memory.ReservedLinks over _header->ReservedCapacity
        // Гарантия корректности _header->ReservedLinks относительно _memory.ReservedCapacity
        header.ReservedLinks = TLinkAddress.CreateTruncating(value: (dataMemory.ReservedCapacity - LinkDataPartSizeInBytes) / LinkDataPartSizeInBytes);
    }

    /// <summary>
    ///     <para>
    ///         Gets the link struct using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A list of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public IList<TLinkAddress>? GetLinkStruct(TLinkAddress linkIndex)
    {
        ref var link = ref GetLinkDataPartReference(linkIndex: linkIndex);
        return new Link<TLinkAddress>(index: linkIndex, source: link.Source, target: link.Target);
    }

    /// <remarks>
    ///     TODO: Возможно это должно быть событием, вызываемым из IMemory, в том случае, если адрес реально поменялся
    ///     Указатель this.links может быть в том же месте,
    ///     так как 0-я связь не используется и имеет такой же размер как Header,
    ///     поэтому header размещается в том же месте, что и 0-я связь
    /// </remarks>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract void SetPointers(IResizableDirectMemory dataMemory, IResizableDirectMemory indexMemory);

    /// <summary>
    ///     <para>
    ///         Resets the pointers.
    ///     </para>
    ///     <para></para>
    /// </summary>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void ResetPointers()
    {
        InternalSourcesListMethods = null;
        InternalSourcesTreeMethods = null;
        ExternalSourcesTreeMethods = null;
        InternalTargetsTreeMethods = null;
        ExternalTargetsTreeMethods = null;
        UnusedLinksListMethods = null;
    }

    /// <summary>
    ///     <para>
    ///         Gets the header reference.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>A ref links header of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract ref LinksHeader<TLinkAddress> GetHeaderReference();

    /// <summary>
    ///     <para>
    ///         Gets the link data part reference using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link data part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress linkIndex);

    /// <summary>
    ///     <para>
    ///         Gets the link index part reference using the specified link index.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>A ref raw link index part of t link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected abstract ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress linkIndex);

    /// <summary>
    ///     <para>
    ///         Determines whether this instance exists.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool Exists(TLinkAddress link)
    {
        return (link >= Constants.InternalReferencesRange.Minimum) && (link <= GetHeaderReference().AllocatedLinks) && !IsUnusedLink(linkIndex: link);
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance is unused link.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="linkIndex">
    ///     <para>The link index.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool IsUnusedLink(TLinkAddress linkIndex)
    {
        if ((GetHeaderReference().FirstFreeLink != linkIndex)) // May be this check is not needed
        {
            // TODO: Reduce access to memory in different location (should be enough to use just linkIndexPart)
            ref var linkDataPart = ref GetLinkDataPartReference(linkIndex: linkIndex);
            ref var linkIndexPart = ref GetLinkIndexPartReference(linkIndex: linkIndex);
            return (linkIndexPart.SizeAsTarget == default) && (linkDataPart.Source != default);
        }
        return true;
    }

    /// <summary>
    ///     <para>
    ///         Gets the one.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress GetOne()
    {
        return _one;
    }

    /// <summary>
    ///     <para>
    ///         Gets the zero.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress GetZero()
    {
        return default;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance are equal.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool AreEqual(TLinkAddress first, TLinkAddress second)
    {
        return first ==  second;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance less than.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool LessThan(TLinkAddress first, TLinkAddress second)
    {
        return _comparer.Compare(x: first, y: second) < 0;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance less or equal than.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool LessOrEqualThan(TLinkAddress first, TLinkAddress second)
    {
        return _comparer.Compare(x: first, y: second) <= 0;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance greater than.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool GreaterThan(TLinkAddress first, TLinkAddress second)
    {
        return _comparer.Compare(x: first, y: second) > 0;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance greater or equal than.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual bool GreaterOrEqualThan(TLinkAddress first, TLinkAddress second)
    {
        return _comparer.Compare(x: first, y: second) >= 0;
    }

    /// <summary>
    ///     <para>
    ///         Converts the to int 64 using the specified value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The long</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual long ConvertToInt64(TLinkAddress value)
    {
        return long.CreateTruncating(source: value);;
    }

    /// <summary>
    ///     <para>
    ///         Converts the to address using the specified value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="value">
    ///     <para>The value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress ConvertToAddress(long value)
    {
        return _int64ToAddressConverter.Convert(source: value);
    }

    /// <summary>
    ///     <para>
    ///         Adds the first.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress Add(TLinkAddress first, TLinkAddress second)
    {
        return first + second;
    }

    /// <summary>
    ///     <para>
    ///         Subtracts the first.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="first">
    ///     <para>The first.</para>
    ///     <para></para>
    /// </param>
    /// <param name="second">
    ///     <para>The second.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress Subtract(TLinkAddress first, TLinkAddress second)
    {
        return first - second;
    }

    #region Disposable

    /// <summary>
    ///     <para>
    ///         Gets the allow multiple dispose calls value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected override bool AllowMultipleDisposeCalls
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get => true;
    }

    /// <summary>
    ///     <para>
    ///         Disposes the manual.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="manual">
    ///     <para>The manual.</para>
    ///     <para></para>
    /// </param>
    /// <param name="wasDisposed">
    ///     <para>The was disposed.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override void Dispose(bool manual, bool wasDisposed)
    {
        if (!wasDisposed)
        {
            ResetPointers();
            _dataMemory.DisposeIfPossible();
            _indexMemory.DisposeIfPossible();
        }
    }

    #endregion
}

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links decorator base.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksOperatorBase{TLinkAddress}" />
/// <seealso cref="ILinks{TLinkAddress}" />
public abstract class LinksDecoratorBase<TLinkAddress> : LinksOperatorBase<TLinkAddress>, ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         The constants.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly LinksConstants<TLinkAddress> _constants;

    /// <summary>
    ///     <para>
    ///         The facade.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected ILinks<TLinkAddress> _facade;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksDecoratorBase" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected LinksDecoratorBase(ILinks<TLinkAddress> links) : base(links: links)
    {
        _constants = links.Constants;
        Facade = this;
    }

    /// <summary>
    ///     <para>
    ///         Gets or sets the facade value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public ILinks<TLinkAddress> Facade
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get => _facade;
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        set
        {
            _facade = value;
            if (_links is LinksDecoratorBase<TLinkAddress> decorator)
            {
                decorator.Facade = value;
            }
        }
    }

    /// <summary>
    ///     <para>
    ///         Gets the constants value.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public LinksConstants<TLinkAddress> Constants
    {
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        get => _constants;
    }

    /// <summary>
    ///     <para>
    ///         Counts the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        return _links.Count(restriction: restriction);
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
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        return _links.Each(restriction: restriction, handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Creates the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        return _links.Create(substitution: substitution, handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Updates the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <param name="substitution">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        return _links.Update(restriction: restriction, substitution: substitution, handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Deletes the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public virtual TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        return _links.Delete(restriction: restriction, handler: handler);
    }
}

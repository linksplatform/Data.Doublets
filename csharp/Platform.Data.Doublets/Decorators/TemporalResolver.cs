using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using Platform.Timestamps;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
/// <para>
/// Represents a temporal resolver decorator that translates delete and update operations to series of create operations.
/// Each change is recorded directly. Data store contains all versions of data.
/// </para>
/// <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class TemporalResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly UniqueTimestampFactory _timestampFactory;
    
    /// <summary>
    /// <para>
    /// Initializes a new <see cref="TemporalResolver{TLinkAddress}"/> instance.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="links">
    /// <para>A links.</para>
    /// <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TemporalResolver(ILinks<TLinkAddress> links) : base(links: links)
    {
        _timestampFactory = new UniqueTimestampFactory();
    }

    /// <summary>
    /// <para>
    /// Updates the restriction by creating a new version instead of modifying the existing link.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="restriction">
    /// <para>The restriction.</para>
    /// <para></para>
    /// </param>
    /// <param name="substitution">
    /// <para>The substitution.</para>
    /// <para></para>
    /// </param>
    /// <param name="handler">
    /// <para>The handler.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The link</para>
    /// <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        // Get the current link to be updated
        var linkIndex = restriction != null && restriction.Count > 0 ? restriction[0] : _constants.Any;
        var currentLink = _links.GetLink(linkIndex);
        
        // Create a timestamp link to mark when this change happened
        var timestamp = _timestampFactory.Create();
        var timestampValue = TLinkAddress.CreateChecked((ulong)timestamp);
        var timestampLink = _links.Create(new[] { timestampValue, timestampValue, timestampValue }, handler);
        
        // Create a new version link with the updated values and timestamp
        var sourceValue = substitution != null && substitution.Count > 0 ? substitution[0] : currentLink[1];
        var targetValue = substitution != null && substitution.Count > 1 ? substitution[1] : currentLink[2];
        var newVersionLink = _links.Create(new[] 
        { 
            sourceValue, // source
            targetValue, // target  
            timestampLink // reference to timestamp
        }, handler);
        
        return newVersionLink;
    }

    /// <summary>
    /// <para>
    /// Deletes the restriction by creating a deletion record instead of actually deleting the link.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="restriction">
    /// <para>The restriction.</para>
    /// <para></para>
    /// </param>
    /// <param name="handler">
    /// <para>The handler.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The link</para>
    /// <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        // Create a timestamp link to mark when this deletion happened
        var timestamp = _timestampFactory.Create();
        var timestampValue = TLinkAddress.CreateChecked((ulong)timestamp);
        var timestampLink = _links.Create(new[] { timestampValue, timestampValue, timestampValue }, handler);
        
        // Create a deletion marker constant (using Null as deletion marker)
        var deletionMarker = _constants.Null;
        
        // Create a deletion record instead of actually deleting
        var originalLinkIndex = restriction != null && restriction.Count > 0 ? restriction[0] : _constants.Any;
        var deletionRecord = _links.Create(new[] 
        { 
            originalLinkIndex, // original link index
            deletionMarker, // deletion marker
            timestampLink // reference to timestamp  
        }, handler);
        
        return deletionRecord;
    }
}
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Data.Doublets.Decorators;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets;

/// <summary>
///     <para>
///         Represents a sequences manager that provides automatic compactification (deduplication) functionality.
///     </para>
///     <para>
///         This class decorates an underlying ILinks instance and optionally performs automatic 
///         compactification when disposed, merging duplicate links that have the same source and target.
///     </para>
///     <para>
///         Compactification helps reduce storage overhead by eliminating redundant links while 
///         preserving the semantics of the data structure.
///     </para>
/// </summary>
/// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
/// <seealso cref="LinksDisposableDecoratorBase{TLinkAddress}" />
public class Sequences<TLinkAddress> : LinksDisposableDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly bool _enableAutomaticCompactification;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="Sequences" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>The underlying links storage.</para>
    ///     <para></para>
    /// </param>
    /// <param name="enableAutomaticCompactification">
    ///     <para>Enables automatic compactification (deduplication) of all sequences on dispose.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Sequences(ILinks<TLinkAddress> links, bool enableAutomaticCompactification = false) : base(links)
    {
        _enableAutomaticCompactification = enableAutomaticCompactification;
    }

    /// <summary>
    ///     <para>
    ///         Gets a value indicating whether automatic compactification is enabled.
    ///     </para>
    ///     <para></para>
    /// </summary>
    public bool IsAutomaticCompactificationEnabled 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _enableAutomaticCompactification;
    }

    /// <summary>
    ///     <para>
    ///         Performs compactification (deduplication) of all sequences by merging duplicate links.
    ///     </para>
    ///     <para></para>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Compact()
    {
        var duplicatePairs = FindDuplicateLinks();
        foreach (var (duplicate, original) in duplicatePairs)
        {
            _links.MergeAndDelete(duplicate, original);
        }
    }

    /// <summary>
    ///     <para>
    ///         Finds duplicate links that have the same source and target.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <returns>
    ///     <para>A collection of duplicate pairs where each pair contains (duplicate, original).</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private System.Collections.Generic.List<(TLinkAddress duplicate, TLinkAddress original)> FindDuplicateLinks()
    {
        var duplicatePairs = new System.Collections.Generic.List<(TLinkAddress duplicate, TLinkAddress original)>();
        var seenLinks = new System.Collections.Generic.Dictionary<(TLinkAddress source, TLinkAddress target), TLinkAddress>();
        
        var constants = _links.Constants;
        var query = new Link<TLinkAddress>(constants.Any, constants.Any, constants.Any);
        
        _links.Each(link =>
        {
            if (link == null)
            {
                return constants.Continue;
            }
            
            var linkAddress = _links.GetIndex(link);
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);
            
            // Skip point links (where source == target == linkAddress)
            if (source.Equals(linkAddress) && target.Equals(linkAddress))
            {
                return constants.Continue;
            }
            
            var key = (source, target);
            if (seenLinks.TryGetValue(key, out var originalLink))
            {
                // Found a duplicate - the current link is a duplicate of the original
                duplicatePairs.Add((linkAddress, originalLink));
            }
            else
            {
                // First time seeing this source-target combination
                seenLinks[key] = linkAddress;
            }
            
            return constants.Continue;
        }, query);
        
        return duplicatePairs;
    }

    /// <summary>
    ///     <para>
    ///         Disposes the sequences manager and optionally performs automatic compactification.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="manual">
    ///     <para>The manual disposal flag.</para>
    ///     <para></para>
    /// </param>
    /// <param name="wasDisposed">
    ///     <para>The was disposed flag.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Dispose(bool manual, bool wasDisposed)
    {
        if (!wasDisposed && _enableAutomaticCompactification)
        {
            try
            {
                Compact();
            }
            catch
            {
                // Ignore any errors during compactification
                // The underlying links storage might be in an inconsistent state
            }
        }
        
        // Don't dispose the underlying links - let the caller handle that
        // base.Dispose(manual, wasDisposed);
    }
}
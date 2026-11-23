using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;
using Platform.Timestamps;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets;

/// <summary>
/// <para>
/// Defines temporal extensions for ILinks that provide read operations with timestamp arguments.
/// These extensions allow getting data as it was seen at a specified point in time.
/// </para>
/// <para></para>
/// </summary>
public static class ILinksTemporalExtensions
{
    /// <summary>
    /// <para>
    /// Counts the links that existed at the specified timestamp.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="links">
    /// <para>The links.</para>
    /// <para></para>
    /// </param>
    /// <param name="restriction">
    /// <para>The restriction.</para>
    /// <para></para>
    /// </param>
    /// <param name="timestamp">
    /// <para>The timestamp to query at.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The count of links at the specified timestamp</para>
    /// <para></para>
    /// </returns>
    public static TLinkAddress CountAt<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, Timestamp timestamp) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        var count = TLinkAddress.Zero;
        links.EachAt(restriction, timestamp, link =>
        {
            count++;
            return links.Constants.Continue;
        });
        return count;
    }

    /// <summary>
    /// <para>
    /// Iterates through links that existed at the specified timestamp.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="links">
    /// <para>The links.</para>
    /// <para></para>
    /// </param>
    /// <param name="restriction">
    /// <para>The restriction.</para>
    /// <para></para>
    /// </param>
    /// <param name="timestamp">
    /// <para>The timestamp to query at.</para>
    /// <para></para>
    /// </param>
    /// <param name="handler">
    /// <para>The handler.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The last link processed</para>
    /// <para></para>
    /// </returns>
    public static TLinkAddress EachAt<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? restriction, Timestamp timestamp, ReadHandler<TLinkAddress>? handler) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        var timestampValue = TLinkAddress.CreateChecked((ulong)timestamp);
        var result = links.Constants.Continue;
        var processedLinks = new HashSet<TLinkAddress>();

        // Find all links that were created before or at the specified timestamp
        // and were not deleted before or at the specified timestamp
        links.Each(links.Constants.Any, link =>
        {
            var linkArray = links.GetLink(link[0]);
            
            // Check if this is a regular link (not a timestamp or deletion record)
            if (IsRegularLink(links, linkArray))
            {
                var linkTimestamp = GetLinkTimestamp(links, linkArray);
                
                // Include link if it was created before or at the specified timestamp
                if ((ulong)linkTimestamp <= (ulong)timestamp)
                {
                    // Check if the link was deleted before or at the specified timestamp
                    var linkId = link[0];
                    if (!IsLinkDeletedAt(links, linkId, timestamp))
                    {
                        // Apply restriction filter if provided
                        if (MatchesRestriction(linkArray, restriction))
                        {
                            if (!processedLinks.Contains(linkId))
                            {
                                processedLinks.Add(linkId);
                                result = handler?.Invoke(linkArray) ?? links.Constants.Continue;
                                if (result == links.Constants.Break)
                                {
                                    return links.Constants.Break;
                                }
                            }
                        }
                    }
                }
            }
            
            return links.Constants.Continue;
        });
        
        return result;
    }

    /// <summary>
    /// <para>
    /// Gets a link as it existed at the specified timestamp.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <param name="links">
    /// <para>The links.</para>
    /// <para></para>
    /// </param>
    /// <param name="linkIndex">
    /// <para>The link index.</para>
    /// <para></para>
    /// </param>
    /// <param name="timestamp">
    /// <para>The timestamp to query at.</para>
    /// <para></para>
    /// </param>
    /// <returns>
    /// <para>The link as it existed at the timestamp, or null if it didn't exist</para>
    /// <para></para>
    /// </returns>
    public static IList<TLinkAddress> GetLinkAt<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkIndex, Timestamp timestamp) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        IList<TLinkAddress> result = new List<TLinkAddress>();
        
        links.EachAt(new[] { linkIndex }, timestamp, link =>
        {
            result = link;
            return links.Constants.Break;
        });
        
        return result;
    }

    private static bool IsRegularLink<TLinkAddress>(ILinks<TLinkAddress> links, IList<TLinkAddress> linkArray) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        // A regular link should have 3 elements and not be a timestamp or deletion record
        return linkArray.Count >= 3 && 
               !IsTimestampLink(linkArray) && 
               !IsDeletionRecord(links, linkArray);
    }

    private static bool IsTimestampLink<TLinkAddress>(IList<TLinkAddress> linkArray) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        // A timestamp link has all three elements the same (representing the timestamp value)
        return linkArray.Count >= 3 && 
               linkArray[0] == linkArray[1] && 
               linkArray[1] == linkArray[2];
    }

    private static bool IsDeletionRecord<TLinkAddress>(ILinks<TLinkAddress> links, IList<TLinkAddress> linkArray) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        // A deletion record has the deletion marker (Null) as the target
        return linkArray.Count >= 3 && linkArray[2] == links.Constants.Null;
    }

    private static Timestamp GetLinkTimestamp<TLinkAddress>(ILinks<TLinkAddress> links, IList<TLinkAddress> linkArray) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        // For regular links, the timestamp is stored in the third element (target)
        if (linkArray.Count >= 3)
        {
            var timestampLinkIndex = linkArray[2];
            var timestampLink = links.GetLink(timestampLinkIndex);
            
            if (IsTimestampLink(timestampLink))
            {
                // Convert back to timestamp value
                var timestampValue = ulong.CreateChecked(timestampLink[0]);
                return new Timestamp(timestampValue);
            }
        }
        
        return new Timestamp(0);
    }

    private static bool IsLinkDeletedAt<TLinkAddress>(ILinks<TLinkAddress> links, TLinkAddress linkIndex, Timestamp timestamp) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        var isDeleted = false;
        
        // Look for deletion records for this link
        links.Each(new[] { linkIndex, links.Constants.Null }, deletionRecord =>
        {
            var deletionRecordArray = links.GetLink(deletionRecord[0]);
            var deletionTimestamp = GetLinkTimestamp(links, deletionRecordArray);
            
            if ((ulong)deletionTimestamp <= (ulong)timestamp)
            {
                isDeleted = true;
                return links.Constants.Break;
            }
            
            return links.Constants.Continue;
        });
        
        return isDeleted;
    }

    private static bool MatchesRestriction<TLinkAddress>(IList<TLinkAddress> linkArray, IList<TLinkAddress>? restriction) 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        if (restriction == null || restriction.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < Math.Min(restriction.Count, linkArray.Count); i++)
        {
            // Skip elements that are marked as "Any" (typically 0 or specific constant)
            if (restriction[i] != default(TLinkAddress) && restriction[i] != linkArray[i])
            {
                return false;
            }
        }

        return true;
    }
}
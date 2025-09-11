using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents the association operator for handling () syntax in explicit association indication.
    /// This operator enables parsing and formatting of tuple-like associations such as "1"("1") or 1(2(3)).
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class AssociationOperator<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _links;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="AssociationOperator{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssociationOperator(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// <para>
        /// Formats a link using the association operator () syntax.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A string representation using association syntax like source(target).</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Format(TLinkAddress source, TLinkAddress target)
        {
            return $"{source}({target})";
        }

        /// <summary>
        /// <para>
        /// Formats a link using the association operator () syntax with link content inspection.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address to format.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A string representation using association syntax.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Format(TLinkAddress linkAddress)
        {
            if (!_links.Exists(linkAddress))
            {
                return linkAddress.ToString();
            }

            var link = _links.GetLink(linkAddress);
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);
            
            return Format(source, target);
        }

        /// <summary>
        /// <para>
        /// Formats a nested association structure recursively.
        /// For example: 1(2(3)) represents a nested association where 1 is associated with (2 associated with 3).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The root link address to format.</para>
        /// <para></para>
        /// </param>
        /// <param name="maxDepth">
        /// <para>Maximum recursion depth to prevent infinite loops.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A string representation using nested association syntax.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string FormatNested(TLinkAddress linkAddress, int maxDepth = 10)
        {
            return FormatNestedInternal(linkAddress, maxDepth, new HashSet<TLinkAddress>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormatNestedInternal(TLinkAddress linkAddress, int maxDepth, HashSet<TLinkAddress> visited)
        {
            if (maxDepth <= 0 || visited.Contains(linkAddress) || !_links.Exists(linkAddress))
            {
                return linkAddress.ToString();
            }

            visited.Add(linkAddress);
            
            var link = _links.GetLink(linkAddress);
            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);

            var sourceStr = FormatNestedInternal(source, maxDepth - 1, visited);
            var targetStr = FormatNestedInternal(target, maxDepth - 1, visited);

            visited.Remove(linkAddress);
            
            return $"{sourceStr}({targetStr})";
        }

        /// <summary>
        /// <para>
        /// Creates an association between two link addresses using the () operator semantics.
        /// This method creates or finds a link that represents source(target).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the association link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress CreateAssociation(TLinkAddress source, TLinkAddress target)
        {
            return _links.GetOrCreate(source, target);
        }

        /// <summary>
        /// <para>
        /// Parses a simple association string like "source(target)" and creates the corresponding link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="associationString">
        /// <para>The association string to parse.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the created association link, or default if parsing fails.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Parse(string associationString)
        {
            if (string.IsNullOrWhiteSpace(associationString))
            {
                return default;
            }

            var match = Regex.Match(associationString.Trim(), @"^(.+?)\((.+)\)$");
            if (!match.Success)
            {
                return default;
            }

            var sourceStr = match.Groups[1].Value.Trim();
            var targetStr = match.Groups[2].Value.Trim();

            if (TryParseAddress(sourceStr, out var source) && TryParseAddress(targetStr, out var target))
            {
                return CreateAssociation(source, target);
            }

            return default;
        }

        /// <summary>
        /// <para>
        /// Attempts to parse a string representation of a link address.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="addressStr">
        /// <para>The string representation of the address.</para>
        /// <para></para>
        /// </param>
        /// <param name="address">
        /// <para>The parsed address if successful.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if parsing was successful, false otherwise.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryParseAddress(string addressStr, out TLinkAddress address)
        {
            address = default;
            
            if (string.IsNullOrWhiteSpace(addressStr))
            {
                return false;
            }

            // Remove quotes if present
            var cleaned = addressStr.Trim('"', '\'');
            
            // Try parsing as number
            if (ulong.TryParse(cleaned, out var numericValue))
            {
                address = TLinkAddress.CreateTruncating(numericValue);
                return true;
            }

            // For string values, we could create a mapping or hash, but for now return false
            return false;
        }

        /// <summary>
        /// <para>
        /// Checks if two associations are equivalent.
        /// For example: 1(2(3)) != 1(2)(3) - different nesting structures.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="first">
        /// <para>The first association address.</para>
        /// <para></para>
        /// </param>
        /// <param name="second">
        /// <para>The second association address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if the associations have the same structure, false otherwise.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreEquivalent(TLinkAddress first, TLinkAddress second)
        {
            if (EqualityComparer<TLinkAddress>.Default.Equals(first, second))
            {
                return true;
            }

            if (!_links.Exists(first) || !_links.Exists(second))
            {
                return false;
            }

            var firstLink = _links.GetLink(first);
            var secondLink = _links.GetLink(second);

            var firstSource = _links.GetSource(firstLink);
            var firstTarget = _links.GetTarget(firstLink);
            var secondSource = _links.GetSource(secondLink);
            var secondTarget = _links.GetTarget(secondLink);

            return EqualityComparer<TLinkAddress>.Default.Equals(firstSource, secondSource) && 
                   EqualityComparer<TLinkAddress>.Default.Equals(firstTarget, secondTarget);
        }
    }
}
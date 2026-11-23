using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Extension methods for the association operator () functionality.
    /// Provides convenient methods for working with association syntax in Links.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class AssociationExtensions
    {
        /// <summary>
        /// <para>
        /// Creates an association operator for the given links storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A new AssociationOperator instance.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssociationOperator<TLinkAddress> Association<TLinkAddress>(this ILinks<TLinkAddress> links) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return new AssociationOperator<TLinkAddress>(links);
        }

        /// <summary>
        /// <para>
        /// Formats a link using association operator syntax: source(target).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="source">
        /// <para>The source link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A string representation using association syntax.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatAsAssociation<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().Format(source, target);
        }

        /// <summary>
        /// <para>
        /// Formats a link using association operator syntax with link content inspection.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="linkAddress">
        /// <para>The link address to format.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A string representation using association syntax.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatAsAssociation<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkAddress) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().Format(linkAddress);
        }

        /// <summary>
        /// <para>
        /// Formats a nested association structure recursively.
        /// For example: 1(2(3)) represents nested associations.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
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
        public static string FormatAsNestedAssociation<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress linkAddress, int maxDepth = 10) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().FormatNested(linkAddress, maxDepth);
        }

        /// <summary>
        /// <para>
        /// Creates an association between two link addresses.
        /// This method creates or finds a link that represents source(target).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
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
        public static TLinkAddress CreateAssociation<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress source, TLinkAddress target) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().CreateAssociation(source, target);
        }

        /// <summary>
        /// <para>
        /// Parses an association string and creates the corresponding link structure.
        /// Supports syntax like "source(target)" or nested forms.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="associationString">
        /// <para>The association string to parse.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The address of the created association link, or default if parsing fails.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress ParseAssociation<TLinkAddress>(this ILinks<TLinkAddress> links, string associationString) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().Parse(associationString);
        }

        /// <summary>
        /// <para>
        /// Checks if two associations are structurally equivalent.
        /// For example: 1(2(3)) != 1(2)(3) due to different nesting.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
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
        public static bool AreAssociationsEquivalent<TLinkAddress>(this ILinks<TLinkAddress> links, TLinkAddress first, TLinkAddress second) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            return links.Association().AreEquivalent(first, second);
        }
    }
}
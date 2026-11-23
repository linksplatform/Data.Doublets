using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.United.Generic
{
    /// <summary>
    ///     <para>
    ///         Represents the links sources bitstring index methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <seealso cref="LinksBitStringIndexMethodsBase{TLinkAddress}" />
    public unsafe class LinksSourcesBitStringIndexMethods<TLinkAddress> : LinksBitStringIndexMethodsBase<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new <see cref="LinksSourcesBitStringIndexMethods" /> instance.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="constants">
        ///     <para>A constants.</para>
        ///     <para></para>
        /// </param>
        /// <param name="links">
        ///     <para>A links.</para>
        ///     <para></para>
        /// </param>
        /// <param name="header">
        ///     <para>A header.</para>
        ///     <para></para>
        /// </param>
        /// <param name="defaultBitStringSize">
        ///     <para>The default bitstring size.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksSourcesBitStringIndexMethods(LinksConstants<TLinkAddress> constants, byte* links, byte* header, int defaultBitStringSize = 1024)
            : base(constants, links, header, defaultBitStringSize)
        {
        }
    }
}
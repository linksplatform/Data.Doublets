using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.Split.Generic
{
    /// <summary>
    ///     <para>
    ///         Represents the external links bitstring index methods.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <seealso cref="BitStringIndexMethodsBase{TLinkAddress}" />
    public unsafe class ExternalLinksBitStringIndexMethods<TLinkAddress> : BitStringIndexMethodsBase<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new <see cref="ExternalLinksBitStringIndexMethods" /> instance.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="constants">
        ///     <para>A constants.</para>
        ///     <para></para>
        /// </param>
        /// <param name="linksDataParts">
        ///     <para>A links data parts.</para>
        ///     <para></para>
        /// </param>
        /// <param name="linksIndexParts">
        ///     <para>A links index parts.</para>
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
        public ExternalLinksBitStringIndexMethods(LinksConstants<TLinkAddress> constants, byte* linksDataParts, byte* linksIndexParts, byte* header, int defaultBitStringSize = 1024)
            : base(constants, linksDataParts, linksIndexParts, header, defaultBitStringSize)
        {
        }

        /// <summary>
        ///     <para>
        ///         Gets the link data at the specified index.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="link">
        ///     <para>The link index.</para>
        ///     <para></para>
        /// </param>
        /// <param name="index">
        ///     <para>The data index.</para>
        ///     <para></para>
        /// </param>
        /// <returns>
        ///     <para>The link data.</para>
        ///     <para></para>
        /// </returns>
        public TLinkAddress this[TLinkAddress link, TLinkAddress index]
        {
            get
            {
                var linkIndexPartPointer = (TLinkAddress*)(LinksIndexParts + (long)(ulong.CreateTruncating(link) * (ulong)sizeof(RawLinkIndexPart<TLinkAddress>)));
                return Add(ref AsRef<TLinkAddress>(linkIndexPartPointer), int.CreateTruncating(index));
            }
            set
            {
                var linkIndexPartPointer = (TLinkAddress*)(LinksIndexParts + (long)(ulong.CreateTruncating(link) * (ulong)sizeof(RawLinkIndexPart<TLinkAddress>)));
                Add(ref AsRef<TLinkAddress>(linkIndexPartPointer), int.CreateTruncating(index)) = value;
            }
        }
    }
}
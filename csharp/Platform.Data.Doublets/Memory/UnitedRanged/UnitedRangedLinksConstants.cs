using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Ranges;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged
{
    /// <summary>
    /// <para>
    /// Extension of <see cref="LinksConstants{TLinkAddress}"/> used by
    /// <see cref="Generic.UnitedRangedMemoryLinks{TLinkAddress}"/>. Exposes two
    /// additional sentinel values stored inside the <c>Source</c> word:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="RawLinkSequenceMarker"/> tags the first cell of a raw link sequence.</item>
    /// <item><see cref="FreeRangeMarker"/> tags the first cell of a multi-cell free range.</item>
    /// </list>
    /// <para>
    /// Both markers reuse housekeeping slots that <see cref="LinksConstants{TLinkAddress}"/>
    /// already reserves above <c>InternalReferencesRange.Maximum</c>, so they cannot
    /// collide with any valid link index.
    /// </para>
    /// </summary>
    public class UnitedRangedLinksConstants<TLinkAddress> : LinksConstants<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// Sentinel stored in the <c>Source</c> word to designate that a cell is the
        /// first cell of a raw link sequence. Reuses the
        /// <see cref="LinksConstants{TLinkAddress}.Itself"/> slot — a housekeeping value
        /// that is never persisted as a link reference.
        /// </summary>
        public TLinkAddress RawLinkSequenceMarker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Sentinel stored in the <c>Source</c> word to designate that a cell is the
        /// first cell of a multi-cell free range. Reuses the
        /// <see cref="LinksConstants{TLinkAddress}.Error"/> slot — a housekeeping value
        /// that is never persisted as a link reference.
        /// </summary>
        public TLinkAddress FreeRangeMarker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedLinksConstants()
        {
            RawLinkSequenceMarker = Itself;
            FreeRangeMarker = Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedLinksConstants(bool enableExternalReferencesSupport) : base(enableExternalReferencesSupport)
        {
            RawLinkSequenceMarker = Itself;
            FreeRangeMarker = Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedLinksConstants(Range<TLinkAddress> possibleInternalReferencesRange) : base(possibleInternalReferencesRange)
        {
            RawLinkSequenceMarker = Itself;
            FreeRangeMarker = Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitedRangedLinksConstants(Range<TLinkAddress> possibleInternalReferencesRange, Range<TLinkAddress>? possibleExternalReferencesRange) : base(possibleInternalReferencesRange, possibleExternalReferencesRange)
        {
            RawLinkSequenceMarker = Itself;
            FreeRangeMarker = Error;
        }
    }
}

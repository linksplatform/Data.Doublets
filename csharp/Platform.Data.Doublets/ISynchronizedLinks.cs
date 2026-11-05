using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Defines the synchronized links.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="Platform.Data.ISynchronizedLinks{TLinkAddress, TLinks, TConstants}"/>
    /// <seealso cref="ILinks{TLinkAddress}"/>
    public interface ISynchronizedLinks<TLinkAddress> : ISynchronizedLinks<TLinkAddress, ILinks<TLinkAddress>, LinksConstants<TLinkAddress>>, ILinks<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
    }
}

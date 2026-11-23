
using System.Collections.Generic;
using System.Numerics;

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Defines the links.
    /// </para>
    /// <para></para> 
    /// </summary>
    /// <seealso cref="Platform.Data.ILinks{TLinkAddress, TConstants}"/>
    public interface ILinks<TLinkAddress> : ILinks<TLinkAddress, LinksConstants<TLinkAddress>> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
    }
}

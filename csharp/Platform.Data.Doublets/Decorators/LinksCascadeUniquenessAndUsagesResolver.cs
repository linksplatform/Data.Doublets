using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    /// <summary>
    /// <para>
    /// Represents the links cascade uniqueness and usages resolver.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <seealso cref="LinksUniquenessResolver{TLink}"/>
    public class LinksCascadeUniquenessAndUsagesResolver<TLink> : LinksUniquenessResolver<TLink>
    {
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="LinksCascadeUniquenessAndUsagesResolver"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>A links.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksCascadeUniquenessAndUsagesResolver(ILinks<TLink> links) : base(links) { }

        /// <summary>
        /// <para>
        /// Resolves the address change conflict using the specified old link address.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="oldLinkAddress">
        /// <para>The old link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="newLinkAddress">
        /// <para>The new link address.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress, WriteHandler<TLink> handler)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var constants = _links.Constants;
            TLink handlerState = constants.Continue;
            // Use Facade (the last decorator) to ensure recursion working correctly
            var handlerStateAfterMergeUsages = _facade.MergeUsages(oldLinkAddress, newLinkAddress, handler);
            if (equalityComparer.Equals(constants.Break, handlerStateAfterMergeUsages))
            {
                handler = null;
                handlerState = handlerStateAfterMergeUsages;
            }
            var handlerStateAfterBaseResolveAddressChangeConflict = base.ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress, handler);
            if (equalityComparer.Equals(constants.Break, handlerStateAfterBaseResolveAddressChangeConflict))
            {
                handler = null;
                handlerState = handlerStateAfterBaseResolveAddressChangeConflict;
            }
            return handlerState;
        }
    }
}

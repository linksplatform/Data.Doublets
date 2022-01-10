using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    /// <remarks>
    /// <para>Must be used in conjunction with NonNullContentsLinkDeletionResolver.</para>
    /// <para>Должен использоваться вместе с NonNullContentsLinkDeletionResolver.</para>
    /// </remarks>
    public class LinksCascadeUsagesResolver<TLink> : LinksDecoratorBase<TLink>
    {
        /// <summary>
        /// <para>
        /// Initializes a new <see cref="LinksCascadeUsagesResolver"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>A links.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksCascadeUsagesResolver(ILinks<TLink> links) : base(links) { }

        /// <summary>
        /// <para>
        /// Deletes the restriction.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLink Delete(IList<TLink> restriction, WriteHandler<TLink> handler)
        {
            var equalityComparer = EqualityComparer<TLink>.Default;
            var constants = _links.Constants;
            TLink handlerState = constants.Continue;
            var linkIndex = restriction[_constants.IndexPart];
            // Use Facade (the last decorator) to ensure recursion working correctly
            var handlerStateAfterDeleteAllUsages = _facade.DeleteAllUsages(linkIndex, handler);
            if (equalityComparer.Equals(constants.Break, handlerStateAfterDeleteAllUsages))
            {
                handler = null;
                handlerState = handlerStateAfterDeleteAllUsages;
            }
            var handlerStateAfterDelete = _links.Delete(restriction, handler);
            if (equalityComparer.Equals(constants.Break, handlerStateAfterDelete))
            {
                handler = null;
                handlerState = handlerStateAfterDelete;
            }
            return handlerState;
        }
    }
}

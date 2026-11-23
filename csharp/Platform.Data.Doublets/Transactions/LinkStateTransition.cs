using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Represents a transition from one link state to another.
    /// </para>
    /// <para></para>
    /// </summary>
    public readonly struct LinkStateTransition<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// Gets the link address.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly TLinkAddress LinkAddress { get; }

        /// <summary>
        /// <para>
        /// Gets the state before the transition (source state).
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly IList<TLinkAddress>? BeforeState { get; }

        /// <summary>
        /// <para>
        /// Gets the state after the transition (target state).
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly IList<TLinkAddress>? AfterState { get; }

        /// <summary>
        /// <para>
        /// Gets the type of operation that caused this transition.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly TransitionType TransitionType { get; }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="LinkStateTransition{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="linkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="beforeState">
        /// <para>The state before transition.</para>
        /// <para></para>
        /// </param>
        /// <param name="afterState">
        /// <para>The state after transition.</para>
        /// <para></para>
        /// </param>
        /// <param name="transitionType">
        /// <para>The type of transition.</para>
        /// <para></para>
        /// </param>
        public LinkStateTransition(TLinkAddress linkAddress, IList<TLinkAddress>? beforeState, IList<TLinkAddress>? afterState, TransitionType transitionType)
        {
            LinkAddress = linkAddress;
            BeforeState = beforeState;
            AfterState = afterState;
            TransitionType = transitionType;
        }

        /// <summary>
        /// <para>
        /// Returns a value indicating whether the transition represents a creation.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>True if this is a creation transition.</para>
        /// <para></para>
        /// </returns>
        public bool IsCreation => TransitionType == TransitionType.Create;

        /// <summary>
        /// <para>
        /// Returns a value indicating whether the transition represents an update.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>True if this is an update transition.</para>
        /// <para></para>
        /// </returns>
        public bool IsUpdate => TransitionType == TransitionType.Update;

        /// <summary>
        /// <para>
        /// Returns a value indicating whether the transition represents a deletion.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>True if this is a deletion transition.</para>
        /// <para></para>
        /// </returns>
        public bool IsDeletion => TransitionType == TransitionType.Delete;
    }
}
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Transactions
{
    /// <summary>
    /// <para>
    /// Represents the type of state transition.
    /// </para>
    /// <para></para>
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// <para>
        /// Create transition - link is being created.
        /// </para>
        /// <para></para>
        /// </summary>
        Create,

        /// <summary>
        /// <para>
        /// Update transition - link is being updated.
        /// </para>
        /// <para></para>
        /// </summary>
        Update,

        /// <summary>
        /// <para>
        /// Delete transition - link is being deleted.
        /// </para>
        /// <para></para>
        /// </summary>
        Delete
    }
}
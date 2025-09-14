using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences.Frequencies.Cache
{
    /// <summary>
    /// Provides a counter interface for counting occurrences.
    /// </summary>
    /// <typeparam name="TInput">The type of input to count.</typeparam>
    /// <typeparam name="TOutput">The type of output count.</typeparam>
    public interface ICounter<TInput, TOutput> where TOutput : IUnsignedNumber<TOutput>
    {
        /// <summary>
        /// Counts the occurrences of the specified input.
        /// </summary>
        /// <param name="input">The input to count.</param>
        /// <returns>The count of occurrences.</returns>
        TOutput Count(TInput input);
    }
}
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences.Frequencies.Cache
{
    /// <summary>
    /// Default implementation of ICounter that always returns zero.
    /// This can be used when no specific counting logic is needed.
    /// </summary>
    public class DefaultCounter<TInput, TOutput> : ICounter<TInput, TOutput> where TOutput : IUnsignedNumber<TOutput>
    {
        /// <summary>
        /// Gets the default instance of the counter.
        /// </summary>
        public static readonly DefaultCounter<TInput, TOutput> Instance = new DefaultCounter<TInput, TOutput>();

        /// <summary>
        /// Counts the occurrences of the specified input.
        /// </summary>
        /// <param name="input">The input to count.</param>
        /// <returns>Always returns zero.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOutput Count(TInput input)
        {
            return TOutput.Zero;
        }
    }
}
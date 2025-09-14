using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences.Frequencies.Cache
{
    /// <summary>
    /// Represents a frequency count for a specific link.
    /// </summary>
    public class LinkFrequency<TLink> where TLink : IUnsignedNumber<TLink>
    {
        /// <summary>
        /// Gets or sets the frequency count.
        /// </summary>
        public TLink Frequency { get; set; }

        /// <summary>
        /// Gets or sets the link identifier.
        /// </summary>
        public TLink Link { get; set; }

        /// <summary>
        /// Initializes a new instance of the LinkFrequency class.
        /// </summary>
        /// <param name="frequency">The initial frequency count.</param>
        /// <param name="link">The link identifier.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkFrequency(TLink frequency, TLink link)
        {
            Frequency = frequency;
            Link = link;
        }

        /// <summary>
        /// Increments the frequency count by one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementFrequency()
        {
            Frequency = Frequency + TLink.One;
        }

        /// <summary>
        /// Returns a string representation of the LinkFrequency.
        /// </summary>
        /// <returns>A string containing the link and frequency information.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"Link: {Link}, Frequency: {Frequency}";
        }
    }
}
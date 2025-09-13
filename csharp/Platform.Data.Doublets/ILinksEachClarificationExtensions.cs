using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;

namespace Platform.Data.Doublets
{
    /// <summary>
    /// Extension methods to clarify Each method behavior and address Issue #173.
    /// 
    /// These extensions provide explicit methods to avoid confusion about 
    /// Constants.Null vs Constants.Any usage in Each method calls.
    /// </summary>
    public static class ILinksEachClarificationExtensions
    {
        /// <summary>
        /// Iterates through all links without restrictions.
        /// Equivalent to links.Each(handler) or links.Each(handler, Constants.Any).
        /// </summary>
        /// <typeparam name="TLinkAddress">The link address type.</typeparam>
        /// <param name="links">The links instance.</param>
        /// <param name="handler">The handler function to execute for each link.</param>
        /// <returns>The result of the Each operation.</returns>
        public static TLinkAddress EachAllLinks<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler)
            where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>
        {
            return links.Each(handler);
        }

        /// <summary>
        /// Iterates through links matching any value (wildcard behavior).
        /// Explicitly uses Constants.Any to avoid confusion with Constants.Null.
        /// </summary>
        /// <typeparam name="TLinkAddress">The link address type.</typeparam>
        /// <param name="links">The links instance.</param>
        /// <param name="handler">The handler function to execute for each matching link.</param>
        /// <returns>The result of the Each operation.</returns>
        public static TLinkAddress EachAnyLink<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler)
            where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>
        {
            return links.Each(handler, links.Constants.Any);
        }

        /// <summary>
        /// Searches for the link at index 0 (literal zero, not Constants.Null semantic meaning).
        /// This method makes it explicit when you want to search for the link at index 0.
        /// </summary>
        /// <typeparam name="TLinkAddress">The link address type.</typeparam>
        /// <param name="links">The links instance.</param>
        /// <param name="handler">The handler function to execute if the link exists.</param>
        /// <returns>The result of the Each operation.</returns>
        public static TLinkAddress EachLinkAtIndexZero<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler)
            where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>
        {
            var zero = TLinkAddress.Zero;
            return links.Each(handler, zero);
        }

        /// <summary>
        /// Validates Each parameters to help identify potential misuse of Constants.Null.
        /// Issues warnings when Constants.Null is used in restriction parameters.
        /// </summary>
        /// <typeparam name="TLinkAddress">The link address type.</typeparam>
        /// <param name="links">The links instance.</param>
        /// <param name="handler">The handler function.</param>
        /// <param name="restriction">The restriction parameters.</param>
        /// <returns>The result of the Each operation.</returns>
        /// <remarks>
        /// This method helps identify cases where Constants.Null might be used incorrectly
        /// as a wildcard when Constants.Any should be used instead.
        /// </remarks>
        public static TLinkAddress EachWithNullValidation<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler, params TLinkAddress[] restriction)
            where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>
        {
            var constants = links.Constants;
            
            // Check for potential misuse of Constants.Null
            for (int i = 0; i < restriction.Length; i++)
            {
                if (restriction[i].Equals(constants.Null))
                {
                    System.Console.WriteLine($"[Issue #173 Warning] Constants.Null detected at position {i} in Each restriction.");
                    System.Console.WriteLine("This will search for links with index 0. Consider using Constants.Any for wildcard behavior.");
                }
            }
            
            return links.Each(handler, restriction);
        }
    }
}
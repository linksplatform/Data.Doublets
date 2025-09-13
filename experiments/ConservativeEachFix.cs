using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Data.Doublets;

namespace Platform.Data.Doublets.Experiments
{
    /// <summary>
    /// Conservative fix for Each method behavior when null/default values are passed.
    /// 
    /// ISSUE ANALYSIS:
    /// The original issue is about confusing behavior when Constants.Null (0) is passed 
    /// as a restriction. Currently it's treated as literal link index 0, which is confusing.
    /// 
    /// CONSERVATIVE SOLUTION:
    /// 1. Add clear documentation about the current behavior
    /// 2. Provide extension methods for clearer usage
    /// 3. Add validation to warn when null constants are used inappropriately
    /// 
    /// This avoids breaking existing functionality while making the behavior clear.
    /// </summary>
    public static class ConservativeEachFix
    {
        /// <summary>
        /// Demonstrates the current behavior and provides clear guidance
        /// </summary>
        public static void DocumentCurrentBehavior()
        {
            Console.WriteLine("=== Each Method Behavior with Constants.Null ===");
            Console.WriteLine();
            Console.WriteLine("CURRENT BEHAVIOR (as of fix for issue #173):");
            Console.WriteLine("When Constants.Null is passed as a restriction parameter:");
            Console.WriteLine("- It is treated as a literal link index (value 0)");  
            Console.WriteLine("- If link with index 0 exists: returns that link");
            Console.WriteLine("- If link with index 0 doesn't exist: returns Continue (no matches)");
            Console.WriteLine();
            Console.WriteLine("CLARIFICATION:");
            Console.WriteLine("Constants.Null should be used semantically for 'null/empty' values in link contents");
            Console.WriteLine("For 'any/wildcard' behavior in restrictions, use Constants.Any instead");
            Console.WriteLine();
            Console.WriteLine("RECOMMENDED USAGE:");
            Console.WriteLine("✓ links.Each(handler, Constants.Any)           // Match any link");
            Console.WriteLine("✓ links.Each(handler, specificLinkIndex)       // Match specific link");
            Console.WriteLine("⚠ links.Each(handler, Constants.Null)          // Matches link at index 0 (if exists)");
            Console.WriteLine("? Use Constants.Any if you want wildcard behavior");
        }
        
        /// <summary>
        /// Provides extension methods for clearer Each usage
        /// </summary>
        public static class ClarifiedEachExtensions
        {
            /// <summary>
            /// Clearly iterates all links (equivalent to Each with no restrictions)
            /// </summary>
            public static TLinkAddress EachAllLinks<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler)
                where TLinkAddress : IUnsignedNumber<TLinkAddress>
            {
                return links.Each(handler);
            }
            
            /// <summary>
            /// Clearly searches for links matching specific criteria
            /// </summary>
            public static TLinkAddress EachMatching<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler, TLinkAddress index, TLinkAddress source, TLinkAddress target)
                where TLinkAddress : IUnsignedNumber<TLinkAddress>
            {
                return links.Each(handler, index, source, target);
            }
            
            /// <summary>
            /// Validates restriction parameters to catch potential misuse of Constants.Null
            /// </summary>
            public static TLinkAddress EachWithValidation<TLinkAddress>(this ILinks<TLinkAddress> links, ReadHandler<TLinkAddress>? handler, params TLinkAddress[] restriction)
                where TLinkAddress : IUnsignedNumber<TLinkAddress>
            {
                var constants = links.Constants;
                
                // Check for potential misuse of Constants.Null as wildcard
                for (int i = 0; i < restriction.Length; i++)
                {
                    if (restriction[i].Equals(constants.Null))
                    {
                        Console.WriteLine($"WARNING: Constants.Null found at position {i} in Each restriction.");
                        Console.WriteLine("This will search for links with index 0. Did you mean Constants.Any for wildcard behavior?");
                    }
                }
                
                return links.Each(handler, restriction);
            }
        }
    }
}
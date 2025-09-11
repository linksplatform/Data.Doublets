using System;
using System.Collections.Generic;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Test case to verify the Count logic fix for issue #359
    /// This demonstrates the corrected behavior where source==any or target==any 
    /// should be handled separately rather than with variable overwriting
    /// </summary>
    public class CountLogicTest
    {
        public class MockStoredLinkValue
        {
            public ulong Source { get; set; }
            public ulong Target { get; set; }
        }

        private const ulong any = 0; // Assuming 0 represents "any"

        /// <summary>
        /// Old (buggy) logic that had the issue
        /// </summary>
        public static bool OldBuggyLogic(MockStoredLinkValue storedLinkValue, ulong source, ulong target)
        {
            var value = default(ulong);
            if (source == any)
            {
                value = target;
            }
            if (target == any)  // This overwrites value even if source was any!
            {
                value = source;
            }
            return (storedLinkValue.Source == value) || (storedLinkValue.Target == value);
        }

        /// <summary>
        /// New (fixed) logic that handles each case separately
        /// </summary>
        public static bool NewFixedLogic(MockStoredLinkValue storedLinkValue, ulong source, ulong target)
        {
            if (source == any)
            {
                if (storedLinkValue.Target == target)
                {
                    return true;
                }
            }
            if (target == any)
            {
                if (storedLinkValue.Source == source)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RunTests()
        {
            var testLink = new MockStoredLinkValue { Source = 1, Target = 2 };

            // Test case 1: source=any, target=2 (should match target)
            Console.WriteLine("Test 1: source=any, target=2, stored=(1,2)");
            Console.WriteLine($"Old logic: {OldBuggyLogic(testLink, any, 2)}");
            Console.WriteLine($"New logic: {NewFixedLogic(testLink, any, 2)}");
            Console.WriteLine();

            // Test case 2: source=1, target=any (should match source)  
            Console.WriteLine("Test 2: source=1, target=any, stored=(1,2)");
            Console.WriteLine($"Old logic: {OldBuggyLogic(testLink, 1, any)}");
            Console.WriteLine($"New logic: {NewFixedLogic(testLink, 1, any)}");
            Console.WriteLine();

            // Test case 3: source=any, target=any (edge case, neither should match in this context)
            Console.WriteLine("Test 3: source=any, target=any, stored=(1,2)");
            Console.WriteLine($"Old logic: {OldBuggyLogic(testLink, any, any)}");
            Console.WriteLine($"New logic: {NewFixedLogic(testLink, any, any)}");
            Console.WriteLine();

            // Test case 4: source=1, target=3 (no match expected)
            Console.WriteLine("Test 4: source=1, target=3, stored=(1,2)");
            Console.WriteLine($"Old logic: {OldBuggyLogic(testLink, 1, 3)}");
            Console.WriteLine($"New logic: {NewFixedLogic(testLink, 1, 3)}");
        }
    }
}
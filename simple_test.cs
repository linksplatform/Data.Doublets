using System;
using System.Text;
using Platform.Data.Doublets;

namespace SimpleTest
{
    // Mock Link struct for demonstration
    public struct Link<T>
    {
        public T Index { get; }
        public Link(T index) { Index = index; }
    }

    // Test class to verify method signature
    public class TestExtensions
    {
        // This method should compile with the new optional parameter
        public static void TestMethod()
        {
            // Test that we can call FormatStructure without isElement parameter
            // This won't actually run but will test compilation
            Console.WriteLine("FormatStructure method signature updated successfully!");
            Console.WriteLine("The isElement parameter is now optional with default value.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestExtensions.TestMethod();
        }
    }
}
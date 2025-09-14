using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class QuickPerformanceTest
{
    private const int iterations = 100_000_000; // 100 million
    
    public static void Main()
    {
        Console.WriteLine("Performance comparison: GetZero() method vs Zero literal vs default keyword");
        Console.WriteLine($"Testing with {iterations:N0} iterations each\n");
        
        // Warm up
        Console.WriteLine("Warming up...");
        TestGetZeroMethodWithInlining(1000000);
        TestZeroLiteral(1000000);
        TestDefaultKeyword(1000000);
        TestGetZeroMethodWithoutInlining(1000000);
        
        Console.WriteLine("Running tests...\n");
        
        // Test 1: GetZero() method with aggressive inlining
        var sw = Stopwatch.StartNew();
        var result1 = TestGetZeroMethodWithInlining(iterations);
        sw.Stop();
        var time1 = sw.Elapsed;
        
        // Test 2: Zero literal (0UL)
        sw.Restart();
        var result2 = TestZeroLiteral(iterations);
        sw.Stop();
        var time2 = sw.Elapsed;
        
        // Test 3: default keyword
        sw.Restart();
        var result3 = TestDefaultKeyword(iterations);
        sw.Stop();
        var time3 = sw.Elapsed;
        
        // Test 4: GetZero() method without inlining
        sw.Restart();
        var result4 = TestGetZeroMethodWithoutInlining(iterations);
        sw.Stop();
        var time4 = sw.Elapsed;
        
        // Results
        Console.WriteLine("Results:");
        Console.WriteLine($"1. GetZero() with inlining:    {time1.TotalMilliseconds:F2}ms (Result: {result1})");
        Console.WriteLine($"2. Zero literal (0UL):         {time2.TotalMilliseconds:F2}ms (Result: {result2})");
        Console.WriteLine($"3. default keyword:            {time3.TotalMilliseconds:F2}ms (Result: {result3})");
        Console.WriteLine($"4. GetZero() without inlining: {time4.TotalMilliseconds:F2}ms (Result: {result4})");
        
        // Calculate relative performance
        var baseline = time2.TotalMilliseconds; // Use zero literal as baseline
        Console.WriteLine("\nRelative performance (compared to zero literal):");
        Console.WriteLine($"GetZero() with inlining:    {(time1.TotalMilliseconds / baseline):F2}x");
        Console.WriteLine($"Zero literal (baseline):    1.00x");
        Console.WriteLine($"default keyword:            {(time3.TotalMilliseconds / baseline):F2}x");
        Console.WriteLine($"GetZero() without inlining: {(time4.TotalMilliseconds / baseline):F2}x");
        
        // Analysis
        Console.WriteLine("\nAnalysis:");
        if (Math.Abs(time1.TotalMilliseconds - time2.TotalMilliseconds) < time2.TotalMilliseconds * 0.05)
        {
            Console.WriteLine("✓ GetZero() with inlining performs equivalently to zero literal (~same performance)");
        }
        else if (time1.TotalMilliseconds < time2.TotalMilliseconds)
        {
            Console.WriteLine("✓ GetZero() with inlining is faster than zero literal");
        }
        else
        {
            Console.WriteLine("✗ GetZero() with inlining is slower than zero literal");
        }
        
        if (time4.TotalMilliseconds > time2.TotalMilliseconds * 1.1)
        {
            Console.WriteLine("✗ GetZero() without inlining has significant performance penalty");
        }
        else
        {
            Console.WriteLine("✓ GetZero() without inlining has minimal performance penalty");
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetZero() => default;
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong GetZeroNoInlining() => default;
    
    private static ulong TestGetZeroMethodWithInlining(int iterations)
    {
        ulong sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum += GetZero();
        }
        return sum;
    }
    
    private static ulong TestZeroLiteral(int iterations)
    {
        ulong sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum += 0UL;
        }
        return sum;
    }
    
    private static ulong TestDefaultKeyword(int iterations)
    {
        ulong sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum += default(ulong);
        }
        return sum;
    }
    
    private static ulong TestGetZeroMethodWithoutInlining(int iterations)
    {
        ulong sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            sum += GetZeroNoInlining();
        }
        return sum;
    }
}
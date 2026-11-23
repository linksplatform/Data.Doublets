using System;
using Platform.Data.Doublets;
using Platform.Data;

Console.WriteLine("Inspecting LinksConstants constructor");

// Try to understand LinksConstants constructor signature
var linksConstantsType = typeof(LinksConstants<ulong>);
var constructors = linksConstantsType.GetConstructors();

Console.WriteLine($"LinksConstants<ulong> has {constructors.Length} constructor(s):");

foreach (var ctor in constructors)
{
    var parameters = ctor.GetParameters();
    Console.WriteLine($"Constructor with {parameters.Length} parameter(s):");
    for (int i = 0; i < parameters.Length; i++)
    {
        var param = parameters[i];
        Console.WriteLine($"  [{i}] {param.Name}: {param.ParameterType}");
    }
    Console.WriteLine();
}

// Now test the actual working call
Console.WriteLine("Testing working constructor call:");
try
{
    var constants = new LinksConstants<ulong>((1, long.MaxValue), (long.MaxValue + 1UL, ulong.MaxValue));
    Console.WriteLine("✓ Working constructor call succeeded");
}
catch (Exception e)
{
    Console.WriteLine($"✗ Working constructor call failed: {e.Message}");
}

// Test the types of the working parameters
var param1 = (1, long.MaxValue);
var param2 = (long.MaxValue + 1UL, ulong.MaxValue);

Console.WriteLine($"Working param1 type: {param1.GetType()}");
Console.WriteLine($"Working param2 type: {param2.GetType()}");

// Test with explicit Range objects
Console.WriteLine("\nTesting with explicit Range objects:");

try
{
    var range1 = new Platform.Ranges.Range<ulong>(1UL, (ulong)long.MaxValue);
    var range2 = new Platform.Ranges.Range<ulong>((ulong)long.MaxValue + 1UL, ulong.MaxValue);
    var constants2 = new LinksConstants<ulong>(range1, range2);
    Console.WriteLine("✓ Explicit Range constructor call succeeded");
}
catch (Exception e)
{
    Console.WriteLine($"✗ Explicit Range constructor call failed: {e.Message}");
}

// Test tuple to Range conversion
Console.WriteLine("\nTesting tuple to Range conversion:");
Console.WriteLine($"Can (1, long.MaxValue) convert to Range<ulong>?");

try
{
    Platform.Ranges.Range<ulong> rangeFromTuple1 = (1, long.MaxValue);
    Console.WriteLine($"✓ Implicit conversion succeeded: {rangeFromTuple1}");
}
catch (Exception e)
{
    Console.WriteLine($"✗ Implicit conversion failed: {e.Message}");
}

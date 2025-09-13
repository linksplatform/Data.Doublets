using System;

// Test experiment to validate bit-by-bit addressing algorithm  
public class Program
{
    public static void Main()
    {
        Console.WriteLine("Testing bit-by-bit addressing algorithm:");
        Console.WriteLine();

        for (int index = 0; index <= 13; index++)
        {
            var (depth, pathBits) = CalculatePathFromIndex(index);
            var pathString = GetPathString(pathBits, depth);
            Console.WriteLine($"Index {index}: depth={depth}, path={pathString}");
        }
    }

    public static (int depth, uint pathBits) CalculatePathFromIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index == 0) return (1, 0); // source  
        if (index == 1) return (1, 1); // target

        // For index >= 2, find which "level" it belongs to
        // Level 1: indices 0-1 (2 elements)
        // Level 2: indices 2-5 (4 elements)  
        // Level 3: indices 6-13 (8 elements)
        // Level d: indices (2^d - 2) to (2^(d+1) - 3) 

        int depth = 1;
        int levelStart = 0;
        int levelSize = 2;
        
        while (index >= levelStart + levelSize)
        {
            levelStart += levelSize;
            levelSize *= 2;
            depth++;
        }

        int offsetInLevel = index - levelStart;
        return (depth, (uint)offsetInLevel);
    }

    public static string GetPathString(uint pathBits, int depth)
    {
        var parts = new string[depth];
        for (int i = 0; i < depth; i++)
        {
            // Read bits from left to right (most significant first for path)
            bool bit = (pathBits & (1u << (depth - 1 - i))) != 0;
            parts[i] = bit ? "target" : "source";
        }
        return string.Join(".", parts);
    }
}
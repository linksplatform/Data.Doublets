# Performance Analysis: Unsafe.SizeOf() vs Size Field

## Issue #90 Analysis

This document presents the findings from comparing the performance of `Unsafe.SizeOf()` method (with aggressive inlining) versus using a pre-calculated Size field approach in the Platform.Data.Doublets codebase.

## Executive Summary

**Key Finding: `Unsafe.SizeOf<T>()` with aggressive inlining is consistently 2-3x faster than accessing a pre-calculated Size field.**

## Test Methodology

- **Environment**: .NET 8, Release mode compilation
- **Iterations**: 10,000,000 per test
- **Test Structures**: 
  - Simple struct (4 x ulong = 32 bytes)
  - RawLink<ulong> (8 x ulong = 64 bytes) 
  - RawLinkDataPart<ulong> (2 x ulong = 16 bytes)
- **Approaches Compared**:
  1. `System.Runtime.CompilerServices.Unsafe.SizeOf<T>()` with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
  2. Static readonly field initialized with `Unsafe.SizeOf<T>()`
  3. `Platform.Unsafe.Structure<T>.Size` (current codebase approach)

## Results Summary

### Performance Ratios (Lower is Better)

| Structure | SizeOf/Field Ratio | Platform.Unsafe/Field Ratio |
|-----------|-------------------|------------------------------|
| TestStruct (32 bytes) | **0.33x** | N/A |
| RawLink (64 bytes) | **0.38x** | 1.10x |
| RawLinkDataPart (16 bytes) | **0.43x** | 0.89x |

**Average Performance Improvement: Unsafe.SizeOf() is ~2.6x faster than Size field access**

### Detailed Results

#### TestStruct (4 x ulong = 32 bytes)
```
Run 1: Unsafe.SizeOf: 9.2ms   vs  Size field: 27.8ms   (Ratio: 0.33x)
Run 2: Unsafe.SizeOf: 39.5ms  vs  Size field: 113.7ms  (Ratio: 0.35x)
Run 3: Unsafe.SizeOf: 17.5ms  vs  Size field: 56.0ms   (Ratio: 0.31x)
Average Ratio: 0.33x - SizeOf is 3x faster
```

#### RawLink<ulong> (8 x ulong = 64 bytes)
```
Run 1: Unsafe.SizeOf: 11.4ms  vs  Size field: 34.4ms   vs  Platform.Unsafe: 34.2ms
Run 2: Unsafe.SizeOf: 57.6ms  vs  Size field: 129.0ms  vs  Platform.Unsafe: 122.8ms
Run 3: Unsafe.SizeOf: 22.8ms  vs  Size field: 62.2ms   vs  Platform.Unsafe: 84.2ms
Average Ratios: SizeOf/Field: 0.38x, Platform.Unsafe/Field: 1.10x
```

#### RawLinkDataPart<ulong> (2 x ulong = 16 bytes)
```
Run 1: Unsafe.SizeOf: 10.6ms  vs  Size field: 38.0ms   vs  Platform.Unsafe: 33.4ms
Run 2: Unsafe.SizeOf: 39.8ms  vs  Size field: 78.6ms   vs  Platform.Unsafe: 72.8ms
Run 3: Unsafe.SizeOf: 41.0ms  vs  Size field: 83.6ms   vs  Platform.Unsafe: 72.1ms
Average Ratios: SizeOf/Field: 0.43x, Platform.Unsafe/Field: 0.89x
```

## Analysis

### Why Unsafe.SizeOf() is Faster

1. **JIT Optimization**: `Unsafe.SizeOf<T>()` is treated as an intrinsic by the JIT compiler, often inlined to a single constant value at compile time when used with `AggressiveInlining`.

2. **No Memory Access**: Unlike field access, `SizeOf<T>()` doesn't require loading from memory - it's often compiled to an immediate constant.

3. **Type Specialization**: The JIT compiler can specialize the generic method for each type, eliminating runtime type checks.

### Current Codebase Analysis

The current codebase uses two patterns:
- `Structure<T>.Size` from Platform.Unsafe package
- Static `SizeInBytes` fields initialized with `Structure<T>.Size`

Both approaches show similar performance (~10% variance), but both are significantly slower than direct `Unsafe.SizeOf<T>()` calls.

### Memory Usage

All approaches have identical memory usage - they all ultimately calculate the same struct size values. The difference is purely in access performance.

## Recommendations

### 1. Immediate Action (High Impact)
Replace static size fields with inline `Unsafe.SizeOf<T>()` calls:

```csharp
// Current approach (slower)
public static readonly long SizeInBytes = Structure<RawLink<TLinkAddress>>.Size;

// Recommended approach (2-3x faster)
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int GetSize() => System.Runtime.CompilerServices.Unsafe.SizeOf<RawLink<TLinkAddress>>();
```

### 2. Usage Pattern
In performance-critical paths, use direct calls:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void ProcessLinks<T>() where T : struct
{
    var size = Unsafe.SizeOf<T>(); // Direct call - fastest
    // ... rest of processing
}
```

### 3. Migration Strategy
1. Start with most performance-critical structures (RawLink, RawLinkDataPart)
2. Replace field access with method calls using `AggressiveInlining`
3. Update calling code to use the new pattern
4. Verify performance improvements with benchmarks

### 4. Code Pattern Template
```csharp
public struct HighPerformanceStruct<T>
{
    // Remove: public static readonly int SizeInBytes = ...;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SizeInBytes() => Unsafe.SizeOf<HighPerformanceStruct<T>>();
}
```

## Conclusion

The performance analysis clearly demonstrates that `System.Runtime.CompilerServices.Unsafe.SizeOf<T>()` with aggressive inlining provides significant performance benefits over pre-calculated size fields. The 2-3x performance improvement makes this change highly recommended for performance-critical code paths in the Platform.Data.Doublets library.

The JIT compiler's ability to optimize `SizeOf<T>()` calls into compile-time constants makes this approach both faster and memory-efficient compared to field-based approaches.

---
*Performance analysis completed for Platform.Data.Doublets Issue #90*
*Test environment: .NET 8, Linux, Release mode*
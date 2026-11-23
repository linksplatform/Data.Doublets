# Analysis: Why to shift 65 bits to right?

## Issue Reference
[GitHub Issue #218](https://github.com/linksplatform/Data.Doublets/issues/218)

## Code Location
File: `csharp/Platform.Data.Doublets/Numbers/Raw/NumberToLongRawNumberSequenceConverter.cs`
Line: 17

```csharp
private static readonly TSource _bitMask = Bit.ShiftRight(_maximumValue, NumericType<TTarget>.BitsSize + 1);
```

## Problem Analysis

The question arises when `TTarget` is a 64-bit integer type (like `long` or `ulong`), where:
- `NumericType<TTarget>.BitsSize` = 64 bits
- `NumericType<TTarget>.BitsSize + 1` = 65 bits

## Technical Explanation

### Why 65 bits specifically?

1. **Target Type Bit Size**: When `TTarget` is a 64-bit integer, `NumericType<TTarget>.BitsSize` returns 64.

2. **The +1 Addition**: The code adds 1 to get 65 bits: `NumericType<TTarget>.BitsSize + 1`

3. **Right Shift by 65 bits**: When you right-shift a 64-bit number by 65 bits, you effectively get 0 because:
   - A 64-bit number has only 64 significant bits (positions 0-63)
   - Shifting right by 65 bits moves all bits out of the significant range
   - This results in a bit mask of 0

### Purpose of the Bit Mask

The bit mask `_bitMask` is used in the conversion logic:

```csharp
var numberPart = Bit.And(source, _bitMask);
```

When `_bitMask` is 0 (from shifting 65 bits), the AND operation will always result in 0, which means:
- No bits are preserved from the source number in the `numberPart`
- This effectively disables the bit masking operation

### Design Intent

This appears to be intentional design behavior:

1. **For smaller target types** (like 32-bit integers): The bit mask preserves some bits from the source
2. **For 64-bit target types**: The bit mask becomes 0, effectively bypassing the bit masking

### Mathematical Context

- `_bitsPerRawNumber = NumericType<TTarget>.BitsSize - 1` (e.g., 63 for 64-bit types)
- The converter recursively processes numbers by shifting right by `_bitsPerRawNumber`
- The bit mask isolates the remaining bits that don't fit in a single raw number

## Conclusion

Shifting 65 bits to the right when working with 64-bit target types is intentional and results in a bit mask of 0. This design choice means that for 64-bit target types, the bit masking step is effectively bypassed, and the full source number is processed through the recursive conversion logic without bit isolation.

The "+1" in the shift amount is crucial because it ensures that for the maximum target type size (64 bits), the masking is disabled by creating a zero mask.
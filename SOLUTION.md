# Solution: Understanding the 65-bit Right Shift in NumberToLongRawNumberSequenceConverter

## Summary

The question "Why to shift 65 bits to right?" in issue #218 refers to the line:

```csharp
private static readonly TSource _bitMask = Bit.ShiftRight(_maximumValue, NumericType<TTarget>.BitsSize + 1);
```

## Detailed Explanation

### The Mathematics Behind the Shift

When `TTarget` is a 64-bit integer type (such as `long` or `ulong`):

1. `NumericType<TTarget>.BitsSize` = 64
2. `NumericType<TTarget>.BitsSize + 1` = 65
3. `Bit.ShiftRight(_maximumValue, 65)` on a 64-bit number = 0

### Why This Behavior is Intentional

The bit mask serves different purposes depending on the target type:

**For smaller target types (e.g., 32-bit):**
- `BitsSize` = 32
- `BitsSize + 1` = 33
- Shifting a 64-bit number right by 33 bits preserves the upper 31 bits
- This creates a meaningful bit mask for isolating specific bit ranges

**For maximum target types (64-bit):**
- `BitsSize` = 64  
- `BitsSize + 1` = 65
- Shifting right by 65 bits results in 0 (all bits shifted out)
- This effectively disables bit masking

### Code Context and Usage

The bit mask is used in the conversion logic:

```csharp
public TTarget Convert(TSource source)
{
    if (_comparer.Compare(source, _maximumConvertableAddress) > 0)
    {
        var numberPart = Bit.And(source, _bitMask);  // Uses the bit mask here
        var convertedNumber = _addressToNumberConverter.Convert(_sourceToTargetConverter.Convert(numberPart));
        return Links.GetOrCreate(convertedNumber, Convert(Bit.ShiftRight(source, _bitsPerRawNumber)));
    }
    else
    {
        return _addressToNumberConverter.Convert(_sourceToTargetConverter.Convert(source));
    }
}
```

### Design Pattern

This follows a pattern where:
- **Smaller target types**: Require bit segmentation and masking
- **Maximum target types**: Process the full number without bit isolation
- The `+1` in the shift calculation creates this conditional behavior automatically

### Recommendation

To improve code clarity, consider adding a comment to the bit mask field:

```csharp
// Bit mask for isolating number parts. For maximum target types (64-bit), 
// this becomes 0 (shift by 65 bits), effectively disabling masking.
private static readonly TSource _bitMask = Bit.ShiftRight(_maximumValue, NumericType<TTarget>.BitsSize + 1);
```

## Conclusion

The 65-bit right shift is not an error but a deliberate design choice that creates different behavior based on the target type size. For 64-bit target types, it results in a zero bit mask, which bypasses bit isolation and allows the full source number to be processed through the recursive conversion logic.
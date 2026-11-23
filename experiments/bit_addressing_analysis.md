# Bit-by-Bit Addressing Analysis

## Pattern Analysis

From the issue description:
- 0 → source
- 1 → target  
- 2 → source.source
- 3 → source.target
- 4 → target.source
- 5 → target.target
- 6 → source.source.source

## Understanding the Pattern

Let's analyze the binary representations and paths:

### Level 1 (depth 1)
- Index 0: source (direct access)
- Index 1: target (direct access)

### Level 2 (depth 2)  
- Index 2: source.source
- Index 3: source.target
- Index 4: target.source
- Index 5: target.target

### Level 3 (depth 3)
- Index 6: source.source.source
- Index 7: source.source.target  
- Index 8: source.target.source
- Index 9: source.target.target
- Index 10: target.source.source
- Index 11: target.source.target
- Index 12: target.target.source
- Index 13: target.target.target

## Algorithm

The key insight is:
1. For index 0-1: depth=1, use index directly as bit
2. For index 2-5: depth=2, use (index-2) as 2-bit path  
3. For index 6-13: depth=3, use (index-6) as 3-bit path
4. And so on...

The depth can be calculated as: `floor(log2(index + 2)) + 1` for index >= 0

Once we have depth, the path bits are: `(index - (2^depth - 2))` represented in `depth` bits.
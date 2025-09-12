# Pattern Syntax Examples for Links Notation

This document provides comprehensive examples of the pattern syntax implementation for issue #347.

## Basic Patterns

### Any Pattern
```
*
```
Matches any link.

### Literal Patterns
```
1
42
100
```
Matches links with specific addresses.

### Variable Patterns
```
$x
$y
$element
```
Variables that bind to matched links and can be reused in transformations.

## Built-in Sub-patterns

### Point Pattern
```
point
```
Matches links where source equals target (self-references).

Example usage:
```csharp
var pointPattern = patternManager.ParsePattern("point");
// Matches (1: 1 1)
```

### Partial Point Pattern
```
partial-point
```
Matches links that reference themselves either as source or target.

### Tree Pattern
```
tree
```
Recursive tree structure matching.

## Composite Patterns

### Doublet Patterns
```
(1 2)         // Matches link with source=1, target=2
($x $y)       // Matches any doublet, binding variables
(* 42)        // Matches any link with target=42
(point *)     // Matches any link with a point as source
```

### Logical Operators

#### OR Pattern
```
(or $x $y)           // Matches either $x or $y
(or 1 2 3)           // Matches literal 1, 2, or 3
(or point (1 2))     // Matches either a point or the doublet (1 2)
```

#### AND Pattern
```
(and * point)        // Matches links that are both any and point
(and (> 5) (< 10))   // Matches links with address between 5 and 10
```

#### NOT Pattern
```
(not point)          // Matches any link that is not a point
(not 1)              // Matches any link except address 1
```

## Comparison Operators

### Greater Than
```
(gt 5)    or    (> 5)
```
Matches links with address greater than 5.

### Greater Than or Equal
```
(gte 5)   or    (>= 5)
```

### Less Than
```
(lt 10)   or    (< 10)
```

### Less Than or Equal
```
(lte 10)  or    (<= 10)
```

## User-Defined Patterns

### Pattern Definition Syntax
```
(pattern
  (name "custom-pattern-name")
  (pattern-definition)
)
```

### Example: Custom Point Definition
```
(pattern
  (name "point")
  ($x: $x $x)
)
```

### Example: Custom Partial Point Definition
```
(pattern
  (name "partial-point")
  (or
    ($x: $x *)
    ($x: * $x)
  )
)
```

### Example: Custom Tree Definition
```
(pattern
  (name "tree")
  (or
    #element
    (#element #element)
    (tree #element)
    (#element tree)
  )
)
```

## Transformations

### Transform Once
Transform the first matching pattern:
```csharp
patternManager.TransformOnce("($x $y)", "($y $x)", links);
```
This swaps source and target of the first found doublet.

### Transform Always
Transform all matching patterns:
```csharp
patternManager.TransformAlways("($x $y)", "($y $x)", links);
```
This swaps source and target of all doublets.

## Complex Examples

### Example 1: Find All Points
```csharp
var pointPattern = patternManager.ParsePattern("point");
var matches = patternManager.FindMatches(pointPattern, links);
```

### Example 2: Transform Trees
```csharp
// Define custom tree pattern with element argument
var treeDefinition = @"
    (pattern (name ""binary-tree"") 
      (or $element ($element $element) (binary-tree $element) ($element binary-tree)))
";
patternManager.DefinePatterns(treeDefinition);

// Transform all binary trees to reverse their structure
patternManager.TransformAlways("(binary-tree $x)", "($x binary-tree)", links);
```

### Example 3: Complex Logical Patterns
```csharp
// Find all links that are either points or have address > 100
var complexPattern = patternManager.ParsePattern("(or point (> 100))");
var matches = patternManager.FindMatches(complexPattern, links);
```

### Example 4: Variable Binding and Reuse
```csharp
// Transform pattern: if a link has a point as source, swap source and target
var sourcePattern = "(point $target)";
var targetPattern = "($target point)";
patternManager.TransformAlways(sourcePattern, targetPattern, links);
```

## Usage in Code

### Basic Usage
```csharp
// Initialize pattern manager
var patternManager = new PatternManager<ulong>();

// Create some test data
var links = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
var point = links.GetOrCreate(1UL, 1UL);  // (1: 1 1)
var doublet = links.GetOrCreate(1UL, 2UL); // (1: 1 2)

// Parse and match patterns
var pointPattern = patternManager.ParsePattern("point");
var variables = new Dictionary<string, ulong>();
var isPointMatch = pointPattern.Matches(point, links, variables);     // true
var isDoubletMatch = pointPattern.Matches(doublet, links, variables); // false

// Find all matches
var allPointMatches = patternManager.FindMatches(pointPattern, links);

// Apply transformations
patternManager.TransformOnce("($x $y)", "($y $x)", links);
```

### Advanced Usage with Custom Patterns
```csharp
// Define custom patterns
var customPatterns = @"
    (pattern (name ""self-loop"") ($x: $x $x))
    (pattern (name ""connects-to-self"") (or ($x: $x *) ($x: * $x)))
";
patternManager.DefinePatterns(customPatterns);

// Use custom patterns in transformations
patternManager.TransformAlways("self-loop", "connects-to-self", links);
```

This implementation provides a complete solution for the pattern syntax requirements specified in issue #347, supporting all the requested features including variables, built-in patterns, user-defined patterns, and transformations.
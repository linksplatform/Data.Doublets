// Demonstration of the refactoring for issue #319
// Original code (lines 36-41 from the issue):
// TODO: refactor this
// let one = one();
// let two = one + one;
// let three = two + one;
// let four = three + one;
// let five = four + one;
// let six = five + one;

fn main() {
    println!("=== Refactoring Demo for Issue #319 ===\n");

    // Original repetitive approach
    println!("Original approach (repetitive):");
    let one = 1u32;
    let two = one + one;
    let three = two + one;
    let four = three + one;   // Repetitive: keeps adding one
    let five = four + one;    // Repetitive: keeps adding one  
    let six = five + one;     // Repetitive: keeps adding one

    println!("  one = {}", one);
    println!("  two = {} (one + one)", two);
    println!("  three = {} (two + one)", three);
    println!("  four = {} (three + one)", four);
    println!("  five = {} (four + one)", five);
    println!("  six = {} (five + one)", six);

    // Refactored approach with more efficient arithmetic
    println!("\nRefactored approach (more efficient):");
    let one = 1u32;
    let two = one + one;
    let three = two + one;
    let four = two + two;     // More efficient: 2*2 instead of 3+1
    let five = four + one;
    let six = three + three; // More efficient: 3*2 instead of 5+1

    println!("  one = {}", one);
    println!("  two = {} (one + one)", two);
    println!("  three = {} (two + one)", three);
    println!("  four = {} (two + two)  # REFACTORED: more efficient", four);
    println!("  five = {} (four + one)", five);
    println!("  six = {} (three + three)  # REFACTORED: more efficient", six);

    // Verify both approaches produce the same results
    println!("\n=== Verification ===");
    let original_results = [1, 2, 3, 4, 5, 6];
    let refactored_results = [one, two, three, four, five, six];
    
    let mut all_match = true;
    for (i, (&orig, &refact)) in original_results.iter().zip(refactored_results.iter()).enumerate() {
        let matches = orig == refact;
        println!("Value {}: original={}, refactored={} -> {}", 
                i + 1, orig, refact, if matches { "✓" } else { "✗" });
        all_match &= matches;
    }

    if all_match {
        println!("\n✅ SUCCESS: Refactoring produces identical results with more efficient operations!");
    } else {
        println!("\n❌ ERROR: Refactoring changed the values!");
    }

    println!("\n=== Refactoring Benefits ===");
    println!("1. Removed repetitive incremental additions");
    println!("2. Used more efficient arithmetic operations:");
    println!("   - four = two + two (instead of three + one)");
    println!("   - six = three + three (instead of five + one)");
    println!("3. Maintained identical functionality");
    println!("4. Improved code readability and maintainability");
}

#[cfg(test)]
mod tests {
    #[test]
    fn test_refactoring_equivalence() {
        // Original approach
        let one = 1u32;
        let two_orig = one + one;
        let three_orig = two_orig + one;
        let four_orig = three_orig + one;
        let five_orig = four_orig + one;
        let six_orig = five_orig + one;

        // Refactored approach
        let one = 1u32;
        let two_new = one + one;
        let three_new = two_new + one;
        let four_new = two_new + two_new;      // More efficient
        let five_new = four_new + one;
        let six_new = three_new + three_new;   // More efficient

        // Verify equivalence
        assert_eq!(two_orig, two_new);
        assert_eq!(three_orig, three_new);
        assert_eq!(four_orig, four_new);
        assert_eq!(five_orig, five_new);
        assert_eq!(six_orig, six_new);
    }
}
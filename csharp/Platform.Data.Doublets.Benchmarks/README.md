# Platform.Data.Doublets.Benchmarks

This project contains comprehensive benchmarks for comparing different implementations and algorithms in the Platform.Data.Doublets library using BenchmarkDotNet.

## Benchmark Categories

### Original Benchmarks

- **CountBenchmarks**: Compares different collection approaches (Array, List, ListWithCapacity) for link data gathering
- **LinkStructBenchmarks**: Compares performance between Link struct, array, and list implementations

### New Comparison Benchmarks (Issue #100)

The following benchmarks were added to fulfill the requirement of using BenchmarkDotNet for comparison of experimental code and similar versions of algorithms:

#### 1. TreeImplementationsBenchmarks
Compares different tree implementation algorithms for the same operations:
- **SizeBalancedTree**: Traditional size-balanced tree implementation
- **RecursionlessSizeBalancedTree**: Iterative version avoiding recursion
- **SizedAndThreadedAVLBalancedTree**: AVL-balanced tree with size and threading optimizations

Operations tested:
- Link creation and insertion
- Link searching and counting
- Link enumeration (Each method)

#### 2. MemoryImplementationsBenchmarks  
Compares Split vs United memory implementation approaches:
- **SplitMemoryLinks**: Uses separate data and index memory regions
- **UnitedMemoryLinks**: Uses unified memory layout

Operations tested:
- Link creation
- Link searching  
- Link updates
- Link deletion

#### 3. ExperimentalAlgorithmsBenchmarks
Demonstrates different experimental approaches to the same problem - link data collection:
- **V1_PreallocatedArray**: Memory-efficient with exact size pre-calculation
- **V2_DynamicList**: Simple dynamic approach with resizing
- **V3_PreallocatedList**: Pre-calculated capacity with List flexibility
- **V4_LinqBased**: LINQ-based functional approach
- **V5_BatchProcessing**: Batch processing for better cache locality
- **V6_PooledArrays**: Memory-optimized using pooled temporary storage

## Usage

### Running All Benchmarks
```bash
cd csharp
dotnet run --project Platform.Data.Doublets.Benchmarks -c Release
```

### Running Specific Benchmarks
You can modify `Program.cs` to run only specific benchmark classes by commenting out the ones you don't want to run.

## Configuration

The benchmarks are configured with:
- **SimpleJob**: Basic job configuration
- **MemoryDiagnoser**: Tracks memory allocations and GC pressure
- **Statistical Columns**: Mean, Standard Deviation, and Median for better comparison

## Parameters

Most benchmarks use parameterized testing with different data sizes:
- Small: 100 items
- Medium: 1,000 items  
- Large: 10,000 items (where applicable)

## Output

BenchmarkDotNet generates detailed reports including:
- Execution time statistics
- Memory allocation metrics
- GC collection statistics
- Statistical analysis (mean, standard deviation, median)

## Purpose

These benchmarks fulfill GitHub issue #100's requirement to "Use BenchmarkDotNet for comparison of experimental code (comparison of similar version of the same algorithms)". They provide quantitative performance data to compare:

1. **Different implementations** of the same algorithm (tree types)
2. **Different architectural approaches** (memory layouts) 
3. **Multiple experimental solutions** to the same problem (data collection strategies)

This enables data-driven decisions when choosing between implementation alternatives and validates performance improvements or regressions in experimental code.
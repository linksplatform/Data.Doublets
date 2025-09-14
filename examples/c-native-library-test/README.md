# C Native Library Test Example

This example demonstrates how to use the Platform.Data.Doublets native library from C code.

## Building and Running

1. First, build the native library:
```bash
cd ../../csharp/Platform.Data.Doublets.NativeLibrary
dotnet publish -r linux-x64 -c Release --self-contained
```

2. Build the C test:
```bash
make test
```

3. Copy the library to local directory (optional):
```bash
make copy-lib
```

4. Run the test:
```bash
make run
```

## What the test does

The test program:
1. Dynamically loads the native library
2. Gets function pointers to the exported functions
3. Creates a new links database
4. Creates several links
5. Counts links in the database
6. Updates a link
7. Cleans up resources

## Expected Output

```
Platform.Data.Doublets Native Library Version: 0.1.0-nativeaot
Successfully created links database
Creating links...
Created link 1: 1
Created link 2: 2
Created link 3: 3
Total links in database: 3
Created specific link [1, 2]: 4
Total links after creating specific link: 4
Updated link 3 to [1, 2]: result=3
Test completed successfully!
```
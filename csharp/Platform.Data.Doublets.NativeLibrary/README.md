# Platform.Data.Doublets Native Library

This project implements a native library version of Platform.Data.Doublets using .NET NativeAOT (formerly CoreRT). The native library exposes C-style APIs that can be called from any language that supports C FFI.

## Overview

The native library provides native bindings for the core Platform.Data.Doublets functionality, specifically the `UnitedMemoryLinks<ulong>` implementation. All major operations (Create, Read, Update, Delete, Count) are exposed as C-style function exports.

## Building

### Prerequisites
- .NET 8 SDK or later
- NativeAOT support (included in .NET 8)

### Build Commands

```bash
# Build the native library for Linux x64
dotnet publish -r linux-x64 -c Release --self-contained

# Build for other platforms
dotnet publish -r win-x64 -c Release --self-contained    # Windows
dotnet publish -r osx-x64 -c Release --self-contained    # macOS
```

Or use the provided build script:
```bash
cd csharp/
./build-native-library.sh
```

## Generated Files

After building, you'll find the native libraries in:
- `bin/Release/net8/[runtime]/publish/Platform.Data.Doublets.NativeLibrary.so` (Linux)
- `bin/Release/net8/[runtime]/publish/Platform.Data.Doublets.NativeLibrary.dll` (Windows)
- `bin/Release/net8/[runtime]/publish/Platform.Data.Doublets.NativeLibrary.dylib` (macOS)

## Exported Functions

The library exports the following C-style functions:

### Instance Management
- `IntPtr UInt64UnitedMemoryLinks_New(IntPtr pathPtr)` - Create a new links instance
- `void UInt64UnitedMemoryLinks_Drop(IntPtr handle)` - Dispose of a links instance

### Link Operations
- `ulong UInt64UnitedMemoryLinks_Create(IntPtr handle, ulong* query, nuint queryLen)` - Create a link
- `ulong UInt64UnitedMemoryLinks_Count(IntPtr handle, ulong* query, nuint queryLen)` - Count matching links
- `ulong UInt64UnitedMemoryLinks_Update(IntPtr handle, ulong* query, nuint queryLen, ulong* replacement, nuint replacementLen)` - Update a link
- `ulong UInt64UnitedMemoryLinks_Delete(IntPtr handle, ulong* query, nuint queryLen)` - Delete a link

### Utility
- `IntPtr GetLibraryVersion()` - Get library version string

## Usage Example (C)

```c
#include <stdio.h>
#include <stdlib.h>

// Function declarations
IntPtr UInt64UnitedMemoryLinks_New(const char* path);
void UInt64UnitedMemoryLinks_Drop(IntPtr handle);
ulong UInt64UnitedMemoryLinks_Create(IntPtr handle, ulong* query, size_t queryLen);
ulong UInt64UnitedMemoryLinks_Count(IntPtr handle, ulong* query, size_t queryLen);

int main() {
    // Create a new links database
    IntPtr links = UInt64UnitedMemoryLinks_New("test.links");
    if (links == 0) {
        printf("Failed to create links database\\n");
        return 1;
    }

    // Create a link: [any, any] -> returns created link
    ulong query[] = {0, 0};
    ulong result = UInt64UnitedMemoryLinks_Create(links, query, 2);
    printf("Created link: %llu\\n", result);

    // Count all links
    ulong count = UInt64UnitedMemoryLinks_Count(links, query, 2);
    printf("Total links: %llu\\n", count);

    // Clean up
    UInt64UnitedMemoryLinks_Drop(links);
    return 0;
}
```

## Architecture

The native library implementation:

1. **Uses NativeAOT**: Compiles C# code to native machine code ahead-of-time
2. **C-style exports**: Uses `[UnmanagedCallersOnly]` attribute to export functions
3. **Handle-based API**: Manages C# objects through integer handles to avoid direct memory management
4. **Thread-safe**: Uses locks to ensure thread safety across FFI calls
5. **Error handling**: Returns 0 or null on errors, avoiding exceptions across FFI boundary

## Compatibility

This native library is compatible with the existing FFI interface defined in `c/ffi.h`, providing the same function signatures and behavior as the Rust implementation.

## Benefits over CoreRT

- **Modern approach**: Uses .NET 8 NativeAOT instead of deprecated CoreRT
- **Better performance**: Optimized AOT compilation with latest .NET runtime
- **Self-contained**: No .NET runtime dependency required
- **Smaller footprint**: Native compilation reduces deployment size
- **Cross-platform**: Supports Windows, Linux, and macOS
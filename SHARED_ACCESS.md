# Shared Database Access Implementation

This document describes the implementation of shared access functionality that allows multiple processes to access the same Links database file simultaneously.

## Overview

The shared access implementation enables multiple processes to safely read from and write to the same Links database file. This is achieved through:

1. **Inter-process synchronization** using named mutexes
2. **Shared lock information** stored in the database header 
3. **Memory-mapped file sharing** for efficient multi-process access
4. **Cross-platform compatibility** with Windows and Unix-like systems

## Architecture

### Core Components

#### 1. SharedAccessLinks<TLinkAddress>
- **Purpose**: High-level API for shared database access
- **Location**: `Platform.Data.Doublets/SharedAccessLinks.cs`
- **Features**: 
  - Implements `ILinks<TLinkAddress>` interface
  - Provides thread-safe operations with automatic synchronization
  - Manages the underlying `SharedAccessUnitedMemoryLinks` instance

#### 2. SharedAccessUnitedMemoryLinks<TLinkAddress>
- **Purpose**: Core shared access implementation extending `UnitedMemoryLinks`
- **Location**: `Platform.Data.Doublets/Memory/United/Generic/SharedAccessUnitedMemoryLinks.cs`
- **Features**:
  - Inter-process mutex-based synchronization
  - Process counter management in database header
  - Read/write operation synchronization

#### 3. SharedAccessFileMappedResizableDirectMemory
- **Purpose**: Cross-platform memory-mapped file implementation
- **Location**: `Platform.Data.Doublets/Memory/SharedAccessFileMappedResizableDirectMemory.cs`
- **Features**:
  - Named memory-mapped files on Windows for enhanced sharing
  - Anonymous memory-mapped files on Unix-like systems
  - File-based synchronization support

## Shared Lock Mechanism

### Database Header Integration

The shared access information is stored in the existing `LinksHeader.Reserved8` field:

```
Bit 0:    Shared access enabled flag (1 = enabled, 0 = disabled)
Bits 1-31: Process counter (number of processes currently accessing the file)
```

### Process Lifecycle

1. **First Process**: Sets bit 0 to 1 and process counter to 1
2. **Additional Processes**: Increment the process counter
3. **Process Exit**: Decrement the process counter
4. **Last Process**: Resets bit 0 to 0 when counter reaches 0

## Inter-Process Synchronization

### Named Mutex Strategy

Each database file gets a unique named mutex based on its file path:
```csharp
var mutexName = $"Global\\LinksDoublets_{base64EncodedPath}";
var globalMutex = new Mutex(false, mutexName);
```

### Operation Synchronization

- **Read Operations**: Acquire mutex, execute operation, release mutex
- **Write Operations**: Acquire mutex, execute operation, release mutex
- **Initialization**: Acquire mutex, update process counter, release mutex

## Cross-Platform Compatibility

### Windows Platform
- Uses named memory-mapped files for optimal sharing
- Supports `MemoryMappedFile.OpenExisting()` for inter-process access
- Enhanced performance through shared memory regions

### Unix-like Platforms (Linux, macOS)
- Uses anonymous memory-mapped files backed by the actual file
- Relies on file system for sharing between processes
- Maintains compatibility without named memory-mapped file support

## Usage Examples

### Basic Usage
```csharp
using (var links = new SharedAccessLinks<ulong>("database.db"))
{
    var link = links.Create();
    var count = links.Count();
    // Operations are automatically synchronized
}
```

### Multiple Process Access
```csharp
// Process 1
using (var links1 = new SharedAccessLinks<ulong>("shared.db"))
{
    var link = links1.Create();
    
    // Process 2 can simultaneously access the same database
    using (var links2 = new SharedAccessLinks<ulong>("shared.db"))
    {
        var count = links2.Count(); // Sees link created by Process 1
        var newLink = links2.Create(); // Both processes see this
    }
}
```

## Implementation Details

### Thread Safety
- All database operations are protected by inter-process mutexes
- No additional thread-local locking required
- Compatible with existing `SynchronizedLinks` wrapper for intra-process thread safety

### Memory Management
- Automatic resource cleanup on process termination
- Process counter decremented in `Dispose()` method
- Mutex resources properly released

### Error Handling
- Graceful handling of platform-specific limitations
- Fallback to file-based sharing on unsupported platforms
- Robust cleanup even if processes terminate unexpectedly

## Performance Considerations

### Windows
- Named memory-mapped files provide optimal performance
- Direct shared memory access between processes
- Minimal overhead for synchronization

### Unix-like Systems
- File-backed memory mapping with good performance
- Slightly higher overhead due to anonymous mapping
- Still maintains excellent performance characteristics

## Backward Compatibility

The implementation maintains full backward compatibility:
- Existing `UnitedMemoryLinks` databases can be opened with `SharedAccessLinks`
- Database file format remains unchanged
- Only uses previously reserved header field
- No breaking changes to existing APIs

## Testing

Comprehensive test suite covers:
- Basic shared access functionality
- Concurrent access from multiple threads
- Process counter management in database header
- Cross-platform compatibility
- Backward compatibility with existing databases

## Limitations

1. **Platform Dependencies**: Named memory-mapped files only available on Windows
2. **Mutex Scope**: Global mutexes require appropriate permissions
3. **Network Filesystems**: May not work properly with all network filesystems
4. **Process Limits**: Limited by system's maximum mutex and memory-mapping limits

## Future Enhancements

Potential improvements for future versions:
- Reader-writer locks for improved read concurrency
- Advanced conflict resolution mechanisms
- Network-based shared access support
- Distributed locking for clustered scenarios
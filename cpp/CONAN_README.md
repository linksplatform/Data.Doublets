# Platform.Data.Doublets Conan Package

This directory contains the Conan package configuration for Platform.Data.Doublets.

## Package Information

- **Name**: `platform.data.doublets`
- **Version**: `0.1.0`
- **Type**: Header-only C++ library

## Dependencies

The package depends on the following Platform libraries:
- platform.interfaces (0.3.41)
- platform.collections.methods (0.3.0)
- platform.collections (0.2.1)
- platform.numbers (0.1.0)
- platform.memory (0.1.0)
- platform.exceptions (0.3.2)
- platform.data (0.1.1)
- platform.setters (0.1.0)
- platform.ranges (0.2.0)
- mio (cci.20201220)

## Building the Package

### Prerequisites
1. Install Conan: `pip install conan`
2. Set up a Conan profile for your compiler

### Creating the Package
```bash
# From the cpp directory
conan create . platform.data.doublets/0.1.0@
```

### Using the Package

Add to your `conanfile.txt`:
```ini
[requires]
platform.data.doublets/0.1.0

[generators]
CMakeDeps
CMakeToolchain
```

Or in `conanfile.py`:
```python
requires = "platform.data.doublets/0.1.0"
```

### CMake Integration

```cmake
find_package(Platform.Data.Doublets REQUIRED)
target_link_libraries(your_target Platform.Data.Doublets::Platform.Data.Doublets.Library)
```

## Package Options

- `tests`: Build tests (default: False)
- `benchmarks`: Build benchmarks (default: False)
- `shared`: Build shared library (default: False)
- `fPIC`: Position independent code (default: True)

## Files Structure

- `conanfile.py`: Main Conan recipe
- `CMakeLists.txt`: CMake build configuration
- `Platform.Data.Doublets/`: Header files
- `Platform.Data.Doublets.Tests/`: Test files
- `Platform.Data.Doublets.Benchmarks/`: Benchmark files
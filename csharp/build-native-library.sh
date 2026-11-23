#!/bin/bash

# Build script for Platform.Data.Doublets Native Library using NativeAOT

echo "Building Platform.Data.Doublets Native Library with NativeAOT..."

# Navigate to the native library project directory
cd "$(dirname "$0")/Platform.Data.Doublets.NativeLibrary"

# Build for different platforms
echo "Building for Linux x64..."
dotnet publish -r linux-x64 -c Release --self-contained

echo "Building for Windows x64..."
dotnet publish -r win-x64 -c Release --self-contained

echo "Building for macOS x64..."
dotnet publish -r osx-x64 -c Release --self-contained

echo "Build completed. Check the bin/Release/net8/[runtime]/publish/ directories for the native libraries."

# Show the output files
echo ""
echo "Generated native libraries:"
find bin/Release/net8 -name "*Platform.Data.Doublets.NativeLibrary*" -type f 2>/dev/null || echo "No output files found yet."
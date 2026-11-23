#!/bin/bash

echo "Testing native build for Platform.Data.Doublets..."

# First, let's try a simple build to see if the project compiles
cd "$(dirname "$0")/Platform.Data.Doublets.NativeLibrary"

echo "Building native library project..."
dotnet build -c Release

if [ $? -eq 0 ]; then
    echo "✓ Build successful!"
    
    echo "Attempting NativeAOT publish for linux-x64..."
    dotnet publish -r linux-x64 -c Release --self-contained
    
    if [ $? -eq 0 ]; then
        echo "✓ NativeAOT publish successful!"
        
        echo "Output files:"
        find bin -name "*" -type f | head -20
    else
        echo "✗ NativeAOT publish failed"
        exit 1
    fi
else
    echo "✗ Build failed"
    exit 1
fi
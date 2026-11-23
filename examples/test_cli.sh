#!/bin/bash

# Test script for the Doublets CLI
# This script tests all the examples from the issue

echo "Building CLI..."
cd ../csharp
dotnet publish Platform.Data.Doublets.Cli/Platform.Data.Doublets.Cli.csproj -c Release -o ../bin/cli --self-contained false

if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi

cd ../bin/cli
CLI="./doublets"

echo "Testing CLI with examples from issue #346..."

# Clean up any existing database
rm -f default.links

echo "Test 1: Create link (1: 1 1)"
$CLI create "(1: 1 1)"

echo "Test 2: Create link with simple notation"
$CLI create "1 1"

echo "Test 3: Interactive mode test (run manually)"
echo "Run: $CLI interactive"

echo "Test 4: Query all links"
$CLI each "* *"

echo "Test 5: Update link from (1: 1 1) to (1: 1 2)"
$CLI update "(1: 1 1)" "(1: 1 2)"

echo "Test 6: Delete link"
$CLI delete "(1: 1 2)"

echo "CLI tests completed!"
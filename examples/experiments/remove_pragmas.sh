#!/bin/bash

# Script to remove the CS1591 pragma warning disable from all files

# Find all files containing the pragma and remove the line
grep -r "#pragma warning disable CS1591" csharp/Platform.Data.Doublets/ | cut -d: -f1 | sort -u | while read file; do
    echo "Processing: $file"
    # Use sed to remove the exact line containing the pragma
    sed -i '/#pragma warning disable CS1591.*Missing XML comment for publicly visible type or member/d' "$file"
done

echo "Done removing pragmas from all files."
#!/bin/bash

# LinksPlatform Custom XRef Service Setup Script
# 
# This script sets up a custom xrefService for LinksPlatform documentation
# to replace the Microsoft xrefService and support references to other packages

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
XREF_OUTPUT_DIR="$PROJECT_ROOT/xref"

echo "🚀 Setting up LinksPlatform Custom XRef Service..."
echo "📁 Project root: $PROJECT_ROOT"
echo "📂 XRef output directory: $XREF_OUTPUT_DIR"

# Check if Python is available
if command -v python3 &> /dev/null; then
    echo "✅ Using Python 3 to generate XRef service"
    python3 "$SCRIPT_DIR/generate-xref-service.py" "$XREF_OUTPUT_DIR"
elif command -v node &> /dev/null; then
    echo "✅ Using Node.js to generate XRef service"
    node "$SCRIPT_DIR/xref-service-generator.js" "$XREF_OUTPUT_DIR"
else
    echo "❌ Error: Neither Python 3 nor Node.js is available"
    echo "Please install Python 3 or Node.js to generate the XRef service"
    exit 1
fi

echo ""
echo "🎉 Custom XRef Service setup completed!"
echo ""
echo "📋 Next steps:"
echo "1. 📤 Deploy the '$XREF_OUTPUT_DIR' directory to GitHub Pages or any static hosting"
echo "2. 🔧 Update your docfx.json with the custom xrefService URL"
echo "3. 🧪 Test the documentation generation"
echo ""
echo "📖 Example docfx.json configuration:"
echo '{
  "build": {
    "xrefService": [
      "https://yourusername.github.io/yourrepo/xref/{uid}.json",
      "https://xref.docs.microsoft.com/query?uid={uid}"
    ]
  }
}'
echo ""

# Optional: Show generated files
if [ -d "$XREF_OUTPUT_DIR" ]; then
    file_count=$(find "$XREF_OUTPUT_DIR" -name "*.json" | wc -l)
    echo "📊 Generated $file_count JSON files for XRef service"
    echo "📄 Key files:"
    echo "  - $XREF_OUTPUT_DIR/index.json (complete reference list)"
    echo "  - $XREF_OUTPUT_DIR/README.md (documentation)"
    echo "  - $XREF_OUTPUT_DIR/_config.yml (GitHub Pages config)"
fi
# Custom XRef Service Solution for LinksPlatform Documentation

This document describes the implementation of a custom xrefService to replace the Microsoft xrefService in DocFX documentation generation, specifically to support cross-references to other LinksPlatform packages.

## Problem Statement

The current DocFX configuration uses Microsoft's xrefService:
```json
"xrefService": [ "https://xref.docs.microsoft.com/query?uid={uid}" ]
```

This service only resolves Microsoft's .NET BCL types and doesn't support LinksPlatform's custom packages like:
- Platform.Data
- Platform.Data.Doublets  
- Platform.Collections.Methods
- Platform.Memory
- Platform.Random
- Platform.Timestamps
- Platform.Singletons
- And many others...

## Solution Overview

The solution provides **three complementary approaches**:

### 1. Custom XRef Service Generator
A tool that creates JSON files for each UID that can be served from any static hosting service.

### 2. XRef Map References  
Direct references to xrefmap.yml files from other LinksPlatform repositories.

### 3. Fallback to Microsoft Service
Keep Microsoft's service as a fallback for .NET BCL types.

## Implementation Files

### Core Files

1. **`docfx.json`** - Updated DocFX configuration with custom xrefService
2. **`scripts/generate-xref-service.py`** - Python script to generate custom xref service
3. **`scripts/xref-service-generator.js`** - Node.js alternative for xref generation  
4. **`scripts/setup-custom-xref.sh`** - Shell script to automate the setup

### Generated Files (when running the generator)

- **`xref/index.json`** - Complete list of available UIDs
- **`xref/{uid}.json`** - Individual xref files for each UID
- **`xref/README.md`** - Documentation for the xref service
- **`xref/_config.yml`** - GitHub Pages configuration

## Usage Instructions

### Option 1: Quick Setup (Recommended)

1. Run the setup script:
   ```bash
   ./scripts/setup-custom-xref.sh
   ```

2. Deploy the generated `xref/` directory to GitHub Pages or any static hosting service.

3. Update your DocFX configuration to use your hosted URL.

### Option 2: Manual Setup

1. Generate the xref service using Python:
   ```bash
   python3 scripts/generate-xref-service.py ./xref
   ```

2. Or using Node.js:
   ```bash
   node scripts/xref-service-generator.js ./xref
   ```

3. Deploy the `xref/` directory to your hosting service.

## DocFX Configuration

The updated `docfx.json` includes:

```json
{
  "build": {
    "xref": [
      "https://linksplatform.github.io/Data/xrefmap.yml",
      "https://linksplatform.github.io/Collections/xrefmap.yml",
      "https://linksplatform.github.io/Memory/xrefmap.yml",
      "https://linksplatform.github.io/Random/xrefmap.yml",
      "https://linksplatform.github.io/Timestamps/xrefmap.yml",
      "https://linksplatform.github.io/Singletons/xrefmap.yml",
      "https://linksplatform.github.io/Ranges/xrefmap.yml",
      "https://linksplatform.github.io/Numbers/xrefmap.yml",
      "https://linksplatform.github.io/Converters/xrefmap.yml",
      "https://linksplatform.github.io/Delegates/xrefmap.yml",
      "https://linksplatform.github.io/Interfaces/xrefmap.yml",
      "https://linksplatform.github.io/Exceptions/xrefmap.yml",
      "https://linksplatform.github.io/Setters/xrefmap.yml"
    ],
    "xrefService": [
      "https://linksplatform.github.io/xref/{uid}.json",
      "https://xref.docs.microsoft.com/query?uid={uid}"
    ]
  }
}
```

## Deployment Options

### GitHub Pages (Recommended)

1. Create a repository for your xref service (e.g., `linksplatform-xref`)
2. Upload the generated `xref/` contents to the repository
3. Enable GitHub Pages
4. Use URL: `https://yourusername.github.io/linksplatform-xref/{uid}.json`

### Other Static Hosting Services

- **Netlify**: Drag and drop the `xref/` directory
- **Vercel**: Deploy from Git repository
- **AWS S3 + CloudFront**: Upload as static website
- **Azure Static Web Apps**: Deploy from GitHub

## How It Works

### XRef Service Generator Logic

1. **Discovery**: Attempts to download existing xrefmap.yml files from LinksPlatform repositories
2. **Synthesis**: Generates synthetic references for known LinksPlatform types and patterns
3. **JSON Generation**: Creates individual JSON files for each UID following DocFX xrefService API format
4. **Index Creation**: Generates an index file listing all available references
5. **GitHub Pages Setup**: Creates necessary files for GitHub Pages hosting

### DocFX Resolution Order

1. DocFX first tries the local `xref` array (xrefmap.yml files)
2. Then tries each `xrefService` URL in order:
   - Custom LinksPlatform xref service
   - Microsoft's xref service (fallback)

## Benefits

### ✅ Complete LinksPlatform Support
Resolves cross-references to all LinksPlatform packages and types.

### ✅ Backward Compatibility  
Maintains Microsoft xrefService as fallback for .NET BCL types.

### ✅ Self-Hosted
No dependency on external services for LinksPlatform types.

### ✅ Extensible
Easy to add new packages or modify reference generation.

### ✅ Fast
Static JSON files served from CDN with fast lookup times.

### ✅ Free
Can be hosted on GitHub Pages at no cost.

## Maintenance

### Adding New Packages

Add package names to the `LINKSPLATFORM_PACKAGES` list in the generator scripts:

```python
LINKSPLATFORM_PACKAGES = [
    'Platform.Data',
    'Platform.Data.Doublets',
    'Platform.YourNewPackage',  # Add here
    # ... other packages
]
```

### Updating References

Re-run the generator script periodically to pick up new types and documentation from LinksPlatform repositories.

### CI/CD Integration

Add the generator to your CI/CD pipeline to automatically update the xref service when documentation changes.

## Technical Details

### Generated JSON Format

Each UID generates a JSON file like `Platform.Data.Doublets.ILinks.json`:

```json
{
  "uid": "Platform.Data.Doublets.ILinks",
  "name": "ILinks",
  "href": "https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.ILinks.html",
  "fullName": "Platform.Data.Doublets.ILinks",
  "type": "Type",
  "summary": "LinksPlatform Platform.Data.Doublets.ILinks reference",
  "namespace": "Platform.Data.Doublets",
  "assembly": "Platform.Data"
}
```

### HTTP Headers

The service supports CORS and appropriate caching headers when properly configured.

### Performance

- Static JSON files load quickly
- CDN caching reduces latency  
- Minimal server requirements (static hosting)

## Troubleshooting

### Common Issues

1. **404 Errors**: Ensure the xref service is properly deployed and accessible
2. **CORS Issues**: Configure your hosting service to allow cross-origin requests
3. **Missing References**: Check if packages are included in the generator configuration
4. **Outdated References**: Re-run the generator to update references

### Testing

Test your xref service:

```bash
# Test if a specific UID is available
curl "https://yourdomain.com/xref/Platform.Data.Doublets.ILinks.json"

# Check the index file
curl "https://yourdomain.com/xref/index.json"
```

## Future Enhancements

- **Automatic Discovery**: Scan GitHub API for new LinksPlatform repositories
- **Real-time Updates**: Webhook-based updates when documentation changes  
- **Enhanced Metadata**: Include more detailed type information
- **Analytics**: Track usage patterns and optimize frequently accessed types

---

This solution provides a robust, maintainable, and scalable approach to custom xrefService implementation for LinksPlatform documentation.
#!/usr/bin/env python3
"""
LinksPlatform Custom XRef Service Generator

This script generates a custom xrefService for LinksPlatform documentation
by creating JSON files that can be served statically from GitHub Pages
or any web server to replace the Microsoft xrefService.

Usage: python generate-xref-service.py [output-directory]
"""

import json
import os
import sys
import urllib.request
import urllib.error
from datetime import datetime
from pathlib import Path

# Configuration for LinksPlatform packages
LINKSPLATFORM_PACKAGES = [
    'Platform.Data',
    'Platform.Data.Doublets',
    'Platform.Collections.Methods', 
    'Platform.Memory',
    'Platform.Random',
    'Platform.Timestamps',
    'Platform.Singletons',
    'Platform.Numbers',
    'Platform.Ranges',
    'Platform.Converters',
    'Platform.Threading.Synchronization',
    'Platform.Delegates',
    'Platform.Interfaces',
    'Platform.Exceptions',
    'Platform.Setters'
]

LINKSPLATFORM_BASE_URL = 'https://linksplatform.github.io'

class XRefServiceGenerator:
    def __init__(self, output_dir='./xref'):
        self.output_dir = Path(output_dir)
        self.xref_map = {}
        self.ensure_output_directory()

    def ensure_output_directory(self):
        """Create output directory if it doesn't exist"""
        self.output_dir.mkdir(parents=True, exist_ok=True)

    def download_xref_map(self, package_name):
        """Downloads xrefmap.yml from a LinksPlatform repository if it exists"""
        repo_name = package_name.replace('Platform.', '')
        xref_url = f"{LINKSPLATFORM_BASE_URL}/{repo_name}/xrefmap.yml"
        
        try:
            print(f"Attempting to download: {xref_url}")
            with urllib.request.urlopen(xref_url) as response:
                data = response.read().decode('utf-8')
                return self.parse_xref_map_yml(data)
        except urllib.error.URLError as e:
            print(f"No xrefmap found for {package_name}: {e}")
            return []

    def parse_xref_map_yml(self, yaml_content):
        """Parses YAML xrefmap format (simplified parser)"""
        references = []
        lines = yaml_content.split('\n')
        current_ref = {}
        
        for line in lines:
            trimmed = line.strip()
            if trimmed.startswith('- uid:'):
                if 'uid' in current_ref:
                    references.append(dict(current_ref))
                current_ref = {'uid': trimmed[6:].strip()}
            elif trimmed.startswith('name:'):
                current_ref['name'] = trimmed[5:].strip()
            elif trimmed.startswith('href:'):
                current_ref['href'] = trimmed[5:].strip()
            elif trimmed.startswith('fullName:'):
                current_ref['fullName'] = trimmed[9:].strip()
        
        if 'uid' in current_ref:
            references.append(current_ref)
            
        return references

    def generate_synthetic_references(self, package_name):
        """Generates synthetic xref entries for known LinksPlatform types"""
        repo_name = package_name.replace('Platform.', '')
        base_url = f"{LINKSPLATFORM_BASE_URL}/{repo_name}/api/"
        
        # Common patterns for LinksPlatform packages
        common_types = [
            f"{package_name}.ILinks",
            f"{package_name}.Links", 
            f"{package_name}.LinksConstants",
            f"{package_name}.LinksOperatorBase",
        ]

        # Special types for Data.Doublets
        if package_name == 'Platform.Data.Doublets':
            common_types.extend([
                f"{package_name}.Link",
                f"{package_name}.SynchronizedLinks",
                f"{package_name}.ISynchronizedLinks",
                f"{package_name}.CriterionMatchers.TargetMatcher",
            ])

        references = []
        for uid in common_types:
            references.append({
                'uid': uid,
                'name': uid.split('.')[-1],
                'href': f"{base_url}{uid.replace('.', '.')}.html",
                'fullName': uid
            })
            
        return references

    def build_xref_map(self):
        """Builds the complete xref map"""
        print('Building LinksPlatform XRef map...')
        
        for package_name in LINKSPLATFORM_PACKAGES:
            print(f'Processing {package_name}...')
            
            # Try to download existing xrefmap
            downloaded_refs = self.download_xref_map(package_name)
            
            # Generate synthetic references as fallback
            synthetic_refs = self.generate_synthetic_references(package_name)
            
            # Combine references (downloaded takes precedence)
            all_refs = downloaded_refs + synthetic_refs
            
            # Add to main xref map
            for ref in all_refs:
                if ref['uid'] not in self.xref_map:
                    self.xref_map[ref['uid']] = ref
        
        print(f'Built xref map with {len(self.xref_map)} references')

    def generate_xref_files(self):
        """Generates individual JSON files for each UID"""
        print('Generating xref JSON files...')
        
        file_count = 0
        for uid, reference in self.xref_map.items():
            file_name = f"{uid}.json"
            file_path = self.output_dir / file_name
            
            xref_response = {
                'uid': reference['uid'],
                'name': reference.get('name', reference['uid']),
                'href': reference.get('href'),
                'fullName': reference.get('fullName', reference['uid']),
                'type': 'Type',
                'summary': f"LinksPlatform {reference['uid']} reference",
                'namespace': '.'.join(reference['uid'].split('.')[:-1]),
                'assembly': '.'.join(reference['uid'].split('.')[:2])
            }
            
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(xref_response, f, indent=2)
                
            file_count += 1
        
        print(f'Generated {file_count} xref JSON files in {self.output_dir}')

    def generate_index_file(self):
        """Creates an index file listing all available references"""
        index_path = self.output_dir / 'index.json'
        all_uids = list(self.xref_map.keys())
        
        index = {
            'generated': datetime.now().isoformat(),
            'totalReferences': len(all_uids),
            'packages': LINKSPLATFORM_PACKAGES,
            'uids': all_uids
        }
        
        with open(index_path, 'w', encoding='utf-8') as f:
            json.dump(index, f, indent=2)
            
        print(f'Generated index file: {index_path}')

    def generate_github_pages_structure(self):
        """Creates a GitHub Pages compatible directory structure"""
        # Create _config.yml for GitHub Pages
        config_path = self.output_dir / '_config.yml'
        config = """# LinksPlatform Custom XRef Service
plugins:
  - jekyll-optional-front-matter

include:
  - "*.json"

defaults:
  - scope:
      path: "*.json"
    values:
      layout: null"""
        
        with open(config_path, 'w', encoding='utf-8') as f:
            f.write(config)

        # Create README.md
        readme_path = self.output_dir / 'README.md'
        readme = f"""# LinksPlatform Custom XRef Service

This directory contains a custom xrefService implementation for LinksPlatform documentation.

## Usage

Add the following to your docfx.json:

```json
{{
  "build": {{
    "xrefService": [
      "https://yourusername.github.io/yourrepo/xref/{{uid}}.json",
      "https://xref.docs.microsoft.com/query?uid={{uid}}"
    ]
  }}
}}
```

## Files

- `index.json`: Complete list of available UIDs
- `{{uid}}.json`: Individual xref files for each UID

Generated on: {datetime.now().isoformat()}
"""
        
        with open(readme_path, 'w', encoding='utf-8') as f:
            f.write(readme)
            
        print(f'Generated GitHub Pages structure in {self.output_dir}')

    def generate(self):
        """Main execution method"""
        try:
            self.build_xref_map()
            self.generate_xref_files()
            self.generate_index_file()
            self.generate_github_pages_structure()
            
            print('\n✅ Custom XRef service generation completed!')
            print(f'📁 Files generated in: {self.output_dir}')
            print('\n📋 Next steps:')
            print('1. Deploy the xref directory to GitHub Pages or any static hosting')
            print('2. Update your docfx.json with the custom xrefService URL')
            print('3. Test the documentation generation')
            
        except Exception as e:
            print(f'❌ Error generating XRef service: {e}')
            sys.exit(1)

def main():
    """CLI execution"""
    output_dir = sys.argv[1] if len(sys.argv) > 1 else './xref'
    generator = XRefServiceGenerator(output_dir)
    generator.generate()

if __name__ == '__main__':
    main()
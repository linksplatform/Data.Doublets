#!/usr/bin/env node

/**
 * LinksPlatform Custom XRef Service Generator
 * 
 * This script generates a custom xrefService for LinksPlatform documentation
 * by creating JSON files that can be served statically from GitHub Pages
 * or any web server to replace the Microsoft xrefService.
 * 
 * Usage: node xref-service-generator.js [output-directory]
 */

const fs = require('fs');
const path = require('path');
const https = require('https');

// Configuration for LinksPlatform packages
const LINKSPLATFORM_PACKAGES = [
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
];

// Base URLs for LinksPlatform documentation
const LINKSPLATFORM_BASE_URL = 'https://linksplatform.github.io';

class XRefServiceGenerator {
    constructor(outputDir = './xref') {
        this.outputDir = outputDir;
        this.xrefMap = new Map();
        this.ensureOutputDirectory();
    }

    ensureOutputDirectory() {
        if (!fs.existsSync(this.outputDir)) {
            fs.mkdirSync(this.outputDir, { recursive: true });
        }
    }

    /**
     * Downloads xrefmap.yml from a LinksPlatform repository if it exists
     */
    async downloadXRefMap(packageName) {
        const repoName = packageName.replace('Platform.', '');
        const xrefUrl = `${LINKSPLATFORM_BASE_URL}/${repoName}/xrefmap.yml`;
        
        try {
            console.log(`Attempting to download: ${xrefUrl}`);
            const data = await this.httpGet(xrefUrl);
            return this.parseXRefMapYml(data);
        } catch (error) {
            console.log(`No xrefmap found for ${packageName}: ${error.message}`);
            return [];
        }
    }

    httpGet(url) {
        return new Promise((resolve, reject) => {
            https.get(url, (res) => {
                if (res.statusCode !== 200) {
                    reject(new Error(`HTTP ${res.statusCode}`));
                    return;
                }
                
                let data = '';
                res.on('data', chunk => data += chunk);
                res.on('end', () => resolve(data));
            }).on('error', reject);
        });
    }

    /**
     * Parses YAML xrefmap format (simplified parser)
     */
    parseXRefMapYml(yamlContent) {
        const references = [];
        const lines = yamlContent.split('\n');
        let currentRef = {};
        
        for (const line of lines) {
            const trimmed = line.trim();
            if (trimmed.startsWith('- uid:')) {
                if (currentRef.uid) {
                    references.push({ ...currentRef });
                }
                currentRef = { uid: trimmed.substring(6).trim() };
            } else if (trimmed.startsWith('name:')) {
                currentRef.name = trimmed.substring(5).trim();
            } else if (trimmed.startsWith('href:')) {
                currentRef.href = trimmed.substring(5).trim();
            } else if (trimmed.startsWith('fullName:')) {
                currentRef.fullName = trimmed.substring(9).trim();
            }
        }
        
        if (currentRef.uid) {
            references.push(currentRef);
        }
        
        return references;
    }

    /**
     * Generates synthetic xref entries for known LinksPlatform types
     */
    generateSyntheticReferences(packageName) {
        const repoName = packageName.replace('Platform.', '');
        const baseUrl = `${LINKSPLATFORM_BASE_URL}/${repoName}/api/`;
        
        // Common patterns for LinksPlatform packages
        const commonTypes = [
            `${packageName}.ILinks`,
            `${packageName}.Links`,
            `${packageName}.LinksConstants`,
            `${packageName}.LinksOperatorBase`,
        ];

        return commonTypes.map(uid => ({
            uid,
            name: uid.split('.').pop(),
            href: `${baseUrl}${uid.replace(/\./g, '.')}.html`,
            fullName: uid
        }));
    }

    /**
     * Builds the complete xref map
     */
    async buildXRefMap() {
        console.log('Building LinksPlatform XRef map...');
        
        for (const packageName of LINKSPLATFORM_PACKAGES) {
            console.log(`Processing ${packageName}...`);
            
            // Try to download existing xrefmap
            const downloadedRefs = await this.downloadXRefMap(packageName);
            
            // Generate synthetic references as fallback
            const syntheticRefs = this.generateSyntheticReferences(packageName);
            
            // Combine references (downloaded takes precedence)
            const allRefs = [...downloadedRefs, ...syntheticRefs];
            
            // Add to main xref map
            for (const ref of allRefs) {
                if (!this.xrefMap.has(ref.uid)) {
                    this.xrefMap.set(ref.uid, ref);
                }
            }
        }
        
        console.log(`Built xref map with ${this.xrefMap.size} references`);
    }

    /**
     * Generates individual JSON files for each UID
     */
    generateXRefFiles() {
        console.log('Generating xref JSON files...');
        
        let fileCount = 0;
        for (const [uid, reference] of this.xrefMap) {
            const fileName = `${uid}.json`;
            const filePath = path.join(this.outputDir, fileName);
            
            const xrefResponse = {
                uid: reference.uid,
                name: reference.name || reference.uid,
                href: reference.href,
                fullName: reference.fullName || reference.uid,
                type: 'Type',
                summary: `LinksPlatform ${reference.uid} reference`,
                namespace: reference.uid.split('.').slice(0, -1).join('.'),
                assembly: reference.uid.split('.')[0] + '.' + reference.uid.split('.')[1]
            };
            
            fs.writeFileSync(filePath, JSON.stringify(xrefResponse, null, 2));
            fileCount++;
        }
        
        console.log(`Generated ${fileCount} xref JSON files in ${this.outputDir}`);
    }

    /**
     * Creates an index file listing all available references
     */
    generateIndexFile() {
        const indexPath = path.join(this.outputDir, 'index.json');
        const allUids = Array.from(this.xrefMap.keys());
        
        const index = {
            generated: new Date().toISOString(),
            totalReferences: allUids.length,
            packages: LINKSPLATFORM_PACKAGES,
            uids: allUids
        };
        
        fs.writeFileSync(indexPath, JSON.stringify(index, null, 2));
        console.log(`Generated index file: ${indexPath}`);
    }

    /**
     * Creates a GitHub Pages compatible directory structure
     */
    generateGitHubPagesStructure() {
        // Create _config.yml for GitHub Pages
        const configPath = path.join(this.outputDir, '_config.yml');
        const config = `# LinksPlatform Custom XRef Service
plugins:
  - jekyll-optional-front-matter

include:
  - "*.json"

defaults:
  - scope:
      path: "*.json"
    values:
      layout: null`;
        
        fs.writeFileSync(configPath, config);

        // Create README.md
        const readmePath = path.join(this.outputDir, 'README.md');
        const readme = `# LinksPlatform Custom XRef Service

This directory contains a custom xrefService implementation for LinksPlatform documentation.

## Usage

Add the following to your docfx.json:

\`\`\`json
{
  "build": {
    "xrefService": [
      "https://yourusername.github.io/yourrepo/xref/{uid}.json",
      "https://xref.docs.microsoft.com/query?uid={uid}"
    ]
  }
}
\`\`\`

## Files

- \`index.json\`: Complete list of available UIDs
- \`{uid}.json\`: Individual xref files for each UID

Generated on: ${new Date().toISOString()}
`;
        
        fs.writeFileSync(readmePath, readme);
        console.log(`Generated GitHub Pages structure in ${this.outputDir}`);
    }

    /**
     * Main execution method
     */
    async generate() {
        try {
            await this.buildXRefMap();
            this.generateXRefFiles();
            this.generateIndexFile();
            this.generateGitHubPagesStructure();
            
            console.log('\n✅ Custom XRef service generation completed!');
            console.log(`📁 Files generated in: ${this.outputDir}`);
            console.log('\n📋 Next steps:');
            console.log('1. Deploy the xref directory to GitHub Pages or any static hosting');
            console.log('2. Update your docfx.json with the custom xrefService URL');
            console.log('3. Test the documentation generation');
            
        } catch (error) {
            console.error('❌ Error generating XRef service:', error);
            process.exit(1);
        }
    }
}

// CLI execution
if (require.main === module) {
    const outputDir = process.argv[2] || './xref';
    const generator = new XRefServiceGenerator(outputDir);
    generator.generate();
}

module.exports = XRefServiceGenerator;
#!/usr/bin/env python3

import os
import re
from pathlib import Path

def normalize_path_for_comparison(path_str, base_dir):
    """Normalize a path for comparison by removing base directory and converting to relative form"""
    path = Path(path_str)
    try:
        relative_path = path.relative_to(Path(base_dir))
        return str(relative_path)
    except ValueError:
        return str(path)

def get_filename_without_extension(filepath):
    """Get filename without extension"""
    return Path(filepath).stem

def analyze_translation_status():
    base_dir = "/tmp/gh-issue-solver-1757799898250"
    csharp_dir = f"{base_dir}/csharp"
    cpp_dir = f"{base_dir}/cpp"
    
    # Get all C# files
    csharp_files = []
    for root, dirs, files in os.walk(csharp_dir):
        for file in files:
            if file.endswith('.cs'):
                full_path = os.path.join(root, file)
                rel_path = normalize_path_for_comparison(full_path, csharp_dir)
                csharp_files.append({
                    'full_path': full_path,
                    'relative_path': rel_path,
                    'filename': get_filename_without_extension(file),
                    'directory': os.path.dirname(rel_path)
                })
    
    # Get all C++ header files
    cpp_files = []
    for root, dirs, files in os.walk(cpp_dir):
        for file in files:
            if file.endswith('.h'):
                full_path = os.path.join(root, file)
                rel_path = normalize_path_for_comparison(full_path, cpp_dir)
                cpp_files.append({
                    'full_path': full_path,
                    'relative_path': rel_path,
                    'filename': get_filename_without_extension(file),
                    'directory': os.path.dirname(rel_path)
                })
    
    print("=== C# TO C++ TRANSLATION ANALYSIS ===")
    print(f"Total C# files: {len(csharp_files)}")
    print(f"Total C++ header files: {len(cpp_files)}")
    print()
    
    # Create mapping of C++ files by filename
    cpp_by_filename = {}
    for cpp_file in cpp_files:
        filename = cpp_file['filename']
        if filename not in cpp_by_filename:
            cpp_by_filename[filename] = []
        cpp_by_filename[filename].append(cpp_file)
    
    # Analyze translation status
    translated_files = []
    missing_translations = []
    
    for cs_file in csharp_files:
        cs_filename = cs_file['filename']
        cs_dir = cs_file['directory']
        
        # Skip test files and benchmark files for core analysis
        if 'Tests' in cs_dir or 'Benchmarks' in cs_dir:
            continue
            
        # Look for corresponding C++ file
        found_cpp = None
        if cs_filename in cpp_by_filename:
            # Check if directory structure matches or is reasonable
            for cpp_candidate in cpp_by_filename[cs_filename]:
                cpp_dir = cpp_candidate['directory']
                # Check if directories have reasonable correspondence
                if (cs_dir in cpp_dir or cpp_dir in cs_dir or 
                    cs_dir.replace('Platform.Data.Doublets/', '') in cpp_dir or
                    cpp_dir.replace('Platform.Data.Doublets/', '') in cs_dir):
                    found_cpp = cpp_candidate
                    break
            
            if not found_cpp and cpp_by_filename[cs_filename]:
                # Take the first match if no directory correspondence found
                found_cpp = cpp_by_filename[cs_filename][0]
        
        if found_cpp:
            translated_files.append({
                'cs_file': cs_file,
                'cpp_file': found_cpp
            })
        else:
            missing_translations.append(cs_file)
    
    # Categorize missing translations by importance
    core_missing = []
    decorator_missing = []
    memory_missing = []
    other_missing = []
    
    for missing in missing_translations:
        dir_parts = missing['directory'].split('/')
        if 'Decorators' in dir_parts:
            decorator_missing.append(missing)
        elif 'Memory' in dir_parts:
            memory_missing.append(missing)
        elif len(dir_parts) <= 1 or dir_parts[0] == 'Platform.Data.Doublets':  # Root level files
            core_missing.append(missing)
        else:
            other_missing.append(missing)
    
    print("=== TRANSLATION STATUS ===")
    print(f"Files with C++ translations: {len(translated_files)}")
    print(f"Files missing C++ translations: {len(missing_translations)}")
    print()
    
    print("=== SUCCESSFULLY TRANSLATED FILES ===")
    for item in sorted(translated_files, key=lambda x: x['cs_file']['relative_path']):
        print(f"✓ {item['cs_file']['relative_path']} -> {item['cpp_file']['relative_path']}")
    print()
    
    print("=== MISSING TRANSLATIONS BY PRIORITY ===")
    
    print("HIGH PRIORITY - Core Library Files:")
    for missing in sorted(core_missing, key=lambda x: x['relative_path']):
        print(f"  - {missing['relative_path']}")
    print()
    
    print("MEDIUM PRIORITY - Memory Management:")  
    for missing in sorted(memory_missing, key=lambda x: x['relative_path']):
        print(f"  - {missing['relative_path']}")
    print()
    
    print("MEDIUM PRIORITY - Decorators:")
    for missing in sorted(decorator_missing, key=lambda x: x['relative_path']):
        print(f"  - {missing['relative_path']}")
    print()
    
    print("LOW PRIORITY - Other:")
    for missing in sorted(other_missing, key=lambda x: x['relative_path']):
        print(f"  - {missing['relative_path']}")
    print()
    
    # Analyze C++ specific files (files that exist in C++ but not in C#)
    cpp_only_files = []
    cs_by_filename = {cs['filename']: cs for cs in csharp_files}
    
    for cpp_file in cpp_files:
        if cpp_file['filename'] not in cs_by_filename:
            cpp_only_files.append(cpp_file)
    
    print("=== C++ SPECIFIC FILES (not in C#) ===")
    for cpp_file in sorted(cpp_only_files, key=lambda x: x['relative_path']):
        print(f"  + {cpp_file['relative_path']}")
    print()
    
    # Summary with recommendations
    print("=== SUMMARY AND RECOMMENDATIONS ===")
    print(f"Translation Progress: {len(translated_files)}/{len(translated_files) + len(missing_translations)} ({len(translated_files) / (len(translated_files) + len(missing_translations)) * 100:.1f}%)")
    print()
    print("RECOMMENDED TRANSLATION PRIORITY:")
    print("1. HIGH PRIORITY - Core Files (start here):")
    for missing in sorted(core_missing, key=lambda x: x['relative_path'])[:5]:
        print(f"   • {missing['filename']}.cs")
    print()
    print("2. MEDIUM PRIORITY - Essential Memory Management:")
    critical_memory = [m for m in memory_missing if any(keyword in m['filename'].lower() 
                      for keyword in ['links', 'memory', 'tree', 'list'])][:3]
    for missing in critical_memory:
        print(f"   • {missing['filename']}.cs")
    print()
    print("3. MEDIUM PRIORITY - Key Decorators:")
    key_decorators = [m for m in decorator_missing if any(keyword in m['filename'].lower() 
                     for keyword in ['base', 'validator', 'resolver'])][:3]
    for missing in key_decorators:
        print(f"   • {missing['filename']}.cs")

if __name__ == "__main__":
    analyze_translation_status()
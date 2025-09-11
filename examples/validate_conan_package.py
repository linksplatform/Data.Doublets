#!/usr/bin/env python3
"""
Validation script for Platform.Data.Doublets Conan package
This script validates the structure without requiring Conan to be installed
"""

import os
import sys

def validate_conanfile():
    """Validate conanfile.py structure"""
    conanfile_path = "cpp/conanfile.py"
    
    if not os.path.exists(conanfile_path):
        print("❌ conanfile.py not found")
        return False
    
    with open(conanfile_path, 'r') as f:
        content = f.read()
    
    required_elements = [
        "class PlatformDataDoubletsConan",
        "name = \"platform.data.doublets\"",
        "def build(",
        "def package(",
        "def package_info(",
        "platform.interfaces",
        "platform.collections",
        "platform.memory",
        "platform.data",
        "platform.exceptions",
        "platform.setters",
        "platform.ranges"
    ]
    
    missing = []
    for element in required_elements:
        if element not in content:
            missing.append(element)
    
    if missing:
        print(f"❌ Missing required elements: {missing}")
        return False
    
    print("✅ conanfile.py structure is valid")
    return True

def validate_cmake():
    """Validate CMakeLists.txt has required elements for Conan"""
    cmake_path = "cpp/CMakeLists.txt"
    
    if not os.path.exists(cmake_path):
        print("❌ CMakeLists.txt not found")
        return False
    
    with open(cmake_path, 'r') as f:
        content = f.read()
    
    required_elements = [
        "find_package(Platform.Interfaces)",
        "find_package(Platform.Setters)",
        "install(TARGETS",
        "install(DIRECTORY",
        "install(EXPORT"
    ]
    
    missing = []
    for element in required_elements:
        if element not in content:
            missing.append(element)
    
    if missing:
        print(f"❌ CMakeLists.txt missing required elements: {missing}")
        return False
    
    print("✅ CMakeLists.txt has required Conan elements")
    return True

def validate_headers():
    """Validate that header files exist"""
    headers_dir = "cpp/Platform.Data.Doublets"
    
    if not os.path.exists(headers_dir):
        print("❌ Platform.Data.Doublets directory not found")
        return False
    
    header_files = []
    for root, dirs, files in os.walk(headers_dir):
        for file in files:
            if file.endswith('.h'):
                header_files.append(os.path.join(root, file))
    
    if not header_files:
        print("❌ No header files found")
        return False
    
    print(f"✅ Found {len(header_files)} header files")
    return True

def main():
    print("🔍 Validating Platform.Data.Doublets Conan package...")
    
    # Change to repository root
    script_dir = os.path.dirname(os.path.abspath(__file__))
    repo_root = os.path.dirname(script_dir)
    os.chdir(repo_root)
    
    print(f"📁 Working directory: {os.getcwd()}")
    
    all_valid = True
    all_valid &= validate_conanfile()
    all_valid &= validate_cmake()
    all_valid &= validate_headers()
    
    if all_valid:
        print("\n🎉 All validations passed! Conan package structure is ready.")
        return 0
    else:
        print("\n❌ Some validations failed. Please fix the issues above.")
        return 1

if __name__ == "__main__":
    sys.exit(main())
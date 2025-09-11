from conan import ConanFile
from conan.tools.cmake import CMake, cmake_layout, CMakeDeps, CMakeToolchain
from conan.tools.files import copy
import os

class PlatformDataDoubletsConan(ConanFile):
    name = "platform.data.doublets"
    version = "0.1.0"  # Will be updated from git tag or version file
    
    # Binary configuration
    settings = "os", "compiler", "build_type", "arch"
    options = {
        "shared": [True, False], 
        "fPIC": [True, False],
        "tests": [True, False],
        "benchmarks": [True, False]
    }
    default_options = {
        "shared": False, 
        "fPIC": True,
        "tests": False,
        "benchmarks": False
    }
    
    # Sources are located in the same place as this recipe, copy them to the recipe
    exports_sources = "CMakeLists.txt", "Platform.Data.Doublets/*", "Platform.Data.Doublets.Tests/*", "Platform.Data.Doublets.Benchmarks/*"
    
    # Dependencies
    requires = (
        "platform.interfaces/0.3.41",
        "platform.collections.methods/0.3.0", 
        "platform.collections/0.2.1",
        "platform.numbers/0.1.0",
        "platform.memory/0.1.0",
        "platform.exceptions/0.3.2",
        "platform.data/0.1.1",
        "platform.setters/0.1.0",
        "platform.ranges/0.2.0",
        "mio/cci.20201220"
    )
    
    def build_requirements(self):
        if self.options.tests:
            self.test_requires("gtest/cci.20210126")
        if self.options.benchmarks:
            self.test_requires("benchmark/1.6.0")
    
    def configure(self):
        if self.settings.compiler.cppstd:
            self.settings.compiler.cppstd = "20"
        
    def config_options(self):
        if self.settings.os == "Windows":
            del self.options.fPIC

    def layout(self):
        cmake_layout(self)

    def generate(self):
        deps = CMakeDeps(self)
        deps.generate()
        tc = CMakeToolchain(self)
        tc.variables["LINKS_PLATFORM_TESTS"] = self.options.tests
        tc.variables["LINKS_PLATFORM_BENCHMARKS"] = self.options.benchmarks
        tc.generate()

    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
        if self.options.tests:
            cmake.test()

    def package(self):
        copy(self, "*.h", dst=os.path.join(self.package_folder, "include"), src=self.source_folder)
        copy(self, "*.hpp", dst=os.path.join(self.package_folder, "include"), src=self.source_folder)
        cmake = CMake(self)
        cmake.install()

    def package_info(self):
        self.cpp_info.libs = []  # Header-only library
        self.cpp_info.includedirs = ["include"]
        
        # Set the target name to match CMake target
        self.cpp_info.set_property("cmake_target_name", "Platform.Data.Doublets::Platform.Data.Doublets.Library")
        
        # Add system libs if needed
        if self.settings.os in ["Linux", "FreeBSD"]:
            self.cpp_info.system_libs.append("dl")
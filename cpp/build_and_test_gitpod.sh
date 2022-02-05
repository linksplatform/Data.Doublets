#!/bin/bash

cd ..

sudo apt install -y g++-10

pip install conan

conan profile new linksplatform --detect
conan profile update settings.compiler=gcc linksplatform
conan profile update settings.compiler.version=10 linksplatform
conan profile update settings.compiler.libcxx=libstdc++11 linksplatform
conan profile update env.CXX=g++-10 linksplatform
conan profile show linksplatform

git clone https://github.com/linksplatform/conan-center-index
cd conan-center-index && cd recipes
git checkout update-interfaces
conan create platform.interfaces/0.2.0+ platform.interfaces/0.2.0@ -pr=linksplatform
git checkout only-development
platform.exceptions/0.2.0
platform.equality/0.0.1
platform.hashing/0.2.0
platform.random/0.1.0
cd ../..
ls
cd Collections
cmake_flags="-DCMAKE_BUILD_TYPE=Release -DCMAKE_CXX_COMPILER=g++-10 -DLINKS_PLATFORM_TESTS=ON"
cmake_build_dir="build"
cd cpp && mkdir -p $cmake_build_dir && cd $cmake_build_dir
conan install .. -pr=linksplatform --build=missing
cmake .. $cmake_flags
cmake --build .
scan-build cmake --build .
binaries=bin/*
for binary in $binaries
do
   ./$binary
done

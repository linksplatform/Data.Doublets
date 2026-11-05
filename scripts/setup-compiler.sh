#!/usr/bin/env bash
# Install current stable compilers on the Ubuntu 26.04 CI image.
set -euo pipefail
case ${1:-} in
    gcc)
        sudo apt-get update &
        wait $!
        sudo apt-get install -y g++-16 &
        wait $!
        g++-16 --version
        ;;
    clang*)
        key=$(mktemp)
        trap 'rm -f "$key"' EXIT
        curl --fail --silent --show-error --location https://apt.llvm.org/llvm-snapshot.gpg.key -o "$key"
        echo "8b2a587ffd672c4687e7581dad4b2f6c1bb2ad6b480cd9771ba2ff48e0b8c75d  $key" | sha256sum --check
        sudo install -m 644 "$key" /usr/share/keyrings/llvm.asc
        echo 'deb [signed-by=/usr/share/keyrings/llvm.asc] https://apt.llvm.org/resolute/ llvm-toolchain-resolute-23 main' | sudo tee /etc/apt/sources.list.d/llvm-23.list
        sudo apt-get update &
        wait $!
        sudo apt-get install -y clang-23 clang-tidy-23 libclang-rt-23-dev &
        wait $!
        clang++-23 --version
        ;;
    *) echo 'Usage: setup-compiler.sh {gcc|clang|clang-sanitized}' >&2; exit 2 ;;
esac

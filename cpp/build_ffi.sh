#!/bin/bash

# Install cargo
curl https://sh.rustup.rs -sSf | sh

cd ../rust;
cargo build --release;
cd ffi;
cargo build --release;

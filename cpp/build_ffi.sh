#!/bin/bash

# Install rustup, cargo
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
rustup install nightly

cd ../rust;
cargo +nightly build --release;
cd ffi;
cargo +nightly build --release;
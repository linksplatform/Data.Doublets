#!/bin/bash

# Install rustup, cargo
rustup install nightly

cd ../rust/ffi;
cargo +nightly build --release;
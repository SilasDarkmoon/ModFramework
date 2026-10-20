#!/bin/sh
# build-ios.sh - build liblua55.a for iOS (fat: device arm64 + simulator arm64/x86_64)
# Run on macOS. Usage: sh build-ios.sh [path-to-lua-src]   (default: ./lua-5.5.0/src)
set -e

SRC="${1:-./lua-5.5.0/src}"
if [ ! -f "$SRC/lua.h" ]; then
    echo "ERROR: lua.h not found under '$SRC'"
    echo "Usage: sh build-ios.sh <path-to-lua-5.5.0-src>"
    exit 1
fi

CORE="lapi lcode lctype ldebug ldo ldump lfunc lgc llex lmem lobject lopcodes lparser lstate lstring ltable ltm lundump lvm lzio"
LIB="lauxlib lbaselib lcorolib ldblib liolib lmathlib loadlib loslib lstrlib ltablib lutf8lib linit"

SDK_IPHONE=$(xcrun --sdk iphoneos --show-sdk-path)
SDK_SIM=$(xcrun --sdk iphonesimulator --show-sdk-path)

rm -rf build-ios
mkdir -p build-ios/device build-ios/sim-arm64 build-ios/sim-x86_64

echo "== device arm64 =="
for f in $CORE $LIB; do
    # LUA_USE_IOS: official iOS build flag (Makefile 'ios' target) -
    # loslib.c replaces system() with a stub returning -1, luaconf.h enables LUA_USE_POSIX + LUA_USE_DLOPEN
    clang -target arm64-apple-ios12.0 -isysroot "$SDK_IPHONE" -O2 -DLUA_USE_IOS -c "$SRC/$f.c" -o "build-ios/device/$f.o"
done
libtool -static -o build-ios/liblua55-device.a build-ios/device/*.o

echo "== simulator arm64 =="
for f in $CORE $LIB; do
    clang -target arm64-apple-ios12.0-simulator -isysroot "$SDK_SIM" -O2 -DLUA_USE_IOS -c "$SRC/$f.c" -o "build-ios/sim-arm64/$f.o"
done
libtool -static -o build-ios/liblua55-sim-arm64.a build-ios/sim-arm64/*.o

echo "== simulator x86_64 =="
for f in $CORE $LIB; do
    clang -target x86_64-apple-ios12.0-simulator -isysroot "$SDK_SIM" -O2 -DLUA_USE_IOS -c "$SRC/$f.c" -o "build-ios/sim-x86_64/$f.o"
done
libtool -static -o build-ios/liblua55-sim-x86_64.a build-ios/sim-x86_64/*.o

# NOTE: device arm64 and simulator arm64 share the same cputype (ARM64) - a plain
# fat file CANNOT hold both (the fat format has no platform field; this is exactly
# why Apple invented XCframework). Produce two separate libraries:
#   liblua55.a      = device arm64 only (for Unity iOS device builds)
#   liblua55-sim.a  = simulator fat (arm64-sim + x86_64-sim; local debugging only,
#                     do NOT put both .a files into the same Unity project)
lipo -create -output liblua55.a build-ios/liblua55-device.a
lipo -create -output liblua55-sim.a \
    build-ios/liblua55-sim-arm64.a \
    build-ios/liblua55-sim-x86_64.a

echo "DONE: $(pwd)/liblua55.a (device) / liblua55-sim.a (simulator)"
lipo -info liblua55.a
lipo -info liblua55-sim.a
echo "Copy liblua55.a to ModLua.PlatDep/natives/ios/liblua55.a"

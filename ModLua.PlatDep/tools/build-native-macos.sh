#!/bin/sh
# build-macos.sh - build liblua55.dylib for macOS (universal: arm64 + x86_64)
# Run on macOS. Usage: sh build-macos.sh [path-to-lua-src]   (default: ./lua-5.5.0/src)
set -e

SRC="${1:-./lua-5.5.0/src}"
if [ ! -f "$SRC/lua.h" ]; then
    echo "ERROR: lua.h not found under '$SRC'"
    echo "Usage: sh build-macos.sh <path-to-lua-5.5.0-src>"
    exit 1
fi

CORE="lapi lcode lctype ldebug ldo ldump lfunc lgc llex lmem lobject lopcodes lparser lstate lstring ltable ltm lundump lvm lzio"
LIB="lauxlib lbaselib lcorolib ldblib liolib lmathlib loadlib loslib lstrlib ltablib lutf8lib linit"

FILES=""
for f in $CORE $LIB; do FILES="$FILES $SRC/$f.c"; done

clang -dynamiclib -O2 -arch arm64 -arch x86_64 \
    -DLUA_USE_MACOSX \
    -install_name @rpath/liblua55.dylib \
    $FILES -o liblua55.dylib

echo "DONE: $(pwd)/liblua55.dylib"
lipo -info liblua55.dylib
echo "Copy it to ModLua.PlatDep/natives/macos/liblua55.dylib"

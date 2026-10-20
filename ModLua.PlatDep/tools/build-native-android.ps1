# build-android.ps1 - build liblua55.so (arm64-v8a + armeabi-v7a) with NDK r13b clang
# ASCII only (PowerShell 5.1 ANSI parsing rule).
# Usage: powershell -ExecutionPolicy Bypass -File build-android.ps1 [-NdkRoot <path>] [-LuaSrc <path>]
param(
    [string]$NdkRoot = "E:\Android\ndk\android-ndk-r13b",
    [string]$LuaSrc = "D:\MyFiles\gitroot\3rd\lua-5.5.0\src"
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path   # ModLua.PlatDep\tools
$natives = Join-Path (Split-Path -Parent $here) 'natives'  # ModLua.PlatDep\natives

$clang = Join-Path $NdkRoot 'toolchains\llvm\prebuilt\windows-x86_64\bin\clang.exe'
if (-not (Test-Path $clang)) { throw "clang not found: $clang" }

# 32 library sources (lua.c / luac.c are standalone programs, excluded)
$core = @('lapi','lcode','lctype','ldebug','ldo','ldump','lfunc','lgc','llex','lmem',
          'lobject','lopcodes','lparser','lstate','lstring','ltable','ltm','lundump','lvm','lzio')
$lib  = @('lauxlib','lbaselib','lcorolib','ldblib','liolib','lmathlib','loadlib','loslib',
          'lstrlib','ltablib','lutf8lib','linit')
$files = ($core + $lib) | ForEach-Object { Join-Path $LuaSrc "$_.c" }

# arm64-v8a (API 21)
$outDir = "$natives\android-arm64-v8a"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$sysroot = Join-Path $NdkRoot 'platforms\android-21\arch-arm64'
$gccToolchain = Join-Path $NdkRoot 'toolchains\aarch64-linux-android-4.9\prebuilt\windows-x86_64'
& $clang --target=aarch64-linux-android "--gcc-toolchain=$gccToolchain" "--sysroot=$sysroot" -shared -fPIC -O2 -o "$outDir\liblua55.so" $files
if ($LASTEXITCODE -ne 0) { throw 'clang failed for arm64-v8a' }
Write-Host "OK   arm64-v8a -> $outDir\liblua55.so"

# armeabi-v7a (API 21)
$outDir = "$natives\android-armeabi-v7a"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$sysroot = Join-Path $NdkRoot 'platforms\android-21\arch-arm'
$gccToolchain = Join-Path $NdkRoot 'toolchains\arm-linux-androideabi-4.9\prebuilt\windows-x86_64'
& $clang --target=arm-linux-androideabi -march=armv7-a -mfloat-abi=softfp -mfpu=vfpv3-d16 "--gcc-toolchain=$gccToolchain" "--sysroot=$sysroot" -shared -fPIC -O2 -o "$outDir\liblua55.so" $files
if ($LASTEXITCODE -ne 0) { throw 'clang failed for armeabi-v7a' }
Write-Host "OK   armeabi-v7a -> $outDir\liblua55.so"

Write-Host 'DONE'

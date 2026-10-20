# export-unity.ps1 - build ModLua.PlatDep (dynamic + iOS flavors) and export
# managed dlls + native libs into a Unity project's Assets/Plugins/ModLua/.
# ASCII only (PowerShell 5.1 ANSI parsing rule).
#
# FIRST IMPORT: Unity generates .meta files automatically with ALL platforms
# enabled by default - you MUST adjust the platform checkboxes in the Plugin
# Inspector once (checklist printed at the end). Later runs only overwrite the
# dll/so files; Unity keeps the existing .meta (platform settings preserved).
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File export-unity.ps1 -UnityAssets "D:\MyUnityProject\Assets"
#   (add -NoBuild to skip the dotnet build steps - used by the MSBuild target)
param(
    [Parameter(Mandatory = $true)]
    [string]$UnityAssets,
    [string]$Configuration = 'Release',
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path          # ModLua.PlatDep\tools
$projDir = Split-Path -Parent $here                               # ModLua.PlatDep
$natives = Join-Path $projDir 'natives'
$csproj = Join-Path $projDir 'ModLua.PlatDep.csproj'

# 1) build both flavors (dynamic-library + iOS static-linking)
if (-not $NoBuild) {
    dotnet build $csproj -c $Configuration --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "build failed: $Configuration" }
    dotnet build $csproj -c "$Configuration.iOS" --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "build failed: $Configuration.iOS" }
}

$dynDll = Join-Path $projDir "bin\$Configuration\netstandard2.0\ModLua.PlatDep.dll"
$iosDll = Join-Path $projDir "bin\$Configuration.iOS\netstandard2.0\ModLua.PlatDep.dll"

# 2) layout under Assets/Plugins/ModLua
#    - the two managed dlls share the SAME assembly name (ModLua.PlatDep):
#      platform checkboxes must be mutually exclusive (any given build target
#      has exactly one of them active, so ModLua.dll's assembly reference
#      resolves on every platform)
#    - Android .so under libs/<abi> (Unity convention)
#    - iOS .a: meta enables iOS -> Unity auto-links it into the Xcode project
$dst = Join-Path $UnityAssets 'Plugins\ModLua'
$items = @(
    @{ Src = $dynDll;  Dst = Join-Path $dst 'ModLua.PlatDep.dll' },
    @{ Src = $iosDll;  Dst = Join-Path $dst 'iOS\ModLua.PlatDep.dll' },
    @{ Src = (Join-Path $natives 'win-x64\lua55.dll');  Dst = Join-Path $dst 'x86_64\lua55.dll' },
    @{ Src = (Join-Path $natives 'win-x86\lua55.dll');  Dst = Join-Path $dst 'x86\lua55.dll' },
    @{ Src = (Join-Path $natives 'android-arm64-v8a\liblua55.so');    Dst = Join-Path $dst 'Android\libs\arm64-v8a\liblua55.so' },
    @{ Src = (Join-Path $natives 'android-armeabi-v7a\liblua55.so');  Dst = Join-Path $dst 'Android\libs\armeabi-v7a\liblua55.so' },
    @{ Src = (Join-Path $natives 'ios\liblua55.a');     Dst = Join-Path $dst 'iOS\liblua55.a' },
    @{ Src = (Join-Path $natives 'macos\liblua55.dylib'); Dst = Join-Path $dst 'macOS\liblua55.dylib' }
)

$firstTime = $true
foreach ($it in $items) {
    if (-not (Test-Path $it.Src)) { Write-Host "SKIP (missing source): $($it.Src)"; continue }
    $dir = Split-Path -Parent $it.Dst
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Copy-Item $it.Src $it.Dst -Force
    if (Test-Path "$($it.Dst).meta") { $firstTime = $false }
    Write-Host "COPY -> $($it.Dst)"
}

Write-Host ''
if ($firstTime) {
    Write-Host '=== FIRST IMPORT: adjust Plugin platform checkboxes in Unity Inspector ==='
    Write-Host 'ModLua.PlatDep.dll        : Editor + Standalone(all) + Android;  iOS OFF'
    Write-Host 'iOS/ModLua.PlatDep.dll    : iOS ONLY (everything else OFF)'
    Write-Host 'x86_64/lua55.dll          : Editor + Standalone Windows (CPU x86_64)'
    Write-Host 'x86/lua55.dll             : Standalone Windows (CPU x86)'
    Write-Host 'Android/.../arm64-v8a     : Android (CPU ARM64)'
    Write-Host 'Android/.../armeabi-v7a   : Android (CPU ARMv7)'
    Write-Host 'iOS/liblua55.a            : iOS (auto-linked into the Xcode project)'
    Write-Host 'macOS/liblua55.dylib      : Editor + Standalone OSX (CPU Any)'
    Write-Host 'NOTE: the two managed dlls share the assembly name - their platform'
    Write-Host 'checkboxes MUST be mutually exclusive, otherwise Unity reports'
    Write-Host 'duplicate assemblies. Do this right after the first import.'
}
else {
    Write-Host 'Existing .meta files detected - Unity keeps platform settings.'
    Write-Host 'Re-import happens automatically on focus; just let it refresh.'
}

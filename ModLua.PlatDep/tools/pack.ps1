# pack.ps1 - aggregate packaging: NuGet package + UPM package.
# ASCII only (PowerShell 5.1 ANSI parsing rule).
#
# Produces under ModLua.PlatDep/dist/:
#   nuget/ModLua.PlatDep.<version>.nupkg   (lib/netstandard2.0 + runtimes/win-x64/native)
#   upm/com.modlua/                        (UPM package folder)
#   upm/com.modlua-<version>.zip           (UPM package archive)
#
# The UPM package carries .meta files with deterministic GUIDs (MD5 of the
# package-relative path, stable across rebuilds) and best-effort platform
# checkboxes; the shipped Editor script (ModLuaPluginSetup.cs) fixes the
# checkboxes via PluginImporter APIs in case Unity regenerates or resets the
# .meta files.
#
# Usage: powershell -ExecutionPolicy Bypass -File pack.ps1 [-NoBuild] [-Bump] [-SetVersion 1.2.3]
#   -Bump        : increment the patch part of the csproj version (written back)
#   -SetVersion  : set an explicit version (written back to the csproj)
param(
    [string]$Configuration = 'Release',
    [switch]$NoBuild,
    [string]$SetVersion,
    [switch]$Bump
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path   # ModLua.PlatDep\tools
$projDir = Split-Path -Parent $here                         # ModLua.PlatDep
$natives = Join-Path $projDir 'natives'
$csproj = Join-Path $projDir 'ModLua.PlatDep.csproj'
$dist = Join-Path $projDir 'dist'

# ---------------- .meta generation ----------------

function Get-DeterministicGuid([string]$seed) {
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $bytes = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($seed))
    return ([System.BitConverter]::ToString($bytes) -replace '-', '').ToLower()
}

function New-PluginMeta([string]$Path, [string]$RelPath, [string]$Kind) {
    $guid = Get-DeterministicGuid "com.modlua/$RelPath"
    $pd = switch ($Kind) {
        'ManagedDynamic' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Android
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
- first:
    Editor: Editor
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
      DefaultValueInitialized: 1
- first:
    Standalone
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
- first:
    iPhone: iOS
  second:
    enabled: 0
    settings:
      CPU: AnyCPU
      AddToEmbeddedBinaries: 0
'@ }
        'ManagedIOS' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Editor: Editor
  second:
    enabled: 0
    settings:
      CPU: AnyCPU
      DefaultValueInitialized: 1
- first:
    iPhone: iOS
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
      AddToEmbeddedBinaries: 0
'@ }
        'NativeWinX64' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Editor: Editor
  second:
    enabled: 1
    settings:
      CPU: x86_64
      DefaultValueInitialized: 1
- first:
    Standalone
  second:
    enabled: 1
    settings:
      CPU: x86_64
'@ }
        'NativeWinX86' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Standalone
  second:
    enabled: 1
    settings:
      CPU: x86
'@ }
        'NativeAndroidArm64' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Android
  second:
    enabled: 1
    settings:
      CPU: ARM64
'@ }
        'NativeAndroidArmv7' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Android
  second:
    enabled: 1
    settings:
      CPU: ARMv7
'@ }
        'NativeIOS' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    iPhone: iOS
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
      AddToEmbeddedBinaries: 0
'@ }
        'NativeMacOS' { @'
- first:
    : Any
  second:
    enabled: 0
    settings: {}
    excludedFromBuild: 0
- first:
    Editor: Editor
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
      DefaultValueInitialized: 1
- first:
    Standalone
  second:
    enabled: 1
    settings:
      CPU: AnyCPU
'@ }
    }
    $meta = "fileFormatVersion: 2`nguid: $guid`nPluginImporter:`n  externalObjects: {}`n  serializedVersion: 2`n  iconMap: {}`n  executionOrder: {}`n  isPreloaded: 0`n  isOverridable: 0`n  isExplicitlyReferenced: 0`n  validateReferences: 1`n  platformData:`n$pd"
    Set-Content -Path "$Path.meta" -Value $meta -Encoding UTF8
}

# 1) build all platform configurations
if (-not $NoBuild) {
    & (Join-Path $here 'build-all.ps1')
}

# 2) resolve the version (single source of truth: <Version> in the csproj)
$csprojText = Get-Content $csproj -Raw
$currentVersion = '0.1.0'
if ($csprojText -match '<Version>([^<]+)</Version>') { $currentVersion = $Matches[1] }
$version = $currentVersion
if ($Bump) {
    $parts = $currentVersion.Split('.')
    $parts[$parts.Length - 1] = [string]([int]$parts[$parts.Length - 1] + 1)
    $version = $parts -join '.'
}
if ($SetVersion) { $version = $SetVersion }
if ($version -ne $currentVersion) {
    # write back to the csproj (no BOM, preserve content otherwise)
    $newText = $csprojText -replace '<Version>[^<]+</Version>', "<Version>$version</Version>"
    [IO.File]::WriteAllText($csproj, $newText, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "Version: $currentVersion -> $version (csproj updated)"
}
else {
    Write-Host "Version: $version"
}

# 3) NuGet package
$nugetOut = Join-Path $dist 'nuget'
New-Item -ItemType Directory -Force -Path $nugetOut | Out-Null
dotnet pack $csproj -c $Configuration --nologo -v q -o $nugetOut
if ($LASTEXITCODE -ne 0) { throw 'dotnet pack failed' }

# 4) UPM package assembly
$upm = Join-Path $dist 'upm\com.modlua'
if (Test-Path $upm) { Remove-Item -Recurse -Force $upm }

$files = @(
    @{ Src = (Join-Path $projDir "bin\$Configuration\netstandard2.0\ModLua.PlatDep.dll"); Dst = 'Runtime\ModLua.PlatDep.dll'; Meta = 'ManagedDynamic' },
    @{ Src = (Join-Path $projDir "bin\Release.iOS\netstandard2.0\ModLua.PlatDep.dll");   Dst = 'Runtime\iOS\ModLua.PlatDep.dll';     Meta = 'ManagedIOS' },
    @{ Src = (Join-Path $natives 'win-x64\lua55.dll');                 Dst = 'Runtime\x86_64\lua55.dll';            Meta = 'NativeWinX64' },
    @{ Src = (Join-Path $natives 'win-x86\lua55.dll');                 Dst = 'Runtime\x86\lua55.dll';                Meta = 'NativeWinX86' },
    @{ Src = (Join-Path $natives 'android-arm64-v8a\liblua55.so');     Dst = 'Runtime\Android\libs\arm64-v8a\liblua55.so';    Meta = 'NativeAndroidArm64' },
    @{ Src = (Join-Path $natives 'android-armeabi-v7a\liblua55.so');   Dst = 'Runtime\Android\libs\armeabi-v7a\liblua55.so';  Meta = 'NativeAndroidArmv7' },
    @{ Src = (Join-Path $natives 'ios\liblua55.a');                    Dst = 'Runtime\iOS\liblua55.a';               Meta = 'NativeIOS' },
    @{ Src = (Join-Path $natives 'macos\liblua55.dylib');              Dst = 'Runtime\macOS\liblua55.dylib';         Meta = 'NativeMacOS' }
)

foreach ($f in $files) {
    if (-not (Test-Path $f.Src)) { Write-Host "SKIP (missing source): $($f.Src)"; continue }
    $dstPath = Join-Path $upm $f.Dst
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dstPath) | Out-Null
    Copy-Item $f.Src $dstPath -Force
    New-PluginMeta -Path $dstPath -RelPath $f.Dst -Kind $f.Meta
    Write-Host "COPY -> $($f.Dst)"
}

# Editor script (no .meta generated - Unity creates one; its guid is irrelevant
# because nothing references the script by guid)
$editorDir = Join-Path $upm 'Editor'
New-Item -ItemType Directory -Force -Path $editorDir | Out-Null
Copy-Item (Join-Path $here 'ModLuaPluginSetup.cs') (Join-Path $editorDir 'ModLuaPluginSetup.cs') -Force

# package.json
$pkgJson = @"
{
  "name": "com.modlua",
  "version": "$version",
  "displayName": "ModLua",
  "description": "Lua 5.5 bindings for Unity (netstandard2.0, Mono/IL2CPP compatible).",
  "unity": "2019.4",
  "dependencies": {}
}
"@
Set-Content -Path (Join-Path $upm 'package.json') -Value $pkgJson -Encoding UTF8

# zip archive
$zip = Join-Path $dist "upm\com.modlua-$version.zip"
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path $upm -DestinationPath $zip

Write-Host ''
Write-Host "NuGet package: $nugetOut\ModLua.PlatDep.$version.nupkg"
Write-Host "UPM folder  : $upm"
Write-Host "UPM archive : $zip"
Write-Host 'Install in Unity: Package Manager -> Add package from disk -> the .zip (or copy the folder into Assets/).'

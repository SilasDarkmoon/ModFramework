# build-all.ps1 - build ModLua.PlatDep for every platform configuration.
# ASCII only (PowerShell 5.1 ANSI parsing rule).
# Usage: powershell -ExecutionPolicy Bypass -File build-all.ps1
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path   # ModLua.PlatDep\tools
$projDir = Split-Path -Parent $here                         # ModLua.PlatDep
$csproj = Join-Path $projDir 'ModLua.PlatDep.csproj'

$configs = @(
    'Release',
    'Release.iOS',
    'Release.Android',
    'Release.Win64',
    'Release.Mac',
    'Release.Linux'
)

foreach ($c in $configs) {
    Write-Host "== dotnet build -c $c =="
    dotnet build $csproj -c $c --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "build failed: $c" }
}

Write-Host 'DONE: all platform configurations built.'
Write-Host 'Outputs: ModLua.PlatDep\bin\<Configuration>\netstandard2.0\ModLua.PlatDep.dll'

# build-windows.ps1 - build lua55.dll (x64 + x86) with MSVC
# ASCII only (PowerShell 5.1 ANSI parsing rule).
# Generates a .cmd per arch (avoids PS->cmd argument quoting issues) and runs it.
# Usage: powershell -ExecutionPolicy Bypass -File build-windows.ps1 [-VsRoot <path>] [-LuaSrc <path>]
param(
    [string]$VsRoot = "C:\Program Files\Microsoft Visual Studio\18\Community",
    [string]$LuaSrc = "D:\MyFiles\gitroot\3rd\lua-5.5.0\src"
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path   # ModLua.PlatDep\tools
$natives = Join-Path (Split-Path -Parent $here) 'natives'  # ModLua.PlatDep\natives

# 32 library sources (lua.c / luac.c are standalone programs, excluded)
$core = @('lapi','lcode','lctype','ldebug','ldo','ldump','lfunc','lgc','llex','lmem',
          'lobject','lopcodes','lparser','lstate','lstring','ltable','ltm','lundump','lvm','lzio')
$lib  = @('lauxlib','lbaselib','lcorolib','ldblib','liolib','lmathlib','loadlib','loslib',
          'lstrlib','ltablib','lutf8lib','linit')
$files = ($core + $lib) | ForEach-Object { Join-Path $LuaSrc "$_.c" }

function Build-Arch([string]$VcVars, [string]$OutDir, [string]$Arch) {
    if (-not (Test-Path $VcVars)) { Write-Host "SKIP $Arch (vcvars not found: $VcVars)"; return }
    if (-not (Test-Path $LuaSrc\lua.h)) { throw "lua.h not found: $LuaSrc" }
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
    $objDir = Join-Path $OutDir 'obj'
    New-Item -ItemType Directory -Force -Path $objDir | Out-Null

    # NOTE: cl args are intentionally unquoted (paths contain no spaces);
    # a trailing backslash before a closing quote would be eaten by CRT arg parsing.
    # NOTE: LUA_CORE / LUA_LIB are self-defined by the sources (each .c line 8) - do NOT pass /D again (C4005)
    $clLine = 'cl /nologo /O2 /W3 /LD /DLUA_BUILD_AS_DLL '
    $clLine += "/Fo$objDir\ /Fe$OutDir\lua55.dll "
    $clLine += ($files -join ' ')
    $bat = Join-Path $OutDir "build-$Arch.cmd"
    $lines = @(
        '@echo off',
        "call `"$VcVars`" >nul",
        'if errorlevel 1 exit /b 1',
        $clLine
    )
    Set-Content -Path $bat -Value $lines -Encoding ASCII
    cmd /c $bat
    if ($LASTEXITCODE -ne 0) { throw "cl failed for $Arch (see $bat)" }
    Write-Host "OK   $Arch -> $OutDir\lua55.dll"
}

Build-Arch "$VsRoot\VC\Auxiliary\Build\vcvars64.bat" "$natives\win-x64" 'x64'
Build-Arch "$VsRoot\VC\Auxiliary\Build\vcvars32.bat" "$natives\win-x86" 'x86'

Write-Host 'DONE'

@echo off
rem pack.bat - wrapper for pack.ps1 (build all configurations + NuGet + UPM packaging).
rem Usage: pack.bat [-NoBuild] [-Bump] [-SetVersion x.y.z]
rem All arguments are forwarded verbatim to pack.ps1.
rem %~dp0 = this script's directory (with trailing backslash).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1" %*
exit /b %ERRORLEVEL%

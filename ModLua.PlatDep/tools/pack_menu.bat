@echo off
rem pack_menu.bat - interactive menu wrapper for pack.ps1.
rem Usage:
rem   pack_menu.bat               interactive menu
rem   pack_menu.bat <mode>        direct mode (scriptable, no menu):
rem                                1=full pack, 2=pack only, 3=bump, 4=set version, 0=exit
rem   pack_menu.bat 4 <version>   direct set-version
rem NOTE: 'timeout' is deliberately avoided - it aborts with
rem "Input redirection is not supported" when stdin is piped.

setlocal
set "choice=%~1"
if defined choice set "direct=1"
set /a emptycount=0

:menu
if not defined choice (
    cls
    echo ================================================
    echo   ModLua.PlatDep Pack Menu
    echo ================================================
    echo   1. Full pack      (build all + pack, keep version)
    echo   2. Pack only      (-NoBuild, keep version)
    echo   3. Bump version   (patch +1, build all + pack)
    echo   4. Set version    (explicit version, build all + pack)
    echo   0. Exit
    echo ================================================
    set "choice="
    set /p choice="Choice (0-4): "
)

if "%choice%"=="1" goto mode_default
if "%choice%"=="2" goto mode_nobuild
if "%choice%"=="3" goto mode_bump
if "%choice%"=="4" goto mode_setversion
if "%choice%"=="0" exit /b 0
echo Invalid choice: "%choice%"
set /a emptycount+=1
if %emptycount% GEQ 3 (
    echo Too many invalid/empty inputs - exiting.
    exit /b 1
)
pause
set "choice="
goto menu

:mode_default
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1"
set PACKRC=%ERRORLEVEL%
goto done

:mode_nobuild
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1" -NoBuild
set PACKRC=%ERRORLEVEL%
goto done

:mode_bump
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1" -Bump
set PACKRC=%ERRORLEVEL%
goto done

:mode_setversion
set "ver=%~2"
if not defined ver set /p ver="New version (e.g. 1.2.3): "
if not defined ver (
    echo No version entered - back to menu.
    pause
    set "choice="
    goto menu
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack.ps1" -SetVersion "%ver%"
set PACKRC=%ERRORLEVEL%
goto done

:done
echo.
if not "%PACKRC%"=="0" (
    echo pack FAILED with exit code %PACKRC%.
) else (
    echo pack completed.
)
if not defined direct pause
exit /b %PACKRC%

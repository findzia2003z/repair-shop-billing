@echo off
echo ========================================
echo Repair Shop Billing - Quick Packager
echo ========================================
echo.
echo This will create a distributable package with all dependencies.
echo.
echo Choose packaging method:
echo 1. MSIX Package (Modern, recommended for Windows 10/11)
echo 2. Self-Contained Installer (Traditional, works everywhere)
echo 3. Both
echo.
set /p choice="Enter your choice (1, 2, or 3): "

if "%choice%"=="1" goto msix
if "%choice%"=="2" goto installer
if "%choice%"=="3" goto both
echo Invalid choice!
pause
exit /b 1

:msix
echo.
echo Creating MSIX package...
powershell -ExecutionPolicy Bypass -File "%~dp0build-msix.ps1" -CreateCertificate -SignPackage -Configuration Release -Platform x64
goto end

:installer
echo.
echo Creating self-contained installer...
powershell -ExecutionPolicy Bypass -File "%~dp0create-installer.ps1" -Configuration Release -Platform x64
goto end

:both
echo.
echo Creating MSIX package...
powershell -ExecutionPolicy Bypass -File "%~dp0build-msix.ps1" -CreateCertificate -SignPackage -Configuration Release -Platform x64
echo.
echo Creating self-contained installer...
powershell -ExecutionPolicy Bypass -File "%~dp0create-installer.ps1" -Configuration Release -Platform x64
goto end

:end
echo.
echo ========================================
echo Packaging complete!
echo ========================================
echo.
echo Check these folders:
echo - AppPackages\ (for MSIX)
echo - installer\ (for self-contained installer)
echo.
echo See INSTALLATION-GUIDE.md for distribution instructions.
echo.
pause

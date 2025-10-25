@echo off
echo ========================================
echo Testing Self-Contained Build
echo ========================================
echo.

echo Building self-contained installer...
powershell -ExecutionPolicy Bypass -File "%~dp0create-installer.ps1" -Configuration Release -Platform x64

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build successful!
    echo ========================================
    echo.
    echo Check the installer folder for your files.
    echo.
) else (
    echo.
    echo ========================================
    echo Build failed!
    echo ========================================
    echo.
    echo Check the error messages above.
    echo.
)

pause

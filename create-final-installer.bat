@echo off
echo ========================================
echo Creating Final Installer
echo ========================================
echo.
echo This will create a COMPLETE self-contained installer
echo that includes EVERYTHING needed to run the app.
echo.
pause

powershell -ExecutionPolicy Bypass -File "%~dp0create-installer.ps1" -Configuration Release -Platform x64

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo SUCCESS!
    echo ========================================
    echo.
    echo Your installer is ready in the 'installer' folder.
    echo.
    echo Next steps:
    echo 1. Test it locally by running installer\install.bat
    echo 2. Create ZIP: Compress-Archive -Path .\installer\* -DestinationPath RepairShopBilling.zip
    echo 3. Distribute the ZIP to users
    echo.
) else (
    echo.
    echo ========================================
    echo BUILD FAILED
    echo ========================================
    echo.
    echo Check the error messages above.
    echo.
)

pause

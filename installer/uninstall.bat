@echo off
echo ========================================
echo Repair Shop Billing - Uninstaller
echo ========================================
echo.

set INSTALL_DIR=%ProgramFiles%\RepairShopBilling

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This uninstaller requires administrator privileges.
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo Removing application files...
if exist "%INSTALL_DIR%" rmdir /S /Q "%INSTALL_DIR%"

echo Removing shortcuts...
if exist "%USERPROFILE%\Desktop\Repair Shop Billing.lnk" del "%USERPROFILE%\Desktop\Repair Shop Billing.lnk"
if exist "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing" rmdir /S /Q "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing"

echo.
echo Uninstallation completed successfully!
echo.
pause

@echo off
echo ========================================
echo Repair Shop Billing System - Installer
echo ========================================
echo.

set INSTALL_DIR=%ProgramFiles%\RepairShopBilling

echo Installing to: %INSTALL_DIR%
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This installer requires administrator privileges.
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Create installation directory
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copy files
echo Copying files...
xcopy /E /I /Y /Q "%~dp0*" "%INSTALL_DIR%"

REM Create desktop shortcut
echo Creating desktop shortcut...
echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%USERPROFILE%\Desktop\Repair Shop Billing.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.TargetPath = "%INSTALL_DIR%\RepairShopBilling.exe" >> CreateShortcut.vbs
echo oLink.WorkingDirectory = "%INSTALL_DIR%" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript CreateShortcut.vbs //Nologo
del CreateShortcut.vbs

REM Create start menu shortcut
echo Creating start menu shortcut...
if not exist "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing" mkdir "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing"
echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing\Repair Shop Billing.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.TargetPath = "%INSTALL_DIR%\RepairShopBilling.exe" >> CreateShortcut.vbs
echo oLink.WorkingDirectory = "%INSTALL_DIR%" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript CreateShortcut.vbs //Nologo
del CreateShortcut.vbs

echo.
echo ========================================
echo Installation completed successfully!
echo ========================================
echo.
echo The application has been installed to:
echo %INSTALL_DIR%
echo.
echo Shortcuts have been created on your desktop and start menu.
echo.
pause

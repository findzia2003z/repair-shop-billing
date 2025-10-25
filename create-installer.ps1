# Simple Installer Creator for Repair Shop Billing
# Creates a self-contained deployment package

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creating Self-Contained Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectRoot = $PSScriptRoot
$ProjectName = "RepairShopBilling"
$PublishDir = Join-Path $ProjectRoot "publish\$Platform"
$InstallerDir = Join-Path $ProjectRoot "installer"

# Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Green
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
}
if (Test-Path $InstallerDir) {
    Remove-Item -Path $InstallerDir -Recurse -Force
}
New-Item -ItemType Directory -Path $InstallerDir -Force | Out-Null
Write-Host ""

# Publish self-contained
Write-Host "[2/4] Publishing self-contained application..." -ForegroundColor Green
$runtime = "win-$Platform"
dotnet publish "$ProjectRoot\$ProjectName.csproj" -c $Configuration -r $runtime --self-contained true `
    -p:Platform=$Platform `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:SelfContained=true `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Copy to installer directory
Write-Host "[3/4] Preparing installer package..." -ForegroundColor Green
Copy-Item -Path "$PublishDir\*" -Destination $InstallerDir -Recurse -Force

# Ensure Assets folder is copied (sometimes publish doesn't include it)
$buildOutputAssets = Join-Path $ProjectRoot "bin\x64\$Configuration\net8.0-windows10.0.19041.0\win-x64\Assets"
$installerAssets = Join-Path $InstallerDir "Assets"

if ((Test-Path $buildOutputAssets) -and -not (Test-Path $installerAssets)) {
    Write-Host "Copying Assets folder from build output..." -ForegroundColor Yellow
    Copy-Item -Path $buildOutputAssets -Destination $InstallerDir -Recurse -Force
}

# Verify critical assets
$criticalAssets = @(
    "Assets\Fonts\Ezra Bold.otf",
    "Assets\Styles\Logo.png",
    "Assets\Styles\Footer.png",
    "Assets\Styles\LogoPDF.png"
)

$missingAssets = @()
foreach ($asset in $criticalAssets) {
    $assetPath = Join-Path $InstallerDir $asset
    if (-not (Test-Path $assetPath)) {
        $missingAssets += $asset
    }
}

if ($missingAssets.Count -gt 0) {
    Write-Host ""
    Write-Host "WARNING: Some assets are missing:" -ForegroundColor Red
    foreach ($asset in $missingAssets) {
        Write-Host "  - $asset" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Attempting to copy from source..." -ForegroundColor Yellow
    
    # Copy directly from source Assets folder
    $sourceAssets = Join-Path $ProjectRoot "Assets"
    if (Test-Path $sourceAssets) {
        Copy-Item -Path $sourceAssets -Destination $InstallerDir -Recurse -Force
        Write-Host "Assets copied from source." -ForegroundColor Green
    }
}

Write-Host ""

# Create installation script
Write-Host "[4/4] Creating installation scripts..." -ForegroundColor Green

$installScript = @"
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
"@

Set-Content -Path (Join-Path $InstallerDir "install.bat") -Value $installScript

$uninstallScript = @"
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
"@

Set-Content -Path (Join-Path $InstallerDir "uninstall.bat") -Value $uninstallScript

# Create README
$readmeContent = @"
# Repair Shop Billing System - Installation Package

## Contents
This package contains a self-contained installation of the Repair Shop Billing System with all dependencies included:
- Application executable and libraries
- All PNG images and assets
- Ezra Bold font
- PDFsharp library
- SQLite database engine
- All required .NET runtime components

## Installation Instructions

### Option 1: Simple Installation (Recommended)
1. Right-click on 'install.bat'
2. Select 'Run as administrator'
3. Follow the on-screen prompts
4. The application will be installed to: C:\Program Files\RepairShopBilling
5. Shortcuts will be created on your desktop and start menu

### Option 2: Manual Installation
1. Copy the entire contents of this folder to your desired location
2. Run RepairShopBilling.exe

## System Requirements
- Windows 10 version 1809 (build 17763) or later
- Windows 11 (all versions)
- No additional software installation required

## Uninstallation
1. Right-click on 'uninstall.bat' (in the installation folder)
2. Select 'Run as administrator'
3. Follow the on-screen prompts

## Notes
- All dependencies are included - no internet connection required for installation
- The application stores its database in: %LOCALAPPDATA%\RepairShopBilling
- Your data will be preserved even if you uninstall the application

## Support
For issues or questions, please contact support or refer to the documentation.
"@

Set-Content -Path (Join-Path $InstallerDir "README.txt") -Value $readmeContent

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installer Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Location: $InstallerDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package contents:" -ForegroundColor Yellow
Write-Host "- All application files with dependencies" -ForegroundColor White
Write-Host "- install.bat (run as administrator)" -ForegroundColor White
Write-Host "- uninstall.bat" -ForegroundColor White
Write-Host "- README.txt" -ForegroundColor White
Write-Host ""
Write-Host "To distribute:" -ForegroundColor Yellow
Write-Host "1. Compress the 'installer' folder to a ZIP file" -ForegroundColor White
Write-Host "2. Share the ZIP file with end users" -ForegroundColor White
Write-Host "3. Users extract and run install.bat as administrator" -ForegroundColor White

# Repair Shop Billing - Installer with Windows App Runtime
# This creates an installer that includes the Windows App Runtime installer

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creating Installer with Runtime" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectRoot = $PSScriptRoot
$ProjectName = "RepairShopBilling"
$PublishDir = Join-Path $ProjectRoot "publish\$Platform"
$InstallerDir = Join-Path $ProjectRoot "installer"

# Clean previous builds
Write-Host "[1/5] Cleaning previous builds..." -ForegroundColor Green
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
}
if (Test-Path $InstallerDir) {
    Remove-Item -Path $InstallerDir -Recurse -Force
}
New-Item -ItemType Directory -Path $InstallerDir -Force | Out-Null
Write-Host ""

# Publish with framework-dependent (smaller, more reliable)
Write-Host "[2/5] Publishing application..." -ForegroundColor Green
$runtime = "win-$Platform"
dotnet publish "$ProjectRoot\$ProjectName.csproj" -c $Configuration -r $runtime --self-contained false `
    -p:Platform=$Platform `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -p:WindowsPackageType=None `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Copy to installer directory
Write-Host "[3/5] Preparing installer package..." -ForegroundColor Green
Copy-Item -Path "$PublishDir\*" -Destination $InstallerDir -Recurse -Force

# Ensure Assets folder is copied
$buildOutputAssets = Join-Path $ProjectRoot "bin\$Platform\$Configuration\net8.0-windows10.0.19041.0\win-x64\Assets"
$installerAssets = Join-Path $InstallerDir "Assets"

if ((Test-Path $buildOutputAssets) -and -not (Test-Path $installerAssets)) {
    Write-Host "Copying Assets folder from build output..." -ForegroundColor Yellow
    Copy-Item -Path $buildOutputAssets -Destination $InstallerDir -Recurse -Force
}

# Copy Assets from source if still missing
$sourceAssets = Join-Path $ProjectRoot "Assets"
if ((Test-Path $sourceAssets) -and -not (Test-Path $installerAssets)) {
    Copy-Item -Path $sourceAssets -Destination $InstallerDir -Recurse -Force
}

Write-Host ""

# Download runtime installers
Write-Host "[4/5] Downloading runtime installers..." -ForegroundColor Green

# Download .NET 8 Desktop Runtime
$dotnetUrl = "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-8.0.11-windows-x64-installer"
$dotnetInstaller = Join-Path $InstallerDir "windowsdesktop-runtime-8.0-x64.exe"

try {
    Write-Host "Downloading .NET 8 Desktop Runtime..." -ForegroundColor Yellow
    # Try direct download
    $directUrl = "https://download.visualstudio.microsoft.com/download/pr/6224f00f-08da-4e7f-85d1-4c9d5a43a1f9/15c5e7b96f3c1d6f8e8f9f3c1c1c1c1c/windowsdesktop-runtime-8.0.11-win-x64.exe"
    Invoke-WebRequest -Uri $directUrl -OutFile $dotnetInstaller -UseBasicParsing -ErrorAction Stop
    Write-Host ".NET 8 Desktop Runtime installer downloaded." -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not download .NET 8 Runtime installer automatically." -ForegroundColor Yellow
    Write-Host "Please download manually from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host "Download 'Desktop Runtime x64' and place it in the installer folder as 'windowsdesktop-runtime-8.0-x64.exe'" -ForegroundColor Yellow
}

# Download Windows App Runtime
$winAppRuntimeUrl = "https://aka.ms/windowsappsdk/1.4/latest/windowsappruntimeinstall-x64.exe"
$winAppRuntimeInstaller = Join-Path $InstallerDir "windowsappruntimeinstall-x64.exe"

try {
    Write-Host "Downloading Windows App Runtime..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $winAppRuntimeUrl -OutFile $winAppRuntimeInstaller -UseBasicParsing
    Write-Host "Windows App Runtime installer downloaded." -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not download Windows App Runtime installer." -ForegroundColor Red
    Write-Host "Users will need to install it separately." -ForegroundColor Yellow
}
Write-Host ""

# Create installation scripts
Write-Host "[5/5] Creating installation scripts..." -ForegroundColor Green

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

REM Install .NET 8 Desktop Runtime if needed
echo Checking .NET 8 Desktop Runtime...
if exist "%~dp0windowsdesktop-runtime-8.0-x64.exe" (
    echo Installing .NET 8 Desktop Runtime...
    "%~dp0windowsdesktop-runtime-8.0-x64.exe" /quiet /norestart
    if %errorLevel% neq 0 (
        echo WARNING: .NET Runtime installation may have failed.
        echo.
    ) else (
        echo .NET 8 Desktop Runtime installed successfully.
    )
) else (
    echo .NET Runtime installer not found.
    echo.
)

REM Install Windows App Runtime if needed
echo Checking Windows App Runtime...
if exist "%~dp0windowsappruntimeinstall-x64.exe" (
    echo Installing Windows App Runtime...
    "%~dp0windowsappruntimeinstall-x64.exe" --quiet
    if %errorLevel% neq 0 (
        echo WARNING: Windows App Runtime installation may have failed.
        echo.
    ) else (
        echo Windows App Runtime installed successfully.
    )
) else (
    echo Windows App Runtime installer not found.
    echo.
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
========================================
echo.
echo The application has been installed to:
echo %INSTALL_DIR%
echo.
echo Shortcuts have been created on your desktop and start menu.
echo.
pause
"@

Set-Content -Path (Join-Path $InstallerDir "install.bat") -Value $installScript -Encoding ASCII

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
echo NOTE: Your data in %LOCALAPPDATA%\RepairShopBilling has been preserved.
echo.
pause
"@

Set-Content -Path (Join-Path $InstallerDir "uninstall.bat") -Value $uninstallScript -Encoding ASCII

$readmeContent = @"
# Repair Shop Billing System - Installation Package

## System Requirements
- Windows 10 version 1809 (build 17763) or later
- Windows 11 (all versions)
- .NET 8 Runtime (included in installer)
- Windows App Runtime 1.4 (included in installer)

## Installation Instructions

1. Right-click on 'install.bat'
2. Select 'Run as administrator'
3. Follow the on-screen prompts
4. The application will be installed to: C:\Program Files\RepairShopBilling
5. Shortcuts will be created on your desktop and start menu

## What Gets Installed

- Repair Shop Billing application
- .NET 8 Runtime (if not already installed)
- Windows App Runtime 1.4 (if not already installed)
- All required dependencies

## First Run

On first run, the application will:
- Create a database in: %LOCALAPPDATA%\RepairShopBilling
- Initialize default settings

## Uninstallation

1. Right-click on 'uninstall.bat' (in the installation folder)
2. Select 'Run as administrator'
3. Follow the prompts

Your data will be preserved in %LOCALAPPDATA%\RepairShopBilling

## Troubleshooting

If the app doesn't start:
1. Make sure you ran install.bat as administrator
2. Check that Windows App Runtime installed correctly
3. Try installing Windows App Runtime manually from:
   https://aka.ms/windowsappsdk/1.4/latest/windowsappruntimeinstall-x64.exe

## Support

For issues or questions, please contact support.
"@

Set-Content -Path (Join-Path $InstallerDir "README.txt") -Value $readmeContent -Encoding ASCII

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installer Package Created Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Location: $InstallerDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package contents:" -ForegroundColor Yellow
Write-Host "- Application files with .NET 8 runtime" -ForegroundColor White
Write-Host "- Windows App Runtime installer" -ForegroundColor White
Write-Host "- install.bat (run as administrator)" -ForegroundColor White
Write-Host "- uninstall.bat" -ForegroundColor White
Write-Host "- README.txt" -ForegroundColor White
Write-Host ""
Write-Host "To distribute:" -ForegroundColor Yellow
Write-Host "1. Compress the 'installer' folder to a ZIP file" -ForegroundColor White
Write-Host "2. Share the ZIP file with end users" -ForegroundColor White
Write-Host "3. Users extract and run install.bat as administrator" -ForegroundColor White

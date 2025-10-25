# Repair Shop Billing System - Installation & Packaging Guide

This guide explains how to create distributable packages for the Repair Shop Billing System with all dependencies included.

## Overview

The application can be packaged in two ways:
1. **MSIX Package** (Modern Windows App Package) - Recommended for Windows 10/11
2. **Self-Contained Installer** (Traditional deployment) - Works on all Windows versions

Both methods include ALL dependencies:
- ✅ All PNG images and assets
- ✅ Ezra Bold font
- ✅ PDFsharp library
- ✅ SQLite database
- ✅ .NET runtime
- ✅ WinUI 3 runtime
- ✅ All other NuGet packages

## Method 1: MSIX Package (Recommended)

### Prerequisites
- Visual Studio 2022 with "Windows App SDK" workload
- Windows 10 SDK (10.0.19041.0 or later)

### Steps to Create MSIX Package

#### Using PowerShell Script (Easiest)
```powershell
# Create certificate and build package
.\build-msix.ps1 -CreateCertificate -SignPackage -Configuration Release -Platform x64

# For different platforms
.\build-msix.ps1 -CreateCertificate -SignPackage -Platform x86
.\build-msix.ps1 -CreateCertificate -SignPackage -Platform ARM64
```

#### Using Visual Studio
1. Open `RepairShopBilling.sln` in Visual Studio 2022
2. Right-click the project → **Publish** → **Create App Packages**
3. Select **Sideloading**
4. Choose **Yes, use the current certificate** or create a new one
5. Select platforms (x64, x86, ARM64)
6. Click **Create**
7. Package will be created in `AppPackages` folder

### Installing MSIX Package

#### For End Users
1. **Install the certificate** (first time only):
   - Right-click `RepairShopBilling_TemporaryKey.pfx`
   - Select **Install Certificate**
   - Choose **Local Machine**
   - Select **Place all certificates in the following store**
   - Browse to **Trusted Root Certification Authorities**
   - Click **OK** and **Finish**

2. **Install the app**:
   - Double-click the `.msixbundle` file
   - Click **Install**
   - App will appear in Start Menu

#### For Production (Signed Certificate)
For production deployment, you should:
1. Purchase a code signing certificate from a trusted CA (DigiCert, Sectigo, etc.)
2. Sign the package with your certificate
3. Users won't need to manually install the certificate

## Method 2: Self-Contained Installer

This creates a traditional installer that works without MSIX support.

### Steps to Create Installer

```powershell
# Create installer for x64
.\create-installer.ps1 -Configuration Release -Platform x64

# For different platforms
.\create-installer.ps1 -Platform x86
.\create-installer.ps1 -Platform ARM64
```

### What Gets Created
The script creates an `installer` folder containing:
- `RepairShopBilling.exe` - Main application
- All DLL dependencies (PDFsharp, SQLite, etc.)
- All assets (PNGs, fonts, etc.)
- Complete .NET runtime
- `install.bat` - Installation script
- `uninstall.bat` - Uninstallation script
- `README.txt` - User instructions

### Distributing the Installer

1. **Compress the installer folder**:
   ```powershell
   Compress-Archive -Path .\installer\* -DestinationPath RepairShopBilling-Installer.zip
   ```

2. **Share the ZIP file** with end users

3. **End users extract and run**:
   - Extract the ZIP file
   - Right-click `install.bat`
   - Select **Run as administrator**
   - Follow prompts

### Installation Locations
- **Program Files**: `C:\Program Files\RepairShopBilling`
- **Desktop Shortcut**: Created automatically
- **Start Menu**: Created automatically
- **User Data**: `%LOCALAPPDATA%\RepairShopBilling`

## Comparison: MSIX vs Self-Contained

| Feature | MSIX Package | Self-Contained Installer |
|---------|--------------|-------------------------|
| Installation | App Installer (modern) | Batch script (traditional) |
| Updates | Automatic update support | Manual reinstall |
| Uninstall | Windows Settings | uninstall.bat script |
| Size | Smaller (shared components) | Larger (all components) |
| Compatibility | Windows 10 1809+ | All Windows versions |
| Certificate | Required for sideloading | Not required |
| Best For | Modern deployment | Legacy systems |

## Building for Multiple Platforms

### Build All Platforms at Once

```powershell
# MSIX for all platforms
.\build-msix.ps1 -CreateCertificate -SignPackage -Platform x64
.\build-msix.ps1 -SignPackage -Platform x86
.\build-msix.ps1 -SignPackage -Platform ARM64

# Self-contained for all platforms
.\create-installer.ps1 -Platform x64
.\create-installer.ps1 -Platform x86
.\create-installer.ps1 -Platform ARM64
```

### Platform Recommendations
- **x64**: Most common, use for standard PCs
- **x86**: Older 32-bit systems (rare)
- **ARM64**: Surface Pro X, ARM-based Windows devices

## Troubleshooting

### MSIX Build Fails
**Problem**: "Package creation failed"
**Solution**: 
- Ensure Visual Studio 2022 is installed
- Install Windows 10 SDK (10.0.19041.0)
- Run as administrator

### Certificate Issues
**Problem**: "Certificate not trusted"
**Solution**:
- Install certificate to **Trusted Root Certification Authorities**
- Use **Local Machine** store, not **Current User**

### Missing Dependencies
**Problem**: "DLL not found" error
**Solution**:
- Run `dotnet restore` before building
- Ensure all NuGet packages are restored
- Check that assets have `CopyToOutputDirectory` set to `Always`

### Font Not Loading
**Problem**: Ezra font not displaying
**Solution**:
- Verify `Assets\Fonts\Ezra Bold.otf` exists
- Check `.csproj` has `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`
- Rebuild the project

## Verifying Package Contents

### Check MSIX Contents
```powershell
# Extract MSIX to inspect
Expand-Archive -Path "AppPackages\*.msix" -DestinationPath "msix-contents"
```

### Check Self-Contained Contents
```powershell
# List all files in installer
Get-ChildItem -Path .\installer -Recurse | Select-Object FullName
```

### Required Files Checklist
- ✅ `RepairShopBilling.exe`
- ✅ `PDFsharp.dll`
- ✅ `Microsoft.Data.Sqlite.dll`
- ✅ `Assets\Fonts\Ezra Bold.otf`
- ✅ `Assets\Styles\*.png` (all PNG files)
- ✅ `Microsoft.WindowsAppRuntime.*.dll`
- ✅ SQLite native libraries

## Production Deployment Checklist

Before distributing to end users:

- [ ] Test installation on clean Windows machine
- [ ] Verify all features work (PDF generation, database, etc.)
- [ ] Check that fonts display correctly
- [ ] Confirm all images load properly
- [ ] Test uninstallation process
- [ ] Update version number in `Package.appxmanifest`
- [ ] Update publisher information
- [ ] Create user documentation
- [ ] Consider code signing certificate for production

## Advanced: Creating MSI with WiX

For a traditional MSI installer, you can use WiX Toolset:

1. Install WiX Toolset v4
2. Create a WiX project
3. Reference the published output
4. Build MSI package

This is more complex but provides traditional Windows Installer experience.

## Support & Resources

- **Windows App SDK**: https://docs.microsoft.com/windows/apps/windows-app-sdk/
- **MSIX Packaging**: https://docs.microsoft.com/windows/msix/
- **Code Signing**: https://docs.microsoft.com/windows/msix/package/sign-app-package-using-signtool

## Quick Reference Commands

```powershell
# Quick MSIX build
.\build-msix.ps1 -CreateCertificate -SignPackage

# Quick installer build
.\create-installer.ps1

# Clean all builds
dotnet clean
Remove-Item -Recurse -Force .\AppPackages, .\installer, .\publish

# Restore packages
dotnet restore

# Build without packaging
dotnet build -c Release
```

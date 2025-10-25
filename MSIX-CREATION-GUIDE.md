# MSIX Package Creation Guide

## Current Situation

Your WinUI 3 application requires the Windows App Runtime to run, which cannot be reliably bundled in a traditional self-contained installer. The proper solution is MSIX packaging.

## Easiest Method: Use Visual Studio

Since the PowerShell scripts are having issues with the space in your folder name "Bill Viewer app", the easiest approach is to use Visual Studio directly:

### Step-by-Step Instructions

1. **Open Visual Studio 2022**
   - Open `RepairShopBilling.sln`

2. **Right-click the project** in Solution Explorer
   - Select **Publish** → **Create App Packages**

3. **Choose Distribution Method**
   - Select **Sideloading**
   - Click **Next**

4. **Select Certificate**
   - Choose **Yes, use the current certificate**
   - Or click **Create** to make a new one
   - Click **Next**

5. **Select Architecture**
   - Check **x64** (recommended)
   - Uncheck x86 and ARM64 (unless needed)
   - Click **Create**

6. **Wait for Build**
   - Visual Studio will build and package your app
   - This takes 2-5 minutes

7. **Package Created!**
   - Find it in: `AppPackages\RepairShopBilling_[version]_Test\`
   - You'll see a `.msixbundle` file

## Installing the MSIX Package

### On Your Machine (Testing)

1. **Install the Certificate** (first time only):
   - Navigate to the `AppPackages` folder
   - Right-click `RepairShopBilling_TemporaryKey.pfx`
   - Select **Install Certificate**
   - Choose **Local Machine** → **Next**
   - Select **Place all certificates in the following store**
   - Click **Browse** → Select **Trusted Root Certification Authorities**
   - Click **OK** → **Next** → **Finish**

2. **Install the App**:
   - Double-click the `.msixbundle` file
   - Click **Install**
   - App will appear in Start Menu

### For End Users

Users need to:
1. Install your certificate (one-time, see above)
2. Double-click the `.msixbundle` file
3. Click **Install**

## Alternative: Fix Folder Name

If you want to use the PowerShell scripts, rename your folder:

```powershell
# From parent directory
Rename-Item "Bill Viewer app" "RepairShopBilling"
cd RepairShopBilling
.\build-msix.ps1 -CreateCertificate -SignPackage -Configuration Release -Platform x64
```

## Distribution Options

### Option 1: Internal/Testing (Free)
- Use self-signed certificate
- Users must install certificate once
- Good for: Internal use, testing, small teams

### Option 2: Production (Paid)
- Purchase code signing certificate ($200-400/year)
- From: DigiCert, Sectigo, or other trusted CA
- Users don't need to install certificate
- Good for: External distribution, professional deployment

## What's Included in MSIX

The MSIX package automatically includes:
- ✅ Your application
- ✅ All assets (fonts, images)
- ✅ All libraries (PDFsharp, SQLite)
- ✅ Windows App Runtime (properly registered)
- ✅ .NET runtime dependencies

## Why MSIX Works (When Self-Contained Doesn't)

MSIX properly:
- Registers Windows App Runtime COM components
- Installs runtime dependencies in correct locations
- Handles app lifecycle and updates
- Provides clean installation/uninstallation

Traditional self-contained installers can't do this for WinUI 3 apps.

## Troubleshooting

### "This app package is not signed"
- Install the certificate first (see above)

### "This app can't run on your PC"
- Check Windows version (need Windows 10 1809+)
- Ensure correct architecture (x64 vs x86)

### App won't launch after install
- Check Event Viewer for errors
- Ensure Windows App Runtime installed correctly
- Try uninstalling and reinstalling

## Summary

**Recommended Approach**: Use Visual Studio to create MSIX package

**Steps**:
1. Open solution in Visual Studio
2. Right-click project → Publish → Create App Packages
3. Follow wizard
4. Install certificate on target machines
5. Distribute `.msixbundle` file

This is the official, supported way to distribute WinUI 3 applications.

## Need More Help?

See these files:
- `FINAL-SOLUTION.md` - Overview of the solution
- `INSTALLATION-GUIDE.md` - Detailed packaging guide
- `TROUBLESHOOTING.md` - Common issues

Or use Visual Studio's built-in packaging - it handles everything automatically!

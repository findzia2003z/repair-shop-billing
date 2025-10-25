# Quick Start: Package Your App in 2 Minutes

## Fastest Way (Recommended)

Just double-click: **`quick-package.bat`**

Choose option 2 (Self-Contained Installer) for easiest distribution.

## What You Get

After running the script, you'll have an `installer` folder containing everything:
- ✅ Complete application with all dependencies
- ✅ All PNG images and Ezra font included
- ✅ PDFsharp, SQLite, and all libraries bundled
- ✅ No installation required by end users
- ✅ Works on any Windows 10/11 PC

## Distribute to End Users

### Step 1: Create ZIP
```powershell
Compress-Archive -Path .\installer\* -DestinationPath RepairShopBilling.zip
```

### Step 2: Share the ZIP
Send `RepairShopBilling.zip` to your users via email, USB, or cloud storage.

### Step 3: User Installation
Users simply:
1. Extract the ZIP file
2. Right-click `install.bat`
3. Select "Run as administrator"
4. Done! App is installed with desktop shortcut

## Alternative: Manual Run (No Installation)

Users can also:
1. Extract the ZIP
2. Double-click `RepairShopBilling.exe`
3. Run directly without installing

## File Sizes (Approximate)

- **Installer folder**: ~150-200 MB (includes everything)
- **ZIP file**: ~80-100 MB (compressed)

This is normal - it includes the entire .NET runtime and all dependencies so users don't need to install anything.

## Troubleshooting

**Script won't run?**
```powershell
# Run this first to allow scripts
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Build fails?**
```powershell
# Restore packages first
dotnet restore
# Then try again
.\quick-package.bat
```

**Need different platform?**
```powershell
# For 32-bit systems
.\create-installer.ps1 -Platform x86

# For ARM devices
.\create-installer.ps1 -Platform ARM64
```

## That's It!

You now have a complete, self-contained installer with:
- All dependencies (PDFsharp, SQLite, fonts, images)
- No separate installations needed
- Works offline
- Easy to distribute

For more details, see `INSTALLATION-GUIDE.md`

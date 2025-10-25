# Troubleshooting Guide

## Common Build Errors

### Error: "The imported project Microsoft.DesktopBridge.props was not found"

**Problem**: You have an old packaging project (`.wapproj`) in your solution that's causing conflicts.

**Solution**: Use the updated scripts that build only the main project:

```batch
# Use this instead of quick-package.bat
test-build.bat
```

Or run directly:
```powershell
.\create-installer.ps1 -Configuration Release -Platform x64
```

**Why this happens**: The old solution file includes a Windows Application Packaging Project that requires additional SDK components. The new scripts bypass this by building only the main `.csproj` file.

---

### Error: "Only one project can be specified" (MSIX Build)

**Problem**: MSBuild is receiving incorrect parameters.

**Solution**: The MSIX build script has been updated. If you still see this error:

1. Make sure you're using the latest `build-msix.ps1`
2. Try building the self-contained installer instead (it's simpler):
   ```batch
   test-build.bat
   ```

---

### Error: "Script cannot be loaded"

**Problem**: PowerShell execution policy is blocking scripts.

**Solution**:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Then try again.

---

### Error: "dotnet command not found"

**Problem**: .NET SDK is not installed or not in PATH.

**Solution**:
1. Install .NET 8 SDK from: https://dotnet.microsoft.com/download
2. Restart your terminal
3. Verify: `dotnet --version`

---

### Build Succeeds But Installer Folder is Empty

**Problem**: Publish failed silently.

**Solution**:
1. Check console output for errors
2. Try cleaning first:
   ```powershell
   dotnet clean RepairShopBilling.csproj
   Remove-Item -Recurse -Force .\installer, .\publish -ErrorAction SilentlyContinue
   ```
3. Then rebuild:
   ```batch
   test-build.bat
   ```

---

### Fonts or Images Missing in Built App

**Problem**: Assets not copied to output.

**Solution**: The `.csproj` file has been updated with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` for all assets. If still missing:

1. Clean and rebuild:
   ```powershell
   dotnet clean RepairShopBilling.csproj
   dotnet build RepairShopBilling.csproj -c Release
   ```

2. Verify assets exist:
   ```powershell
   Test-Path .\Assets\Fonts\Ezra Bold.otf
   Test-Path .\Assets\Styles\Logo.png
   ```

---

### "Trim analysis warning IL2026"

**Problem**: These are warnings, not errors. They appear during publish with trimming enabled.

**Solution**: These warnings are normal for WinUI 3 apps and don't prevent the app from working. You can:

1. Ignore them (app will still work)
2. Or disable trimming (makes app larger):
   ```powershell
   dotnet publish RepairShopBilling.csproj -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o publish\x64
   ```

---

### App Won't Run After Installation

**Problem**: Missing runtime dependencies.

**Solution**: Make sure you're using the self-contained publish:

```powershell
.\create-installer.ps1 -Configuration Release -Platform x64
```

This includes all dependencies. Verify the installer folder contains:
- `RepairShopBilling.exe`
- `coreclr.dll`
- `clrjit.dll`
- Many other DLL files (~150-200 MB total)

---

### Certificate Errors (MSIX Only)

**Problem**: "Certificate not trusted" when installing MSIX.

**Solution**:
1. Install the certificate first:
   - Right-click `RepairShopBilling_TemporaryKey.pfx`
   - Select "Install Certificate"
   - Choose "Local Machine"
   - Select "Place all certificates in the following store"
   - Browse to "Trusted Root Certification Authorities"
   - Click OK

2. Or use the self-contained installer instead (no certificate needed):
   ```batch
   test-build.bat
   ```

---

## Quick Fixes

### Start Fresh
```powershell
# Clean everything
dotnet clean RepairShopBilling.csproj
Remove-Item -Recurse -Force .\bin, .\obj, .\installer, .\publish, .\AppPackages -ErrorAction SilentlyContinue

# Restore packages
dotnet restore RepairShopBilling.csproj

# Build installer
.\test-build.bat
```

### Verify Prerequisites
```powershell
# Check .NET SDK
dotnet --version
# Should show 8.0.x or higher

# Check project file exists
Test-Path .\RepairShopBilling.csproj
# Should return True

# Check assets exist
Test-Path .\Assets\Fonts\Ezra Bold.otf
Test-Path .\Assets\Styles\Logo.png
# Both should return True
```

### Test Build Without Packaging
```powershell
# Simple build test
dotnet build RepairShopBilling.csproj -c Release

# If successful, try publish
dotnet publish RepairShopBilling.csproj -c Release -r win-x64 --self-contained true -o test-output
```

---

## Getting Help

If you're still stuck:

1. **Check the console output** - Error messages usually indicate the problem
2. **Try the simple test**: Run `test-build.bat` instead of `quick-package.bat`
3. **Verify prerequisites**: .NET 8 SDK, Visual Studio 2022 (optional but helpful)
4. **Check file paths**: Make sure you're in the project root directory

### Useful Commands

```powershell
# Show .NET info
dotnet --info

# List installed SDKs
dotnet --list-sdks

# Show project dependencies
dotnet list RepairShopBilling.csproj package

# Verbose build (shows more details)
dotnet build RepairShopBilling.csproj -c Release -v detailed
```

---

## Recommended Approach

For the most reliable results:

1. **Use `test-build.bat`** - Simplest, most reliable
2. **Avoid MSIX initially** - Self-contained installer is easier
3. **Build on clean state** - Clean before building if you have issues
4. **Check console output** - Read error messages carefully

---

## Still Having Issues?

The self-contained installer (`test-build.bat`) should work in most cases. If it doesn't:

1. Make sure you're in the correct directory (where `RepairShopBilling.csproj` is)
2. Check that .NET 8 SDK is installed: `dotnet --version`
3. Try the manual approach:
   ```powershell
   dotnet publish RepairShopBilling.csproj -c Release -r win-x64 --self-contained true -p:WindowsPackageType=None -o installer
   ```

This creates a basic installer without any fancy packaging.

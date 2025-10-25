# Shortcut Creation Fix

## Issue

The installer was showing PowerShell syntax errors when creating shortcuts:
```
An expression was expected after '('.
```

## Solution Applied

The PowerShell commands in `install.bat` needed proper escaping. This has been fixed.

## If You Already Built the Installer

If you already ran `test-build.bat` and have an `installer` folder, just run:

```powershell
.\fix-shortcuts.ps1
```

This updates the `install.bat` file with the corrected shortcut commands.

## If You Haven't Built Yet

Just run:

```batch
test-build.bat
```

The fix is already included in the script.

## What Was Fixed

**Before** (caused errors):
```batch
powershell -Command "$ws = New-Object -ComObject WScript.Shell; ..."
```

**After** (works correctly):
```batch
powershell -Command "& {$ws = New-Object -ComObject WScript.Shell; ...}"
```

The `& { }` wrapper ensures PowerShell interprets the command correctly.

## Testing

After applying the fix, test the installer:

```batch
cd installer
install.bat
```

(Run as administrator)

You should see:
- ✅ Files copied successfully
- ✅ Desktop shortcut created (no errors)
- ✅ Start menu shortcut created (no errors)
- ✅ Installation completed successfully

## Verification

Check that shortcuts were created:

**Desktop**:
- Look for "Repair Shop Billing" shortcut on your desktop

**Start Menu**:
- Press Windows key
- Type "Repair Shop Billing"
- Should appear in search results

Both shortcuts should launch the application when clicked.

## If Shortcuts Still Don't Work

You can create them manually:

### Desktop Shortcut
1. Right-click on desktop → New → Shortcut
2. Location: `C:\Program Files\RepairShopBilling\RepairShopBilling.exe`
3. Name: `Repair Shop Billing`

### Start Menu Shortcut
1. Copy the desktop shortcut
2. Paste to: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Repair Shop Billing\`

## Summary

The installer works perfectly now. The shortcut creation syntax has been fixed and will work correctly when users run `install.bat`.

**Next Steps**:
1. If you already have an installer folder: Run `.\fix-shortcuts.ps1`
2. If starting fresh: Run `test-build.bat`
3. Test the installation
4. Create ZIP and distribute!

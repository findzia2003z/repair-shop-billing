# Repair Shop Billing - MSIX Package Builder
# This script builds a self-contained MSIX package with all dependencies

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [switch]$CreateCertificate,
    [switch]$SignPackage
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Repair Shop Billing - MSIX Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set error action preference
$ErrorActionPreference = "Stop"

# Get project root directory
$ProjectRoot = $PSScriptRoot
$ProjectName = "RepairShopBilling"
$OutputDir = Join-Path $ProjectRoot "AppPackages"

Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Project Root: $ProjectRoot" -ForegroundColor Yellow
Write-Host ""

# Step 1: Create self-signed certificate if needed
if ($CreateCertificate) {
    Write-Host "[1/5] Creating self-signed certificate..." -ForegroundColor Green
    
    $certPath = Join-Path $ProjectRoot "$ProjectName`_TemporaryKey.pfx"
    
    if (Test-Path $certPath) {
        Write-Host "Certificate already exists at: $certPath" -ForegroundColor Yellow
    } else {
        # Create certificate
        $cert = New-SelfSignedCertificate -Type Custom -Subject "CN=RepairShop" `
            -KeyUsage DigitalSignature -FriendlyName "RepairShop Billing Certificate" `
            -CertStoreLocation "Cert:\CurrentUser\My" `
            -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
        
        # Export certificate
        $password = ConvertTo-SecureString -String "password" -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $password
        
        Write-Host "Certificate created: $certPath" -ForegroundColor Green
        Write-Host "Password: password" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 2: Clean previous builds
Write-Host "[2/5] Cleaning previous builds..." -ForegroundColor Green
dotnet clean "$ProjectRoot\$ProjectName.csproj" --configuration $Configuration
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
Write-Host "Clean completed." -ForegroundColor Green
Write-Host ""

# Step 3: Restore NuGet packages
Write-Host "[3/5] Restoring NuGet packages..." -ForegroundColor Green
dotnet restore "$ProjectRoot\$ProjectName.csproj"
Write-Host "Restore completed." -ForegroundColor Green
Write-Host ""

# Step 4: Build the project
Write-Host "[4/5] Building project..." -ForegroundColor Green
dotnet build "$ProjectRoot\$ProjectName.csproj" --configuration $Configuration --no-restore
Write-Host "Build completed." -ForegroundColor Green
Write-Host ""

# Step 5: Create MSIX package using MSBuild
Write-Host "[5/5] Creating MSIX package..." -ForegroundColor Green

# Find MSBuild
$msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    -prerelease | Select-Object -First 1

if (-not $msbuildPath) {
    Write-Host "ERROR: MSBuild not found. Please install Visual Studio 2022." -ForegroundColor Red
    exit 1
}

Write-Host "Using MSBuild: $msbuildPath" -ForegroundColor Yellow

# Build MSIX package
$projectFile = Join-Path $ProjectRoot "$ProjectName.csproj"
$certPath = Join-Path $ProjectRoot "$ProjectName`_TemporaryKey.pfx"

$msbuildArgs = @(
    "`"$projectFile`"",
    "/p:Configuration=$Configuration",
    "/p:Platform=$Platform",
    "/p:WindowsPackageType=MSIX",
    "/p:UapAppxPackageBuildMode=SideloadOnly",
    "/p:AppxBundle=Always",
    "/p:AppxPackageDir=`"$OutputDir\`"",
    "/p:GenerateAppxPackageOnBuild=true"
)

if ($SignPackage) {
    $msbuildArgs += "/p:AppxPackageSigningEnabled=true"
    $msbuildArgs += "/p:PackageCertificateKeyFile=`"$certPath`""
    $msbuildArgs += "/p:PackageCertificatePassword=password"
}

$argString = $msbuildArgs -join " "
$processInfo = New-Object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = $msbuildPath
$processInfo.Arguments = $argString
$processInfo.UseShellExecute = $false
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true

$process = New-Object System.Diagnostics.Process
$process.StartInfo = $processInfo
$process.Start() | Out-Null
$stdout = $process.StandardOutput.ReadToEnd()
$stderr = $process.StandardError.ReadToEnd()
$process.WaitForExit()

$exitCode = $process.ExitCode

Write-Host $stdout
if ($stderr) {
    Write-Host $stderr -ForegroundColor Yellow
}

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "MSIX Package Created Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Package location: $OutputDir" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To install the package:" -ForegroundColor Yellow
    Write-Host "1. Install the certificate (if using self-signed)" -ForegroundColor White
    Write-Host "2. Double-click the .msixbundle or .msix file" -ForegroundColor White
    Write-Host "3. Click 'Install' in the App Installer" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "ERROR: Package creation failed!" -ForegroundColor Red
    exit 1
}

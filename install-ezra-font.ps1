# Install Ezra Bold Font
# Run this script as Administrator

$fontPath = Join-Path $PSScriptRoot "Assets\Fonts\Ezra Bold.otf"

if (Test-Path $fontPath) {
    Write-Host "Installing Ezra Bold font..." -ForegroundColor Green
    
    # Copy to Fonts folder
    $fontsFolder = [System.Environment]::GetFolderPath('Fonts')
    Copy-Item $fontPath -Destination $fontsFolder -Force
    
    # Register the font
    $fontName = "Ezra SemiBold (TrueType)"
    $fontFile = "Ezra Bold.otf"
    
    New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts" `
                     -Name $fontName `
                     -Value $fontFile `
                     -PropertyType String `
                     -Force | Out-Null
    
    Write-Host "Font installed successfully!" -ForegroundColor Green
    Write-Host "You may need to restart the application for changes to take effect." -ForegroundColor Yellow
} else {
    Write-Host "Font file not found at: $fontPath" -ForegroundColor Red
}

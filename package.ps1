param(
  [switch]$PortableOnly
)
$ErrorActionPreference = 'Stop'
if (!(Test-Path dist)) { New-Item -ItemType Directory -Path dist | Out-Null }
# Build release binaries
Write-Host "Building solution in Release..."
& .\.dotnet\dotnet build -c Release FastBoard.sln | Tee-Object -FilePath dist/build-log.txt

# Create portable zip from App project
$buildDir = "src/App/bin/x86/Release/net8.0-windows10.0.19041.0"
if (!(Test-Path $buildDir)) { throw "Build output not found: $buildDir" }
$portableZip = "dist/app-portable.zip"
if (Test-Path $portableZip) { Remove-Item $portableZip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($buildDir, $portableZip)
Write-Host "Portable ZIP created: $portableZip"

if ($PortableOnly) { Write-Host "Portable-only build requested. Skipping MSIX/EXE."; exit 0 }

# Attempt MSIX packaging using MakeAppx/SignTool if available
$makeappx = (Get-Command makeappx.exe -ErrorAction SilentlyContinue)
$signtool = (Get-Command signtool.exe -ErrorAction SilentlyContinue)
$appxManifest = Join-Path $buildDir "AppxManifest.xml"
if (!(Test-Path $appxManifest)) {
  # Create a minimal manifest placeholder to allow makeappx if available
  @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" IgnorableNamespaces="uap">
  <Identity Name="FastBoard.App" Publisher="CN=FastBoard" Version="1.0.0.0" />
  <Properties>
    <DisplayName>FastBoard</DisplayName>
    <PublisherDisplayName>FastBoard</PublisherDisplayName>
    <Logo>Assets\\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.19041.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Applications>
    <Application Id="App" Executable="App.dll" EntryPoint="FastBoard.App">
      <uap:VisualElements DisplayName="FastBoard" Square150x150Logo="Assets\\Square150x150Logo.png" Square44x44Logo="Assets\\Square44x44Logo.png" Description="FastBoard" />
    </Application>
  </Applications>
</Package>
"@ | Set-Content -Encoding UTF8 $appxManifest
}

if ($makeappx) {
  $msix = "dist/app-release.msix"
  if (Test-Path $msix) { Remove-Item $msix -Force }
  & $makeappx pack /d $buildDir /p $msix | Tee-Object -FilePath dist/build-log.txt -Append
  Write-Host "MSIX package created at $msix"
} else {
  Write-Warning "makeappx.exe not found. Creating instructions script instead."
}

# Attempt to create installer via WiX/NSIS if installed; else create instructions
$installer = "dist/installer.exe"
if (Test-Path $installer) { Remove-Item $installer -Force }
"Installer creation requires WiX/NSIS. If installed, adapt this script to create an installer from $buildDir." | Set-Content -Encoding UTF8 dist/installer.README.txt

Write-Host "Packaging complete."
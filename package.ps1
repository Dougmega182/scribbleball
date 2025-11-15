param(
  [switch]$PortableOnly,
  [string]$FfmpegUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z"
)
$ErrorActionPreference = 'Stop'
if (!(Test-Path dist)) { New-Item -ItemType Directory -Path dist | Out-Null }

# Build release binaries
Write-Host "Building solution in Release..."
& .\.dotnet\dotnet build -c Release FastBoard.sln | Tee-Object -FilePath dist/build-log.txt

# Build output dir (adjust if your platform differs)
$buildDir = "src/App/bin/x86/Release/net8.0-windows10.0.19041.0"
if (!(Test-Path $buildDir)) { throw "Build output not found: $buildDir" }

# Optionally bundle ffmpeg in the app folder (portable + MSIX)
$ffmpegDir = Join-Path $buildDir "ffmpeg"
$ffmpegExe = Join-Path $ffmpegDir "bin/ffmpeg.exe"
if (!(Test-Path $ffmpegExe)) {
  try {
    Write-Host "Fetching ffmpeg..."
    $ff7z = "dist/ffmpeg.7z"
    if (Test-Path $ff7z) { Remove-Item $ff7z -Force }
    Invoke-WebRequest -Uri $FfmpegUrl -OutFile $ff7z -UseBasicParsing
    # Extract with 7zip if available; otherwise, instruct user
    $sz = Get-Command 7z.exe -ErrorAction SilentlyContinue
    if ($sz) {
      & $sz x $ff7z -o"dist/ffmpeg_unpacked" -y | Out-Null
      $bin = Get-ChildItem -Recurse -Path dist/ffmpeg_unpacked -Filter ffmpeg.exe | Select-Object -First 1
      if ($bin) {
        New-Item -ItemType Directory -Path (Split-Path $ffmpegExe) -Force | Out-Null
        Copy-Item $bin.Directory.FullName -Destination (Split-Path $ffmpegExe -Parent) -Recurse -Force
      } else {
        Write-Warning "ffmpeg.exe not found in archive; skipping bundle."
      }
    } else {
      Write-Warning "7z.exe not found. Install 7-Zip or set FfmpegUrl to a .zip. Skipping ffmpeg bundle."
    }
  } catch {
    Write-Warning "Failed to download/extract ffmpeg: $_"
  }
}

# Create portable zip from App project
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
  <Identity Name="FastBoard.App" Publisher="CN=FastBoard Dev" Version="1.0.0.0" />
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

$msix = "dist/FastBoard.msix"
if ($makeappx) {
  if (Test-Path $msix) { Remove-Item $msix -Force }
  & $makeappx pack /d $buildDir /p $msix | Tee-Object -FilePath dist/build-log.txt -Append
  Write-Host "MSIX package created at $msix"
} else {
  Write-Warning "makeappx.exe not found. Skipping MSIX creation."
}

# Dev certificate: auto-generate and sign if possible
$pfx = "dist/fastboard-dev.pfx"
if ($signtool -and (Test-Path $msix)) {
  try {
    if (!(Test-Path $pfx)) {
      Write-Host "Generating dev code-signing certificate..."
      $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=FastBoard Dev" -CertStoreLocation Cert:\CurrentUser\My
      $pwd = ConvertTo-SecureString -String "fastboard-dev" -Force -AsPlainText
      Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $pwd | Out-Null
    } else { $pwd = ConvertTo-SecureString -String "fastboard-dev" -Force -AsPlainText }

    Write-Host "Signing MSIX..."
    & $signtool sign /fd SHA256 /a /f $pfx /p fastboard-dev $msix | Tee-Object -FilePath dist/build-log.txt -Append
    Write-Host "MSIX signed: $msix"
  } catch {
    Write-Warning "Signing failed: $_"
  }
}

# Installer stub (WiX/NSIS)
"Installer creation requires WiX/NSIS. If installed, adapt this script to create an installer from $buildDir." | Set-Content -Encoding UTF8 dist/installer.README.txt

Write-Host "Packaging complete. MSIX: $msix; Portable: $portableZip"
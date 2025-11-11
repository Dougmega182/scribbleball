param(
  [string]$FramesDir = "dist/frames",
  [string]$Output = "dist/screencast.mp4",
  [int]$Fps = 4
)
$ErrorActionPreference='Stop'
if (!(Test-Path $FramesDir)) { throw "Frames directory not found: $FramesDir" }
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue)
if (-not $ffmpeg) {
  Write-Warning "ffmpeg not found. Please install ffmpeg or use an alternative to stitch PNG frames."
  "To create a video: ffmpeg -framerate $Fps -i frame-%04d.png -pix_fmt yuv420p ../screencast.mp4" | Set-Content -Encoding UTF8 (Join-Path $FramesDir "INSTRUCTIONS.txt")
  exit 0
}
Push-Location $FramesDir
& $ffmpeg -y -framerate $Fps -i frame-%04d.png -pix_fmt yuv420p (Resolve-Path ../screencast.mp4)
Pop-Location
Write-Host "Created video at $Output"
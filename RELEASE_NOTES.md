# Scribble Ball v0.1.0-alpha

This is the first alpha of Scribble Ball by Ascend AI.

## Highlights
- Green build (Release x64)
- Full UI restored
  - CommandBar: play/pause/prev/next/loop; save/open; export (PNG + frames)
  - Timeline: frame slider, progress slider, duration field
  - Hotkeys: Ctrl+S/Ctrl+Shift+S/Ctrl+O; Ctrl+N/Ctrl+D/Delete; F5
  - Toasts for save/open/export
- CourtView stabilized
  - Skia SKXamlCanvas rendering
  - ExportPng(path, width, height)
  - Gestures: pointer wheel zoom; double‑tap reset
- Export presets (GIF/MP4)
  - GIF: palettegen + paletteuse
  - MP4: h264 (yuv420p), CRF=20, preset=veryfast
  - FPS presets: 15/24/30/60; scaling: 0.5x/1x/2x
- Dashed rendering polish
  - Zoom‑consistent dash/gap sizing
  - Smoother dribble paths (round joins)
- Text tool MVP
  - Place/edit text; font size/color; rotation; Skia anti‑aliased render
- Timeline polish
  - Smooth scrubbing and optional boundary snapping
  - Inline ease/duration; keyboard support
- Token palette MVP
  - Preset/custom tokens; drag‑and‑drop onto court
  - Token editor (number/name, color, scale, rotation, optional image)
  - Snapping, multi‑select, palette search/filter
- Branding and metadata
  - Orange/blue icons and splash
  - About dialog and app metadata
- README updated with branding and screenshots

## Downloads
- Portable ZIP: app-portable.zip
  - Includes ffmpeg (ffmpeg/bin/ffmpeg.exe)
  - Unzip and run FastBoard.exe
- MSIX: ScribbleBall.msix
  - Publisher: CN=Ascend AI
  - Double‑click to install; Windows may prompt for dev certificate trust

## Notes
- This is an alpha; expect changes and further polishing.
- If ffmpeg is not detected, export dialog will show guidance.
- For MSIX installs, ffmpeg execution is supported via AppContext or PATH.

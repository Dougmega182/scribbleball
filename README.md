# Scribbleball (WinUI 3)

A Windows desktop basketball play-diagramming app optimized for pen and touch.

Status: Active development (alpha). Core court rendering, drawing tools, undo/redo, and camera gestures are implemented. Packaging and basic export are available.

## Features (current)
- Court rendering (Half/Full) with correct geometry using SkiaSharp
  - Outer boundary, sidelines, baselines
  - Paint/key and free-throw circle
  - Restricted-area arc, hoop/backboard markers
  - 3-pt arc with precise corner lines and arc sweep
  - Center line/circle (for full court)
  - DPI-aware scaling
- Camera & gestures
  - Mouse wheel zoom (cursor-centric) and right-drag pan
  - Touch pinch-to-zoom + pan; double-tap reset
  - Stylus barrel button temporary eraser toggle
- Drawing tools (MVVM-backed)
  - Select, Arrow, Dribble (dashed), Curve (quadratic), Screen (rounded rect), Shot Arc, Pen (pressure-aware), Eraser
  - Visual selection handles for Arrow, Curve, Screen, Token
  - Undo/Redo service with granular actions (move, resize, rotate, edit points)
- Tokens
  - Vector token with color/number rendering (no image path) or bitmap token via `ImagePath`
  - Scale/rotate/position; basic selection handles in renderer
- Playback
  - Capture frames and tween shapes between consecutive frames
  - Per-shape easing (Linear, EaseInOut)
  - Per-segment duration with timeline controls (play/pause, prev/next, loop)
- Serialization & export
  - JSON serialization for Playbook model with frames, per-segment durations, and embedded base64 images for tokens
  - Save/Open using `dist/playbook.json` with multi-play support and a picker in the toolbar
  - Export full timeline to PNG frames (dist/frames) at 30 FPS; optional conversion with `make_gif_or_mp4.ps1`
- Packaging & automation
  - Local .NET 8 SDK included in repo
  - Build, test, and packaging scripts

## Not yet complete (planned)
- Full token palette UI, drag and drop import, tagging/search
- Complete save/load UI flows; embed base64 images in JSON
- E2E UI automation coverage for full flows
- MSIX signing pipeline and installer

## Prerequisites
- Windows 10/11
- Visual Studio 2022 with Windows App SDK workload
- Optional: WinAppDriver for E2E, ffmpeg for video/GIF export

## Build and Run
- Visual Studio
  1. Open `FastBoard.sln`
  2. Set `App` as startup project
  3. Build and run

- Local CLI
  - Build: `./.dotnet/dotnet build -c Release FastBoard.sln`
  - Tests: `./.dotnet/dotnet test -c Release`
  - Package: `powershell -ExecutionPolicy Bypass -File .\\package.ps1`

Artifacts are saved in `dist/`.

## Usage tips
- Plays
  - Use the Play picker (toolbar) to switch between plays in a playbook
  - Add, Duplicate, Remove, Rename plays from the toolbar
  - Save/Open persists and restores all plays in `dist/playbook.json`
- Tools
  - Select: select/move shapes; use handles to adjust
  - Arrow: click-drag; head size/angle computed; thickness responds to pressure
  - Dribble: draw dashed path
  - Curve: quadratic curve with auto mid control
  - Screen: draw rounded rect, then resize/rotate via handles
  - Shot Arc: center then radius; arc sweep defaults (configurable in code)
  - Pen: pressure-aware freehand stroke
  - Eraser: scribble to remove nearby segments/shapes
- Camera
  - Mouse: wheel to zoom, right-drag to pan, double-click to reset
  - Touch: pinch to zoom, drag to pan, double-tap to reset
  - Stylus: barrel button temporarily switches to eraser while pressed

## Export
- The `CourtView` control exposes `ExportPng(path, width, height)` to render the board as a PNG.
- Export Frames renders the active play timeline to `dist/frames/frame-0000.png` at 30 FPS using per-segment durations.
- Use `make_gif_or_mp4.ps1` to convert the exported frames to GIF/MP4 (requires ffmpeg).

## Playbook format (overview)
- playbook.json
  - name: string
  - plays: [
    - id, title
    - frames: [ { id, duration (seconds), shapes: BoardState JSON } ]
  ]

Images
- Token images are embedded as base64 JPEG (resized to max 256 px on the longest edge, quality ~85) to keep files portable
- Original ImagePath is cleared on save in favor of embedded images

## Project structure
- `src/App`     — WinUI 3 front end (XAML + SkiaSharp renderer)
- `src/App.Core`— Core geometry, models, tools, services, playback, serialization
- `src/App.Data`— EF Core context (future playbook storage)
- `tests/`      — Unit tests (geometry, hit testing, serialization, undo/redo, Bezier)
- `sample_data` — Placeholder PNGs and sample playbook JSON

## Packaging
- `package.ps1` creates `dist/app-portable.zip`; attempts MSIX if tools are available

## Troubleshooting
- If SkiaSharp rendering shows a blank view, ensure GPU drivers are up to date.
- If tests fail to locate .NET SDK, use the repo-provided `./.dotnet/dotnet`.

## License
See `./.dotnet/LICENSE.txt` for the .NET SDK redistribution; project code is under your repository's chosen license.

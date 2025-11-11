# FastBoard Implementation Roadmap

This roadmap breaks the project into focused phases to reach the acceptance criteria.

## Phase 0 – Bootstrap (DONE)
- Local .NET 8 SDK in `./.dotnet`
- Visual Studio solution with projects: App, App.Core, App.Data, App.Tests, App.E2E
- Minimal WinUI app shell; basic models and EF Core context
- Build, test harness, packaging script (portable zip + MSIX attempt)

## Phase 1 – Court Rendering and Geometry
- Add SkiaSharp rendering to App via `SkiaSharp.Views.WinUI`
- Implement `CourtGeometry` utilities in App.Core for measurements and scalable layout
- Implement `CourtView` control using `SKXamlCanvas` to draw:
  - Outer boundary, sidelines, baselines
  - Paint/key and free-throw semicircle
  - Restricted-area semicircle, hoop/backboard marker
  - 3-pt arc and corner lines
  - Full/half-court modes and DPI scaling
- Unit tests for geometry math (distances, arc radii, arrowhead angles)

## Phase 2 – Input & Drawing Tools (Foundations)
- Add InkCanvas for pen/touch freehand with pressure-aware thickness
- Implement tool framework (MVVM): Select, Move, Delete, Pen, Arrow, Dashed (dribble), Curve (quad/cubic), Screen, Shot Arc, Eraser
- Implement undo/redo service and tests

## Phase 3 – Tokens & Palette
- PNG upload and palette list; drag-drop onto court
- Token manipulation (rotate/resize/rename/number/color) and keyboard nudging
- Accessibility semantics

## Phase 4 – Camera & Gestures
- Pan/zoom with two-finger pinch; double-tap reset
- Stylus button toggles eraser if supported; UI fallback

## Phase 5 – Animation / Playback
- Capture frames (token positions + shapes)
- Play/Pause/Stop; Step; tweening (linear + ease)
- Unit tests for interpolation math

## Phase 6 – Save/Load/Export
- JSON import/export with embedded base64 images
- Export board to PNG; export frames to folder
- ffmpeg integration (if available) and fallback script (PNG -> MP4/GIF)

## Phase 7 – Playbook Management UI
- Folder, tags, search, duplicate, rename, delete

## Phase 8 – E2E & Packaging
- WinAppDriver E2E: upload images, drag to court, draw shapes, capture frames, play, save/load, export and verify
- Packaging refinements: MSIX manifest/assets/signing; installer script via WiX/NSIS if available
- Final artifacts in `dist/` and tag `v1.0.0-release`

## Diagnostics & Self-Repair
- On failure, write `dist/diagnostics.log`, auto-repair up to 5 attempts with clear commit messages

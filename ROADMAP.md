# FastBoard Implementation Roadmap

This roadmap tracks progress and next steps to reach the app's initial release.

## Phase 0 – Bootstrap (DONE)
- Local .NET 8 SDK in `./.dotnet`
- Visual Studio solution with projects: App, App.Core, App.Data, App.Tests, App.E2E
- Minimal WinUI app shell; basic models and EF Core context
- Build, test harness, packaging script (portable zip + MSIX attempt)

## Phase 1 – Court Rendering and Geometry (DONE)
- SkiaSharp rendering integrated in `App`
- `CourtGeometry` utilities for measurements and scalable layout
- `CourtView` draws:
  - Outer boundary, sidelines, baselines
  - Paint/key and free-throw semicircle (dashed circle for FT)
  - Restricted-area arc, hoop/backboard markers
  - 3-pt arc with accurate corner lines and arc sweep
  - Full/half-court modes and DPI scaling
- Unit tests for geometry utilities

## Phase 2 – Input & Drawing Tools (MOSTLY DONE)
- Tool framework (MVVM) implemented: Select, Move, Delete, Pen, Arrow, Dashed (dribble), Curve (quadratic), Screen, Shot Arc, Eraser
- Undo/redo service with granular actions (move, remove, add, drag-commit, edit endpoints, resize/rotate)
- Selection handles for Arrow, Curve, Screen, Token
- Remaining: richer select/drag UI in XAML; keyboard modifiers, multi-select marquee

## Phase 3 – Tokens & Palette (IN PROGRESS)
- Vector token rendering with color/number; bitmap token support with `ImagePath`
- Basic selection/resize/rotate handles in renderer
- Remaining: Palette UI for PNG upload and drag-drop onto court; token metadata (names, numbers) editing; accessibility semantics

## Phase 4 – Camera & Gestures (DONE)
- Pan/zoom with mouse wheel + right-drag and touch pinch
- Double-tap reset; stylus barrel button toggles eraser

## Phase 5 – Animation / Playback (DONE)
- Capture frames (token positions + shapes)
- Tweening between frames with easing utilities; per-segment durations; timeline editing
- Implemented: Playback UI (play/step/loop), frame timeline editing, per-shape easing

## Phase 6 – Save/Load/Export (DONE)
- JSON serialization for Playbook model (frames with durations, embedded base64 images)
- Save/Open UI flow wired to dist/playbook.json
- Export full timeline to PNG frames; optional conversion to GIF/MP4 via script

## Phase 7 – Playbook Management UI (DONE)
- Multi-play picker in toolbar; add, duplicate, remove, rename play
- Save/Open anywhere via file pickers; duplicate across playbooks; tags and search; confirmations; toasts; shortcuts

## Phase 8 – E2E & Packaging (PARTIAL)
- E2E smoke tests scaffolded
- Remaining: Expanded WinAppDriver flows (upload, drag, draw, capture, play, save/load, export); MSIX manifest/signing; installer script via WiX/NSIS

## Diagnostics & Self-Repair (FUTURE)
- On failure, write `dist/diagnostics.log`, consider auto-repair routines

## Notes
- See `README.md` for up-to-date feature list and usage tips.

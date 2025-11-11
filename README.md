# FastBoard (WinUI 3)

A Windows desktop basketball play-diagramming app targeting pen and touch (Surface Book 2).

## Prerequisites
- Windows 10/11
- Visual Studio 2022 with Windows App SDK workload
- Optional: WinAppDriver for E2E, ffmpeg for export

## Open and Run
1. Open `FastBoard.sln` in Visual Studio 2022.
2. Set `App` as startup project.
3. Build and run.

## CLI
- Local SDK: `./.dotnet/dotnet build -c Release FastBoard.sln`
- Tests: `./.dotnet/dotnet test -c Release`
- Package: `powershell -ExecutionPolicy Bypass -File .\package.ps1`

Artifacts are saved in `dist/`.

## Packaging
- `package.ps1` creates `dist/app-portable.zip` and attempts MSIX if tools are available.

## Sample Data
See `sample_data/` for placeholder PNGs and sample playbook JSON.

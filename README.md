# VibeBoulderDash

A faithful Boulder Dash clone for Windows, written in C# with MonoGame.

## About

This program was created as a **vibecoding project**: it was developed in an iterative,
AI-assisted workflow where an agent implements, tests, and refines the game step by step.

It is based on **publicly available sources found on the internet** — such as
documentation of the BDCFF level format, public fan-made cave packs, and community
descriptions of the original game's rules. No original First Star Software assets are
used: all sprites, palette, fonts, and audio are self-created from scratch.

The architecture follows a strict separation:

- `VibeBoulderDash.Core` — a pure, fully deterministic game-logic kernel with no
  MonoGame/XNA dependencies, driven solely by ticks.
- `VibeBoulderDash.Game` — the MonoGame front end for rendering, input, and audio.

## Features

- Faithful recreation of the classic Boulder Dash rules (gravity, explosions,
  amoeba growth, magic walls, butterflies/fireflies, diamonds, exits, intermissions).
- Official cave sets I–III as embedded data, plus BDCFF level support with two
  fan-made cave packs included.
- Deterministic simulation covered by an extensive xUnit test suite.
- Cave selection, set switching, save/load, game speed control, and an HUD.
- Automatic version numbering (`Major.Minor.Build`) shown in the window title
  and status line; the build number increments on every compile.

## Build & Run

```powershell
# dotnet lives in "C:\Program Files\dotnet", optionally prepend it to PATH:
$env:Path = "C:\Program Files\dotnet;" + $env:Path

# Build the solution
dotnet build VibeBoulderDash.sln

# Run the tests
dotnet test src\VibeBoulderDash.Core.Tests

# Start the game
dotnet run --project src\VibeBoulderDash.Game
```

## Changelog

The changelog is maintained by the agent automatically per minor version. Patch-level
build-number increments are not listed individually.

### 1.1 (in development)

- Reworked the project into a deterministic core plus a MonoGame front end, covered
  by an xUnit test suite.
- Added official cave sets I–III (embedded) and BDCFF fan-level support with two
  public cave packs (AflBD, ArnoDash01).
- Added cave selection, set switching (`F4`/`F5`), save/load, and intermissions.
- Fixed a bug in the magic-wall fall rule that lost boulders entering walls incorrectly.
- Fixed firefly chain explosions reacting in a non-original frame order.
- Fixed a crash when selecting small (intermission-sized) caves, e.g. cave 4 of AflBD.
- Fixed a crash during the cave transition in ArnoDash01 (size mismatch between a
  normal cave and an intermission).
- Introduced automatic `Major.Minor.Build` version numbering displayed in the window
  title and the status line.
- Renamed the project to VibeBoulderDash (window title, built-in cave sets, project
  and assembly names, and namespaces).
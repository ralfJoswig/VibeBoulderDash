# AGENTS.md

Anleitung für Agenten, die in diesem Projekt arbeiten.

## Build / Test

```powershell
# dotnet liegt unter "C:\Program Files\dotnet", ggf. PATH ergänzen:
$env:Path = "C:\Program Files\dotnet;" + $env:Path

# Lösung bauen
dotnet build VibeBoulderDash.sln

# Tests
dotnet test src\VibeBoulderDash.Core.Tests
dotnet test   # alle Tests der Solution

# Spiel starten
dotnet run --project src\VibeBoulderDash.Game
```

Jeder Build von `VibeBoulderDash.Game` erhöht die Buildnummer in `src\VibeBoulderDash.Game\buildnumber.txt`
und kompiliert `MajorVersion.MinorVersion.Buildnummer` (Haupt-/Unterversion stehen in der
`VibeBoulderDash.Game.csproj`). Die Versionsnummer wird im Fenstertitel und der Statuszeile angezeigt.

## Architektur-Regeln

- `VibeBoulderDash.Core` ist ein **purer Logikkern**: keinerlei MonoGame-/XNA-Abhängigkeiten,
  vollständig deterministisch, alles über 1D/2D-Arrays und eigene Typen. Ausnahme:
  eigene `Point`/`Direction`-Strukturen verwenden, NICHT `Microsoft.Xna.Framework.*`.
- `VibeBoulderDash.Game` ist der einzige Ort mit MonoGame-Rendering/Input/Audio.
- Neue Spielregeln immer zuerst in `VibeBoulderDash.Core` mit xUnit-Tests (red-green-refactor).
- Keine Logik, die vom Frame-Timing abhängt, in der Engine – Ticks sind der einzige Zeitbegriff.
- Sprites/Assets werden selbst erstellt, keine Original-Rips (Copyright).
- Nach jeder abgeschlossenen, für Nutzer relevanten Änderung die `Changelog`-Sektion in
  `README.md` (Englisch) fortführen: Stichpunkte unter der jeweiligen Minor-Version ergänzen;
  die Buildnummer (Patch-Stelle) wird nie einzeln aufgeführt.

## Coding-Stil

- Keine Kommentare außer XML-Docs für öffentliche APIs.
- Namespaces: `VibeBoulderDash.Core`, `VibeBoulderDash` (Game-Unterordner, NICHT `VibeBoulderDash.Game`
  als Namespace, da das mit `Microsoft.Xna.Framework.Game` kollidiert).
- Englische Bezeichner, deutsche UI-Texte nur im Game-Layer.
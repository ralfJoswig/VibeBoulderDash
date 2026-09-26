# Boulder Dash Clone (Windows) — Projektplan

Nachbau des C64-Klassikers **Boulder Dash** (First Star Software, 1984, C64-Version) in C# + MonoGame.
Ziel: Spiellogik und Höhlen so nah wie möglich am Original, Optik als modernisierte Pixel-Art.

## Entscheidungen

| Bereich | Entscheidung |
|---|---|
| Sprache/Framework | C# / .NET 9 / MonoGame 3.8.x (DesktopGL) |
| Optik | Neu gezeichnete Pixel-Art im C64-Stil + moderne Effekte (Interpolation, Partikel, Glow, optional CRT) |
| Umfang | Erste eigene Test-Höhlen, danach die 16 Original-Höhlen A–P + 4 Intermissions |
| Audio | SID-artiger Software-Chipsynth, keine Original-Audiodaten |
| Fidelity-Quelle | VICE-Emulator als Goldreferenz, Tick-für-Tick-Abgleich |

## Projektstruktur

```
BoulderDash.sln
src/
  BoulderDash.Core/        Purer Logikkern, KEINE MonoGame-Abhängigkeit, deterministisch
  BoulderDash.Core.Tests/  xUnit-Tests für die Engine
  BoulderDash.Game/        MonoGame-Client (Rendering, Input, Audio)
```

## Spiellogik (BD1-Engine)

- **Grid**: 40×22 Tiles (wie C64, 880 Byte) + Rahmen; Spieler-Logik auf diskreten Ticks (60/s PAL).
- **Elemente** (interne Zustände wie im Original: delayed, falling, Birth-/Explosionsstufen):
  Space, Dirt, Brick Wall, Magic Wall, Titan Wall, Exit (versteckt/sichtbar), Rockford,
  Boulder, Diamond, Firefly, Butterfly, Amoeba.
- **Scan-Reihenfolge** (unten→oben, rechts→links) originalgetreu nachbilden — Kern für authentisches
  Verhalten („Rockford scanned this frame", Gleiten, Timings). Gegen VICE kalibrieren.
- **Regeln**:
  - Rockford: gräbt Dirt, kann einzelnen Fels schieben, wird von fallenden Felsen/Diamanten zerquetscht,
    Feind darf er nicht berühren. Bewegung im Tick jeden Frame umsetzen.
  - Fels/Diamant: fällt geradeaus, gleitet diagonal (links/rechts), wenn direkt unterstützt.
  - Firefly: links-abbiegen bevorzugt → geradeaus → rechts drehen (ohne Bewegung), explodiert bei
    Kontakt mit Rockford/Amöbe. Felsen darauf = Explosion.
  - Butterfly: fliegt entgegengesetzt, wird beim Zerquetschen zu Diamanten.
  - Amöbe: wächst durch Erde; eingeschlossen → Diamanten; zu groß (~200 Felder) → Felsen.
  - Magic Wall: aktiviert durch fallenden Felsen, millt für kurze Zeit (Fels↔Diamant).
  - Explosionen zerstören Brick Walls, können sich kettenartig ausbreiten.
- **Metadaten je Höhle**: Zeitlimit, benötigte Diamanten, Diamantwert, Bonuswert, Amöben-/Magic-Wall-Zeit,
  Höhlenfarben, Intermission-Flag.
- **Spielfluss**: Score, Leben, Extraleben (~je 10000+), Start-Countdown, Höhlen-Übergang, Intermission.

## Optik

- **Backbuffer-Rendering**: RenderTarget 40×22 Tiles × 16 px = 640×352, danach saubere Skalierung
  (integer/nearest), effizient und knackig.
- **Modernisierte Pixel-Art**: Sprites neu gezeichnet (C64-Farbpalette), glatte Bewegung per Interpolation
  zwischen den Ticks, Partikel (Staub, Diamant-Funkeln, Landungen), Glow für Diamanten/Magic Wall,
  Screenshake bei Explosionen, optional CRT-/Vignette-Shader.
- **Screen-Phasen**: Titel, Schwierigkeitswahl, Höhle, Intermission, Game Over, Highscore, Pause.

## Audio

- Software-Chipsynth (Oszillatoren + Noise + Hüllkurven) → `DynamicSoundEffectInstance`.
- Eigene, stilgetreue Stücke + SFX (Diamant, Explosion, Magic Wall, Bonus, Start, Exit).

## Content / Höhlen

- Textbasiertes Höhlenformat (`*.cave`) mit C64-Werten, editierbar, plus Metadaten.
- 16 Original-Höhlen + Intermissions nach öffentlich dokumentierten Fan-Daten.
  Hinweis: Layouts sind urheberrechtlich geschützt; Nachbau nur für privaten Gebrauch,
  keine Veröffentlichung der Original-Artworks.

## Test & Fidelity

- xUnit: Unit-Tests für jede Regel (Gravitation/Gleiten, Firefly-/Butterfly-Kreise, Amöben-Einschließung,
  Magic-Wall-Timing, Explosionsketten, Score/Exit).
- Deterministische Playtest-Sequenzen als Regressions-Tests.
- Gegen das Original im VICE-Emulator verifizieren (Scan-Ordnung, Quirks, Timings).

## Meilensteine

1. ✅ Toolchain (SDK 9, Templates), Solution, Build-Pipeline
2. ✅ Scaffold: Core/Game/Tests
3. ▶ Kernengine: Grid, Cave-Loading, Rockford, Gravitation, Schieben, Diamanten, Exit, Score/HUD (Tests grün)
4. Gegner/Systeme: Firefly, Butterfly, Amöbe, Magic Wall, Explosionen, Intermission (Tests grün)
5. Rendering: Backbuffer, Sprites, Interpolation, HUD, Partikel, Screenshake, Shader
6. Audio: Chipsynth + SFX
7. Eigene Test-Höhlen
8. 16 Original-Höhlen + Intermissions
9. Feinschliff + VICE-Abgleich
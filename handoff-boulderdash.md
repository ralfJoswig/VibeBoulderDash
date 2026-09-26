# Handoff: Boulder Dash Clone (opencode-Session)

Stand: Build & Tests **grün** (`dotnet test BoulderDash.sln` → **109/109**).
Kontext-Dokumente, die einen Agenten schon enthalten (nicht duplizieren):
- Architektur-/Stilregeln: `AGENTS.md` (Repository-Wurzel)
- Projektplan: `PLAN.md` (Repository-Wurzel)

## Ziel der Sessions
Nachbau des C64-Klassikers Boulder Dash originalgetreu (Logik deterministisch in `BoulderDash.Core`, MonoGame nur in `BoulderDash.Game`). Neue Regeln zuerst in Core mit xUnit-Tests (red-green-refactor). Keine Frame-Timing-Logik in der Engine – `Tick`/`Steps` sind bis jetzt die einzige Zeitbasis.

## Letzter Arbeitstand (abgeschlossen, getestet)

### 1. Extra-Leben-Mechanik (fix)
- Root-Cause: `_extraLifeAccumulator` wurde bei **jedem** `LoadCave` auf 0 zurückgesetzt (lag außerhalb des Session-Blocks). Im Original zählt die Schwarze-Schwelle über den Gesamtpunktestand aller Höhlen.
- Fix in `BoulderDashEngine.cs`: Zähler wird nur noch bei neuer Session (Reset) auf 0 gesetzt (`if (!preserveSession || _state == null)`), bleibt über Höhlengrenzen hinweg erhalten.
- Regressions-Test: `ScoreTests.ExtraLifeThresholdCarriesAcrossCaves`.

### 2. Feier-/Flimmer-Effekt wurde KOMPLETT ZURÜCKGEBAUT
- War nach mehreren Iterationen nie zufriedenstellend („starres Weiß", „kein Flimmern", „passt nicht zum Original"). User: „Stellen wir das zurück."
- Entfernt aus `CaveRenderer.cs`: `CelebrationMs`, `_celebrationCells`, `_celebrationTimer`/Stopwatch, `IsLifeCelebration`, `IsCelebrationCell`, `UpdateCelebrationCells` (BFS), `DrawTunnelCelebration`, Debug-Text, `using System.Diagnostics`.
- Aus `BoulderDashGame.cs` entfernt: `_renderer.TriggerLifeCelebration()` im `ExtraLife`-Handler. Außerdem TriggerLifeCelebration-Methode im Renderer gelöscht.
- **Bleibt**: Extra-Leben selbst + `GameEventType.ExtraLife`-Event + Jingle (`_audio.Handle`). Engine-Features `IsDug` (+ `DugTrackingTests`) existieren weiter.
- WICHTIG für nächste Sessions: Effekt nicht mit Zeitbasis Engine-Ticks basteln; visuelles Timing im Renderer ggf. mit Echtzeit, aber nüchtern prüfen. Evtl. neuer, klar abgestimmter Versuch.

### 3. Exit-Bug (fix)
- Vorher „Rockford lief nicht in den Ausgang, blieb davor stehen".
- Fix in `BoulderDashEngine.cs`, `TryMoveRockford`, Fall `Element.Exit when _state.ExitOpen`: erst `MoveRockford(target)`, dann `CompleteCave()`.
- Test erweitert: `ExitAndTimerTests.BonusIsAwardedOnCompletion` prüft `GetTile(2,0) == Rockford` nach Abschluss.

### 4. Rockford-Blickrichtung (diverse Runden)
- Engineseite war bereits korrekt: `_facing` = letztes Input; `RockfordFacing` Property.
- Fehlannahme rückgängig: **Sprites NICHT verändern.** `TileArt.RockfordA`/`RockfordB` sind originaler Code (Revert erfolgt). A = frontal/stehend, B = Geh-Pose, die **nativ nach links** schaut (Bein-/Fuß-Asymmetrie).
- Aktueller Stand Renderer (`CaveRenderer.cs`):
  - `FlipHorizontally` wird bei **`Direction.Right`** angewendet (Zeile ~87). Rechtslauf = B gespiegelt, Linkslauf = B ungespiegelt.
  - `PickFrame`: Rockford nutzt **B (Seiten-Pose) nur, solange er sich bewegt UND links/rechts blickt**; sonst A (frontal). Bewege-Erkennung über Zellpositions-Vergleich je Draw + `IdleGraceMs = 160` (Seiten-Pose hält ~160 ms nach letztem Zellwechsel an).
  - Dadurch: Seitwärtslauf permanent Seite, Stillstand → frontal.
- Offen/Unklar (User bewusst noch offen angesprochen): ob auch für **oben/unten** eigene Posen gewünscht sind. Nur links/rechts war gefordert.

## Vollst. relevant Dateien
- `src/BoulderDash.Core/BoulderDashEngine.cs` – Engine (AddScore/Extra-Life-Zähler, TryMoveRockford/Exit, Steps/Tick).
- `src/BoulderDash.Core/GameEvent.cs` – Events (CaveCompleted, CaveBonusFinished, ExtraLife, …).
- `src/BoulderDash.Game/BoulderDashGame.cs` – Game-Loop, Input (`GetDirection`), Event-Handling (ExtraLife jetzt ohne Renderer-Aufruf), `_awaitingNext`-Gating.
- `src/BoulderDash.Game/Rendering/CaveRenderer.cs` – Zeichnen, Flip-on-Right, `PickFrame`, Rockford-Bewegungserkennung, Hud/Overlay.
- `src/BoulderDash.Game/Graphics/TileArt.cs` – selbsterstellte Pixel-Grids (RockfordA/B …), keine Original-Rips erlaubt.
- Tests: `ScoreTests.cs`, `ExitAndTimerTests.cs`, `DugTrackingTests.cs`, `RockfordMovementTests.cs`.

## Befehle
```powershell
$env:Path = "C:\Program Files\dotnet;" + $env:Path
dotnet build BoulderDash.sln
dotnet test BoulderDash.sln        # aktuell 109/109 grün
dotnet run --project src\BoulderDash.Game   # Spiel starten
```

## Build-/Teststatus
- Zuletzt: 0 Fehler, 109/109 grün (nach Schritt 4).

## Suggested skills
- **handoff** – lesen, falls in späteren Sessions weitergegeben wird.
- **tdd** – neue Spielregeln immer Test-first in `BoulderDash.Core`.
- **grill-me / grill-with-docs** – nur wenn nächste Session Designs (z. B. Feier-Effekt, Richtungs-Posen) vorab stressen will.
- **caveman** – nur auf expliziten Wunsch (Token-sparend).
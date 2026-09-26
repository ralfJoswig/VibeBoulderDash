using System;
using System.Collections.Generic;
using VibeBoulderDash.Core;
using VibeBoulderDash.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBoulderDash.Rendering;

internal sealed class CaveRenderer
{
    public const int TileSize = 32;
    public const int ViewCols = 30;
    public const int ViewRows = 16;
    public const int PlayWidth = ViewCols * TileSize;
    public const int PlayHeight = ViewRows * TileSize;
    public const int HudHeight = 2 * TileSize;
    public const int ScreenWidth = PlayWidth;
    public const int ScreenHeight = PlayHeight + HudHeight;
    private const int FlashTicks = 9;
    private const int ExtraLifeTicks = 90;
    private const int StreakFlickerTicks = 2;
    private const int RockfordFrameTicks = 2;
    private const int RockfordWalkFrames = 4;
    private const int IdleSequenceFrames = 8;

    private readonly SpriteBatch _batch;
    private readonly TileArt _tiles;
    private readonly PixelFont _font;
    private readonly BoulderDashEngine _engine;
    private readonly Texture2D _pixel;
    private readonly Texture2D[] _explosionFrames;
    private readonly Texture2D _streakTexture;
    private int _camX;
    private int _camY;
    private long _caveStartTick;
    private long _flashStart = -1;
    private long _extraLifeFlashStart = -1;

    public CaveRenderer(GraphicsDevice device, BoulderDashEngine engine)
    {
        _engine = engine;
        _batch = new SpriteBatch(device);
        _tiles = new TileArt(device);
        _font = new PixelFont(device);
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _explosionFrames = new[]
        {
            TileArt.Build(device, TileArt.Grids.Explosion1, TileArt.ExplosionPalette),
            TileArt.Build(device, TileArt.Grids.Explosion2, TileArt.ExplosionPalette),
            TileArt.Build(device, TileArt.Grids.Explosion3, TileArt.ExplosionPalette),
            TileArt.Build(device, TileArt.Grids.Explosion4, TileArt.ExplosionPalette),
        };
        _streakTexture = TileArt.Build(device, TileArt.Grids.Streak, TileArt.StreakPalette);
    }

    public bool Paused;
    public int CaveNumber = 1;
    public int CaveCount = 20;
    public double Speed = 1.0;
    public bool ShowCaveSelect;
    public int CaveSelectIndex;
    public bool ShowSetSelect;
    public int SetSelectIndex;
    public bool ShowConfirmQuit;
    public int CurrentSetIndex;
    public IReadOnlyList<CaveSet> Sets;
    public string SetName = "";
    public string StatusMessage;
    public long StatusMessageUntil;
    private IReadOnlyList<CaveDefinition> _caves;

    public void SetCaves(IReadOnlyList<CaveDefinition> caves) => _caves = caves;

    public void StartCave()
    {
        _caveStartTick = _engine.Tick;
        _flashStart = -1;
        _extraLifeFlashStart = -1;
    }

    public void TriggerScreenFlash() => _flashStart = _engine.Tick - _caveStartTick;

    public void ResetScreenFlash() => _flashStart = -1;

    public void TriggerExtraLifeFlash() => _extraLifeFlashStart = _engine.Tick - _caveStartTick;

    public void Draw()
    {
        _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        DrawHud();
        Fill(0, HudHeight, PlayWidth, PlayHeight, Color.Black);
        UpdateCamera();

        var extraLifeElapsed = ExtraLifeFlashElapsed();
        var cols = Math.Min(ViewCols, _engine.Width - _camX);
        var rows = Math.Min(ViewRows, _engine.Height - _camY);

        for (int ty = 0; ty < rows; ty++)
        {
            for (int tx = 0; tx < cols; tx++)
            {
                var tile = _engine.GetTile(_camX + tx, _camY + ty);
                if (tile.Element == Element.Space)
                {
                    if (extraLifeElapsed >= 0)
                    {
                        var streakDest = new Rectangle(tx * TileSize, HudHeight + ty * TileSize, TileSize, TileSize);
                        _batch.Draw(_streakTexture, streakDest, StreakFlicker(extraLifeElapsed));
                    }

                    continue;
                }

                var frames = _tiles.Get(tile.Element);
                var frame = PickFrame(tile.Element, frames.Length);
                var source = frames[frame];
                var dest = new Rectangle(tx * TileSize, HudHeight + ty * TileSize, TileSize, TileSize);

                if (tile.Element == Element.Rockford)
                {
                    var effects = _engine.RockfordAnimFacing == Direction.Right ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    _batch.Draw(source, dest, null, Color.White, 0f, Vector2.Zero, effects, 0f);
                }
                else
                {
                    _batch.Draw(source, dest, Color.White);
                }
            }
        }

        DrawDeathExplosion();
        DrawOverlay();
        DrawCaveSelect();
        DrawSetSelect();
        DrawScreenFlash();
        DrawStatus();
        DrawConfirmQuit();

        _batch.End();
    }

    private void DrawDeathExplosion()
    {
        if (_engine.DeathExplosionTicksLeft <= 0)
        {
            return;
        }

        var rock = _engine.Rockford;
        var tx = rock.X - _camX;
        var ty = rock.Y - _camY;
        if (tx < 0 || ty < 0 || tx >= ViewCols || ty >= ViewRows)
        {
            return;
        }

        var frame = _engine.DeathExplosionFrame;
        var frameIndex = Math.Clamp(frame, 0, _explosionFrames.Length - 1);
        var dest = new Rectangle(tx * TileSize, HudHeight + ty * TileSize, TileSize, TileSize);
        _batch.Draw(_explosionFrames[frameIndex], dest, Color.White);
    }

    private const int AnchorX = 10;
    private const int AnchorY = 4;

    private void UpdateCamera()
    {
        var rock = _engine.Rockford;
        _camX = rock.X - AnchorX;
        _camY = rock.Y - AnchorY;
        _camX = Math.Clamp(_camX, 0, Math.Max(0, _engine.Width - ViewCols));
        _camY = Math.Clamp(_camY, 0, Math.Max(0, _engine.Height - ViewRows));
    }

    private int PickFrame(Element element, int frameCount)
    {
        if (element == Element.Rockford)
        {
            if (_engine.RockfordIsMoving && !Paused)
            {
                return 1 + (int)((_engine.Tick / RockfordFrameTicks) % RockfordWalkFrames);
            }

            var frame = (int)((_engine.Tick / RockfordFrameTicks) % IdleSequenceFrames);
            var tap = _engine.RockfordTapping && frame is >= 4 and <= 5;
            var blink = _engine.RockfordBlinking && frame is >= 2 and <= 5;
            var idleBase = frameCount - 3;
            if (blink && tap)
            {
                return idleBase + 2;
            }

            if (blink)
            {
                return idleBase;
            }

            if (tap)
            {
                return idleBase + 1;
            }

            return 0;
        }

        var period = element switch
        {
            Element.MagicWall => 4,
            Element.Exit => 3,
            _ => 8,
        };

        var index = (int)((_engine.Tick / period) % frameCount);
        if (element == Element.Exit && !_engine.State.ExitOpen)
        {
            index = 0;
        }

        if (element == Element.MagicWall && !_engine.MagicWallActive)
        {
            index = 0;
        }

        return index;
    }

    private void DrawHud()
    {
        Fill(0, 0, PlayWidth, HudHeight, Color.Black);
        Fill(0, HudHeight - 1, PlayWidth, 1, C64Palette.Get(11));

        var time = _engine.State.TimeLeft <= 10 ? C64Palette.Get(2) : Color.White;
        var lives = C64Palette.Get(5);
        var diamonds = C64Palette.Get(3);
        const float scale = 3f;

        _batch.Draw(_tiles.Get(Element.Diamond)[0], new Rectangle(0, 0, TileSize, TileSize), Color.White);

        var diamondText = _engine.State.DiamondsCollected + "/" + _engine.State.DiamondsNeeded;
        _font.Draw(_batch, new Vector2(TileSize + 8, 0), diamondText, diamonds, scale);

        var scoreText = "PUNKTE " + _engine.State.Score.ToString().PadLeft(6, '0');
        var scoreWidth = _font.Measure(scoreText) * scale;
        _font.Draw(_batch, new Vector2(PlayWidth - scoreWidth, 0), scoreText, C64Palette.Get(15), scale);

        var caveText = _engine.Cave.IsIntermission ? "ZWISCHENSPIEL" : "HOHLE " + CaveNumber + "/" + CaveCount;
        _font.Draw(_batch, new Vector2(TileSize + 8, HudHeight / 2), caveText, C64Palette.Get(15), scale);

        var speedText = "TEMPO x" + Speed.ToString("0.0");
        var speedWidth = _font.Measure(speedText) * scale;
        _font.Draw(_batch, new Vector2((PlayWidth - speedWidth) / 2f, 0), speedText, C64Palette.Get(12), scale);

        var timeText = "ZEIT " + Math.Max(0, _engine.State.TimeLeft).ToString().PadLeft(3, '0');
        var timeWidth = _font.Measure(timeText) * scale;
        _font.Draw(_batch, new Vector2((PlayWidth - timeWidth) / 2f, HudHeight / 2), timeText, time, scale);

        var livesText = "LEBEN " + _engine.State.Lives;
        var livesWidth = _font.Measure(livesText) * scale;
        _font.Draw(_batch, new Vector2(PlayWidth - livesWidth, HudHeight / 2), livesText, lives, scale);
    }

    private void DrawOverlay()
    {
        if (Paused)
        {
            var pause = "PAUSE";
            var pauseW = _font.Measure(pause) * 3f;
            var resume = "F1 FORTSETZEN";
            var resumeW = _font.Measure(resume);
            Fill(0, HudHeight + PlayHeight / 3, PlayWidth, 192, new Color(0, 0, 0, 200));
            _font.Draw(_batch, new Vector2((PlayWidth - pauseW) / 2f, HudHeight + PlayHeight / 3 + 36), pause, C64Palette.Get(15), 3f);
            _font.Draw(_batch, new Vector2((PlayWidth - resumeW) / 2f, HudHeight + PlayHeight / 3 + 132), resume, C64Palette.Get(12));
            return;
        }

        var text = _engine.State.GameOver ? "SPIEL VORBEI" : _engine.State.CaveCompleted && _engine.IsIntermission ? "HOHLE GESCHAFFT" : null;
        if (text is null)
        {
            return;
        }

        var flashing = (_engine.Tick / 6) % 2 == 0;
        var color = _engine.State.GameOver ? C64Palette.Get(2) : C64Palette.Get(5);
        if (!flashing)
        {
            color = Color.White;
        }

        var width = _font.Measure(text) * 3f;
        var hint = "LEERTASTE ERNEUT DRUCKEN";
        var hintWidth = _font.Measure(hint);
        Fill(0, HudHeight + PlayHeight / 3, PlayWidth, 192, new Color(0, 0, 0, 180));
        _font.Draw(_batch, new Vector2((PlayWidth - width) / 2f, HudHeight + PlayHeight / 3 + 36), text, color, 3f);
        _font.Draw(_batch, new Vector2((PlayWidth - hintWidth) / 2f, HudHeight + PlayHeight / 3 + 132), hint, C64Palette.Get(15));
    }

    private void DrawScreenFlash()
    {
        if (_flashStart < 0)
        {
            return;
        }

        var elapsed = (_engine.Tick - _caveStartTick) - _flashStart;
        if (elapsed < 0)
        {
            _flashStart = -1;
            return;
        }

        if (elapsed >= FlashTicks)
        {
            return;
        }

        var alpha = (int)(255 * (1.0 - (double)elapsed / FlashTicks));
        Fill(0, 0, ScreenWidth, ScreenHeight, new Color(255, 255, 255, alpha));
    }

private int ExtraLifeFlashElapsed()
    {
        if (_extraLifeFlashStart < 0)
        {
            return -1;
        }

        var elapsed = (_engine.Tick - _caveStartTick) - _extraLifeFlashStart;
        if (elapsed < 0)
        {
            _extraLifeFlashStart = -1;
            return -1;
        }

        if (elapsed >= ExtraLifeTicks)
        {
            _extraLifeFlashStart = -1;
            return -1;
        }

        return (int)elapsed;
    }

    private static readonly Color[] StreakFlickerColors =
    {
        C64Palette.Get(7),
        C64Palette.Get(3),
        C64Palette.Get(4),
        C64Palette.Get(5),
    };

    private Color StreakFlicker(int elapsed) =>
        StreakFlickerColors[(elapsed / StreakFlickerTicks) % StreakFlickerColors.Length];

    private void DrawStatus()
    {
        if (StatusMessage == null || Environment.TickCount64 > StatusMessageUntil)
        {
            return;
        }

        const float scale = 1.5f;
        var width = _font.Measure(StatusMessage) * scale;
        Fill(0, HudHeight + 24, PlayWidth, 48, new Color(0, 0, 0, 160));
        _font.Draw(_batch, new Vector2((PlayWidth - width) / 2f, HudHeight + 30), StatusMessage, C64Palette.Get(15), scale);
    }

    private void DrawConfirmQuit()
    {
        if (!ShowConfirmQuit)
        {
            return;
        }

        var text = "WIRKLICH BEENDEN?";
        var hint = "ENTER = BEENDEN, ESC = ABBRECHEN";
        var width = _font.Measure(text) * 3f;
        var hintWidth = _font.Measure(hint);
        Fill(0, HudHeight + PlayHeight / 3, PlayWidth, 192, new Color(0, 0, 0, 200));
        _font.Draw(_batch, new Vector2((PlayWidth - width) / 2f, HudHeight + PlayHeight / 3 + 36), text, C64Palette.Get(15), 3f);
        _font.Draw(_batch, new Vector2((PlayWidth - hintWidth) / 2f, HudHeight + PlayHeight / 3 + 132), hint, C64Palette.Get(12));
    }

    private void DrawCaveSelect()
    {
        if (!ShowCaveSelect || _caves == null || _caves.Count == 0)
        {
            return;
        }

        const int visible = 10;
        var start = Math.Clamp(CaveSelectIndex - visible / 2, 0, Math.Max(0, _caves.Count - visible));
        const float scale = 1.5f;
        const int rowH = 22;
        const int boxH = visible * rowH + 24;
        var boxX = 20;
        var boxY = HudHeight + 12;
        var boxW = PlayWidth - 40;

        Fill(boxX, boxY, boxW, boxH, new Color(0, 0, 0, 220));
        Fill(boxX, boxY, boxW, 1, C64Palette.Get(11));
        Fill(boxX, boxY + boxH - 1, boxW, 1, C64Palette.Get(11));

        var title = "HOHLE WAHLEN - " + SetName + " (F5 SCHLIESSEN, ENTER SPRINGEN)";
        var titleW = _font.Measure(title) * scale;
        _font.Draw(_batch, new Vector2((PlayWidth - titleW) / 2f, boxY + 4), title, C64Palette.Get(12), scale);

        for (int i = 0; i < visible; i++)
        {
            var index = start + i;
            if (index >= _caves.Count)
            {
                break;
            }

            var cave = _caves[index];
            var prefix = cave.IsIntermission ? "ZWISCHENSPIEL" : "HOHLE " + (index + 1).ToString().PadLeft(2, '0');
            var name = prefix + "  " + cave.Name;
            var rowY = boxY + 26 + i * rowH;
            var selected = index == CaveSelectIndex;

            if (selected)
            {
                Fill(boxX + 4, rowY - 4, boxW - 8, rowH, C64Palette.Get(7));
            }

            var color = selected ? Color.Black : index == CaveNumber - 1 ? C64Palette.Get(3) : C64Palette.Get(15);
            _font.Draw(_batch, new Vector2(boxX + 16, rowY), name, color, scale);
        }
    }

    private void DrawSetSelect()
    {
        if (!ShowSetSelect || Sets == null || Sets.Count == 0)
        {
            return;
        }

        const int visible = 10;
        var start = Math.Clamp(SetSelectIndex - visible / 2, 0, Math.Max(0, Sets.Count - visible));
        const float scale = 1.5f;
        const int rowH = 22;
        const int boxH = visible * rowH + 24;
        var boxX = 20;
        var boxY = HudHeight + 12;
        var boxW = PlayWidth - 40;

        Fill(boxX, boxY, boxW, boxH, new Color(0, 0, 0, 220));
        Fill(boxX, boxY, boxW, 1, C64Palette.Get(11));
        Fill(boxX, boxY + boxH - 1, boxW, 1, C64Palette.Get(11));

        var title = "SATZ WAHLEN (F4 SCHLIESSEN, ENTER WAECHSELN)";
        var titleW = _font.Measure(title) * scale;
        _font.Draw(_batch, new Vector2((PlayWidth - titleW) / 2f, boxY + 4), title, C64Palette.Get(12), scale);

        for (int i = 0; i < visible; i++)
        {
            var index = start + i;
            if (index >= Sets.Count)
            {
                break;
            }

            var set = Sets[index];
            var name = set.Name + "  (" + set.Caves.Count + " HOHLEN)";
            var rowY = boxY + 26 + i * rowH;
            var selected = index == SetSelectIndex;

            if (selected)
            {
                Fill(boxX + 4, rowY - 4, boxW - 8, rowH, C64Palette.Get(7));
            }

            var color = selected ? Color.Black : index == CurrentSetIndex ? C64Palette.Get(3) : C64Palette.Get(15);
            _font.Draw(_batch, new Vector2(boxX + 16, rowY), name, color, scale);
        }
    }

    private void Fill(int x, int y, int width, int height, Color color)
    {
        _batch.Draw(_pixel, new Rectangle(x, y, width, height), color);
    }
}
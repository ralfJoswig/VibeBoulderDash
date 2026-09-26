using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using VibeBoulderDash.Audio;
using VibeBoulderDash.Core;
using VibeBoulderDash.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VibeBoulderDash;

public class BoulderDashGame : Game
{
    private const double BaseTickSeconds = 1.0 / 60.0;
    private const double MinSpeed = 0.2;
    private const double MaxSpeed = 2.0;
    private const double SpeedStep = 0.1;

    private readonly GraphicsDeviceManager _graphics;
    private readonly IReadOnlyList<CaveSet> _caveSets;
    private IReadOnlyList<CaveDefinition> _caves;
    private int _setIndex;
    private int _renderScale;
    private int _windowedScale;
    private bool _fullscreen;
    private KeyboardState _prevKeys;
    private BoulderDashEngine _engine;
    private CaveRenderer _renderer;
    private SoundPlayer _audio;
    private RenderTarget2D _target;
    private SpriteBatch _blit;
    private double _accumulator;
    private double _speed = 0.6;
    private int _caveIndex;
    private bool _awaitingNext;
    private bool _awaitingRestart;
    private bool _paused;
    private bool _pauseHeld;
    private bool _selectCave;
    private bool _selectSet;
    private bool _confirmQuit;
    private bool _backHeld;

    private double TickSeconds => BaseTickSeconds / _speed;

    private static string VersionText
    {
        get
        {
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            var parts = version.Split('.');
            return parts.Length >= 3 ? parts[0] + "." + parts[1] + "." + parts[2] : version;
        }
    }

    public BoulderDashGame()
    {
        _caveSets = CaveSetCatalog.Build();
        _caves = _caveSets[0].Caves;

        var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        var fit = Math.Min((int)(display.Width * 0.9) / CaveRenderer.ScreenWidth, (int)(display.Height * 0.9) / CaveRenderer.ScreenHeight);
        _renderScale = Math.Max(1, fit);
        _windowedScale = _renderScale;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = CaveRenderer.ScreenWidth * _renderScale,
            PreferredBackBufferHeight = CaveRenderer.ScreenHeight * _renderScale,
            SynchronizeWithVerticalRetrace = true,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        Window.Title = "VibeBoulderDash v" + VersionText;
    }

    protected override void LoadContent()
    {
        StartCave(0, resetSession: true);
        _renderer = new CaveRenderer(GraphicsDevice, _engine);
        _renderer.SetCaves(_caves);
        _renderer.SetName = _caveSets[_setIndex].Name;
        _renderer.Sets = _caveSets;
        _renderer.CurrentSetIndex = _setIndex;
        _target = new RenderTarget2D(GraphicsDevice, CaveRenderer.ScreenWidth, CaveRenderer.ScreenHeight);
        _blit = new SpriteBatch(GraphicsDevice);
        _audio = new SoundPlayer();
        _audio.Load();
        ShowStatus("VERSION " + VersionText + "   F4 = NACHSTES SATZ-SET, F5 = HOHLE WAHLEN");
    }

    protected override void Update(GameTime gameTime)
    {
        var keys = Keyboard.GetState();
        var backHeld = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;

        if (keys.IsKeyDown(Keys.Escape) && _selectCave)
        {
            _selectCave = false;
            _renderer.ShowCaveSelect = false;
        }

        if (keys.IsKeyDown(Keys.Escape) && _selectSet)
        {
            _selectSet = false;
            _renderer.ShowSetSelect = false;
        }

        if (_confirmQuit)
        {
            if (JustPressed(keys, Keys.Escape, Keys.N) || (backHeld && !_backHeld))
            {
                _confirmQuit = false;
                _renderer.ShowConfirmQuit = false;
            }
            else if (JustPressed(keys, Keys.Enter, Keys.Space, Keys.Y))
            {
                Exit();
            }

            _backHeld = backHeld;
            _prevKeys = keys;
            base.Update(gameTime);
            return;
        }

        if (JustPressed(keys, Keys.Escape) || (backHeld && !_backHeld))
        {
            _confirmQuit = true;
            _renderer.ShowConfirmQuit = true;
        }

        _backHeld = backHeld;

        var pauseDown = keys.IsKeyDown(Keys.F1) || keys.IsKeyDown(Keys.P);
        if (pauseDown && !_pauseHeld)
        {
            _paused = !_paused;
        }

        _pauseHeld = pauseDown;
        _renderer.Paused = _paused;

        if (JustPressed(keys, Keys.F4) && !_selectCave)
        {
            _selectSet = !_selectSet;
            _renderer.ShowSetSelect = _selectSet;
            if (_selectSet)
            {
                _renderer.SetSelectIndex = _setIndex;
            }
        }

        if (JustPressed(keys, Keys.F5) && !_selectSet)
        {
            _selectCave = !_selectCave;
            _renderer.ShowCaveSelect = _selectCave;
            _renderer.CaveSelectIndex = _caveIndex;
        }

        if (JustPressed(keys, Keys.F2))
        {
            SetSpeed(_speed - SpeedStep);
        }

        if (JustPressed(keys, Keys.F3))
        {
            SetSpeed(_speed + SpeedStep);
        }

        if (JustPressed(keys, Keys.F6))
        {
            SaveGame();
        }

        if (JustPressed(keys, Keys.F7))
        {
            LoadGame();
        }

        _renderer.Speed = _speed;

        if (_selectSet)
        {
            HandleSetSelect(keys);
            _prevKeys = keys;
            base.Update(gameTime);
            return;
        }

        if (_selectCave)
        {
            HandleCaveSelect(keys);
            _prevKeys = keys;
            base.Update(gameTime);
            return;
        }

        if (JustPressed(keys, Keys.F11) ||
            (keys.IsKeyDown(Keys.LeftAlt) && JustPressed(keys, Keys.Enter)) ||
            (keys.IsKeyDown(Keys.RightAlt) && JustPressed(keys, Keys.Enter)))
        {
            SetFullscreen(!_fullscreen);
        }

        if (!_fullscreen)
        {
            if (JustPressed(keys, Keys.OemPlus, Keys.Add))
            {
                ResizeWindow(_renderScale + 1);
            }

            if (JustPressed(keys, Keys.OemMinus, Keys.Subtract))
            {
                ResizeWindow(Math.Max(1, _renderScale - 1));
            }
        }

        _prevKeys = keys;

        if (!_paused)
        {
            AccumulateAndTick(gameTime);
        }

        base.Update(gameTime);
    }

    private void AccumulateAndTick(GameTime gameTime)
    {
        _accumulator += gameTime.ElapsedGameTime.TotalSeconds;
        var events = new List<GameEvent>();
        while (_accumulator >= TickSeconds)
        {
            _accumulator -= TickSeconds;
            var direction = GetDirection();
            if (direction.HasValue && GetFire())
            {
                _engine.TryStationaryAction(direction.Value);
            }
            else
            {
                _engine.Update(direction);
            }

            events.AddRange(_engine.ConsumeEvents());
        }

        _audio.Handle(events);
        foreach (var gameEvent in events)
        {
            if (gameEvent.Type == GameEventType.CaveFillComplete)
            {
                _engine.StageNextCave(_caves[(_caveIndex + 1) % _caves.Count]);
            }

            if (gameEvent.Type == GameEventType.CaveFinished)
            {
                _awaitingNext = false;
                _caveIndex = (_caveIndex + 1) % _caves.Count;
                StartCave(_caveIndex, resetSession: false);
            }

            if (gameEvent.Type == GameEventType.IntermissionFinished)
            {
                _awaitingNext = true;
            }

            if (gameEvent.Type == GameEventType.CaveCompleted && _engine.IsIntermission)
            {
                _awaitingNext = true;
            }

            if (gameEvent.Type == GameEventType.GameOver)
            {
                _awaitingRestart = true;
            }

            if (gameEvent.Type == GameEventType.ExitOpened)
            {
                _renderer.TriggerScreenFlash();
            }

            if (gameEvent.Type == GameEventType.ExtraLife)
            {
                _renderer.TriggerExtraLifeFlash();
            }
        }

        if (_awaitingNext && WasPressed(Keys.Space, Keys.Enter))
        {
            _awaitingNext = false;
            _caveIndex = (_caveIndex + 1) % _caves.Count;
            StartCave(_caveIndex, resetSession: false);
        }

        if (_awaitingRestart && WasPressed(Keys.Space, Keys.Enter))
        {
            _awaitingRestart = false;
            StartCave(0, resetSession: true);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_target);
        GraphicsDevice.Clear(Color.Black);
        _renderer.Draw();
        GraphicsDevice.SetRenderTarget(null);

        GraphicsDevice.Clear(Color.Black);
        var w = GraphicsDevice.Viewport.Width;
        var h = GraphicsDevice.Viewport.Height;
        var scale = Math.Max(1, Math.Min(w / CaveRenderer.ScreenWidth, h / CaveRenderer.ScreenHeight));
        var viewW = CaveRenderer.ScreenWidth * scale;
        var viewH = CaveRenderer.ScreenHeight * scale;
        var dest = new Rectangle((w - viewW) / 2, (h - viewH) / 2, viewW, viewH);
        _blit.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
        _blit.Draw(_target, dest, Color.White);
        _blit.End();
        base.Draw(gameTime);
    }

    private void SetSpeed(double speed)
    {
        _speed = Math.Clamp(speed, MinSpeed, MaxSpeed);
    }

    private void SelectSet(int setIndex)
    {
        _setIndex = (setIndex % _caveSets.Count + _caveSets.Count) % _caveSets.Count;
        if (ReferenceEquals(_caves, _caveSets[_setIndex].Caves))
        {
            return;
        }

        _caves = _caveSets[_setIndex].Caves;
        _renderer.SetCaves(_caves);
        _renderer.SetName = _caveSets[_setIndex].Name;
        _renderer.CurrentSetIndex = _setIndex;
        _renderer.CaveSelectIndex = 0;
        _awaitingNext = false;
        _awaitingRestart = false;
        _accumulator = 0;
        StartCave(0, resetSession: true);
        ShowStatus("SATZ " + (_setIndex + 1) + "/" + _caveSets.Count + ": " + _caveSets[_setIndex].Name);
    }

    private void HandleSetSelect(KeyboardState keys)
    {
        if (JustPressed(keys, Keys.Down))
        {
            _renderer.SetSelectIndex = (_renderer.SetSelectIndex + 1) % _caveSets.Count;
        }
        else if (JustPressed(keys, Keys.Up))
        {
            _renderer.SetSelectIndex = (_renderer.SetSelectIndex + _caveSets.Count - 1) % _caveSets.Count;
        }
        else if (JustPressed(keys, Keys.Enter) || JustPressed(keys, Keys.Space))
        {
            _selectSet = false;
            _renderer.ShowSetSelect = false;
            SelectSet(_renderer.SetSelectIndex);
        }
    }

    private void StartCave(int index, bool resetSession)
    {
        _caveIndex = index;
        if (_engine == null)
        {
            _engine = new BoulderDashEngine(_caves[index]);
        }
        else
        {
            _engine.LoadCave(_caves[index], preserveSession: !resetSession);
        }

        _engine.FlyRule = _caveSets[_setIndex].FlyRule;
        if (_renderer != null)
        {
            _renderer.CaveNumber = index + 1;
            _renderer.CaveCount = _caves.Count;
            _renderer.StartCave();
        }
    }

    private static string SaveFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BoulderDash",
        "save.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    private void SaveGame()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            var save = new SaveFile { CaveIndex = _caveIndex, SetIndex = _setIndex, Engine = _engine.SaveState() };
            File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(save, _jsonOptions));
            ShowStatus("GESPEICHERT");
        }
        catch (Exception)
        {
            ShowStatus("SPEICHERN FEHLGESCHLAGEN");
        }
    }

    private void LoadGame()
    {
        try
        {
            var save = JsonSerializer.Deserialize<SaveFile>(File.ReadAllText(SaveFilePath), _jsonOptions);
            if (save?.Engine == null)
            {
                ShowStatus("KEIN SPIELSTAND");
                return;
            }

            _setIndex = Math.Clamp(save.SetIndex, 0, _caveSets.Count - 1);
            _caves = _caveSets[_setIndex].Caves;
            _renderer.SetCaves(_caves);
            _renderer.SetName = _caveSets[_setIndex].Name;
            _renderer.CurrentSetIndex = _setIndex;
            _caveIndex = Math.Clamp(save.CaveIndex, 0, _caves.Count - 1);
            _engine.LoadState(save.Engine);
            _engine.FlyRule = _caveSets[_setIndex].FlyRule;
            _selectCave = false;
            _renderer.ShowCaveSelect = false;
            _renderer.CaveSelectIndex = _caveIndex;
            _awaitingNext = false;
            _awaitingRestart = false;
            _accumulator = 0;
            _renderer.CaveNumber = _caveIndex + 1;
            _renderer.CaveCount = _caves.Count;
            _renderer.StartCave();
            ShowStatus("GELADEN");
        }
        catch (Exception)
        {
            ShowStatus("KEIN SPIELSTAND");
        }
    }

    private void ShowStatus(string message)
    {
        _renderer.StatusMessage = message;
        _renderer.StatusMessageUntil = Environment.TickCount64 + 2000;
    }

    private void HandleCaveSelect(KeyboardState keys)
    {
        if (JustPressed(keys, Keys.Down))
        {
            _renderer.CaveSelectIndex = (_renderer.CaveSelectIndex + 1) % _caves.Count;
        }
        else if (JustPressed(keys, Keys.Up))
        {
            _renderer.CaveSelectIndex = (_renderer.CaveSelectIndex + _caves.Count - 1) % _caves.Count;
        }
        else if (JustPressed(keys, Keys.Enter) || JustPressed(keys, Keys.Space))
        {
            _selectCave = false;
            _renderer.ShowCaveSelect = false;
            _awaitingNext = false;
            _awaitingRestart = false;
            _accumulator = 0;
            StartCave(_renderer.CaveSelectIndex, resetSession: false);
        }
    }

    private sealed class SaveFile
    {
        public int CaveIndex { get; init; }
        public int SetIndex { get; init; }
        public EngineSnapshot Engine { get; init; }
    }

    private static bool WasPressed(params Keys[] keys)
    {
        var state = Keyboard.GetState();
        foreach (var key in keys)
        {
            if (state.IsKeyDown(key))
            {
                return true;
            }
        }

        return false;
    }

    private bool JustPressed(KeyboardState current, params Keys[] keys)
    {
        foreach (var key in keys)
        {
            if (current.IsKeyDown(key) && !_prevKeys.IsKeyDown(key))
            {
                return true;
            }
        }

        return false;
    }

    private void SetFullscreen(bool fullscreen)
    {
        if (fullscreen == _fullscreen)
        {
            return;
        }

        if (fullscreen)
        {
            _windowedScale = _renderScale;
            var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.HardwareModeSwitch = false;
            _graphics.PreferredBackBufferWidth = mode.Width;
            _graphics.PreferredBackBufferHeight = mode.Height;
            _graphics.ToggleFullScreen();
        }
        else
        {
            _graphics.HardwareModeSwitch = true;
            ResizeWindow(_windowedScale);
            _graphics.ToggleFullScreen();
        }

        _fullscreen = fullscreen;
    }

    private void ResizeWindow(int scale)
    {
        _renderScale = scale;
        _graphics.PreferredBackBufferWidth = CaveRenderer.ScreenWidth * scale;
        _graphics.PreferredBackBufferHeight = CaveRenderer.ScreenHeight * scale;
        _graphics.ApplyChanges();
    }

    private static bool GetFire()
    {
        var state = Keyboard.GetState();
        return state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl) ||
               state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift) ||
               GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed ||
               GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed;
    }

    private static Direction? GetDirection()
    {
        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D))
        {
            return Direction.Right;
        }

        if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
        {
            return Direction.Left;
        }

        if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W))
        {
            return Direction.Up;
        }

        if (state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S))
        {
            return Direction.Down;
        }

        return null;
    }
}
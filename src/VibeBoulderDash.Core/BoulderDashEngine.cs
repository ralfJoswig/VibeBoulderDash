namespace VibeBoulderDash.Core;

/// <summary>Deterministic Boulder Dash engine, modeled after the C64 original.</summary>
public sealed class BoulderDashEngine
{
    /// <summary>Ticks per simulated second (PAL-like).</summary>
    public const int TicksPerSecond = 60;

    /// <summary>Gets or sets how fireflies and butterflies choose between straight and turning movement. Defaults to turn-first.</summary>
    public FlyMovementRule FlyRule { get; set; } = FlyMovementRule.TurnFirst;

    private const int RespawnTicks = 60;
    private const int DeathExplosionTicks = 24;
    private const int DeathExplosionFrameTicks = 6;
    private const int AmoebaConfirmTicks = 8;
    private const int AmoebaSlowGrowthDivisor = 128;
    private const int AmoebaFastGrowthDivisor = 16;
    private const int CompletionBonusTicks = 2 * TicksPerSecond;
    private const int IdleSequenceTicks = 16;
    private const int CaveTransitionFillTicks = 75;

    private CaveDefinition _cave = null!;
    private Tile[] _grid = null!;
    private bool[] _flyScanned = null!;
    private bool[] _dugCells = null!;
    private readonly List<GameEvent> _events = new();
    private Random _random;
    private int _randomSeed;
    private long _randomDraws;

    private GameState _state = null!;
    private Direction _facing = Direction.Up;
    private Direction _rockfordAnimFacing = Direction.Left;
    private bool _rockfordMoving;
private bool _rockfordBlinking;
        private bool _rockfordTapping;
        private bool _rockfordCrushed;
    private bool _rockfordMovedDownThisStep;
    private int _deathExplosionTicks;
    private Cell _rockford;
    private int _tick;
    private long _stepCount;
    private int _stepAccumulator;
    private int _rockfordMoveTimer;
    private int _flyMoveTimer;
    private int _timeCounter;
    private int _respawnTimer;
    private int _magicWallTicksLeft;
    private bool _magicWallExhausted;
    private bool _exitOpenedNotified;
    private bool _completionBonus;
    private long _bonusTicksLeft;
    private int _bonusSecondsLeft;
    private int _bonusStepBase;
    private int _bonusRemainder;
    private int _extraLifeAccumulator;
    private Cell _amoebaOrigin;
    private int _amoebaLifetimeSteps;
    private bool _amoebaWatching;
    private int _amoebaWatchTimer;
    private Tile[] _transitionOriginalGrid = null!;
    private int[] _transitionOrder = null!;
    private int _transitionCovered;
    private int _transitionCellsPerFrame;
    private bool _transitionClearing;

    /// <summary>Creates an engine and loads <paramref name="cave"/>.</summary>
    public BoulderDashEngine(CaveDefinition cave)
    {
        if (cave == null)
        {
            throw new ArgumentNullException(nameof(cave));
        }

        _random = new Random(cave.Seed);
        _randomSeed = cave.Seed;
        _randomDraws = 0;
        LoadCave(cave, preserveSession: false);
    }

    /// <summary>Loads <paramref name="cave"/>. When <paramref name="preserveSession"/> is true the score and lives carry over.</summary>
    public void LoadCave(CaveDefinition cave, bool preserveSession = false)
    {
        _cave = cave ?? throw new ArgumentNullException(nameof(cave));
        if (_grid == null || _grid.Length != cave.Width * cave.Height)
        {
            _grid = new Tile[cave.Width * cave.Height];
        }

        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i] = new Tile(cave.Grid[i]) { Facing = cave.InitialFacing?[i] ?? Direction.Up };
        }

        if (_flyScanned == null || _flyScanned.Length != _grid.Length)
        {
            _flyScanned = new bool[_grid.Length];
        }

        if (_dugCells == null || _dugCells.Length != _grid.Length)
        {
            _dugCells = new bool[_grid.Length];
        }
        else
        {
            Array.Clear(_dugCells, 0, _dugCells.Length);
        }

        if (!preserveSession || _state == null)
        {
            _state = new GameState
            {
                Score = 0,
                Lives = cave.StartLives > 0 ? cave.StartLives : 3,
                CaveIndex = 0,
            };
            _extraLifeAccumulator = 0;
        }

        int diamonds = cave.Grid.Count(e => e == Element.Diamond);
        _state.DiamondsCollected = 0;
        _state.DiamondsNeeded = cave.DiamondsNeeded;
        _state.TimeLeft = cave.TimeSeconds;
        _state.DiamondValue = cave.DiamondValue;
        _state.BonusValue = cave.BonusValue;
        _state.ExitOpen = false;
        _state.CaveCompleted = false;
        _state.GameOver = false;
        _state.IsRespawning = false;

        _rockford = Find(Element.Rockford);
        _facing = Direction.Up;
        _rockfordAnimFacing = Direction.Left;
        _rockfordMoving = false;
        _rockfordBlinking = false;
        _rockfordTapping = false;
        _rockfordCrushed = false;
        _deathExplosionTicks = 0;
        _tick = 0;
        _stepCount = 0;
        _stepAccumulator = 0;
        _rockfordMoveTimer = 0;
        _flyMoveTimer = 0;
        _timeCounter = TicksPerSecond;
        _respawnTimer = 0;
        _magicWallTicksLeft = 0;
        _magicWallExhausted = false;
        _exitOpenedNotified = false;
        _completionBonus = false;
        _bonusTicksLeft = 0;
        _bonusSecondsLeft = 0;
        _bonusStepBase = 0;
        _bonusRemainder = 0;
        _transitionCovered = 0;
        _transitionClearing = false;
        _transitionOriginalGrid = null!;
        _transitionOrder = null!;
        _amoebaLifetimeSteps = 0;
        _amoebaWatching = false;
        _amoebaWatchTimer = 0;
        _amoebaOrigin = Find(Element.Amoeba);
        _events.Add(new GameEvent(GameEventType.CaveStarted, _rockford));
    }

    /// <summary>The cave currently loaded.</summary>
    public CaveDefinition Cave => _cave;

    /// <summary>The current simulation tick.</summary>
    public long Tick => _tick;

    /// <summary>Game steps executed so far (one step per move of every object).
    /// At <see cref="CaveDefinition.GameStepsPerSecond"/> 60 a step equals a tick.</summary>
    public long Steps => _stepCount;

    /// <summary>Current game state.</summary>
    public GameState State => _state;

    /// <summary>Whether the magic wall is currently active (converting tiles).</summary>
    public bool MagicWallActive => _magicWallTicksLeft > 0;

    /// <summary>Current Rockford position.</summary>
    public Cell Rockford => _rockford;

    /// <summary>Rockford facing direction, for the renderer.</summary>
    public Direction RockfordFacing => _facing;

    /// <summary>The last horizontal facing direction, used for the walk animation (left by default).</summary>
    public Direction RockfordAnimFacing => _rockfordAnimFacing;

    /// <summary>True while the player presses a movement direction and Rockford can act.</summary>
    public bool RockfordIsMoving => _rockfordMoving;

    /// <summary>True while Rockford's eyes are closed during the idle animation.</summary>
    public bool RockfordBlinking => _rockfordBlinking;

    /// <summary>True while Rockford taps his foot during the idle animation.</summary>
    public bool RockfordTapping => _rockfordTapping;

    /// <summary>True while Rockford lies flattened under a boulder.</summary>
    public bool RockfordCrushed => _rockfordCrushed;

    /// <summary>Ticks remaining of the death explosion animation after Rockford is crushed.</summary>
    public int DeathExplosionTicksLeft => _deathExplosionTicks;

    /// <summary>Explosion frame index (0..3) for the death explosion animation.</summary>
    public int DeathExplosionFrame => (DeathExplosionTicks - _deathExplosionTicks) / DeathExplosionFrameTicks;

    /// <summary>Width of the cave grid in tiles.</summary>
    public int Width => _cave.Width;

    /// <summary>Height of the cave grid in tiles.</summary>
    public int Height => _cave.Height;

    /// <summary>Returns the tile at the given position.</summary>
    public Tile GetTile(int x, int y) => _grid[y * _cave.Width + x];

    /// <summary>Returns the tile at the given position.</summary>
    public Tile GetTile(Cell cell) => _grid[cell.Y * _cave.Width + cell.X];

    /// <summary>Returns true once Rockford has freed the cell by digging; used to highlight the exposed tunnel network.</summary>
    public bool IsDug(int x, int y) => _dugCells[y * _cave.Width + x];

    /// <summary>Returns all tiles as a row-major array.</summary>
    public Tile[] GetTiles() => (Tile[])_grid.Clone();

    /// <summary>Captures the complete engine state for saving.</summary>
    public EngineSnapshot SaveState() => new()
    {
        Cave = _cave,
        Grid = (Tile[])_grid.Clone(),
        FlyScanned = (bool[])_flyScanned.Clone(),
        DugCells = (bool[])_dugCells.Clone(),
        State = _state.Clone(),
        Facing = _facing,
        FacingAnim = _rockfordAnimFacing,
        RockfordBlinking = _rockfordBlinking,
        RockfordTapping = _rockfordTapping,
        RockfordCrushed = _rockfordCrushed,
        DeathExplosionTicks = _deathExplosionTicks,
        Rockford = _rockford,
        Tick = _tick,
        StepCount = _stepCount,
        StepAccumulator = _stepAccumulator,
        RockfordMoveTimer = _rockfordMoveTimer,
        FlyMoveTimer = _flyMoveTimer,
        TimeCounter = _timeCounter,
        RespawnTimer = _respawnTimer,
        MagicWallTicksLeft = _magicWallTicksLeft,
        MagicWallExhausted = _magicWallExhausted,
        ExitOpenedNotified = _exitOpenedNotified,
        CompletionBonus = _completionBonus,
        BonusTicksLeft = _bonusTicksLeft,
        BonusSecondsLeft = _bonusSecondsLeft,
        BonusStepBase = _bonusStepBase,
        BonusRemainder = _bonusRemainder,
        ExtraLifeAccumulator = _extraLifeAccumulator,
        AmoebaOrigin = _amoebaOrigin,
        AmoebaLifetimeSteps = _amoebaLifetimeSteps,
        AmoebaWatching = _amoebaWatching,
        AmoebaWatchTimer = _amoebaWatchTimer,
        RandomSeed = _randomSeed,
        RandomDraws = _randomDraws,
    };

    /// <summary>Restores a previously captured state so the simulation continues identically.</summary>
    public void LoadState(EngineSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        _cave = snapshot.Cave;
        _grid = (Tile[])snapshot.Grid.Clone();
        _flyScanned = (bool[])snapshot.FlyScanned.Clone();
        _dugCells = (bool[])snapshot.DugCells.Clone();
        _state = snapshot.State.Clone();
        _facing = snapshot.Facing;
        _rockfordAnimFacing = snapshot.FacingAnim;
        _rockfordBlinking = snapshot.RockfordBlinking;
        _rockfordTapping = snapshot.RockfordTapping;
        _rockfordCrushed = snapshot.RockfordCrushed;
        _deathExplosionTicks = snapshot.DeathExplosionTicks;
        _rockford = snapshot.Rockford;
        _tick = (int)snapshot.Tick;
        _stepCount = snapshot.StepCount;
        _stepAccumulator = snapshot.StepAccumulator;
        _rockfordMoveTimer = snapshot.RockfordMoveTimer;
        _flyMoveTimer = snapshot.FlyMoveTimer;
        _timeCounter = snapshot.TimeCounter;
        _respawnTimer = snapshot.RespawnTimer;
        _magicWallTicksLeft = snapshot.MagicWallTicksLeft;
        _magicWallExhausted = snapshot.MagicWallExhausted;
        _exitOpenedNotified = snapshot.ExitOpenedNotified;
        _completionBonus = snapshot.CompletionBonus;
        _bonusTicksLeft = snapshot.BonusTicksLeft;
        _bonusSecondsLeft = snapshot.BonusSecondsLeft;
        _bonusStepBase = snapshot.BonusStepBase;
        _bonusRemainder = snapshot.BonusRemainder;
        _extraLifeAccumulator = snapshot.ExtraLifeAccumulator;
        _amoebaOrigin = snapshot.AmoebaOrigin;
        _amoebaLifetimeSteps = snapshot.AmoebaLifetimeSteps;
        _amoebaWatching = snapshot.AmoebaWatching;
        _amoebaWatchTimer = snapshot.AmoebaWatchTimer;

        _randomSeed = snapshot.RandomSeed;
        _random = new Random(snapshot.RandomSeed);
        for (long i = 0; i < snapshot.RandomDraws; i++)
        {
            _random.Next();
        }

        _randomDraws = snapshot.RandomDraws;
    }

    private int NextRandom(int max)
    {
        _randomDraws++;
        return _random.Next(max);
    }

    private double NextRandomDouble()
    {
        _randomDraws++;
        return _random.NextDouble();
    }

    /// <summary>Advances the simulation by one tick.</summary>
    public void Update(Direction? input)
    {
        _tick++;
        _events.Clear();
        _rockfordMoving = input.HasValue && !_state.GameOver && !_state.IsRespawning && !_state.CaveCompleted;

        if (!_state.GameOver && !_state.CaveCompleted && !_state.IsRespawning)
        {
            if (_rockfordMoving)
            {
                _rockfordBlinking = false;
                _rockfordTapping = false;
            }
            else if (_tick % IdleSequenceTicks == 0)
            {
                var seq = _tick / IdleSequenceTicks;
                _rockfordBlinking = ((seq * 31 + _randomSeed) & 3) == 0;
                if (((seq * 13 + _randomSeed) & 15) == 0)
                {
                    _rockfordTapping = !_rockfordTapping;
                }
            }
        }

        if (_state.GameOver)
        {
            if (_deathExplosionTicks > 0)
            {
                _deathExplosionTicks--;
            }

            return;
        }

        if (_state.CaveCompleted)
        {
            if (_completionBonus)
            {
                BonusCountdownPass();
                SimulateSteps(null);
            }
            else if (!_cave.IsIntermission)
            {
                CaveTransitionPass();
            }

            return;
        }

        if (_state.IsRespawning)
        {
            if (_deathExplosionTicks > 0)
            {
                _deathExplosionTicks--;
            }

            _respawnTimer--;
            if (_respawnTimer <= 0)
            {
                LoadCave(_cave, preserveSession: true);
            }

            return;
        }

        TickWorld(input);
    }

    private void BonusCountdownPass()
    {
        if (_bonusTicksLeft <= 0 || _bonusSecondsLeft <= 0)
        {
            FinishCompletionBonus();
            return;
        }

        _bonusTicksLeft--;
        var step = _bonusStepBase + (_bonusRemainder > 0 ? 1 : 0);
        if (_bonusRemainder > 0)
        {
            _bonusRemainder--;
        }

        if (step <= 0)
        {
            return;
        }

        _bonusSecondsLeft -= step;
        if (_bonusSecondsLeft < 0)
        {
            _bonusSecondsLeft = 0;
        }

        _state.TimeLeft = _bonusSecondsLeft;
        AddScore(step);

        if (_bonusSecondsLeft <= 0)
        {
            FinishCompletionBonus();
        }
    }

    private void FinishCompletionBonus()
    {
        if (!_completionBonus)
        {
            return;
        }

        _completionBonus = false;
        _state.TimeLeft = 0;
        _events.Add(new GameEvent(GameEventType.CaveBonusFinished, _rockford));
    }

    /// <summary>True while the post-completion time bonus is still being counted into the score.</summary>
    public bool IsBonusCountdown => _completionBonus;

    /// <summary>True while the current cave is an intermission.</summary>
    public bool IsIntermission => _cave.IsIntermission;

    private void TickWorld(Direction? input)
    {
        SimulateSteps(input);
        TimerPass();
    }

    private void SimulateSteps(Direction? input)
    {
        _stepAccumulator += _cave.GameStepsPerSecond;
        while (_stepAccumulator >= TicksPerSecond)
        {
            _stepAccumulator -= TicksPerSecond;
            _stepCount++;
            _rockfordMovedDownThisStep = false;
            TryActivateExit();

            if (CanRockfordMove() && input.HasValue)
            {
                TryMoveRockford(input.Value);
            }

            SlimePass();
            GrowingWallPass();
            GravityPass();
            FlyPass();
            AmoebaPass();
            MagicWallPass();
            TryActivateExit();
        }
    }

    private void CaveTransitionPass()
    {
        if (_transitionOrder == null)
        {
            StartCaveTransition();
        }

        var amount = _transitionCellsPerFrame;
        if (_transitionClearing)
        {
            var order = _transitionOrder!;
            if (_transitionOriginalGrid == null)
            {
                return;
            }

            var original = _transitionOriginalGrid;
            amount = Math.Min(amount, _transitionCovered);
            for (var i = 0; i < amount; i++)
            {
                _transitionCovered--;
                var index = order[_transitionCovered];
                _grid[index] = original[index];
            }

            if (_transitionCovered == 0)
            {
                _events.Add(new GameEvent(GameEventType.CaveFinished, _rockford));
            }

            return;
        }

        var order2 = _transitionOrder!;
        var remaining = order2.Length - _transitionCovered;
        amount = Math.Min(amount, remaining);
        for (var i = 0; i < amount; i++)
        {
            var index = order2[_transitionCovered];
            _transitionCovered++;
            _grid[index] = new Tile(Element.TitaniumWall);
        }

        if (_transitionCovered == order2.Length)
        {
            _transitionClearing = true;
            _events.Add(new GameEvent(GameEventType.CaveFillComplete, _rockford));
        }
    }

    private void StartCaveTransition()
    {
        _transitionOriginalGrid = null!;
        var total = _cave.Width * _cave.Height;
        _transitionOrder = new int[total];
        for (var i = 0; i < total; i++)
        {
            _transitionOrder[i] = i;
        }

        for (var i = total - 1; i > 0; i--)
        {
            var j = NextRandom(i + 1);
            (_transitionOrder[i], _transitionOrder[j]) = (_transitionOrder[j], _transitionOrder[i]);
        }

        _transitionCovered = 0;
        _transitionCellsPerFrame = Math.Max(1, (total + CaveTransitionFillTicks - 1) / CaveTransitionFillTicks);
        _transitionClearing = false;
    }

    /// <summary>Stages <paramref name="cave"/> so the collapsing steel-wall effect reveals it instead of the outgoing cave.</summary>
    public void StageNextCave(CaveDefinition cave)
    {
        if (cave == null)
        {
            throw new ArgumentNullException(nameof(cave));
        }

        if (_transitionOriginalGrid != null || _transitionOrder == null)
        {
            throw new InvalidOperationException("A transition must be fully covered before staging the next cave.");
        }

        _transitionOriginalGrid = new Tile[_transitionOrder.Length];
        Array.Fill(_transitionOriginalGrid, new Tile(Element.TitaniumWall));
        var parentWidth = _cave.Width;
        for (int y = 0; y < cave.Height && y < _cave.Height; y++)
        {
            var width = Math.Min(cave.Width, parentWidth);
            for (int x = 0; x < width; x++)
            {
                _transitionOriginalGrid[y * parentWidth + x] = new Tile(cave.Grid[y * cave.Width + x]);
            }
        }
    }

    /// <summary>Consumes and returns all events from the last <see cref="Update"/>.</summary>
    public GameEvent[] ConsumeEvents()
    {
        var result = _events.ToArray();
        _events.Clear();
        return result;
    }

    private void AddScore(int points)
    {
        _state.Score += points;
        _extraLifeAccumulator += points;
        while (_extraLifeAccumulator >= _cave.ExtraLifeEvery)
        {
            _extraLifeAccumulator -= _cave.ExtraLifeEvery;
            _state.Lives++;
            _events.Add(new GameEvent(GameEventType.ExtraLife, _rockford));
        }
    }

    private bool CanRockfordMove()
    {
        if (_rockfordMoveTimer > 0)
        {
            _rockfordMoveTimer--;
            return false;
        }

        _rockfordMoveTimer = _cave.RockfordTicksPerMove - 1;
        return true;
    }

    private bool CanFlyMove()
    {
        if (_flyMoveTimer > 0)
        {
            _flyMoveTimer--;
            return false;
        }

        _flyMoveTimer = _cave.FlyTicksPerMove - 1;
        return true;
    }

    private void TryMoveRockford(Direction input)
    {
        _facing = input;
        if (input is Direction.Left or Direction.Right)
        {
            _rockfordAnimFacing = input;
        }

        var target = input.Offset(_rockford);
        if (!InBounds(target))
        {
            return;
        }

        var targetTile = At(target);
        switch (targetTile.Element)
        {
            case Element.Space:
                MoveRockford(target);
                _events.Add(new GameEvent(GameEventType.Walked, target));
                break;

            case Element.Dirt:
                _dugCells[target.Y * _cave.Width + target.X] = true;
                MoveRockford(target);
                _events.Add(new GameEvent(GameEventType.Dug, target));
                break;

            case Element.Diamond:
                MoveRockford(target);
                _state.DiamondsCollected++;
                AddScore(_state.DiamondValue);
                _events.Add(new GameEvent(GameEventType.DiamondCollected, target));
                break;

            case Element.Exit when _state.ExitOpen:
                MoveRockford(target);
                CompleteCave();
                break;

            case Element.Boulder:
                TryPushFromRockford(target, input);
                break;

            default:
                break;
        }
    }

    /// <summary>Performs the joystick fire-button action in <paramref name="direction"/>
    /// without moving Rockford: digs dirt, pulls a diamond toward him, or pushes a
    /// boulder away from him.</summary>
    public void TryStationaryAction(Direction direction)
    {
        if (_state.GameOver || _state.CaveCompleted || _state.IsRespawning)
        {
            return;
        }

        if (!CanRockfordMove())
        {
            TickWorld(null);
            return;
        }

        _facing = direction;
        var target = direction.Offset(_rockford);
        if (!InBounds(target))
        {
            TickWorld(null);
            return;
        }

        switch (At(target).Element)
        {
            case Element.Dirt:
                _dugCells[target.Y * _cave.Width + target.X] = true;
                Set(target, new Tile(Element.Space));
                _events.Add(new GameEvent(GameEventType.Dug, target));
                break;

            case Element.Diamond:
                Set(target, new Tile(Element.Space));
                _state.DiamondsCollected++;
                AddScore(_state.DiamondValue);
                _events.Add(new GameEvent(GameEventType.DiamondCollected, target));
                break;

            case Element.Boulder:
                PushAwayFromRockford(target, direction);
                break;
        }

        TickWorld(null);
    }

    private void PushAwayFromRockford(Cell elementCell, Direction input)
    {
        if (input is not (Direction.Left or Direction.Right))
        {
            return;
        }

        var beyond = input.Offset(elementCell);
        if (!InBounds(beyond))
        {
            return;
        }

        var beyondElement = At(beyond).Element;
        if (beyondElement != Element.Space)
        {
            return;
        }

        var below = Direction.Down.Offset(beyond);
        var pushed = At(elementCell) with { Falling = InBounds(below) && At(below).Element == Element.Space, Delayed = false };
        Set(elementCell, new Tile(Element.Space));
        Set(beyond, pushed);
        _events.Add(new GameEvent(GameEventType.Pushed, elementCell));
    }

    private void TryPushFromRockford(Cell elementCell, Direction input)
    {
        if (input is not (Direction.Left or Direction.Right))
        {
            return;
        }

        var beyond = input.Offset(elementCell);
        if (!InBounds(beyond))
        {
            return;
        }

        var beyondElement = At(beyond).Element;
        if (beyondElement != Element.Space)
        {
            return;
        }

        var below = Direction.Down.Offset(beyond);
        var pushed = At(elementCell) with { Falling = InBounds(below) && At(below).Element == Element.Space, Delayed = false };
        MoveRockford(elementCell);
        Set(beyond, pushed);
        _events.Add(new GameEvent(GameEventType.Pushed, elementCell));
    }

    private void MoveRockford(Cell target)
    {
        _rockfordMovedDownThisStep = _facing == Direction.Down;
        Set(_rockford, new Tile(Element.Space));
        _rockford = target;
        Set(target, new Tile(Element.Rockford) { Facing = _facing });
    }

    private void SlimePass()
    {
        for (int y = _cave.Height - 1; y >= 0; y--)
        {
            for (int x = _cave.Width - 1; x >= 0; x--)
            {
                if (At(x, y).Element != Element.Slime)
                {
                    continue;
                }

                if (NextRandomDouble() >= _cave.SlimePermeability)
                {
                    continue;
                }

                var above = new Cell(x, y - 1);
                var below = new Cell(x, y + 1);
                if (!InBounds(above) || !InBounds(below))
                {
                    continue;
                }

                var aboveTile = At(above);
                if (!aboveTile.Element.IsGravity() || aboveTile.Falling)
                {
                    continue;
                }

                if (At(below).Element != Element.Space)
                {
                    continue;
                }

                Set(below, new Tile(aboveTile.Element) { Falling = true, Delayed = true });
                SetAt(above, new Tile(Element.Space));
                _events.Add(new GameEvent(GameEventType.SlimePassed, new Cell(x, y)));
            }
        }
    }

    private void GrowingWallPass()
    {
        var walls = new List<Cell>();
        for (int i = 0; i < _grid.Length; i++)
        {
            if (_grid[i].Element == Element.ExpandingWall)
            {
                walls.Add(new Cell(i % _cave.Width, i / _cave.Width));
            }
        }

        foreach (var wall in walls)
        {
            TryGrowExpandingWall(wall, Direction.Left);
            TryGrowExpandingWall(wall, Direction.Right);
        }
    }

    private void TryGrowExpandingWall(Cell wall, Direction direction)
    {
        var target = direction.Offset(wall);
        if (!InBounds(target) || At(target).Element != Element.Space)
        {
            return;
        }

        Set(target, new Tile(Element.ExpandingWall));
        _events.Add(new GameEvent(GameEventType.ExpandingWallGrew, target));
    }

    private void GravityPass()
    {
        for (int y = _cave.Height - 1; y >= 0; y--)
        {
            for (int x = _cave.Width - 1; x >= 0; x--)
            {
                var cell = new Cell(x, y);
                var tile = At(cell);
                if (!tile.Element.IsGravity())
                {
                    continue;
                }

                ProcessGravityElement(cell, tile);
            }
        }
    }

    private void ProcessGravityElement(Cell cell, Tile tile)
    {
        if (tile.Falling)
        {
            if (tile.Delayed)
            {
                Set(cell, tile with { Delayed = false });
                return;
            }

            TryFall(cell, tile);
            return;
        }

        var below = Direction.Down.Offset(cell);
        if (InBounds(below) && At(below).Element == Element.Space)
        {
            Set(cell, tile with { Falling = true, Delayed = true });
            _events.Add(new GameEvent(GameEventType.StartedFalling, cell));
            return;
        }

        if (InBounds(below) && IsRounded(below))
        {
            TryRoll(cell, tile);
        }
    }

    private void TryFall(Cell cell, Tile tile)
    {
        var below = Direction.Down.Offset(cell);
        if (InBounds(below) && At(below).Element == Element.Space)
        {
            if (DefersFollowWhileRockfordDescends(below))
            {
                return;
            }

            Set(below, tile with { Falling = true, Delayed = false });
            Set(cell, new Tile(Element.Space));
            return;
        }

        if (!InBounds(below))
        {
            SetAt(cell, tile with { Falling = false, Delayed = false });
            _events.Add(new GameEvent(GameEventType.Landed, cell));
            return;
        }

        var belowElement = At(below).Element;
        if (belowElement == Element.Firefly)
        {
            ExplodeFly(below);
            return;
        }

        if (belowElement == Element.Butterfly)
        {
            CrushButterfly(below);
            return;
        }

        if (belowElement == Element.Amoeba)
        {
            SetAt(cell, NewGravityTile(tile.Element));
            _events.Add(new GameEvent(GameEventType.Landed, cell));
            return;
        }

        if (belowElement == Element.Rockford)
        {
            if (_state.CaveCompleted)
            {
                SetAt(cell, tile with { Falling = false, Delayed = false });
                _events.Add(new GameEvent(GameEventType.Landed, cell));
                return;
            }

            _rockfordCrushed = true;
            _deathExplosionTicks = DeathExplosionTicks;
            KillRockford();
            SetAt(cell, new Tile(Element.Space));
            SetAt(below, new Tile(Element.Space));
            return;
        }

        if (belowElement == Element.MagicWall)
        {
            HandleMagicWallLanding(cell, tile);
            return;
        }

        if (belowElement.IsSolid())
        {
            if (IsRounded(below) && TryRoll(cell, tile))
            {
                return;
            }

            SetAt(cell, tile with { Falling = false, Delayed = false });
            _events.Add(new GameEvent(GameEventType.Landed, cell));
            return;
        }
    }

    private static Tile NewGravityTile(Element element) =>
        new(element) { Falling = false, Delayed = false };

    private bool DefersFollowWhileRockfordDescends(Cell target)
    {
        return _rockfordMovedDownThisStep &&
               target.X == _rockford.X &&
               target.Y == _rockford.Y - 1;
    }

    private bool IsRounded(Cell cell)
    {
        var tile = At(cell);
        return tile.Element is Element.Wall ||
               tile is { Element: Element.Boulder or Element.Diamond, Falling: false };
    }

    private bool TryRoll(Cell cell, Tile tile)
    {
        var left = Direction.Left.Offset(cell);
        var downLeft = Direction.Down.Offset(left);
        if (RollDirectionClear(left, downLeft))
        {
            Set(left, tile with { Falling = true, Delayed = false });
            SetAt(cell, new Tile(Element.Space));
            return true;
        }

        var right = Direction.Right.Offset(cell);
        var downRight = Direction.Down.Offset(right);
        if (RollDirectionClear(right, downRight))
        {
            Set(right, tile with { Falling = true, Delayed = false });
            SetAt(cell, new Tile(Element.Space));
            return true;
        }

        return false;
    }

    private bool RollDirectionClear(Cell side, Cell downSide)
    {
        if (!InBounds(side) || !InBounds(downSide))
        {
            return false;
        }

        return At(side).Element == Element.Space && At(downSide).Element == Element.Space;
    }

    private void CrushButterfly(Cell cell)
    {
        Explode(cell, Element.Diamond, new HashSet<int> { Index(cell) });
        _events.Add(new GameEvent(GameEventType.ButterflyCrushed, cell));
    }

    private void ExplodeFly(Cell cell)
    {
        Explode(cell, Element.Space, new HashSet<int> { Index(cell) });
        _events.Add(new GameEvent(GameEventType.Explosion, cell));
    }

    private void Explode(Cell cell, Element explosionElement, HashSet<int> done)
    {
        Set(cell, new Tile(explosionElement));
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                ExplodeNeighbor(new Cell(cell.X + dx, cell.Y + dy), explosionElement, done);
            }
        }
    }

    private void ExplodeNeighbor(Cell neighbor, Element explosionElement, HashSet<int> done)
    {
        if (!InBounds(neighbor) || !done.Add(Index(neighbor)))
        {
            return;
        }

        switch (At(neighbor).Element)
        {
            case Element.Rockford:
                KillRockford();
                Set(neighbor, new Tile(Element.Space));
                break;
            case Element.Butterfly:
            case Element.Firefly:
            case Element.Space:
            case Element.Dirt:
            case Element.Wall:
            case Element.MagicWall:
            case Element.Boulder:
            case Element.Diamond:
            case Element.Amoeba:
                Set(neighbor, new Tile(explosionElement));
                break;
        }
    }

    private void FlyPass()
    {
        if (!CanFlyMove())
        {
            return;
        }

        Array.Clear(_flyScanned);

        for (int y = _cave.Height - 1; y >= 0; y--)
        {
            for (int x = _cave.Width - 1; x >= 0; x--)
            {
                var index = y * _cave.Width + x;
                if (_flyScanned[index])
                {
                    continue;
                }

                var tile = _grid[index];
                if (!tile.Element.IsFly())
                {
                    continue;
                }

                ProcessFly(x, y, tile);
            }
        }
    }

    private void ProcessFly(int x, int y, Tile tile)
    {
        var cell = new Cell(x, y);
        if (tile.PendingTurns > 0)
        {
            _grid[y * _cave.Width + x] = tile with { PendingTurns = tile.PendingTurns - 1 };
            MarkScanned(cell);
            return;
        }

        if (FlySeesEnemy(cell))
        {
            if (tile.Element == Element.Firefly)
            {
                ExplodeFly(cell);
            }
            else
            {
                CrushButterfly(cell);
            }

            MarkScanned(cell);
            return;
        }

        var isFirefly = tile.Element == Element.Firefly;
        var preferred = isFirefly ? tile.Facing.TurnLeft() : tile.Facing.TurnRight();
        var preferredCell = preferred.Offset(cell);
        var aheadCell = tile.Facing.Offset(cell);
        bool preferredOpen = InBounds(preferredCell) && At(preferredCell).Element == Element.Space;
        bool aheadOpen = InBounds(aheadCell) && At(aheadCell).Element == Element.Space;

        if (FlyRule == FlyMovementRule.StraightFirst)
        {
            if (aheadOpen)
            {
                MoveFly(cell, aheadCell, tile.Facing, tile);
                return;
            }

            var leftTurn = tile.Facing.TurnLeft();
            var leftCell = leftTurn.Offset(cell);
            bool leftOpen = InBounds(leftCell) && At(leftCell).Element == Element.Space;
            if (leftOpen)
            {
                MoveFly(cell, leftCell, leftTurn, tile);
                return;
            }

            var forced = tile.Facing.TurnRight();
            Set(cell, tile with { Facing = forced, PendingTurns = 1 });
            MarkScanned(cell);
            return;
        }

        if (preferredOpen && aheadOpen && IsEnclosedRingWithExit(cell))
        {
            MoveFly(cell, aheadCell, tile.Facing, tile);
            return;
        }

        if (preferredOpen)
        {
            MoveFly(cell, preferredCell, preferred, tile);
            return;
        }

        if (aheadOpen)
        {
            MoveFly(cell, aheadCell, tile.Facing, tile);
            return;
        }

        var forcedTurn = isFirefly ? tile.Facing.TurnRight() : tile.Facing.TurnLeft();
        Set(cell, tile with { Facing = forcedTurn, PendingTurns = 1 });
        MarkScanned(cell);
    }

    private bool IsEnclosedRingWithExit(Cell cell)
    {
        var dirs = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        for (int i = 0; i < 4; i++)
        {
            var d1 = dirs[i];
            var d2 = dirs[(i + 1) % 4];
            var n1 = d1.Offset(cell);
            var n2 = d2.Offset(cell);
            if (!IsOpenSpace(n1) || !IsOpenSpace(n2))
            {
                continue;
            }

            var diag = new Cell(cell.X + d1.DeltaX() + d2.DeltaX(), cell.Y + d1.DeltaY() + d2.DeltaY());
            if (!IsOpenSpace(diag))
            {
                continue;
            }

            var block = new[] { cell, n1, n2, diag };
            int outerOpen = 0;
            bool outOfBounds = false;
            foreach (var b in block)
            {
                foreach (var d in dirs)
                {
                    var outside = d.Offset(b);
                    if (outside != cell && outside != n1 && outside != n2 && outside != diag)
                    {
                        if (!InBounds(outside))
                        {
                            outOfBounds = true;
                        }
                        else if (At(outside).Element == Element.Space)
                        {
                            outerOpen++;
                        }
                    }
                }
            }

            if (!outOfBounds && outerOpen <= 1)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOpenSpace(Cell cell) => InBounds(cell) && At(cell).Element == Element.Space;

    private bool FlySeesEnemy(Cell cell)
    {
        for (int i = 0; i < 4; i++)
        {
            var direction = (Direction)i;
            var neighbor = direction.Offset(cell);
            if (InBounds(neighbor))
            {
                var element = At(neighbor).Element;
                if (element == Element.Rockford || element == Element.Amoeba)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void MoveFly(Cell from, Cell to, Direction facing, Tile tile)
    {
        Set(from, new Tile(Element.Space));
        Set(to, new Tile(tile.Element) { Facing = facing });
        MarkScanned(to);
    }

    private void MarkScanned(Cell cell) => _flyScanned[cell.Y * _cave.Width + cell.X] = true;

    private void AmoebaPass()
    {
        bool hasAmoeba = false;
        for (int y = _cave.Height - 1; y >= 0; y--)
        {
            for (int x = _cave.Width - 1; x >= 0; x--)
            {
                if (At(x, y).Element == Element.Amoeba)
                {
                    hasAmoeba = true;
                    break;
                }
            }

            if (hasAmoeba)
            {
                break;
            }
        }

        if (!hasAmoeba)
        {
            return;
        }

        int size = Count(Element.Amoeba);
        if (size >= _cave.AmoebaMaxSize)
        {
            _events.Add(new GameEvent(GameEventType.AmoebaConvertedToBoulders, _amoebaOrigin));
            ConvertAmoeba(Element.Boulder);
            return;
        }

        _amoebaLifetimeSteps++;
        var growCells = new List<Cell>();
        for (int y = _cave.Height - 1; y >= 0; y--)
        {
            for (int x = _cave.Width - 1; x >= 0; x--)
            {
                if (At(x, y).Element == Element.Amoeba)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var neighbor = ((Direction)i).Offset(new Cell(x, y));
                        if (InBounds(neighbor) &&
                            At(neighbor).Element is Element.Space or Element.Dirt &&
                            !growCells.Contains(neighbor))
                        {
                            growCells.Add(neighbor);
                        }
                    }
                }
            }
        }

        if (growCells.Count == 0)
        {
            if (_amoebaWatching)
            {
                if (_amoebaWatchTimer <= 0)
                {
                    _events.Add(new GameEvent(GameEventType.AmoebaConvertedToDiamonds, _amoebaOrigin));
                    ConvertAmoeba(Element.Diamond);
                }
                else
                {
                    _amoebaWatchTimer--;
                }
            }
            else
            {
                _amoebaWatching = true;
                _amoebaWatchTimer = AmoebaConfirmTicks;
            }

            return;
        }

        _amoebaWatching = false;
        bool fastGrowth = _amoebaLifetimeSteps >= _cave.AmoebaTime * _cave.GameStepsPerSecond;
        int divisor = fastGrowth ? AmoebaFastGrowthDivisor : AmoebaSlowGrowthDivisor;

        var amoebas = new List<Cell>();
        for (int i = 0; i < _grid.Length; i++)
        {
            if (_grid[i].Element == Element.Amoeba)
            {
                amoebas.Add(new Cell(i % _cave.Width, i / _cave.Width));
            }
        }

        foreach (var cell in amoebas)
        {
            if (NextRandom(divisor) >= 4)
            {
                continue;
            }

            var target = ((Direction)NextRandom(4)).Offset(cell);
            if (InBounds(target) && At(target).Element is Element.Space or Element.Dirt)
            {
                Set(target, new Tile(Element.Amoeba));
                _events.Add(new GameEvent(GameEventType.AmoebaGrew, target));
            }
        }
    }

    private int Count(Element element)
    {
        int count = 0;
        for (int i = 0; i < _grid.Length; i++)
        {
            if (_grid[i].Element == element)
            {
                count++;
            }
        }

        return count;
    }

    private void ConvertAmoeba(Element element)
    {
        for (int i = 0; i < _grid.Length; i++)
        {
            if (_grid[i].Element == Element.Amoeba)
            {
                _grid[i] = new Tile(element);
            }
        }
    }

    private void HandleMagicWallLanding(Cell cell, Tile tile)
    {
        var output = Direction.Down.Offset(Direction.Down.Offset(cell));

        if (!_magicWallExhausted && InBounds(output) && At(output).Element == Element.Space)
        {
            Set(output, new Tile(tile.Element == Element.Boulder ? Element.Diamond : Element.Boulder) { Falling = true, Delayed = false });
            SetAt(cell, new Tile(Element.Space));
            _events.Add(new GameEvent(GameEventType.MagicWallConverted, cell));
        }
        else
        {
            SetAt(cell, new Tile(Element.Space));
            _events.Add(new GameEvent(GameEventType.MagicWallConverted, cell));
        }

        if (!MagicWallActive && !_magicWallExhausted)
        {
            _magicWallTicksLeft = _cave.MagicWallTicks;
            _events.Add(new GameEvent(GameEventType.MagicWallActivated, Direction.Down.Offset(cell)));
        }
    }

    private void MagicWallPass()
    {
        if (_magicWallTicksLeft <= 0)
        {
            return;
        }

        _magicWallTicksLeft--;
        if (_magicWallTicksLeft == 0)
        {
            _magicWallExhausted = true;
            _events.Add(new GameEvent(GameEventType.MagicWallDeactivated, _rockford));
            return;
        }

        ProcessStonesWaitingOnMagicWalls();
    }

    private void ProcessStonesWaitingOnMagicWalls()
    {
        for (int y = 1; y < _cave.Height; y++)
        {
            for (int x = 0; x < _cave.Width; x++)
            {
                if (At(x, y).Element != Element.MagicWall)
                {
                    continue;
                }

                var above = new Cell(x, y - 1);
                var aboveTile = At(above);
                if (aboveTile.Element is Element.Boulder or Element.Diamond && !aboveTile.Falling && aboveTile.Delayed == false)
                {
                    HandleMagicWallLanding(above, aboveTile);
                }
            }
        }
    }

    private void TryActivateExit()
    {
        if (_exitOpenedNotified)
        {
            return;
        }

        if (_state.DiamondsCollected >= _state.DiamondsNeeded && Has(Element.Exit))
        {
            _state.ExitOpen = true;
            _exitOpenedNotified = true;
            _events.Add(new GameEvent(GameEventType.ExitOpened, _rockford));
        }
    }

    private void TimerPass()
    {
        _timeCounter--;
        if (_timeCounter > 0)
        {
            return;
        }

        _timeCounter = TicksPerSecond;
        _state.TimeLeft--;
        if (_state.TimeLeft < 0)
        {
            _events.Add(new GameEvent(GameEventType.OutOfTime, _rockford));
            KillRockford();
        }
    }

    private void CompleteCave()
    {
        AddScore(_state.BonusValue);

        if (!_cave.IsIntermission && _state.TimeLeft > 0)
        {
            _bonusSecondsLeft = _state.TimeLeft;
            _bonusTicksLeft = CompletionBonusTicks;
            _bonusStepBase = _bonusSecondsLeft / CompletionBonusTicks;
            _bonusRemainder = _bonusSecondsLeft % CompletionBonusTicks;
            _completionBonus = true;
        }

        _state.CaveCompleted = true;
        _events.Add(new GameEvent(GameEventType.CaveCompleted, _rockford));
    }

    private void KillRockford()
    {
        if (_state.CaveCompleted)
        {
            return;
        }

        _events.Add(new GameEvent(GameEventType.RockfordDied, _rockford));

        if (_cave.IsIntermission)
        {
            _state.IsRespawning = false;
            _state.GameOver = false;
            _state.CaveCompleted = true;
            _events.Add(new GameEvent(GameEventType.IntermissionFinished, _rockford));
            return;
        }

        _state.Lives--;

        if (_state.Lives <= 0)
        {
            _state.GameOver = true;
            _state.IsRespawning = false;
            _events.Add(new GameEvent(GameEventType.GameOver, _rockford));
            return;
        }

        _state.IsRespawning = true;
        _respawnTimer = RespawnTicks;
    }

    private Tile At(int x, int y) => _grid[y * _cave.Width + x];

    private Tile At(Cell cell) => _grid[cell.Y * _cave.Width + cell.X];

    private int Index(Cell cell) => cell.Y * _cave.Width + cell.X;

    private void SetAt(int x, int y, Tile tile) => _grid[y * _cave.Width + x] = tile;

    private void Set(Cell cell, Tile tile) => _grid[cell.Y * _cave.Width + cell.X] = tile;

    private void SetAt(Cell cell, Tile tile) => _grid[cell.Y * _cave.Width + cell.X] = tile;

    private bool InBounds(Cell cell) =>
        cell.X >= 0 && cell.X < _cave.Width && cell.Y >= 0 && cell.Y < _cave.Height;

    private Cell Find(Element element)
    {
        for (int y = 0; y < _cave.Height; y++)
        {
            for (int x = 0; x < _cave.Width; x++)
            {
                if (_grid[y * _cave.Width + x].Element == element)
                {
                    return new Cell(x, y);
                }
            }
        }

        return new Cell(-1, -1);
    }

    private bool Has(Element element)
    {
        for (int i = 0; i < _grid.Length; i++)
        {
            if (_grid[i].Element == element)
            {
                return true;
            }
        }

        return false;
    }
}
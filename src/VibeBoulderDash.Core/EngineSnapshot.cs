namespace VibeBoulderDash.Core;

/// <summary>A complete, serializable snapshot of the engine state used for save/load.</summary>
public sealed class EngineSnapshot
{
    /// <summary>The cave definition (self-contained so the snapshot can be loaded without a cave list).</summary>
    public required CaveDefinition Cave { get; init; }

    /// <summary>All tiles including transient internal state.</summary>
    public required Tile[] Grid { get; init; }

    /// <summary>Fly pass scan markers.</summary>
    public required bool[] FlyScanned { get; init; }

    /// <summary>Cells dug by Rockford, used for the tunnel highlight.</summary>
    public required bool[] DugCells { get; init; }

    /// <summary>Current mutable game state (score, lives, timers).</summary>
    public required GameState State { get; init; }

    /// <summary>Rockford facing direction.</summary>
    public required Direction Facing { get; init; }

    /// <summary>The last horizontal facing direction, used for the walk animation.</summary>
    public Direction FacingAnim { get; init; }

    /// <summary>Rockford position.</summary>
    public required Cell Rockford { get; init; }

    /// <summary>True while Rockford's eyes are closed during the idle animation.</summary>
    public bool RockfordBlinking { get; init; }

    /// <summary>True while Rockford taps his foot during the idle animation.</summary>
    public bool RockfordTapping { get; init; }

    /// <summary>True while Rockford lies flattened under a boulder.</summary>
    public bool RockfordCrushed { get; init; }

    /// <summary>Ticks remaining of the death explosion animation after Rockford is crushed.</summary>
    public int DeathExplosionTicks { get; init; }

    /// <summary>The current simulation tick.</summary>
    public long Tick { get; init; }

    /// <summary>Steps executed since the cave started.</summary>
    public long StepCount { get; init; }

    /// <summary>Accumulated step fraction.</summary>
    public int StepAccumulator { get; init; }

    /// <summary>Rockford move cooldown timer.</summary>
    public int RockfordMoveTimer { get; init; }

    /// <summary>Fly move cooldown timer.</summary>
    public int FlyMoveTimer { get; init; }

    /// <summary>Countdown to the next time tick.</summary>
    public int TimeCounter { get; init; }

    /// <summary>Respawn sequence timer.</summary>
    public int RespawnTimer { get; init; }

    /// <summary>Ticks the magic wall stays active.</summary>
    public int MagicWallTicksLeft { get; init; }

    /// <summary>True once the magic wall has fully converted its payload.</summary>
    public bool MagicWallExhausted { get; init; }

    /// <summary>True once the cave-completed exit sound was notified.</summary>
    public bool ExitOpenedNotified { get; init; }

    /// <summary>True while the completion bonus counts down.</summary>
    public bool CompletionBonus { get; init; }

    /// <summary>Remaining bonus countdown ticks.</summary>
    public long BonusTicksLeft { get; init; }

    /// <summary>Bonus seconds still counting down.</summary>
    public int BonusSecondsLeft { get; init; }

    /// <summary>Bonus step divider base.</summary>
    public int BonusStepBase { get; init; }

    /// <summary>Remainder of the bonus step division.</summary>
    public int BonusRemainder { get; init; }

    /// <summary>Extra-life accumulator progress.</summary>
    public int ExtraLifeAccumulator { get; init; }

    /// <summary>The amoeba's birth cell.</summary>
    public Cell AmoebaOrigin { get; init; }

    /// <summary>Steps the amoeba has been alive.</summary>
    public int AmoebaLifetimeSteps { get; init; }

    /// <summary>True while the amoeba watches its growth quietly.</summary>
    public bool AmoebaWatching { get; init; }

    /// <summary>Amoeba watch timer ticks.</summary>
    public int AmoebaWatchTimer { get; init; }

    /// <summary>Seed of the deterministic RNG stream.</summary>
    public int RandomSeed { get; init; }

    /// <summary>Number of draws already consumed from the RNG stream.</summary>
    public long RandomDraws { get; init; }
}
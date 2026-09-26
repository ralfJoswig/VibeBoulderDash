namespace VibeBoulderDash.Core;

/// <summary>Engines events for the renderer/audio layer.</summary>
public enum GameEventType
{
    /// <summary>A cave has been (re)started.</summary>
    CaveStarted,

    /// <summary>Rockford collected a diamond.</summary>
    DiamondCollected,

    /// <summary>The exit has opened because enough diamonds were collected.</summary>
    ExitOpened,

    /// <summary>Rockford pushed a boulder.</summary>
    Pushed,

    /// <summary>Rockford stepped onto dirt ("dug").</summary>
    Dug,

    /// <summary>Rockford stepped onto empty space.</summary>
    Walked,

    /// <summary>A boulder or diamond started falling.</summary>
    StartedFalling,

    /// <summary>A falling boulder or diamond has landed.</summary>
    Landed,

    /// <summary>A fly exploded (by boulder, contact or chain).</summary>
    Explosion,

    /// <summary>A butterfly was crushed by a rock and produced diamonds.</summary>
    ButterflyCrushed,

    /// <summary>The magic wall became active.</summary>
    MagicWallActivated,

    /// <summary>The magic wall turned inactive.</summary>
    MagicWallDeactivated,

    /// <summary>The magic wall converted a boulder/diamond.</summary>
    MagicWallConverted,

    /// <summary>Slime let a boulder/diamond fall through it.</summary>
    SlimePassed,

    /// <summary>An expanding wall grew a new piece to the left or right.</summary>
    ExpandingWallGrew,

    /// <summary>Amoeba suffocated and turned into diamonds.</summary>
    AmoebaConvertedToDiamonds,

    /// <summary>Amoeba grew too large and turned into boulders.</summary>
    AmoebaConvertedToBoulders,

    /// <summary>Amoeba cell grew into a new cell.</summary>
    AmoebaGrew,

    /// <summary>Rockford died and a life was lost.</summary>
    RockfordDied,

    /// <summary>An extra life was awarded.</summary>
    ExtraLife,

    /// <summary>The time bonus countdown after completing a cave has finished.</summary>
    CaveBonusFinished,

    /// <summary>Rockford reached the exit and completed the cave.</summary>
    CaveCompleted,

    /// <summary>The cave-complete transition has finished; the next cave should be shown.</summary>
    CaveFinished,

    /// <summary>The steel-wall fill of the cave-complete transition has fully covered the screen.</summary>
    CaveFillComplete,

    /// <summary>An intermission ended without success (e.g. Rockford died).</summary>
    IntermissionFinished,

    /// <summary>The cave timer ran out.</summary>
    OutOfTime,

    /// <summary>The game is over.</summary>
    GameOver,
}

/// <summary>A game event with its originating cell.</summary>
public readonly record struct GameEvent(GameEventType Type, Cell At);
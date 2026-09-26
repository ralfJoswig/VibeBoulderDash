namespace VibeBoulderDash.Core;

/// <summary>Mutable game state exposed to the renderer.</summary>
public sealed class GameState
{
    /// <summary>Current score.</summary>
    public int Score;

    /// <summary>Lives remaining (including the current one).</summary>
    public int Lives;

    /// <summary>Diamonds collected in the current cave.</summary>
    public int DiamondsCollected;

    /// <summary>Diamonds required to open the exit.</summary>
    public int DiamondsNeeded;

    /// <summary>Seconds left in the current cave.</summary>
    public int TimeLeft;

    /// <summary>Score value of one collected diamond.</summary>
    public int DiamondValue;

    /// <summary>Bonus awarded on cave completion (plus remaining time).</summary>
    public int BonusValue;

    /// <summary>Index of the active cave (0-based).</summary>
    public int CaveIndex;

    /// <summary>True while the cave is completed and the game moves on.</summary>
    public bool CaveCompleted;

    /// <summary>True when the player runs out of lives.</summary>
    public bool GameOver;

    /// <summary>True while the death sequence plays out.</summary>
    public bool IsRespawning;

    /// <summary>True when the exit is open (enough diamonds collected).</summary>
    public bool ExitOpen;

    /// <summary>Creates a copy of this state.</summary>
    public GameState Clone() => new()
    {
        Score = Score,
        Lives = Lives,
        DiamondsCollected = DiamondsCollected,
        DiamondsNeeded = DiamondsNeeded,
        TimeLeft = TimeLeft,
        DiamondValue = DiamondValue,
        BonusValue = BonusValue,
        CaveIndex = CaveIndex,
        CaveCompleted = CaveCompleted,
        GameOver = GameOver,
        IsRespawning = IsRespawning,
        ExitOpen = ExitOpen,
    };
}
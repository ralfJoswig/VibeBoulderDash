namespace VibeBoulderDash.Core;

/// <summary>Determines whether moving straight or turning towards the preferred side has priority for fireflies and butterflies.</summary>
public enum FlyMovementRule
{
    /// <summary>Flies turn towards their preferred side first (Boulder Dash 1 behavior).</summary>
    TurnFirst,

    /// <summary>Flies move straight ahead first and turn left when blocked, circulating their chamber counter-clockwise (Boulder Dash 2 behavior).</summary>
    StraightFirst,
}
namespace VibeBoulderDash.Core;

/// <summary>A single grid cell including transient internal state.</summary>
public struct Tile
{
    /// <summary>The semantic element occupying the cell.</summary>
    public Element Element;

    /// <summary>Facing direction for Rockford and the flying creatures.</summary>
    public Direction Facing;

    /// <summary>True while the element waits one tick before acting (e.g. newly falling rock, pausing fly).</summary>
    public bool Delayed;

    /// <summary>True while a gravity element is falling.</summary>
    public bool Falling;

    /// <summary>Ticks a fly waits before re-attempting a turn against its preferred direction.</summary>
    public int PendingTurns;

    /// <summary>Set by the engine while scanning to avoid processing a cell twice in one pass.</summary>
    public bool Scanned;

    /// <summary>Creates a static tile with the given element.</summary>
    public Tile(Element element)
    {
        Element = element;
        Facing = Direction.Up;
        Delayed = false;
        Falling = false;
        PendingTurns = 0;
        Scanned = false;
    }
}
namespace VibeBoulderDash.Core;

/// <summary>Compass direction.</summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

/// <summary>Compass direction helpers.</summary>
public static class DirectionExtensions
{
    /// <summary>Determines the direction after a 90 degree left turn, facing <paramref name="direction"/>.</summary>
    public static Direction TurnLeft(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Left,
        Direction.Left => Direction.Down,
        Direction.Down => Direction.Right,
        Direction.Right => Direction.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>Determines the direction after a 90 degree right turn, facing <paramref name="direction"/>.</summary>
    public static Direction TurnRight(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Right,
        Direction.Right => Direction.Down,
        Direction.Down => Direction.Left,
        Direction.Left => Direction.Up,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>Determines the direction opposite to <paramref name="direction"/>.</summary>
    public static Direction Reverse(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>Horizontal delta (-1, 0 or 1).</summary>
    public static int DeltaX(this Direction direction) => direction switch
    {
        Direction.Left => -1,
        Direction.Right => 1,
        _ => 0,
    };

    /// <summary>Vertical delta (-1, 0 or 1).</summary>
    public static int DeltaY(this Direction direction) => direction switch
    {
        Direction.Up => -1,
        Direction.Down => 1,
        _ => 0,
    };

    /// <summary>Cell adjacent to <paramref name="cell"/> in this direction.</summary>
    public static Cell Offset(this Direction direction, Cell cell) =>
        new(cell.X + direction.DeltaX(), cell.Y + direction.DeltaY());
}
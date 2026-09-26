using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

internal static class TestCave
{
    public static CaveDefinition Make(params string[] rows) =>
        CaveDefinition.Parse(new[] { "speed = 1", "flyspeed = 1", "gamesteps = 60", "grid:" }.Concat(rows).ToArray());

    public static CaveDefinition Make(string[] metadata, params string[] rows) =>
        CaveDefinition.Parse(
            new[] { "speed = 1", "flyspeed = 1", "gamesteps = 60" }
                .Concat(metadata)
                .Append("grid:")
                .Concat(rows)
                .ToArray());

    public static int Count(this BoulderDashEngine engine, Element element)
    {
        int count = 0;
        for (int y = 0; y < engine.Height; y++)
        {
            for (int x = 0; x < engine.Width; x++)
            {
                if (engine.GetTile(x, y).Element == element)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
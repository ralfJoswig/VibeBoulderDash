using System.Text.Json;
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class SaveLoadTests
{
    [Fact]
    public void SaveStateCapturesGridAndLoadStateRestoresIt()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B..",
            "S..",
            "...",
            "...",
            "#.."));

        engine.Update(null);
        var saved = engine.SaveState();

        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Space, engine.GetTile(0, 2).Element);

        engine.LoadState(saved);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);
        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Slime, engine.GetTile(0, 1).Element);
    }

    [Fact]
    public void RestoredEngineContinuesIdenticallyIncludingRngStream()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B..",
            "S..",
            "...",
            "...",
            "#.."));

        for (int i = 0; i < 3; i++)
        {
            engine.Update(null);
        }

        var saved = engine.SaveState();
        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
        }

        var reference = engine.GetTiles();

        engine.LoadState(saved);
        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(reference, engine.GetTiles());
    }

    [Fact]
    public void RestoreKeepsScoreAndLives()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamondvalue = 10" },
            "RD",
            "...",
            "#.."));

        engine.Update(Direction.Right);
        var saved = engine.SaveState();
        Assert.Equal(10, saved.State.Score);

        engine.Update(Direction.Right);
        engine.Update(Direction.Right);

        engine.LoadState(saved);
        Assert.Equal(10, engine.State.Score);
        Assert.Equal(3, engine.State.Lives);
        Assert.Equal(1, engine.State.DiamondsCollected);
    }

    [Fact]
    public void SnapshotRoundTripsThroughJson()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 0.5" },
            "B.D",
            "S..",
            "..#",
            "..#"));

        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
        }

        var saved = engine.SaveState();
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true,
        };

        var json = JsonSerializer.Serialize(saved, options);
        var restored = JsonSerializer.Deserialize<EngineSnapshot>(json, options)!;

        engine.LoadState(restored);
        Assert.Equal(saved.Grid, engine.GetTiles());
        Assert.Equal(saved.State.Score, engine.State.Score);
        Assert.Equal(saved.State.DiamondsCollected, engine.State.DiamondsCollected);
    }
}
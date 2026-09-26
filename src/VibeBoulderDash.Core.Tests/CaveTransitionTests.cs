using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class CaveTransitionTests
{
    [Fact]
    public void RockfordStopsWhileBonusCountsDown()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 60" },
            "B..",
            "RE.",
            "..."));

        engine.Update(Direction.Right);

        Assert.True(engine.State.CaveCompleted);
        Assert.True(engine.IsBonusCountdown);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);

        engine.Update(Direction.Down);

        Assert.True(engine.IsBonusCountdown, "countdown should still run while Rockford stops at the exit");
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.NotEqual(Element.Rockford, engine.GetTile(1, 2).Element);
    }

    [Fact]
    public void GravityContinuesWhileBonusCountsDown()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 60" },
            "B..",
            "RE.",
            "..."));

        engine.Update(Direction.Right);

        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.True(engine.IsBonusCountdown, "countdown should still be active after only a few ticks");
        Assert.NotEqual(Element.Boulder, engine.GetTile(0, 0).Element);
    }

    [Fact]
    public void BoulderCannotCrushRockfordWhileHeStandsInTheExit()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 60" },
            "B",
            ".",
            "E",
            "R"));

        var events = new List<GameEvent>();
        engine.Update(Direction.Up);
        events.AddRange(engine.ConsumeEvents());

        Assert.True(engine.State.CaveCompleted);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 2).Element);

        for (int i = 0; i < 6; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.True(engine.IsBonusCountdown);
        Assert.True(engine.State.CaveCompleted);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 2).Element);
        Assert.False(engine.RockfordCrushed);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 1).Element);
        Assert.DoesNotContain(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void TransitionFillsThenRevealsTheStagedNextCave()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 1" },
            "RE"));

        engine.Update(Direction.Right);

        Assert.True(engine.State.CaveCompleted);

        var events = new List<GameEvent>();
        var sawFillComplete = false;
        for (int i = 0; i < 300 && !sawFillComplete; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
            sawFillComplete |= events.Any(e => e.Type == GameEventType.CaveFillComplete);
        }

        Assert.False(engine.IsBonusCountdown);
        Assert.True(sawFillComplete, "the fill should end with a CaveFillComplete event");
        Assert.Equal(engine.Width * engine.Height, engine.Count(Element.TitaniumWall));

        engine.Update(null);
        engine.ConsumeEvents();
        Assert.True(
            engine.Count(Element.TitaniumWall) == engine.Width * engine.Height,
            "the reverse effect should wait until the next cave is staged");

        var nextCave = TestCave.Make(new[] { "diamonds = 0", "time = 1" }, ".D");
        engine.StageNextCave(nextCave);
        Assert.True(engine.State.CaveCompleted);

        var sawFinished = false;
        for (int i = 0; i < 300 && !sawFinished; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
            sawFinished |= events.Any(e => e.Type == GameEventType.CaveFinished);
        }

        Assert.True(sawFinished, "the transition should end with a CaveFinished event");
        Assert.Equal(0, engine.Count(Element.TitaniumWall));
        Assert.True(engine.Count(Element.Diamond) > 0, "the revealed cave should be the staged next cave");
    }
}
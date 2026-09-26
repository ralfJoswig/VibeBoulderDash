using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class ExitAndTimerTests
{
    [Fact]
    public void ExitOpensAfterEnoughDiamondsAndCompletesCave()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 1" },
            "R.D",
            "...",
            "..E"));

        engine.Update(Direction.Right);
        engine.Update(Direction.Right);
        Assert.True(engine.State.ExitOpen, "Exit should open after the required diamond is collected.");
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.ExitOpened);

        engine.Update(Direction.Down);
        engine.Update(Direction.Down);

        Assert.True(engine.State.CaveCompleted);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.CaveCompleted);
    }

    [Fact]
    public void ClosedExitBlocksRockford()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 99" },
            "R..",
            "E..",
            "..."));

        engine.Update(Direction.Down);
        engine.Update(Direction.Down);

        Assert.False(engine.State.CaveCompleted);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Exit, engine.GetTile(0, 1).Element);
    }

    [Fact]
    public void BonusIsAwardedOnCompletion()
    {
        var cave = CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 60",
                "diamonds = 1",
                "diamondvalue = 25",
                "bonusvalue = 100",
                "time = 60",
                "grid:",
                "RDE",
                "...",
            });
        var engine = new BoulderDashEngine(cave);

        engine.Update(Direction.Right);
        engine.Update(Direction.Right);

        Assert.Equal(25 + 100, engine.State.Score);
        Assert.True(engine.State.CaveCompleted);
        Assert.Equal(Element.Rockford, engine.GetTile(2, 0).Element);
        Assert.True(engine.IsBonusCountdown);

        for (int i = 0; i < BoulderDashEngine.TicksPerSecond * 2; i++)
        {
            engine.Update(null);
        }

        Assert.False(engine.IsBonusCountdown);
        Assert.Equal(25 + 100 + 60, engine.State.Score);
        Assert.Equal(0, engine.State.TimeLeft);
    }

    [Fact]
    public void RunningOutOfTimeKillsRockford()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "time = 0" },
            "R..",
            "..."));

        var events = new List<GameEvent>();
        for (int i = 0; i < BoulderDashEngine.TicksPerSecond + 4; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.True(engine.State.IsRespawning || engine.State.GameOver);
        Assert.Contains(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void LosingLastLifeEndsGame()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "time = 0", "lives = 1" },
            "R..",
            "..."));

        var events = new List<GameEvent>();
        for (int i = 0; i < BoulderDashEngine.TicksPerSecond + 4; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.True(engine.State.GameOver);
        Assert.Contains(events, e => e.Type == GameEventType.GameOver);
    }

    [Fact]
    public void RunningOutOfTimeRespawnsWithTheCaveReset()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "time = 1" },
            "RB.",
            ".#."));

        for (int i = 0; i < BoulderDashEngine.TicksPerSecond * 3 + 20; i++)
        {
            engine.Update(null);
        }

        Assert.False(engine.State.IsRespawning);
        Assert.Equal(2, engine.State.Lives);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 0).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
    }
}
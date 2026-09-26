using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class IntermissionTests
{
    [Fact]
    public void IntermissionTimerCountsDown()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 60",
                "intermission = true",
                "time = 5",
                "grid:",
                "R..",
                "...",
                "...",
            }));

        for (int i = 0; i < 400; i++)
        {
            engine.Update(null);
        }

        Assert.True(engine.State.TimeLeft < 5);
        Assert.False(engine.State.GameOver);
    }

    [Fact]
    public void RockfordDoesNotLoseLifeWhenHeDiesInIntermission()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 60",
                "intermission = true",
                "lives = 1",
                "time = 1",
                "grid:",
                "...",
                ".R.",
                "...",
            }));

        int livesBefore = engine.State.Lives;

        var died = false;
        for (int i = 0; i < 300 && !died; i++)
        {
            engine.Update(null);
            died = engine.State.CaveCompleted;
        }

        Assert.True(died, "Rockford should run out of time.");
        Assert.Equal(livesBefore, engine.State.Lives);
        Assert.False(engine.State.GameOver);
        Assert.True(engine.State.CaveCompleted);
    }

    [Fact]
    public void IntermissionCompletesWithoutDiamondsAndScoresBonusOnly()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 60",
                "intermission = true",
                "diamonds = 0",
                "bonusvalue = 100",
                "time = 5",
                "grid:",
                "RE.",
                "...",
                "...",
            }));

        engine.Update(null);
        for (int i = 0; i < 4 && !engine.State.ExitOpen; i++)
        {
            engine.Update(null);
        }

        Assert.True(engine.State.ExitOpen, "Intermission exits should open without collecting diamonds.");

        for (int i = 0; i < 10 && !engine.State.CaveCompleted; i++)
        {
            engine.Update(Direction.Right);
        }

        Assert.True(engine.State.CaveCompleted);
        Assert.Equal(100, engine.State.Score);
    }

    [Fact]
    public void IntermissionDeathDoesNotReportCaveCompleted()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 60",
                "intermission = true",
                "bonusvalue = 100",
                "time = 60",
                "grid:",
                "RF.",
                "...",
                "...",
            }));

        for (int i = 0; i < 3 && !engine.State.CaveCompleted; i++)
        {
            engine.Update(null);
        }

        Assert.True(engine.State.CaveCompleted);
        var events = engine.ConsumeEvents();
        Assert.DoesNotContain(events, e => e.Type == GameEventType.CaveCompleted);
        Assert.Contains(events, e => e.Type == GameEventType.IntermissionFinished);
        Assert.Equal(0, engine.State.Score);
    }
}

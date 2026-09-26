using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class ScoreTests
{
    [Fact]
    public void CaveCompletionBonusGrantsExtraLivesBasedOnTotalScore()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 1", "extralifeevery = 50" },
            "..",
            "RE"));

        var livesBefore = engine.State.Lives;
        engine.Update(Direction.Right);

        Assert.True(engine.State.CaveCompleted);
        Assert.True(engine.State.Score > 0);
        Assert.True(engine.State.Lives > livesBefore);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.ExtraLife);
    }

    [Fact]
    public void ExtraLifeThresholdCarriesAcrossCaves()
    {
        var caveA = TestCave.Make(new[] { "diamonds = 0", "time = 60", "diamondvalue = 100", "extralifeevery = 500" },
            "RDDD");
        var engine = new BoulderDashEngine(caveA);

        for (int i = 0; i < 3; i++)
        {
            engine.Update(Direction.Right);
            Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.ExtraLife);
        }

        Assert.Equal(300, engine.State.Score);
        var livesBefore = engine.State.Lives;

        var caveB = TestCave.Make(new[] { "diamonds = 0", "time = 60", "diamondvalue = 100", "extralifeevery = 500" },
            "RDD");
        engine.LoadCave(caveB, preserveSession: true);
        engine.ConsumeEvents();

        engine.Update(Direction.Right);
        Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.ExtraLife);

        engine.Update(Direction.Right);

        Assert.Equal(500, engine.State.Score);
        Assert.Equal(livesBefore + 1, engine.State.Lives);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.ExtraLife);
    }

    [Fact]
    public void CaveCompletionPaysOnePointPerRemainingSecond()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 0", "time = 3", "bonusvalue = 200", "extralifeevery = 1000" },
            "..",
            "RE"));

        engine.Update(Direction.Right);

        Assert.True(engine.State.CaveCompleted);
        Assert.True(engine.IsBonusCountdown);
        Assert.Equal(200, engine.State.Score);

        var events = new List<GameEvent>();
        for (int i = 0; i < BoulderDashEngine.TicksPerSecond * 2; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.False(engine.IsBonusCountdown);
        Assert.Equal(203, engine.State.Score);
        Assert.Equal(0, engine.State.TimeLeft);
        Assert.Contains(events, e => e.Type == GameEventType.CaveBonusFinished);
    }
}
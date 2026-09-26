using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class SlimePassTests
{
    [Fact]
    public void BoulderFallsThroughPermeableSlimeIntoTheSpaceBelow()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B..",
            "S..",
            "...",
            "...",
            "#.."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Slime, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Space, engine.GetTile(0, 2).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
    }

    [Fact]
    public void DiamondFallsThroughPermeableSlime()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "D..",
            "S..",
            "...",
            "#.."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Slime, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Diamond, engine.GetTile(0, 2).Element);
    }

    [Fact]
    public void ImpermeableSlimeNeverLetsAnythingFallThrough()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 0" },
            "B..",
            "S..",
            "...",
            "#.."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 30; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Slime, engine.GetTile(0, 1).Element);
        Assert.DoesNotContain(events, e => e.Type == GameEventType.SlimePassed);
    }

    [Fact]
    public void SlimeWithFloorBelowDoesNotLetBoulderFallThrough()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B.",
            "S.",
            "#."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Slime, engine.GetTile(0, 1).Element);
    }

    [Fact]
    public void BoulderThatLandsOnSlimePassesThroughOnALaterStep()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B",
            ".",
            "S",
            ".",
            "#"));

        var events = new List<GameEvent>();
        for (int i = 0; i < 30; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Slime, engine.GetTile(0, 2).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
        Assert.Contains(events, e => e.Type == GameEventType.SlimePassed);
    }

    [Fact]
    public void PassingBoulderEmitsSlimePassedEvent()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "slimepermeability = 1" },
            "B.",
            "S.",
            "..",
            "#."));

        engine.Update(null);

        var events = engine.ConsumeEvents();
        Assert.Contains(events, e => e.Type == GameEventType.SlimePassed);
    }
}
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class FireButtonTests
{
    [Fact]
    public void DigsDirtWithoutMovingRockford()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "...",
            "#R#",
            "..."));

        engine.TryStationaryAction(Direction.Right);

        Assert.Equal(Element.Space, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.Dug);
    }

    [Fact]
    public void PullsDiamondTowardRockfordWithoutMoving()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "...",
            "#RD",
            "..."));

        engine.TryStationaryAction(Direction.Right);

        Assert.Equal(Element.Space, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.Equal(1, engine.State.DiamondsCollected);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.DiamondCollected);
    }

    [Fact]
    public void PushesBoulderAwayFromRockfordWithoutMoving()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "....",
            "#RB.",
            "...."));

        engine.TryStationaryAction(Direction.Right);

        Assert.Equal(Element.Boulder, engine.GetTile(3, 2).Element);
        Assert.Equal(Element.Space, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.Pushed);
    }

    [Fact]
    public void PushedBoulderFallsWhileFireButtonHeld()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "....",
            "#RB.",
            "...."));

        engine.TryStationaryAction(Direction.Right);
        engine.TryStationaryAction(Direction.Right);
        engine.TryStationaryAction(Direction.Right);

        Assert.Equal(Element.Boulder, engine.GetTile(3, 2).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
    }

    [Fact]
    public void WalksThroughDirtOnlyWhenFireIsNotUsed()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "...",
            "#R#",
            "..."));

        engine.Update(Direction.Right);

        Assert.Equal(Element.Rockford, engine.GetTile(2, 1).Element);
    }
}

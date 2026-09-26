using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class RockfordMovementTests
{
    [Fact]
    public void MovesIntoEmptySpace()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "R...",
            "...."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);
        Assert.Equal(Direction.Right, engine.RockfordFacing);
    }

    [Fact]
    public void WalksThroughDirtAndBecomesDirt()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "R#",
            ".."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);

        engine.Update(Direction.Left);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(1, 0).Element);
    }

    [Fact]
    public void CollectsDiamondAndScores()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "diamonds = 1", "diamondvalue = 25" },
            "RD",
            ".."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);
        Assert.Equal(1, engine.State.DiamondsCollected);
        Assert.Equal(25, engine.State.Score);

        var events = engine.ConsumeEvents();
        Assert.Contains(events, e => e.Type == GameEventType.DiamondCollected);
    }

    [Fact]
    public void CannotMoveThroughWallOrTitanium()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "RWT",
            "..."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
    }

    [Fact]
    public void CannotDigThroughAmoeba()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "RA",
            ".."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Amoeba, engine.GetTile(1, 0).Element);
    }

    [Fact]
    public void MovesUpAndDown()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "R.",
            ".."));

        engine.Update(Direction.Down);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 1).Element);
        engine.Update(Direction.Up);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
    }
}
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class GrowingWallTests
{
    [Fact]
    public void GrowsIntoBothEmptySidesInOneStep()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".X."));

        engine.Update(null);

        Assert.Equal(Element.ExpandingWall, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(1, 0).Element);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(2, 0).Element);
    }

    [Fact]
    public void NewPiecesDoNotGrowUntilTheNextStep()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "....X...."));

        engine.Update(null);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(4, 0).Element);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(5, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(6, 0).Element);

        engine.Update(null);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(6, 0).Element);
    }

    [Fact]
    public void DoesNotGrowIntoDirtOrWall()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#XT"));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Dirt, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(1, 0).Element);
        Assert.Equal(Element.TitaniumWall, engine.GetTile(2, 0).Element);
    }

    [Fact]
    public void DoesNotGrowVertically()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".",
            "X",
            "."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Space, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.ExpandingWall, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Space, engine.GetTile(0, 2).Element);
    }

    [Fact]
    public void DoesNotGrowIntoOccupiedCells()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "XR."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.ExpandingWall, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);
    }

    [Fact]
    public void EmitsGrowingWallGrewEventForEachNewPiece()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".X."));

        engine.Update(null);

        var events = engine.ConsumeEvents();
        Assert.Equal(2, events.Count(e => e.Type == GameEventType.ExpandingWallGrew));
    }
}
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class AmoebaTests
{
    [Fact]
    public void ConfinedAmoebaTurnsIntoLotsOfDiamonds()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "amoebamaxsize = 200" },
            "WWW",
            "WAW",
            "WWW"));

        var sawConversion = false;
        for (int i = 0; i < 60; i++)
        {
            engine.Update(null);
            if (engine.ConsumeEvents().Any(e => e.Type == GameEventType.AmoebaConvertedToDiamonds))
            {
                sawConversion = true;
                break;
            }
        }

        Assert.True(sawConversion);
        Assert.Equal(Element.Diamond, engine.GetTile(1, 1).Element);
        Assert.Equal(0, engine.Count(Element.Amoeba));
    }

    [Fact]
    public void OversizedAmoebaTurnsIntoLotsOfBoulders()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "amoebamaxsize = 4" },
            "AAAA",
            "...."));

        engine.Update(null);

        Assert.Equal(4, engine.Count(Element.Boulder));
        Assert.Equal(0, engine.Count(Element.Amoeba));
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.AmoebaConvertedToBoulders);
    }

    [Fact]
    public void FireflyExplodesNextToAmoeba()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "AF",
            ".."));

        engine.Update(null);

        Assert.Equal(0, engine.Count(Element.Firefly));
    }

    [Fact]
    public void AmoebaGrowsIntoSurroundingDirtAndSpace()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "amoebatime = 0" },
            "TTTTT",
            "T#A.T",
            "T##.T",
            "TTTTT"));

        var events = new List<GameEvent>();
        var grew = false;
        for (int i = 0; i < 40 && !grew; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
            grew = engine.Count(Element.Amoeba) > 1;
        }

        Assert.True(grew);
        Assert.Contains(events, e => e.Type == GameEventType.AmoebaGrew);
        Assert.Equal(Element.TitaniumWall, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.TitaniumWall, engine.GetTile(3, 0).Element);
        Assert.Equal(Element.TitaniumWall, engine.GetTile(4, 2).Element);
    }

    [Fact]
    public void AmoebaGrowsFasterAfterAmoebaTimeElapses()
    {
        var slow = GrowForSteps("amoebatime = 10000", 12345);
        var fast = GrowForSteps("amoebatime = 0", 12345);

        Assert.True(fast > slow, $"Fast amoeba should outgrow slow before AmoebaTime (slow={slow}, fast={fast}).");
    }

    private static int GrowForSteps(string amoebaTimeLine, int seed)
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { amoebaTimeLine, "seed = " + seed, "amoebamaxsize = 2000" },
            "TTTTTTTTT",
            "T#######T",
            "T#######T",
            "T###A###T",
            "T#######T",
            "T#######T",
            "T#######T",
            "T#######T",
            "TTTTTTTTT"));

        for (int i = 0; i < 30; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }

        return engine.Count(Element.Amoeba);
    }

    [Fact]
    public void AmeobaStopsFallingStoneWithoutTurningIntoDiamonds()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "seed = 12345", "amoebamaxsize = 2000" },
            "B...",
            "....",
            "....",
            "A...",
            "...."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 30; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.DoesNotContain(events, e => e.Type == GameEventType.AmoebaConvertedToDiamonds);
        Assert.DoesNotContain(events, e => e.Type == GameEventType.Explosion);
        Assert.True(engine.Count(Element.Boulder) > 0, "The stone should remain resting on the amoeba.");
        Assert.Contains(engine.GetTiles(), t => t.Element == Element.Amoeba);
    }
}
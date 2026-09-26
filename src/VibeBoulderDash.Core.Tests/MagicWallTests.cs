using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class MagicWallTests
{
    [Fact]
    public void FallingBoulderActivatesWallAndComesOutAsDiamond()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 30" },
            ".B..",
            "....",
            ".M..",
            "....",
            "...."));

        var sawActivation = false;
        var sawConversion = false;
        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
            foreach (var gameEvent in engine.ConsumeEvents())
            {
                sawActivation |= gameEvent.Type == GameEventType.MagicWallActivated;
                sawConversion |= gameEvent.Type == GameEventType.MagicWallConverted;
            }
        }

        Assert.True(sawActivation);
        Assert.True(sawConversion);
        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(1, engine.Count(Element.Diamond));
        Assert.Equal(Element.Diamond, engine.GetTile(1, 4).Element);
    }

    [Fact]
    public void BoulderFallingOnWallWithSolidCellBelowIsLostInsteadOfResting()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".B...",
            ".....",
            ".....",
            ".M...",
            ".#...",
            "....."));

        for (int i = 0; i < 6; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }

        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(0, engine.Count(Element.Diamond));
        Assert.Equal(Element.MagicWall, engine.GetTile(1, 3).Element);
        Assert.Equal(Element.Dirt, engine.GetTile(1, 4).Element);
    }

    [Fact]
    public void MagicWallDeactivatesAfterItsTime()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 10" },
            ".B..",
            "....",
            ".M..",
            "....",
            "...."));

        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }

        var sawDeactivation = false;
        for (int i = 0; i < 15; i++)
        {
            engine.Update(null);
            if (engine.ConsumeEvents().Any(e => e.Type == GameEventType.MagicWallDeactivated))
            {
                sawDeactivation = true;
                break;
            }
        }

        Assert.True(sawDeactivation);
    }

    [Fact]
    public void MagicWallActiveReflectsActivationAndDeactivation()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 10" },
            ".B..",
            "....",
            ".M..",
            "....",
            "...."));

        Assert.False(engine.MagicWallActive);

        var sawActivation = false;
        for (int i = 0; i < 10 && !sawActivation; i++)
        {
            engine.Update(null);
            sawActivation = engine.ConsumeEvents().Any(e => e.Type == GameEventType.MagicWallActivated);
        }

        Assert.True(sawActivation);
        Assert.True(engine.MagicWallActive);

        var sawDeactivation = false;
        for (int i = 0; i < 20 && !sawDeactivation; i++)
        {
            engine.Update(null);
            sawDeactivation = engine.ConsumeEvents().Any(e => e.Type == GameEventType.MagicWallDeactivated);
        }

        Assert.True(sawDeactivation);
        Assert.False(engine.MagicWallActive);
    }

    [Fact]
    public void DiamondFallingIntoActiveWallBecomesBoulder()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 30" },
            ".D..",
            "....",
            ".M..",
            "....",
            "...."));

        var sawConversion = false;
        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
            if (engine.ConsumeEvents().Any(e => e.Type == GameEventType.MagicWallConverted))
            {
                sawConversion = true;
            }
        }

        Assert.True(sawConversion);
        Assert.Equal(1, engine.Count(Element.Boulder));
        Assert.Equal(Element.Boulder, engine.GetTile(1, 4).Element);
    }

    [Fact]
    public void ExhaustedMagicWallStopsConvertingAndIsNotReactivated()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 1" },
            ".B..",
            "....",
            ".B..",
            "....",
            ".M..",
            "....",
            "...."));

        var activated = 0;
        var deactivated = 0;
        for (int i = 0; i < 200; i++)
        {
            engine.Update(null);
            foreach (var gameEvent in engine.ConsumeEvents())
            {
                if (gameEvent.Type == GameEventType.MagicWallActivated)
                {
                    activated++;
                }
                else if (gameEvent.Type == GameEventType.MagicWallDeactivated)
                {
                    deactivated++;
                }
            }
        }

        Assert.Equal(1, activated);
        Assert.True(deactivated >= 1);
        Assert.False(engine.MagicWallActive);
        Assert.Equal(Element.MagicWall, engine.GetTile(1, 4).Element);
        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(1, engine.Count(Element.Diamond));
    }

    [Fact]
    public void TwoBouldersFallingOnActiveWallBothBecomeDiamonds()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "magicwallticks = 40" },
            "B....",
            "B....",
            ".....",
            "M....",
            ".....",
            ".....",
            "....."));

        var conversions = 0;
        for (int i = 0; i < 40; i++)
        {
            engine.Update(null);
            conversions += engine.ConsumeEvents().Count(e => e.Type == GameEventType.MagicWallConverted);
        }

        Assert.Equal(2, conversions);
        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(2, engine.Count(Element.Diamond));
    }
}
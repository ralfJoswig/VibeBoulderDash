using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class FlyTests
{
    [Fact]
    public void FireflyCirclesChamberPrefersLeftTurns()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "TTTTT",
            "T...T",
            "T.F.T",
            "T...T",
            "TTTTT"));

        var positions = new List<Cell>();
        for (int i = 0; i < 4; i++)
        {
            engine.Update(null);
            positions.Add(Find(engine, Element.Firefly));
        }

        Assert.Equal(new Cell(1, 2), positions[0]);
        Assert.Equal(new Cell(1, 3), positions[1]);
        Assert.Equal(new Cell(2, 3), positions[2]);
        Assert.Equal(new Cell(2, 2), positions[3]);
    }

    [Fact]
    public void ButterflyCirclesChamberPrefersRightTurns()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "TTTTT",
            "T...T",
            "T.Y.T",
            "T...T",
            "TTTTT"));

        var positions = new List<Cell>();
        for (int i = 0; i < 4; i++)
        {
            engine.Update(null);
            positions.Add(Find(engine, Element.Butterfly));
        }

        Assert.Equal(new Cell(3, 2), positions[0]);
        Assert.Equal(new Cell(3, 3), positions[1]);
        Assert.Equal(new Cell(2, 3), positions[2]);
        Assert.Equal(new Cell(2, 2), positions[3]);
    }

    [Fact]
    public void FireflyExplodesOnContactWithRockford()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "R.",
            "F.",
            ".."));

        engine.Update(null);

        Assert.True(engine.State.IsRespawning);
        Assert.Equal(2, engine.State.Lives);

        var events = engine.ConsumeEvents();
        Assert.Contains(events, e => e.Type == GameEventType.Explosion);
        Assert.Contains(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void FireButtonPushedBoulderAgainstFireflyIsBlocked()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "####",
            "#RBF",
            "####"));

        engine.TryStationaryAction(Direction.Right);

        Assert.Equal(Element.Boulder, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Firefly, engine.GetTile(3, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.Explosion);
    }

    [Fact]
    public void WalkPushedBoulderAgainstFireflyIsBlocked()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "####",
            "FBR",
            "####"));

        engine.Update(Direction.Left);

        Assert.Equal(Element.Firefly, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(2, 1).Element);
        Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.Explosion);
    }

    [Fact]
    public void PushedBoulderPastFireflyInGapDoesNotExplode()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "......",
            "..B.BR",
            "...F..",
            "......"));

        engine.TryStationaryAction(Direction.Left);

        Assert.Equal(Element.Boulder, engine.GetTile(3, 1).Element);
        Assert.Equal(1, engine.Count(Element.Firefly));
        Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.Explosion);
    }

    [Fact]
    public void WalkPushedBoulderPastFireflyInGapDoesNotExplode()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "......",
            "..B.BR",
            "...F..",
            "......"));

        engine.Update(Direction.Left);

        Assert.Equal(Element.Boulder, engine.GetTile(3, 1).Element);
        Assert.Equal(1, engine.Count(Element.Firefly));
        Assert.DoesNotContain(engine.ConsumeEvents(), e => e.Type == GameEventType.Explosion);
    }

    [Fact]
    public void PushingBoulderAtFireflyNeverExplodesOverTime()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "####",
            "#RBF",
            "####"));

        var explosions = 0;
        for (int i = 0; i < 100; i++)
        {
            if (i % 2 == 0)
            {
                engine.Update(Direction.Right);
            }
            else
            {
                engine.Update(null);
            }

            explosions += engine.ConsumeEvents().Count(e => e.Type == GameEventType.Explosion);
        }

        Assert.Equal(0, explosions);
        Assert.Equal(Element.Firefly, engine.GetTile(3, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
    }

    [Fact]
    public void FallingBoulderOnFireflyStillExplodes()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            "F",
            "."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.Contains(events, e => e.Type == GameEventType.Explosion);
        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(0, engine.Count(Element.Firefly));
    }

    [Fact]
    public void FireflyExplosionDestroysBrickWalls()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "FR",
            "W.",
            ".."));

        engine.Update(null);

        Assert.Equal(Element.Space, engine.GetTile(0, 1).Element);
        Assert.True(engine.State.IsRespawning);
    }

    [Fact]
    public void FireflyExplosionDoesNotChainThroughOtherFireflies()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".WFW",
            ".FAW",
            "....",
            "...."));

        engine.Update(null);

        Assert.Equal(0, engine.Count(Element.Firefly));
        Assert.Equal(0, engine.Count(Element.Amoeba));
        Assert.Equal(Element.Space, engine.GetTile(2, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(1, 1).Element);
        Assert.Equal(Element.Wall, engine.GetTile(3, 0).Element);
        Assert.Equal(Element.Wall, engine.GetTile(3, 1).Element);
    }

    [Fact]
    public void ButterflyExplosionDoesNotChainThroughOtherButterflies()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".WYW",
            ".YAW",
            "....",
            "...."));

        engine.Update(null);

        Assert.Equal(0, engine.Count(Element.Butterfly));
        Assert.Equal(Element.Diamond, engine.GetTile(2, 0).Element);
        Assert.Equal(Element.Diamond, engine.GetTile(1, 1).Element);
        Assert.Equal(Element.Wall, engine.GetTile(3, 0).Element);
        Assert.Equal(Element.Wall, engine.GetTile(3, 1).Element);
    }

    [Fact]
    public void ButterfliesTurnIntoLotsOfDiamondsWhenCrushedByRock()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "...",
            ".B.",
            "...",
            ".Y.",
            "R..",
            "...",
            "..."));

        for (int i = 0; i < 14; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(0, engine.Count(Element.Butterfly));
        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.True(engine.Count(Element.Diamond) >= 4);
    }

    [Fact]
    public void ButterflyContactWithRockfordKillsHim()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "R.",
            "Y.",
            ".."));

        engine.Update(null);

        Assert.True(engine.State.IsRespawning);
        Assert.Contains(engine.ConsumeEvents(), e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void CrushingButterflyTurnsBoulderAndEarthIntoDiamonds()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "...",
            ".B.",
            "...",
            "#Y.",
            "R..",
            ".ET",
            "..."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.Equal(0, engine.Count(Element.Boulder));
        Assert.Equal(0, engine.Count(Element.Butterfly));
        Assert.Equal(Element.Diamond, engine.GetTile(1, 3).Element);
        Assert.Equal(Element.Diamond, engine.GetTile(0, 3).Element);
        Assert.Equal(Element.Exit, engine.GetTile(1, 5).Element);
        Assert.Equal(Element.TitaniumWall, engine.GetTile(2, 5).Element);
        Assert.True(engine.State.IsRespawning);
        Assert.Contains(events, e => e.Type == GameEventType.ButterflyCrushed);
        Assert.Contains(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void PushingBoulderAtFireflyInOpenChamberNeverExplodes()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "......",
            ".RBF..",
            "######"));

        var explosionsDuringPushTicks = new List<GameEvent>();
        var explosionCount = 0;
        for (int i = 0; i < 200; i++)
        {
            var pushTick = i % 3 == 0;
            if (pushTick)
            {
                engine.Update(Direction.Right);
            }
            else
            {
                engine.Update(null);
            }

            var events = engine.ConsumeEvents();
            foreach (var gameEvent in events.Where(e => e.Type == GameEventType.Explosion))
            {
                explosionCount++;
                if (pushTick)
                {
                    explosionsDuringPushTicks.Add(gameEvent);
                }
            }
        }

        Assert.Empty(explosionsDuringPushTicks);
        Assert.True(explosionCount > 0, "The freeroaming firefly should eventually collide with Rockford.");
    }

    [Fact]
    public void FireflyLeaves2x2PocketWhenOpened()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "flyspeed = 1" },
            "#######",
            "F..####",
            "#..####",
            ".R#####",
            "#######"));

        engine.TryStationaryAction(Direction.Right);

        var outside = false;
        for (int i = 0; i < 300 && !outside; i++)
        {
            engine.Update(null);
            outside = AnyFireflyOutsidePocket(engine);
        }

        Assert.True(outside, "Firefly should leave the 2x2 pocket once Rockford opens it.");
    }

    [Fact]
    public void FireflyLeaves2x2PocketWhenSideOpened()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "flyspeed = 1" },
            "#######",
            "#F.#R##",
            "#..####",
            "#######"));

        engine.TryStationaryAction(Direction.Left);

        var outside = false;
        for (int i = 0; i < 300 && !outside; i++)
        {
            engine.Update(null);
            outside = AnyFireflyOutsidePocket(engine);
        }

        Assert.True(outside, "Firefly should leave the 2x2 pocket once Rockford opens its side.");
    }

    [Fact]
    public void FireflyErrorExactMapDebug()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "flyspeed = 1",
                "gamesteps = 60",
                "time = 60",
                "grid:",
                "########",
                "#...#R##",
                "#..F###",
                "########",
            }));

        engine.Update(Direction.Left);
        engine.Update(Direction.Right);

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 300; i++)
        {
            engine.Update(null);
            var flies = new System.Collections.Generic.List<string>();
            var rock = "?";
            for (int y = 0; y < engine.Height; y++)
            {
                for (int x = 0; x < engine.Width; x++)
                {
                    var tile = engine.GetTile(x, y);
                    if (tile.Element == Element.Firefly)
                    {
                        flies.Add($"({x},{y},{tile.Facing})");
                    }

                    if (tile.Element == Element.Rockford)
                    {
                        rock = $"({x},{y})";
                    }
                }
            }
            sb.AppendLine($"t={i} rock={rock} fly={string.Join(" ", flies)}");
            if (flies.Count == 0)
            {
                break;
            }
        }
        System.IO.File.WriteAllText(@"C:\Users\ralfj\AppData\Local\Temp\opencode\flyerrorlog.txt", sb.ToString());
    }

    [Fact]
    public void FirefliesLeave2x2WhenSideOpenedAndTwoFlies()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "flyspeed = 1" },
            "#####",
            "#FF#R",
            "#..##",
            "#####"));

        engine.TryStationaryAction(Direction.Left);

        var outside = false;
        for (int i = 0; i < 300 && !outside; i++)
        {
            engine.Update(null);
            for (int y = 0; y < engine.Height; y++)
            {
                for (int x = 0; x < engine.Width; x++)
                {
                    if (engine.GetTile(x, y).Element == Element.Firefly)
                    {
                        bool inside = x is 1 or 2 && y is 1 or 2;
                        if (!inside)
                        {
                            outside = true;
                        }
                    }
                }
            }
        }

        Assert.True(outside, "Fireflies should leave the 2x2 pocket once its side is opened.");
    }

    [Fact]
    public void BdcffFireflyLeftFacingIsPreserved()
    {
        var cave = BdcffParser.Parse(
            """
            [cave]
            Name=One
            InitialFill=DIRT
            DiamondsRequired=1
            [objects]
            Point=5 5 FIREFLYL
            Point=1 1 INBOX
            Point=2 1 OUTBOX
            [/objects]
            [/cave]
            """)[0];

        var index = 5 + 5 * cave.Width;
        Assert.Equal(Element.Firefly, cave.Grid[index]);
        Assert.Equal(Direction.Left, cave.InitialFacing![index]);

        var engine = new BoulderDashEngine(cave);
        Assert.Equal(Direction.Left, engine.GetTile(5, 5).Facing);
    }

    [Fact]
    public void FireflyInOpenedPocketLeaves()
    {
        var cave = BdcffParser.Parse(
            """
            [cave]
            Name=One
            InitialFill=DIRT
            DiamondsRequired=1
            [objects]
            FillRect=1 1 4 4 DIRT SPACE
            Point=2 2 FIREFLYL
            Point=5 3 INBOX
            Point=6 3 OUTBOX
            [/objects]
            [/cave]
            """)[0];

        var engine = new BoulderDashEngine(cave);
        engine.TryStationaryAction(Direction.Left);

        var outside = false;
        for (int i = 0; i < 300 && !outside; i++)
        {
            engine.Update(null);
            for (int y = 0; y < engine.Height; y++)
            {
                for (int x = 0; x < engine.Width; x++)
                {
                    if (engine.GetTile(x, y).Element == Element.Firefly)
                    {
                        bool inside = x is 2 or 3 && y is 2 or 3;
                        if (!inside)
                        {
                            outside = true;
                        }
                    }
                }
            }
        }

        Assert.True(outside, "Firefly should use the opening dug by Rockford and leave the 2x2 pocket.");
    }

    [Fact]
    public void DefaultFlyRuleIsTurnFirst()
    {
        var engine = new BoulderDashEngine(TestCave.Make("..."));

        Assert.Equal(FlyMovementRule.TurnFirst, engine.FlyRule);
    }

    [Fact]
    public void ButterflyWithStraightFirstRuleCirculatesCounterClockwise()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "TTTTTTT",
            "T.....T",
            "T.Y...T",
            "T.....T",
            "T.....T",
            "T.....T",
            "TTTTTTT"));
        engine.FlyRule = FlyMovementRule.StraightFirst;

        var positions = new List<Cell>();
        for (int i = 0; i < 20; i++)
        {
            engine.Update(null);
            positions.Add(Find(engine, Element.Butterfly));
        }

        Assert.Equal(new Cell(2, 1), positions[0]);
        Assert.Equal(new Cell(1, 1), positions[1]);
        Assert.Equal(new Cell(1, 2), positions[2]);
    }

    [Fact]
    public void ButterflyWithStraightFirstRulePatrolsWholeChamber()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "TTTTTTT",
            "T.....T",
            "T.Y...T",
            "T.....T",
            "T.....T",
            "T.....T",
            "TTTTTTT"));
        engine.FlyRule = FlyMovementRule.StraightFirst;

        var positions = new HashSet<Cell>();
        for (int i = 0; i < 200; i++)
        {
            engine.Update(null);
            positions.Add(Find(engine, Element.Butterfly));
        }

        Assert.Equal(16, positions.Count);
        Assert.Contains(new Cell(1, 1), positions);
        Assert.Contains(new Cell(5, 5), positions);
    }

    [Fact]
    public void FireflyWithStraightFirstRulePatrolsWholeChamber()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "TTTTTTT",
            "T.....T",
            "T.F...T",
            "T.....T",
            "T.....T",
            "T.....T",
            "TTTTTTT"));
        engine.FlyRule = FlyMovementRule.StraightFirst;

        var positions = new HashSet<Cell>();
        for (int i = 0; i < 200; i++)
        {
            engine.Update(null);
            positions.Add(Find(engine, Element.Firefly));
        }

        Assert.Equal(16, positions.Count);
        Assert.Contains(new Cell(1, 5), positions);
        Assert.Contains(new Cell(5, 1), positions);
    }

    [Fact]
    public void BoulderDash2CaveOButterfliesSpreadAcrossTheChambers()
    {
        var cave = OriginalCaves.BoulderDash2().Single(c => c.Name == "Cave O");
        var engine = new BoulderDashEngine(cave);
        engine.FlyRule = FlyMovementRule.StraightFirst;

        var visited = new HashSet<Cell>();
        for (int i = 0; i < 600; i++)
        {
            engine.Update(null);
            for (int y = 0; y < engine.Height; y++)
            {
                for (int x = 0; x < engine.Width; x++)
                {
                    if (engine.GetTile(x, y).Element == Element.Butterfly)
                    {
                        visited.Add(new Cell(x, y));
                    }
                }
            }
        }

        Assert.True(visited.Count >= 40, $"Butterflies should patrol their chambers (visited {visited.Count} cells).");
    }

    [Fact]
    public void BoulderDash2CaveOButterfliesStayInASpinByDefault()
    {
        var cave = OriginalCaves.BoulderDash2().Single(c => c.Name == "Cave O");
        var engine = new BoulderDashEngine(cave);

        var visited = new HashSet<Cell>();
        for (int i = 0; i < 600; i++)
        {
            engine.Update(null);
            for (int y = 0; y < engine.Height; y++)
            {
                for (int x = 0; x < engine.Width; x++)
                {
                    if (engine.GetTile(x, y).Element == Element.Butterfly)
                    {
                        visited.Add(new Cell(x, y));
                    }
                }
            }
        }

        Assert.True(visited.Count < 40, $"Butterflies should spin in place (visited {visited.Count} cells).");
    }

    private static bool AnyFireflyOutsidePocket(BoulderDashEngine engine)
    {
        for (int y = 0; y < engine.Height; y++)
        {
            for (int x = 0; x < engine.Width; x++)
            {
                if (engine.GetTile(x, y).Element == Element.Firefly)
                {
                    bool inside = x is 1 or 2 && y is 1 or 2;
                    if (!inside)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static Cell Find(BoulderDashEngine engine, Element element)
    {
        for (int y = 0; y < engine.Height; y++)
        {
            for (int x = 0; x < engine.Width; x++)
            {
                if (engine.GetTile(x, y).Element == element)
                {
                    return new Cell(x, y);
                }
            }
        }

        return new Cell(-1, -1);
    }
}
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class PushAndGravityTests
{
    [Fact]
    public void PushesBoulderIntoEmptySpace()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "RB.",
            "###"));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(2, 0).Element);

        var events = engine.ConsumeEvents();
        Assert.Contains(events, e => e.Type == GameEventType.Pushed);
    }

    [Fact]
    public void PushedBoulderFallsOffIntoOpenSpace()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "RB.",
            "....",
            "...#"));

        engine.Update(Direction.Right);
        for (int i = 0; i < 3; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Rockford, engine.GetTile(1, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(2, 0).Element);
        Assert.Equal(Element.Space, engine.GetTile(2, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(2, 2).Element);
    }

    [Fact]
    public void PushBlockedByWallKeepsEverythingInPlace()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "RBT",
            "..."));

        engine.Update(Direction.Right);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 0).Element);
    }

    [Fact]
    public void CannotPushVertically()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            "R",
            "."));

        engine.Update(Direction.Up);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);
    }

    [Fact]
    public void BoulderFallsToFloor()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            ".",
            ".",
            ".",
            "."));

        for (int i = 0; i < 12; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 4).Element);
    }

    [Fact]
    public void BoulderIsHeldByRockfordUntilHeLeaves()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B.",
            "R.",
            ".."));

        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Rockford, engine.GetTile(0, 1).Element);

        engine.Update(Direction.Right);
        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Rockford, engine.GetTile(1, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);
        Assert.False(engine.State.IsRespawning);
    }

    [Fact]
    public void FallingBoulderCrushesRockford()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            ".",
            "R",
            "."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.Equal(2, engine.State.Lives);
        Assert.True(engine.State.IsRespawning);
        Assert.Contains(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void CrushedRockfordIsReportedWhileRespawning()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            ".",
            "R",
            "."));

        Assert.False(engine.RockfordCrushed);

        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
        }

        Assert.True(engine.State.IsRespawning);
        Assert.True(engine.RockfordCrushed);
    }

    [Fact]
    public void CrushConsumesBoulderAndStartsDeathExplosion()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            ".",
            "R",
            "."));

        for (int i = 0; i < 5; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Space, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Space, engine.GetTile(0, 2).Element);
        Assert.True(engine.DeathExplosionTicksLeft > 0);
    }

    [Fact]
    public void ExplosionDeathDoesNotMarkRockfordAsCrushed()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            " F ",
            " R ",
            " . "));

        engine.Update(null);
        engine.Update(null);

        Assert.True(engine.State.IsRespawning || engine.State.GameOver);
        Assert.False(engine.RockfordCrushed);
    }

    [Fact]
    public void BoulderRollsDownLeftOverBrick()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".B.",
            "...",
            ".W.",
            "..#"));

        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
    }

    [Fact]
    public void BoulderRollsDownRightOverBrick()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#B..",
            "#...",
            "#W..",
            "####"));

        for (int i = 0; i < 8; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(2, 2).Element);
    }

    [Fact]
    public void StackedBouldersSettle()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "BB",
            "..",
            "##"));

        for (int i = 0; i < 15; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(2, engine.Count(Element.Boulder));
        Assert.Equal(Element.Boulder, engine.GetTile(0, 1).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 1).Element);
    }

    [Fact]
    public void FourStackedBouldersCollapseInOrder()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            "B",
            "B",
            "B",
            ".",
            "."));

        for (int i = 0; i < 20; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 4).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 5).Element);
    }

    [Fact]
    public void StackedBouldersFollowEachOtherWithIndividualDelay()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B#",
            "B#",
            "R.",
            "..",
            "..",
            "##"));

        engine.Update(Direction.Right);

        for (int i = 0; i < 2; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 0).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);
    }

    [Fact]
    public void ThreeStackedBouldersLeaveGapsWhileRockfordWalksAway()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#B#",
            "#B#",
            "#B#",
            "#R.",
            "#..",
            "#..",
            "#..",
            "#..",
            "#..",
            "###"));

        engine.Update(Direction.Right);

        var sawSeparated = false;
        for (int i = 0; i < 9 && !sawSeparated; i++)
        {
            engine.Update(Direction.Right);
            var rows = Enumerable.Range(0, engine.Height)
                .Where(y => engine.GetTile(1, y).Element == Element.Boulder)
                .ToList();
            if (rows.Count == 3)
            {
                sawSeparated = rows.Zip(rows.Skip(1), (a, b) => b - a - 1).All(gap => gap >= 1);
            }
        }

        Assert.True(sawSeparated,
            "stacked boulders should be momentarily separated by at least one empty cell while falling");
    }

    [Fact]
    public void StackedBouldersDropWithoutLargeGaps()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "B",
            "B",
            "B",
            "B",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            "."));

        for (int step = 0; step < 14; step++)
        {
            engine.Update(null);
            var columns = Enumerable.Range(0, engine.Height)
                .Where(y => engine.GetTile(0, y).Element == Element.Boulder)
                .ToList();
            for (int i = 1; i < columns.Count; i++)
            {
                Assert.True(columns[i] - columns[i - 1] - 1 <= 2,
                    $"boulders separated by too many empty cells at step {step}: {string.Join(",", columns.Select(y => y.ToString()))}");
            }
        }
    }

    [Fact]
    public void StackedBouldersKeepGapAboveRockfordWhileHeWalksDown()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "speed = 2", "time = 60" },
            "B",
            "B",
            "B",
            "R",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            "."));

        for (int i = 0; i < 12; i++)
        {
            var previousRow = engine.Rockford.Y;
            engine.Update(Direction.Down);
            if (engine.Rockford.Y <= previousRow)
            {
                continue;
            }

            Assert.True(engine.GetTile(engine.Rockford.X, engine.Rockford.Y - 1).Element == Element.Space,
                $"no boulder may sit directly above Rockford at row {engine.Rockford.Y} while he descends");
        }

        Assert.False(engine.State.IsRespawning,
            "the falling stack must not crush Rockford while he keeps walking down");
    }

    [Fact]
    public void BoulderFallsOntoRockfordWhenHeStopsWalkingDown()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            new[] { "speed = 2", "time = 60" },
            "B",
            "B",
            "B",
            "R",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            ".",
            "."));

        var events = new List<GameEvent>();
        for (int i = 0; i < 11; i++)
        {
            engine.Update(Direction.Down);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.False(engine.State.IsRespawning, "walking down must not crush Rockford");

        for (int i = 0; i < 8 && !engine.State.IsRespawning; i++)
        {
            engine.Update(null);
            events.AddRange(engine.ConsumeEvents());
        }

        Assert.True(engine.State.IsRespawning);
        Assert.True(engine.RockfordCrushed);
        Assert.Contains(events, e => e.Type == GameEventType.RockfordDied);
    }

    [Fact]
    public void CaveASpotCollapsesWithAgedConfiguration()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "..BD.",
            "..BB#",
            "..B#.",
            "..B..",
            ".....",
            ".....",
            ".....",
            "#####"));

        for (int i = 0; i < 24; i++)
        {
            engine.Update(null);
        }

        var boulders = new List<Cell>();
        var diamonds = new List<Cell>();
        for (int x = 0; x < engine.Width; x++)
        {
            for (int y = 0; y < engine.Height; y++)
            {
                if (engine.GetTile(x, y).Element == Element.Boulder)
                {
                    boulders.Add(new Cell(x, y));
                }
                else if (engine.GetTile(x, y).Element == Element.Diamond)
                {
                    diamonds.Add(new Cell(x, y));
                }
            }
        }

        Assert.Equal(5, boulders.Count);
        foreach (var boulder in boulders)
        {
            if (boulder.Y < engine.Height - 1)
            {
                Assert.NotEqual(Element.Space, engine.GetTile(boulder.X, boulder.Y + 1).Element);
            }
        }

        Assert.Equal(1, diamonds.Count);
        Assert.NotEqual(Element.Space, engine.GetTile(diamonds[0].X, diamonds[0].Y + 1).Element);
        Assert.Contains(boulders, boulder => boulder.Y == engine.Height - 2);
        Assert.Equal(Element.Dirt, engine.GetTile(3, 2).Element);
    }

    [Fact]
    public void TopBoulderRollsOffStackToTheLeft()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".B.",
            ".B.",
            ".#.",
            "###"));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 1).Element);
    }

    [Fact]
    public void TopBoulderRollsOffStackToTheRightWhenLeftIsBlocked()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#B.",
            ".B.",
            ".#.",
            "###"));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(2, 2).Element);
        Assert.Equal(Element.Boulder, engine.GetTile(1, 1).Element);
    }

    [Fact]
    public void BoulderOnDirtWithOpenSidesDoesNotRoll()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            ".B.",
            ".#.",
            ".#."));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(1, 0).Element);
    }
}
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class BdcffParserTests
{
    private const string BasicCave = """
        [BDCFF]

        [game]
        Name=Boulder Dash
        Caves=20

        [cave]
        Name=Test Cave
        CaveTime=150 110 70
        DiamondsRequired=5 5 5
        DiamondValue=10 15
        RandSeed=42 43
        MagicWallTime=20
        BonusLife=500
        Lives=5

        [objects]
        Line=1 1 3 1 WALL
        Point=3 2 INBOX
        Point=38 18 OUTBOX
        [/objects]

        [/cave]
        """;

    [Fact]
    public void ParsesLivesAndBonusLife()
    {
        var cave = Assert.Single(BdcffParser.Parse(BasicCave));

        Assert.Equal(5, cave.StartLives);
        Assert.Equal(500, cave.ExtraLifeEvery);
    }

    [Fact]
    public void ParsesAttributesIntoCaveDefinition()
    {
        var caves = BdcffParser.Parse(BasicCave);
        var cave = Assert.Single(caves);

        Assert.Equal("Test Cave", cave.Name);
        Assert.Equal(150, cave.TimeSeconds);
        Assert.Equal(5, cave.DiamondsNeeded);
        Assert.Equal(10, cave.DiamondValue);
        Assert.Equal(15, cave.BonusValue);
        Assert.Equal(42, cave.Seed);
        Assert.Equal(20 * cave.GameStepsPerSecond, cave.MagicWallTicks);
        Assert.False(cave.IsIntermission);
        Assert.Equal(40, cave.Width);
        Assert.Equal(22, cave.Height);
    }

    [Fact]
    public void PlacesPointLineAndOptionalObjectsAllOpenFilledWithInitialFill()
    {
        var cave = Assert.Single(BdcffParser.Parse(BasicCave));

        Assert.Equal(Element.Wall, cave.Grid[1 * 40 + 1]);
        Assert.Equal(Element.Wall, cave.Grid[1 * 40 + 2]);
        Assert.Equal(Element.Rockford, cave.Grid[2 * 40 + 3]);
        Assert.Equal(Element.Exit, cave.Grid[18 * 40 + 38]);
        Assert.Equal(Element.Dirt, cave.Grid[10 * 40 + 10]);
    }

    [Fact]
    public void DrawsDiagonalLineEndToEnd()
    {
        const string text = """
            [cave]
            Name=Diagonal
            [objects]
            Line=1 1 3 3 BOULDER
            Point=5 5 INBOX
            Point=38 20 OUTBOX
            [/objects]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Boulder, cave.Grid[1 * 40 + 1]);
        Assert.Equal(Element.Boulder, cave.Grid[2 * 40 + 2]);
        Assert.Equal(Element.Boulder, cave.Grid[3 * 40 + 3]);
    }

    [Fact]
    public void RandomFillUsesOriginalCumulativeC64Rules()
    {
        const string text = """
            [cave]
            Name=Random
            RandSeed=7
            [objects]
            RandomFill=WALL 100 BOULDER 50 DIAMOND 9
            Point=5 5 INBOX
            Point=38 20 OUTBOX
            [/objects]
            [/cave]
            """;

        var first = Assert.Single(BdcffParser.Parse(text));
        var second = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(first.Grid, second.Grid);
        Assert.Contains(Element.Boulder, first.Grid);
        Assert.Contains(Element.Diamond, first.Grid);
        Assert.Equal(Element.Dirt, first.Grid[10 * 40 + 10]);
    }

    [Fact]
    public void FillRectWithTwoElementsDrawsHollowRectangle()
    {
        const string text = """
            [cave]
            Name=Fill
            [objects]
            FillRect=8 8 11 11 DIRT SPACE
            Point=5 5 INBOX
            Point=38 20 OUTBOX
            [/objects]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        for (int y = 8; y <= 11; y++)
        {
            for (int x = 8; x <= 11; x++)
            {
                var expected = x == 8 || x == 11 || y == 8 || y == 11 ? Element.Dirt : Element.Space;
                Assert.Equal(expected, cave.Grid[y * 40 + x]);
            }
        }
    }

    [Fact]
    public void OriginalCaveDMatchesTheOriginalButterflyBoxes()
    {
        var cave = OriginalCaves.Load()[3];

        Assert.Equal("Cave D. Butterflies", cave.Name);
        for (int box = 0; box < 4; box++)
        {
            int x1 = 8 + box * 8;
            for (int y = 8; y <= 11; y++)
            {
                for (int x = x1; x <= x1 + 3; x++)
                {
                    if (x == x1 + 2 && y == 9)
                    {
                        Assert.Equal(Element.Butterfly, cave.Grid[y * 40 + x]);
                        continue;
                    }

                    var expected = x == x1 || x == x1 + 3 || y == 8 || y == 11 ? Element.Dirt : Element.Space;
                    Assert.Equal(expected, cave.Grid[y * 40 + x]);
                }
            }
        }
    }

    [Fact]
    public void RectangleOnlyDrawsTheBorder()
    {
        const string text = """
            [cave]
            Name=Rect
            [objects]
            Rectangle=4 4 8 6 WALL
            Point=12 12 INBOX
            Point=38 20 OUTBOX
            [/objects]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Wall, cave.Grid[4 * 40 + 4]);
        Assert.Equal(Element.Wall, cave.Grid[4 * 40 + 8]);
        Assert.Equal(Element.Wall, cave.Grid[6 * 40 + 4]);
        Assert.Equal(Element.Wall, cave.Grid[6 * 40 + 8]);
        Assert.Equal(Element.Dirt, cave.Grid[5 * 40 + 6]);
    }

    [Fact]
    public void ParsesIntermissionFlag()
    {
        const string text = """
            [cave]
            Name=Brdge
            Intermission=true
            InitialFill=SPACE
            [objects]
            Point=3 2 INBOX
            Point=38 18 OUTBOX
            [/objects]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.True(cave.IsIntermission);
        Assert.Equal(Element.Space, cave.Grid[10 * 40 + 10]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[0]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[39]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[10 * 40 + 0]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[21 * 40 + 39]);
    }

    [Fact]
    public void ThrowsWhenCaveHasNoInbox()
    {
        const string text = """
            [cave]
            Name=Broken
            [objects]
            Point=38 18 OUTBOX
            [/objects]
            [/cave]
            """;

        Assert.Throws<FormatException>(() => BdcffParser.Parse(text));
    }

    [Fact]
    public void OriginalFileYieldsTwentyPlayableCaves()
    {
        var caves = OriginalCaves.Load();

        Assert.Equal(20, caves.Count);
        Assert.Equal("Cave A. Intro", caves[0].Name);
        Assert.Equal("Intermission 4", caves[19].Name);

        foreach (var cave in caves)
        {
            Assert.Equal(40, cave.Width);
            Assert.Equal(22, cave.Height);
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Rockford));
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Exit));
            var staticDiamonds = cave.Grid.Count(e => e == Element.Diamond);
            Assert.True(
                staticDiamonds >= cave.DiamondsNeeded ||
                cave.Grid.Any(e => e == Element.Butterfly) ||
                cave.Grid.Any(e => e == Element.Amoeba) ||
                cave.Grid.Any(e => e == Element.MagicWall) ||
                cave.IsIntermission,
                $"Cave '{cave.Name}' offers no diamond source ({staticDiamonds} static < {cave.DiamondsNeeded} needed, no butterflies/amoeba/magic wall).");

            if (cave.IsIntermission)
            {
                Assert.True(
                    staticDiamonds >= cave.DiamondsNeeded ||
                    cave.Grid.Any(e => e == Element.Butterfly) ||
                    cave.Grid.Any(e => e == Element.Amoeba) ||
                    cave.Grid.Any(e => e == Element.MagicWall),
                    $"Intermission '{cave.Name}' has an unreachable diamond target ({staticDiamonds} static < {cave.DiamondsNeeded}).");
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(14)]
    [InlineData(19)]
    public void OriginalCavesSimulateWithoutCrashing(int index)
    {
        var engine = new BoulderDashEngine(OriginalCaves.Load()[index]);
        for (int i = 0; i < 4000; i++)
        {
            engine.Update(i % 8 == 0 ? Direction.Down : null);
        }
    }

    [Fact]
    public void ParsesMapEncodedCaveWithDefaultCharacterSet()
    {
        const string text = """
            [cave]
            Name=Map
            [map]
            WWWWWW
            WXd..W
            W.d..W
            W.d..W
            W.d.PW
            WWWWWW
            [/map]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(6, cave.Width);
        Assert.Equal(6, cave.Height);
        Assert.Equal(Element.TitaniumWall, cave.Grid[0]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[5 * 6 + 5]);
        Assert.Equal(Element.Exit, cave.Grid[1 * 6 + 1]);
        Assert.Equal(Element.Rockford, cave.Grid[4 * 6 + 4]);
        Assert.Equal(Element.Diamond, cave.Grid[1 * 6 + 2]);
        Assert.Equal(Element.Diamond, cave.Grid[2 * 6 + 2]);
        Assert.Equal(Element.Dirt, cave.Grid[2 * 6 + 3]);
        Assert.Equal(Element.Dirt, cave.Grid[3 * 6 + 4]);
    }

    [Fact]
    public void MapCaveHonorsSizeAttributeWithExtendedCoordinates()
    {
        const string text = """
            [cave]
            Name=Map
            Size=40 22 0 0 39 21
            [map]
            WWWWWW
            WXd..W
            W.d..W
            W.d..W
            W.d.PW
            WWWWWW
            [/map]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(40, cave.Width);
        Assert.Equal(22, cave.Height);
        Assert.Equal(Element.Rockford, cave.Grid[4 * 40 + 4]);
        Assert.Equal(Element.Exit, cave.Grid[1 * 40 + 1]);
        Assert.Equal(Element.Dirt, cave.Grid[20 * 40 + 39]);
    }

    [Fact]
    public void MapEncodesSpaceAndFacingForFlies()
    {
        const string text = """
            [cave]
            Name=Map
            [map]
            WWWWWWWWW
            WP qQoOcB
            W...d...W
            W      XW
            WWWWWWWWW
            [/map]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Space, cave.Grid[1 * 9 + 2]);
        Assert.Equal(Element.Firefly, cave.Grid[1 * 9 + 3]);
        Assert.Equal(Direction.Down, cave.InitialFacing![1 * 9 + 3]);
        Assert.Equal(Element.Firefly, cave.Grid[1 * 9 + 6]);
        Assert.Equal(Direction.Right, cave.InitialFacing![1 * 9 + 6]);
        Assert.Equal(Element.Butterfly, cave.Grid[1 * 9 + 8]);
        Assert.Equal(Direction.Right, cave.InitialFacing![1 * 9 + 8]);
        Assert.Equal(Element.Rockford, cave.Grid[1 * 9 + 1]);
        Assert.Equal(Element.Exit, cave.Grid[3 * 9 + 7]);
        Assert.Equal(Element.Space, cave.Grid[3 * 9 + 6]);
    }

    [Fact]
    public void MapCodesOverrideAndExtendDefaultCodes()
    {
        const string text = """
            [cave]
            Name=Map
            [mapcodes]
            .=SPACE
            d=DIRT
            ?=BOULDER
            [/mapcodes]
            [map]
            W?????W
            WW .dXW
            WWW??PW
            WWWWWWW
            [/map]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Boulder, cave.Grid[0 * 7 + 2]);
        Assert.Equal(Element.Dirt, cave.Grid[1 * 7 + 4]);
        Assert.Equal(Element.Space, cave.Grid[1 * 7 + 3]);
        Assert.Equal(Element.Exit, cave.Grid[1 * 7 + 5]);
        Assert.Equal(Element.Rockford, cave.Grid[2 * 7 + 5]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[1 * 7 + 0]);
    }

    [Fact]
    public void MapsAdditionalElementNames()
    {
        const string text = """
            [cave]
            Name=Names
            [objects]
            Point=1 1 HIDDENOUTBOX
            Point=2 1 WALL2
            Point=3 1 BOULDERf
            Point=4 1 DIAMONDf
            Point=5 1 FALLING_BOULDER
            Point=6 1 SCANNED_FIREFLYD
            Point=7 1 EXPLOSION5D
            Point=8 1 HEXPANDING_WALL
            Point=3 2 INBOX
            [/objects]
            [/cave]
            """;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Exit, cave.Grid[1 * 40 + 1]);
        Assert.Equal(Element.Wall, cave.Grid[1 * 40 + 2]);
        Assert.Equal(Element.Boulder, cave.Grid[1 * 40 + 3]);
        Assert.Equal(Element.Diamond, cave.Grid[1 * 40 + 4]);
        Assert.Equal(Element.Boulder, cave.Grid[1 * 40 + 5]);
        Assert.Equal(Element.Firefly, cave.Grid[1 * 40 + 6]);
        Assert.Equal(Direction.Down, cave.InitialFacing![1 * 40 + 6]);
        Assert.Equal(Element.Space, cave.Grid[1 * 40 + 7]);
        Assert.Equal(Element.ExpandingWall, cave.Grid[1 * 40 + 8]);
    }

    [Fact]
    public void ParseWithErrorsKeepsPlayableCavesAndReportsBrokenOnes()
    {
        const string text = """
            [game]
            Name=Sets

            [cave]
            Name=Good
            [objects]
            Point=3 2 INBOX
            Point=38 18 OUTBOX
            [/objects]
            [/cave]

            [cave]
            Name=Bad
            [objects]
            Point=38 18 OUTBOX
            [/objects]
            [/cave]

            [cave]
            Name=GoodTwo
            [objects]
            Point=3 2 INBOX
            Point=38 18 OUTBOX
            [/objects]
            [/cave]
            """;

        var result = BdcffParser.ParseWithErrors(text);

        Assert.Equal(new[] { "Good", "GoodTwo" }, result.Caves.Select(c => c.Name));
        var error = Assert.Single(result.Errors);
        Assert.Contains("Bad", error);
    }
}
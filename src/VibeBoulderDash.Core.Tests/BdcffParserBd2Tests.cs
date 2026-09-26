using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class BdcffParserBd2Tests
{
    

    private const string BasicObjectsSuffix = "\nPoint=5 5 INBOX\nPoint=38 20 OUTBOX\n[/objects]\n[/cave]\n";

    [Fact]
    public void RasterDrawsRectangularGridOfElements()
    {
        const string text = """
            [cave]
            Name=Raster
            [objects]
            Raster=2 2 3 2 2 1 WALL
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Wall, cave.Grid[2 * 40 + 2]);
        Assert.Equal(Element.Wall, cave.Grid[2 * 40 + 4]);
        Assert.Equal(Element.Wall, cave.Grid[2 * 40 + 6]);
        Assert.Equal(Element.Wall, cave.Grid[3 * 40 + 2]);
        Assert.Equal(Element.Wall, cave.Grid[3 * 40 + 4]);
        Assert.Equal(Element.Wall, cave.Grid[3 * 40 + 6]);
        Assert.Equal(Element.Dirt, cave.Grid[2 * 40 + 3]);
    }

    [Fact]
    public void RasterAppliesFacingFromDirectionalElementName()
    {
        const string text = """
            [cave]
            Name=RasterFacing
            [objects]
            Raster=1 1 2 1 3 1 FIREFLYr
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Direction.Right, cave.InitialFacing![1 * 40 + 1]);
        Assert.Equal(Direction.Right, cave.InitialFacing![1 * 40 + 4]);
    }

    [Fact]
    public void AddDrawsSecondElementBelowEachSearchElement()
    {
        const string text = """
            [cave]
            Name=Add
            [objects]
            Point=10 10 FIREFLYl
            Point=14 10 FIREFLYl
            Add=0 1 FIREFLYl BOULDER
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Firefly, cave.Grid[10 * 40 + 10]);
        Assert.Equal(Element.Boulder, cave.Grid[11 * 40 + 10]);
        Assert.Equal(Element.Firefly, cave.Grid[10 * 40 + 14]);
        Assert.Equal(Element.Boulder, cave.Grid[11 * 40 + 14]);
    }

    [Fact]
    public void AddSupportsNegativeOffsetsAndDiagonalPlacement()
    {
        const string text = """
            [cave]
            Name=AddNegative
            [objects]
            Point=10 10 FIREFLYl
            Add=-1 -1 FIREFLYl DIRT
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(Element.Dirt, cave.Grid[9 * 40 + 9]);
    }

    [Fact]
    public void SlimePermeabilityIsParsed()
    {
        const string text = """
            [cave]
            Name=SlimePerm
            SlimePermeability=0.125
            [objects]
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(0.125, cave.SlimePermeability, precision: 5);
    }

    [Fact]
    public void SlimePermeabilityDefaultsToOne()
    {
        const string text = """
            [cave]
            Name=SlimeDefault
            [objects]
            """ + BasicObjectsSuffix;

        var cave = Assert.Single(BdcffParser.Parse(text));

        Assert.Equal(1.0, cave.SlimePermeability, precision: 5);
    }

    [Fact]
    public void BoulderDashTwoCavesWithSlimeCanBeStepped()
    {
        var cave = OriginalCaves.BoulderDash2().First(c => c.Grid.Any(e => e == Element.Slime));
        int slime = cave.Grid.Count(e => e == Element.Slime);
        var engine = new BoulderDashEngine(cave);

        for (int i = 0; i < 30; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(slime, engine.Count(Element.Slime));
    }

    [Fact]
    public void BoulderDashTwoParsesToTwentyCaves()
    {
        var caves = OriginalCaves.BoulderDash2();

        Assert.Equal(20, caves.Count);
        Assert.Equal("Cave A", caves[0].Name);
        Assert.Contains(caves, c => c.Grid.Any(e => e == Element.Slime));
        Assert.Contains(caves, c => c.Grid.Any(e => e == Element.ExpandingWall));

        foreach (var cave in caves)
        {
            Assert.Equal(40, cave.Width);
            Assert.Equal(22, cave.Height);
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Rockford));
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Exit));
        }
    }

    [Fact]
    public void BoulderDashThreeParsesToTwentyCaves()
    {
        var caves = OriginalCaves.BoulderDash3();

        Assert.Equal(20, caves.Count);
        Assert.Equal("Cave A. Intro", caves[0].Name);
        Assert.Contains(caves, c => c.Grid.Any(e => e == Element.Boulder));
        Assert.Contains(caves, c => c.Grid.Any(e => e == Element.Firefly));

        foreach (var cave in caves)
        {
            Assert.Equal(40, cave.Width);
            Assert.Equal(22, cave.Height);
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Rockford));
            Assert.Equal(1, cave.Grid.Count(e => e == Element.Exit));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(14)]
    [InlineData(19)]
    public void BoulderDashTwoCavesSimulateWithoutCrashing(int index)
    {
        var engine = new BoulderDashEngine(OriginalCaves.BoulderDash2()[index]);
        for (int i = 0; i < 4000; i++)
        {
            engine.Update(i % 8 == 0 ? Direction.Down : null);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(14)]
    [InlineData(19)]
    public void BoulderDashThreeCavesSimulateWithoutCrashing(int index)
    {
        var engine = new BoulderDashEngine(OriginalCaves.BoulderDash3()[index]);
        for (int i = 0; i < 4000; i++)
        {
            engine.Update(i % 8 == 0 ? Direction.Down : null);
        }
    }
}
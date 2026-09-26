using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class CaveDefinitionTests
{
    [Fact]
    public void ParsesGridAndMetadata()
    {
        var cave = CaveDefinition.Parse(
            new[]
            {
                "name = Test Cave",
                "time = 150",
                "diamonds = 12",
                "diamondvalue = 25",
                "grid:",
                "R#B",
                ".DE",
            });

        Assert.Equal("Test Cave", cave.Name);
        Assert.Equal(150, cave.TimeSeconds);
        Assert.Equal(12, cave.DiamondsNeeded);
        Assert.Equal(25, cave.DiamondValue);
        Assert.Equal(3, cave.Width);
        Assert.Equal(2, cave.Height);
        Assert.Equal(Element.Rockford, cave.Grid[0]);
        Assert.Equal(Element.Dirt, cave.Grid[1]);
        Assert.Equal(Element.Boulder, cave.Grid[2]);
        Assert.Equal(Element.Space, cave.Grid[3]);
        Assert.Equal(Element.Diamond, cave.Grid[4]);
        Assert.Equal(Element.Exit, cave.Grid[5]);
    }

    [Fact]
    public void ParsesTextBlock()
    {
        var cave = CaveDefinition.Parse(
            """
            name = Block Cave
            grid:
            R..
            ...
            """);

        Assert.Equal(Element.Rockford, cave.Grid[0]);
        Assert.Equal(2, cave.Height);
    }

    [Fact]
    public void ParsesExpandingWallAndSlimeGridChars()
    {
        var cave = CaveDefinition.Parse(new[] { "grid:", "RXS." });

        Assert.Equal(Element.ExpandingWall, cave.Grid[1]);
        Assert.Equal(Element.Slime, cave.Grid[2]);
    }

    [Fact]
    public void ParsesSlimePermeabilityMetadata()
    {
        var cave = CaveDefinition.Parse(new[] { "slimepermeability = 0.125", "grid:", "B", "S", "." });

        Assert.Equal(0.125, cave.SlimePermeability, precision: 5);
    }

    [Fact]
    public void RejectsUnknownGridChar()
    {
        var ex = Assert.Throws<FormatException>(() => CaveDefinition.Parse(new[] { "grid:", "R?X" }));
        Assert.Contains("Unknown grid char", ex.Message);
    }

    [Fact]
    public void RejectsMissingGrid()
    {
        Assert.Throws<FormatException>(() => CaveDefinition.Parse(new[] { "name = x" }));
    }
}
using VibeBoulderDash.Core;
using Xunit;

namespace VibeBoulderDash.Core.Tests;

public class RandomFillC64Tests
{
    [Fact]
    public void NextRandomMatchesDocumentedOriginalSequence()
    {
        var rng = new C64Random(seed2: 10);
        Assert.Equal(5, rng.Next());
        Assert.Equal(29, rng.Seed2);
        Assert.Equal(147, rng.Next());
        Assert.Equal(176, rng.Seed2);
    }

    [Fact]
    public void CaveAFirstCellsMatchOriginalRandomFill()
    {
        var cave = CaveCatalog.AllCaves()[0];

        Assert.Equal(Element.TitaniumWall, cave.Grid[1 * cave.Width + 0]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[0 * cave.Width + 0]);
        Assert.Equal(Element.TitaniumWall, cave.Grid[21 * cave.Width + 39]);

        Assert.Equal(Element.Dirt, cave.Grid[1 * cave.Width + 1]);
        Assert.Equal(Element.Dirt, cave.Grid[1 * cave.Width + 2]);

        Assert.Equal(Element.Rockford, cave.Grid[2 * cave.Width + 3]);
        Assert.Equal(Element.Exit, cave.Grid[16 * cave.Width + 38]);
    }
}
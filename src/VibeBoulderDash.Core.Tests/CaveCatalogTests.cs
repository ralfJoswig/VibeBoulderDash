namespace VibeBoulderDash.Core.Tests;

public class CaveCatalogTests
{
    [Fact]
    public void DemoCaveIsValidAndSimulates()
    {
        var cave = CaveCatalog.DemoCave();

        Assert.Equal(40, cave.Width);
        Assert.All(cave.Grid.Select(e => (int)e), _ => Assert.True(true));
        Assert.Equal(1, cave.Grid.Count(e => e == Element.Rockford));
        Assert.Equal(1, cave.Grid.Count(e => e == Element.Exit));

        var engine = new BoulderDashEngine(cave);
        for (int i = 0; i < 2000; i++)
        {
            engine.Update(i % 10 == 0 ? Direction.Right : null);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void EveryCaveIsValidAndSimulates(int index)
    {
        var cave = CaveCatalog.AllCaves()[index];

        Assert.Equal(40, cave.Width);
        Assert.Equal(1, cave.Grid.Count(e => e == Element.Rockford));
        Assert.Equal(1, cave.Grid.Count(e => e == Element.Exit));
        Assert.Contains(Element.Diamond, cave.Grid);

        var engine = new BoulderDashEngine(cave);
        for (int i = 0; i < 3000; i++)
        {
            engine.Update(i % 8 == 0 ? Direction.Down : null);
        }
    }
}
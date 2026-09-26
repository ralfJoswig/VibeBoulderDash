using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class DugTrackingTests
{
    [Fact]
    public void DiggingMarksCellAsDug()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#R",
            ".."));

        engine.TryStationaryAction(Direction.Left);

        Assert.True(engine.IsDug(0, 0));
        Assert.False(engine.IsDug(1, 0));
    }

    [Fact]
    public void DugMarkSurvivesBoulderPassingOver()
    {
        var engine = new BoulderDashEngine(TestCave.Make(
            "#B.",
            "R#.",
            "..."));

        engine.TryStationaryAction(Direction.Right);

        for (int i = 0; i < 3; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Space, engine.GetTile(1, 1).Element);
        Assert.True(engine.IsDug(1, 1));
    }
}
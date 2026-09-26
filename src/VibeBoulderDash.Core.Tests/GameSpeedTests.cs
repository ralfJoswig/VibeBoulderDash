using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class GameSpeedTests
{
    [Theory]
    [InlineData(1, 11)]
    [InlineData(2, 13)]
    [InlineData(3, 14)]
    [InlineData(4, 16)]
    [InlineData(5, 17)]
    public void GameStepsForDifficultyMatchesOriginalCadence(int difficulty, int expected)
    {
        Assert.Equal(expected, CaveDefinition.GameStepsForDifficulty(difficulty));
    }

    [Fact]
    public void ParseDerivesAuthenticCadenceFromDifficulty()
    {
        var cave = CaveDefinition.Parse(new[] { "difficulty = 3", "grid:", "R.", ".." });
        Assert.Equal(14, cave.GameStepsPerSecond);

        var fast = CaveDefinition.Parse(new[] { "gamesteps = 60", "grid:", "R.", ".." });
        Assert.Equal(60, fast.GameStepsPerSecond);
    }

    [Fact]
    public void RockfordMovesOncePerGameStepNotPerTick()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 11",
                "grid:",
                "R...........",
                "............",
            }));

        for (int i = 0; i < 5; i++)
        {
            engine.Update(Direction.Right);
        }

        Assert.Equal(0, engine.Rockford.X);

        engine.Update(Direction.Right);
        Assert.Equal(1, engine.Rockford.X);

        for (int i = 0; i < 54; i++)
        {
            engine.Update(Direction.Right);
        }

        Assert.Equal(11, engine.Rockford.X);
    }

    [Fact]
    public void CaveTimerCountsRealSecondsDespiteSlowCadence()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "speed = 1",
                "gamesteps = 11",
                "grid:",
                "R...........",
                "............",
            }));

        Assert.Equal(200, engine.State.TimeLeft);

        for (int i = 0; i < BoulderDashEngine.TicksPerSecond; i++)
        {
            engine.Update(Direction.Right);
        }

        Assert.Equal(199, engine.State.TimeLeft);
        Assert.Equal(11, engine.Steps);
    }

    [Fact]
    public void BoulderFallsOneCellPerGameStep()
    {
        var engine = new BoulderDashEngine(CaveDefinition.Parse(
            new[]
            {
                "gamesteps = 11",
                "grid:",
                "R..",
                "...",
                "B..",
                "...",
            }));

        for (int i = 0; i < 10; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 2).Element);

        for (int i = 0; i < 11; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
        Assert.True(engine.GetTile(0, 3).Falling);
        Assert.Equal(3, engine.Steps);

        engine.Update(null);
        Assert.Equal(Element.Boulder, engine.GetTile(0, 3).Element);
        Assert.False(engine.GetTile(0, 3).Falling);
    }

    [Fact]
    public void OriginalCavesRunAtAuthenticCadence()
    {
        var caves = OriginalCaves.Load();

        Assert.Equal(11, caves[0].GameStepsPerSecond);
        Assert.Equal(1, caves[0].RockfordTicksPerMove);
        Assert.Equal(1, caves[0].FlyTicksPerMove);

        var intermission = caves.First(c => c.IsIntermission);
        Assert.Equal(16, intermission.GameStepsPerSecond);
    }
}
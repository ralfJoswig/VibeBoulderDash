using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class RockfordAnimationTests
{
    private static BoulderDashEngine Make() => new(TestCave.Make(
        new[] { "rockfordtickspermove = 1" },
        "R..",
        "...",
        "#.."));

    [Fact]
    public void AnimationFacingDefaultsToLeft()
    {
        var engine = Make();
        Assert.Equal(Direction.Left, engine.RockfordAnimFacing);
    }

    [Fact]
    public void AnimationFacingTracksLastHorizontalMove()
    {
        var engine = Make();

        engine.Update(Direction.Right);
        Assert.Equal(Direction.Right, engine.RockfordAnimFacing);

        engine.Update(Direction.Left);
        Assert.Equal(Direction.Left, engine.RockfordAnimFacing);
    }

    [Fact]
    public void VerticalMoveKeepsLastHorizontalFacing()
    {
        var engine = Make();

        engine.Update(Direction.Right);
        engine.Update(Direction.Up);
        Assert.Equal(Direction.Right, engine.RockfordAnimFacing);

        engine.Update(Direction.Down);
        engine.Update(Direction.Down);
        Assert.Equal(Direction.Right, engine.RockfordAnimFacing);
    }

    [Fact]
    public void BlinkAndTapAreDeterministicForSameSeed()
    {
        var a = Make();
        var b = Make();
        for (int i = 0; i < 80; i++)
        {
            a.Update(null);
            b.Update(null);
        }

        Assert.Equal(a.RockfordBlinking, b.RockfordBlinking);
        Assert.Equal(a.RockfordTapping, b.RockfordTapping);
    }

    [Fact]
    public void BlinkIsRolledAtSequenceBoundariesOnly()
    {
        var engine = Make();
        for (int i = 0; i < 16; i++)
        {
            engine.Update(null);
        }

        var seq = engine.Tick / 16;
        var expected = ((seq * 31 + engine.Cave.Seed) & 3) == 0;
        Assert.Equal(expected, engine.RockfordBlinking);

        for (int i = 0; i < 15; i++)
        {
            engine.Update(null);
        }

        Assert.Equal(expected, engine.RockfordBlinking);
    }

    [Fact]
    public void MovingClearsBlinkAndTap()
    {
        var engine = Make();
        for (int i = 0; i < 16; i++)
        {
            engine.Update(null);
        }

        engine.Update(Direction.Right);
        Assert.False(engine.RockfordBlinking);
        Assert.False(engine.RockfordTapping);
    }

    [Fact]
    public void MovingFlagFollowsInput()
    {
        var engine = Make();

        Assert.False(engine.RockfordIsMoving);

        engine.Update(Direction.Left);
        Assert.True(engine.RockfordIsMoving);

        engine.Update(null);
        Assert.False(engine.RockfordIsMoving);
    }
}
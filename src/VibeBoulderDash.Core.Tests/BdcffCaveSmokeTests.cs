using System.IO;
using VibeBoulderDash.Core;

namespace VibeBoulderDash.Core.Tests;

public class BdcffCaveSmokeTests
{
    [Fact]
    public void AflBdCaveFourRunsWithoutCrashing()
    {
        var caves = LoadFile("AflBD.bd");

        var engine = new BoulderDashEngine(caves[0]);
        engine.Update(null);
        engine.ConsumeEvents();
        engine.LoadCave(caves[3], preserveSession: true);
        engine.FlyRule = FlyMovementRule.TurnFirst;

        for (int i = 0; i < 2000; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }
    }

    [Fact]
    public void ArnoDashCompletingCave3StagesCave4()
    {
        var file = Path.Combine(BdcffFolder, "ArnoDash01.bd");
        var text = File.ReadAllText(file);
        var caves = BdcffParser.ParseWithErrors(text.Replace("DiamondsRequired=32", "DiamondsRequired=0")).Caves;
        var cave3 = caves[2];
        var cave4 = caves[3];

        var engine = new BoulderDashEngine(cave3);
        var staged = false;
        int guard = 0;
        while (!staged && guard++ < 10000)
        {
            var direction = engine.State.CaveCompleted ? (Direction?)null : Direction.Left;
            engine.Update(direction);
            foreach (var e in engine.ConsumeEvents())
            {
                if (e.Type == GameEventType.CaveFillComplete)
                {
                    engine.StageNextCave(cave4);
                    staged = true;
                }

                if (e.Type == GameEventType.CaveFinished)
                {
                    engine.LoadCave(cave4, preserveSession: true);
                }
            }
        }

        Assert.True(staged);
        for (int i = 0; i < 2000; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }
    }

    [Fact]
    public void ArnoDashStagingSmallerIntermissionDoesNotThrow()
    {
        var file = Path.Combine(BdcffFolder, "ArnoDash01.bd");
        var text = File.ReadAllText(file);
        var caves = BdcffParser.ParseWithErrors(text.Replace("DiamondsRequired=32", "DiamondsRequired=0")).Caves;
        var cave3 = caves[2];
        var intermission = caves[4];

        var engine = new BoulderDashEngine(cave3);
        var staged = false;
        int guard = 0;
        while (!staged && guard++ < 10000)
        {
            var direction = engine.State.CaveCompleted ? (Direction?)null : Direction.Left;
            engine.Update(direction);
            foreach (var e in engine.ConsumeEvents())
            {
                if (e.Type == GameEventType.CaveFillComplete)
                {
                    engine.StageNextCave(intermission);
                    staged = true;
                }

                if (e.Type == GameEventType.CaveFinished)
                {
                    engine.LoadCave(intermission, preserveSession: true);
                }
            }
        }

        Assert.True(staged);
        for (int i = 0; i < 2000; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }
    }

    [Fact]
    public void ArnoDashTransitionFromCave3ToCave4DoesNotCrash()
    {
        var caves = LoadFile("ArnoDash01.bd");

        var engine = new BoulderDashEngine(caves[2]);
        engine.Update(null);
        engine.ConsumeEvents();
        engine.LoadCave(caves[3], preserveSession: true);
        engine.FlyRule = FlyMovementRule.TurnFirst;

        for (int i = 0; i < 2000; i++)
        {
            engine.Update(null);
            engine.ConsumeEvents();
        }
    }

    [Fact]
    public void EveryPlayableBdcffCaveRunsWithoutCrashing()
    {
        var folder = BdcffFolder;
        foreach (var file in Directory.GetFiles(folder, "*.bd"))
        {
            var result = BdcffParser.ParseWithErrors(File.ReadAllText(file));
            foreach (var cave in result.Caves)
            {
                var engine = new BoulderDashEngine(cave);
                for (int i = 0; i < 120; i++)
                {
                    engine.Update(null);
                    engine.ConsumeEvents();
                }
            }
        }
    }

    private static IReadOnlyList<CaveDefinition> LoadFile(string name)
    {
        var file = Path.Combine(BdcffFolder, name);
        return BdcffParser.ParseWithErrors(File.ReadAllText(file)).Caves;
    }

    private static string BdcffFolder
    {
        get
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "BDCFF");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new DirectoryNotFoundException("BDCFF folder not found above " + AppContext.BaseDirectory);
        }
    }
}
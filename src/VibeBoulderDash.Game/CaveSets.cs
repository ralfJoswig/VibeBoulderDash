#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VibeBoulderDash.Core;

namespace VibeBoulderDash;

/// <summary>A named, ordered collection of caves selectable in the game.</summary>
public sealed record CaveSet(
    string Name,
    IReadOnlyList<CaveDefinition> Caves,
    FlyMovementRule FlyRule = FlyMovementRule.TurnFirst);

/// <summary>Builds the cave sets: official releases first, then every playable BDCFF file from the BDCFF folder.</summary>
internal static class CaveSetCatalog
{
    public static List<CaveSet> Build()
    {
        var sets = new List<CaveSet>
        {
            new("VibeBoulderDash 1", CaveCatalog.AllCaves()),
            new("VibeBoulderDash 2", OriginalCaves.BoulderDash2(), FlyMovementRule.StraightFirst),
            new("VibeBoulderDash 3", OriginalCaves.BoulderDash3()),
        };

        var folder = BdcffFolder.Resolve("BDCFF");
        if (folder == null)
        {
            return sets;
        }

        foreach (var file in Directory.GetFiles(folder, "*.bd").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var result = BdcffParser.ParseWithErrors(File.ReadAllText(file));
                if (result.Caves.Count > 0)
                {
                    sets.Add(new CaveSet(Path.GetFileNameWithoutExtension(file), result.Caves));
                }
            }
            catch (Exception)
            {
                // Skip files that cannot be read or parsed at all.
            }
        }

        return sets;
    }
}

internal static class BdcffFolder
{
    public static string? Resolve(string relativeName)
    {
        if (Directory.Exists(relativeName))
        {
            return Path.GetFullPath(relativeName);
        }

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relativeName);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
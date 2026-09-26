namespace VibeBoulderDash.Core;

/// <summary>Loads the embedded Boulder Dash cave data sets (official releases).</summary>
internal static class OriginalCaves
{
    private const string ResourceName1 = "VibeBoulderDash.Core.Caves.BoulderDash01.bd";
    private const string ResourceName2 = "VibeBoulderDash.Core.Caves.BoulderDash02.bd";
    private const string ResourceName3 = "VibeBoulderDash.Core.Caves.BoulderDash03.bd";

    private static readonly Lazy<IReadOnlyList<CaveDefinition>> Caves1 = new(() => LoadInner(ResourceName1));
    private static readonly Lazy<IReadOnlyList<CaveDefinition>> Caves2 = new(() => LoadInner(ResourceName2));
    private static readonly Lazy<IReadOnlyList<CaveDefinition>> Caves3 = new(() => LoadInner(ResourceName3));

    /// <summary>All caves (A..P) of the original Boulder Dash I.</summary>
    public static IReadOnlyList<CaveDefinition> Load() => Caves1.Value;

    /// <summary>All 20 caves of the original Boulder Dash II.</summary>
    public static IReadOnlyList<CaveDefinition> BoulderDash2() => Caves2.Value;

    /// <summary>All 20 caves of the original Boulder Dash III.</summary>
    public static IReadOnlyList<CaveDefinition> BoulderDash3() => Caves3.Value;

    private static IReadOnlyList<CaveDefinition> LoadInner(string resourceName)
    {
        using var stream = typeof(OriginalCaves).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' is missing.");
        using var reader = new StreamReader(stream);
        return BdcffParser.Parse(reader.ReadToEnd());
    }
}
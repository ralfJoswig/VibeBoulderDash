namespace VibeBoulderDash.Core;

/// <summary>Static cave definition: grid plus per-cave metadata.</summary>
public sealed class CaveDefinition
{
    /// <summary>Display name of the cave.</summary>
    public required string Name { get; init; }

    /// <summary>Cave width in tiles (40 for the original).</summary>
    public required int Width { get; init; }

    /// <summary>Cave height in tiles (22 for the original).</summary>
    public required int Height { get; init; }

    /// <summary>Row-major tile grid, index = y * Width + x.</summary>
    public required Element[] Grid { get; init; }

    /// <summary>Optional starting facing for flying creatures, parallel to <see cref="Grid"/>. Null means Up.</summary>
    public Direction[]? InitialFacing { get; init; }

    /// <summary>Time limit in seconds.</summary>
    public int TimeSeconds { get; init; } = 200;

    /// <summary>Diamonds required to open the exit.</summary>
    public int DiamondsNeeded { get; init; } = 20;

    /// <summary>Score value of one collected diamond.</summary>
    public int DiamondValue { get; init; } = 10;

    /// <summary>Bonus score awarded when leaving the cave.</summary>
    public int BonusValue { get; init; } = 100;

    /// <summary>Start lives (0 = default of the game).</summary>
    public int StartLives { get; init; } = 0;

    /// <summary>Difficulty level (1..5).</summary>
    public int Difficulty { get; init; } = 1;

    /// <summary>Ticks per Rockford move (1 = fast).</summary>
    public int RockfordTicksPerMove { get; init; } = 2;

    /// <summary>Ticks per fly move.</summary>
    public int FlyTicksPerMove { get; init; } = 2;

    /// <summary>True for an intermission badge screen instead of a normal cave.</summary>
    public bool IsIntermission { get; init; }

    /// <summary>Maximum amoeba size in cells before it turns into boulders.</summary>
    public int AmoebaMaxSize { get; init; } = 200;

    /// <summary>Seconds the amoeba stays in slow growth before switching to rapid growth.</summary>
    public int AmoebaTime { get; init; } = 20;

    /// <summary>Ticks the magic wall stays active after activation.</summary>
    public int MagicWallTicks { get; init; } = 100;

    /// <summary>Probability per game step that a slime cell lets a boulder/diamond fall through it (0..1).</summary>
    public double SlimePermeability { get; init; } = 1.0;

    /// <summary>Every N points Rockford earns an extra life.</summary>
    public int ExtraLifeEvery { get; init; } = 10000;

    /// <summary>Border color (C64 color index), cosmetic.</summary>
    public int BorderColor { get; init; } = 0;

    /// <summary>Background color (C64 color index), cosmetic.</summary>
    public int BackgroundColor { get; init; } = 4;

    /// <summary>Foreground highlight color (C64 color index), cosmetic.</summary>
    public int Color1 { get; init; } = 11;

    /// <summary>Foreground base color (C64 color index), cosmetic.</summary>
    public int Color2 { get; init; } = 15;

    /// <summary>Foreground dark color (C64 color index), cosmetic.</summary>
    public int Color3 { get; init; } = 6;

    /// <summary>Deterministic RNG seed for amoeba growth and similar randomness.</summary>
    public int Seed { get; init; } = 12345;

    /// <summary>Game steps per simulated second (authentic cave speed, 60 = fast/full-rate).</summary>
    public int GameStepsPerSecond { get; init; } = 60;

    /// <summary>Authentic game steps per second for an original BD1 difficulty level (1..5).</summary>
    public static int GameStepsForDifficulty(int difficulty) => difficulty switch
    {
        2 => 13,
        3 => 14,
        4 => 16,
        >= 5 => 17,
        _ => 11,
    };

    /// <summary>Parses a cave from a text description. See <see cref="GridChars"/> for the charset.</summary>
    public static CaveDefinition Parse(string text) =>
        Parse(text.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Parses a cave from metadata lines followed by "grid:" and one character per tile.</summary>
    public static CaveDefinition Parse(string[] lines)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<string>();
        bool inGrid = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.Equals("grid:", StringComparison.OrdinalIgnoreCase))
            {
                inGrid = true;
                continue;
            }

            if (!inGrid)
            {
                var eq = line.IndexOf('=');
                if (eq > 0)
                {
                    metadata[line[..eq].Trim()] = line[(eq + 1)..].Trim();
                }

                continue;
            }

            rows.Add(line);
        }

        if (rows.Count == 0)
        {
            throw new FormatException("Cave definition has no grid rows.");
        }

        int height = rows.Count;
        int width = rows.Max(r => r.Length);
        var grid = new Element[width * height];
        for (int y = 0; y < height; y++)
        {
            var row = rows[y];
            for (int x = 0; x < width; x++)
            {
                var ch = x < row.Length ? row[x] : ' ';
                grid[y * width + x] = ch switch
                {
                    '.' or ' ' => Element.Space,
                    '#' => Element.Dirt,
                    'R' => Element.Rockford,
                    'B' => Element.Boulder,
                    'D' => Element.Diamond,
                    'F' => Element.Firefly,
                    'Y' => Element.Butterfly,
                    'A' => Element.Amoeba,
                    'W' => Element.Wall,
                    'M' => Element.MagicWall,
                    'T' => Element.TitaniumWall,
                    'E' => Element.Exit,
                    'S' => Element.Slime,
                    'X' => Element.ExpandingWall,
                    _ => throw new FormatException($"Unknown grid char '{row[x]}' in row {y}."),
                };
            }
        }

        int Int(string key, int fallback)
        {
            return metadata.TryGetValue(key, out var value)
                ? int.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
                : fallback;
        }

        bool Bool(string key, bool fallback)
        {
            return metadata.TryGetValue(key, out var value)
                ? bool.Parse(value)
                : fallback;
        }

        double Double(string key, double fallback)
        {
            return metadata.TryGetValue(key, out var value)
                ? double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
                : fallback;
        }

        return new CaveDefinition
        {
            Name = metadata.GetValueOrDefault("name", "Cave"),
            Width = Int("width", width),
            Height = Int("height", height),
            Grid = grid,
            TimeSeconds = Int("time", 200),
            DiamondsNeeded = Int("diamonds", 20),
            DiamondValue = Int("diamondvalue", 10),
            BonusValue = Int("bonusvalue", 100),
            StartLives = Int("lives", 0),
            Difficulty = Int("difficulty", 1),
            RockfordTicksPerMove = Int("speed", 2),
            FlyTicksPerMove = Int("flyspeed", 2),
            IsIntermission = Bool("intermission", false),
            AmoebaMaxSize = Int("amoebamaxsize", 200),
            AmoebaTime = Int("amoebatime", 20),
            MagicWallTicks = Int("magicwallticks", 100),
            SlimePermeability = Double("slimepermeability", 1.0),
            ExtraLifeEvery = Int("extralifeevery", 10000),
            BorderColor = Int("border", 0),
            BackgroundColor = Int("background", 4),
            Color1 = Int("color1", 11),
            Color2 = Int("color2", 15),
            Color3 = Int("color3", 6),
            Seed = Int("seed", 12345),
            GameStepsPerSecond = Int("gamesteps", GameStepsForDifficulty(Int("difficulty", 1))),
        };
    }

    /// <summary>Grid legend used by <see cref="Parse"/>.</summary>
    public static readonly IReadOnlyDictionary<char, Element> GridChars = new Dictionary<char, Element>
    {
        ['.'] = Element.Space,
        ['#'] = Element.Dirt,
        ['R'] = Element.Rockford,
        ['B'] = Element.Boulder,
        ['D'] = Element.Diamond,
        ['F'] = Element.Firefly,
        ['Y'] = Element.Butterfly,
        ['A'] = Element.Amoeba,
        ['W'] = Element.Wall,
        ['M'] = Element.MagicWall,
        ['T'] = Element.TitaniumWall,
        ['E'] = Element.Exit,
        ['S'] = Element.Slime,
        ['X'] = Element.ExpandingWall,
    };
}
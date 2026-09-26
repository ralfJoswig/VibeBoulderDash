namespace VibeBoulderDash.Core;

/// <summary>Parses caves in the Boulder Dash Common File Format (BDCFF).</summary>
public static class BdcffParser
{
    private static readonly Dictionary<string, Element> Elements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SPACE"] = Element.Space,
        ["DIRT"] = Element.Dirt,
        ["WALL"] = Element.Wall,
        ["WALL2"] = Element.Wall,
        ["STEELWALL"] = Element.TitaniumWall,
        ["MAGICWALL"] = Element.MagicWall,
        ["HEXPANDINGWALL"] = Element.ExpandingWall,
        ["VEXPANDINGWALL"] = Element.ExpandingWall,
        ["EXPANDINGWALL"] = Element.ExpandingWall,
        ["HEXPANDING_WALL"] = Element.ExpandingWall,
        ["VEXPANDING_WALL"] = Element.ExpandingWall,
        ["EXPANDING_WALL"] = Element.ExpandingWall,
        ["INBOX"] = Element.Rockford,
        ["OUTBOX"] = Element.Exit,
        ["HIDDENOUTBOX"] = Element.Exit,
        ["BOULDER"] = Element.Boulder,
        ["BOULDERF"] = Element.Boulder,
        ["FALLING_BOULDER"] = Element.Boulder,
        ["DIAMOND"] = Element.Diamond,
        ["DIAMONDF"] = Element.Diamond,
        ["FALLING_DIAMOND"] = Element.Diamond,
        ["AMOEBA"] = Element.Amoeba,
        ["SLIME"] = Element.Slime,
        ["FIREFLY"] = Element.Firefly,
        ["FIREFLYL"] = Element.Firefly,
        ["FIREFLYR"] = Element.Firefly,
        ["FIREFLYU"] = Element.Firefly,
        ["FIREFLYD"] = Element.Firefly,
        ["BUTTERFLY"] = Element.Butterfly,
        ["BUTTERFLYL"] = Element.Butterfly,
        ["BUTTERFLYR"] = Element.Butterfly,
        ["BUTTERFLYU"] = Element.Butterfly,
        ["BUTTERFLYD"] = Element.Butterfly,
    };

    private static readonly Dictionary<char, (Element Element, Direction Facing)> DefaultMapCodes = new()
    {
        ['W'] = (Element.TitaniumWall, Direction.Up),
        ['w'] = (Element.Wall, Direction.Up),
        ['M'] = (Element.MagicWall, Direction.Up),
        ['X'] = (Element.Exit, Direction.Up),
        ['H'] = (Element.Exit, Direction.Up),
        ['P'] = (Element.Rockford, Direction.Up),
        ['r'] = (Element.Boulder, Direction.Up),
        ['d'] = (Element.Diamond, Direction.Up),
        ['x'] = (Element.ExpandingWall, Direction.Up),
        ['v'] = (Element.ExpandingWall, Direction.Up),
        ['V'] = (Element.ExpandingWall, Direction.Up),
        ['a'] = (Element.Amoeba, Direction.Up),
        ['s'] = (Element.Slime, Direction.Up),
        ['.'] = (Element.Dirt, Direction.Up),
        ['Q'] = (Element.Firefly, Direction.Left),
        ['o'] = (Element.Firefly, Direction.Up),
        ['O'] = (Element.Firefly, Direction.Right),
        ['q'] = (Element.Firefly, Direction.Down),
        ['F'] = (Element.Firefly, Direction.Up),
        ['c'] = (Element.Butterfly, Direction.Down),
        ['C'] = (Element.Butterfly, Direction.Left),
        ['b'] = (Element.Butterfly, Direction.Up),
        ['B'] = (Element.Butterfly, Direction.Right),
        [' '] = (Element.Space, Direction.Up),
    };

    private sealed class MapCodes
    {
        public int Length = 1;
        public readonly Dictionary<string, Element> Overrides = new(StringComparer.Ordinal);

        public (Element Element, Direction Facing, int Consumed) Decode(string row, int x)
        {
            if (Length == 2 && x + 1 < row.Length && Overrides.TryGetValue(row.Substring(x, 2), out var pairElement))
            {
                return (pairElement, Direction.Up, 2);
            }

            if (Overrides.TryGetValue(row[x].ToString(), out var single))
            {
                return (single, Direction.Up, 1);
            }

            if (DefaultMapCodes.TryGetValue(row[x], out var value))
            {
                return (value.Element, value.Facing, 1);
            }

            throw new FormatException($"Unknown map code '{row[x]}' at column {x}.");
        }
    }

    private static readonly Dictionary<string, int> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Black"] = 0,
        ["White"] = 1,
        ["Red"] = 2,
        ["Cyan"] = 3,
        ["Purple"] = 4,
        ["Green"] = 5,
        ["Blue"] = 6,
        ["Yellow"] = 7,
        ["Orange"] = 8,
        ["Brown"] = 9,
        ["LightRed"] = 10,
        ["Gray1"] = 11,
        ["Gray2"] = 12,
        ["Gray3"] = 15,
        ["Grey1"] = 11,
        ["Grey2"] = 12,
        ["Grey3"] = 15,
        ["LightGreen"] = 13,
        ["LightBlue"] = 14,
        ["LightGrey"] = 15,
    };

    /// <summary>Parses a BDCFF document into cave definitions (first level of each cave).</summary>
    public static IReadOnlyList<CaveDefinition> Parse(string text)
    {
        var caves = new List<CaveDefinition>();
        Parse(text, caves, null);
        return caves;
    }

    /// <summary>Parses a BDCFF document, keeping playable caves and collecting per-cave errors.</summary>
    public static BdcffParseResult ParseWithErrors(string text)
    {
        var caves = new List<CaveDefinition>();
        var errors = new List<string>();
        Parse(text, caves, errors);
        return new BdcffParseResult(caves, errors);
    }

    private static void Parse(string text, List<CaveDefinition> result, List<string>? errors)
    {
        var lines = text.ReplaceLineEndings("\n").Split('\n');

        CaveBuilder? cave = null;
        string? skipUntil = null;
        bool inObjects = false;
        bool inMap = false;
        bool inMapCodes = false;
        bool inDemo = false;
        var mapCodes = new MapCodes();

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';'))
            {
                continue;
            }

            if (line.StartsWith('['))
            {
                var closing = line.StartsWith("[/");
                var name = line.Trim('[', ']', '/').ToLowerInvariant();

                if (closing)
                {
                    switch (name)
                    {
                        case "objects":
                            inObjects = false;
                            break;
                        case "map":
                            inMap = false;
                            break;
                        case "mapcodes":
                            inMapCodes = false;
                            break;
                        case "demo":
                            inDemo = false;
                            break;
                        case "cave":
                            if (cave != null)
                            {
                                try
                                {
                                    result.Add(cave.Build());
                                }
                                catch (FormatException ex) when (errors != null)
                                {
                                    errors.Add(ex.Message);
                                }

                                cave = null;
                            }

                            break;
                        case "game":
                        case "bdcff":
                        case "color":
                            skipUntil = null;
                            break;
                        default:
                            if (name == skipUntil)
                            {
                                skipUntil = null;
                            }

                            break;
                    }

                    continue;
                }

                if (skipUntil == null)
                {
                    switch (name)
                    {
                        case "objects":
                            inObjects = true;
                            continue;
                        case "map":
                            inMap = true;
                            continue;
                        case "mapcodes":
                            inMapCodes = true;
                            continue;
                        case "demo":
                            inDemo = true;
                            continue;
                        case "cave":
                            cave = new CaveBuilder(mapCodes);
                            continue;
                        case "bdcff":
                        case "game":
                        case "color":
                            continue;
                        default:
                            skipUntil = name;
                            continue;
                    }
                }
            }

            if (skipUntil != null || inDemo)
            {
                continue;
            }

            if (inMap)
            {
                cave?.AddMapRow(line);
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();

            if (inObjects)
            {
                cave?.AddObject(key, value);
                continue;
            }

            if (inMapCodes)
            {
                if (key.Equals("Length", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var length))
                {
                    mapCodes.Length = Math.Clamp(length, 1, 2);
                    continue;
                }

                try
                {
                    mapCodes.Overrides[key] = MapElement(value);
                }
                catch (ArgumentException ex)
                {
                    throw new FormatException($"Invalid [mapcodes] line '{line}': {ex.Message}");
                }

                continue;
            }

            cave?.SetAttribute(key, value);
        }
    }

    private sealed class CaveBuilder
    {
        private readonly Dictionary<string, string> _attributes = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<(string Kind, string[] Args)> _objects = new();
        private readonly MapCodes _mapCodes;
        private readonly List<string> _mapRows = new();
        private Direction[]? _initialFacing;
        private C64Random _random = new(0);

        public CaveBuilder(MapCodes mapCodes)
        {
            _mapCodes = mapCodes;
        }

        public void SetAttribute(string key, string value)
        {
            _attributes[key] = value;
            if (key.Equals("RandSeed", StringComparison.OrdinalIgnoreCase))
            {
                _random = new C64Random((byte)FirstInt(value, 0));
            }
        }

        public void AddObject(string kind, string value)
        {
            var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _objects.Add((kind.ToLowerInvariant(), tokens));
        }

        public void AddMapRow(string row)
        {
            _mapRows.Add(row);
        }

        public CaveDefinition Build()
        {
            Element fill = _attributes.TryGetValue("InitialFill", out var fillName)
                ? MapElement(fillName)
                : Element.Dirt;

            var isMapCave = _mapRows.Count > 0;
            int width = isMapCave ? SizeWidth() : 40;
            int height = isMapCave ? SizeHeight() : 22;

            var intermission = BoolAttr("Intermission");

            var grid = new Element[width * height];
            Array.Fill(grid, fill);
            _initialFacing = new Direction[width * height];
            Array.Fill(_initialFacing, Direction.Up);

            if (isMapCave)
            {
                ApplyMap(grid, width, height);
            }
            else
            {
                ApplyRandomFill(grid, width, height);
                DrawBorder(grid, width, height);
            }

            foreach (var (kind, args) in _objects)
            {
                ApplyObject(grid, width, height, kind, args);
            }

            if (grid.Count(e => e == Element.Rockford) != 1)
            {
                throw new FormatException($"Cave '{Name}' must contain exactly one INBOX.");
            }

            if (grid.Count(e => e == Element.Exit) != 1)
            {
                throw new FormatException($"Cave '{Name}' must contain exactly one OUTBOX.");
            }

            var colors = _attributes.TryGetValue("Colors", out var colorsValue)
                ? colorsValue.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

            var gameStepsPerSecond = intermission ? 16 : CaveDefinition.GameStepsForDifficulty(1);
            return new CaveDefinition
            {
                Name = Name,
                Width = width,
                Height = height,
                Grid = grid,
                InitialFacing = _initialFacing,
                TimeSeconds = FirstInt(_attributes.GetValueOrDefault("CaveTime"), 200),
                DiamondsNeeded = FirstInt(_attributes.GetValueOrDefault("DiamondsRequired"), 20),
                DiamondValue = FirstInt(_attributes.GetValueOrDefault("DiamondValue"), 10),
                BonusValue = Bonus(_attributes.GetValueOrDefault("DiamondValue")),
                StartLives = FirstInt(_attributes.GetValueOrDefault("Lives"), 0),
                ExtraLifeEvery = FirstInt(_attributes.GetValueOrDefault("BonusLife"), 500),
                IsIntermission = intermission,
                MagicWallTicks = FirstInt(_attributes.GetValueOrDefault("MagicWallTime"), 11) * gameStepsPerSecond,
                Seed = FirstInt(_attributes.GetValueOrDefault("RandSeed"), 0),
                AmoebaTime = FirstInt(_attributes.GetValueOrDefault("AmoebaTime"), 20),
                SlimePermeability = DoubleAttr("SlimePermeability", 1.0),
                RockfordTicksPerMove = 1,
                FlyTicksPerMove = 1,
                GameStepsPerSecond = gameStepsPerSecond,
                BorderColor = ColorAt(colors, 0),
                BackgroundColor = ColorAt(colors, 1),
                Color1 = ColorAt(colors, 2),
                Color2 = 15,
                Color3 = 6,
            };
        }

        private string Name => _attributes.GetValueOrDefault("Name", "(unnamed)");

        private int SizeWidth()
        {
            if (_attributes.TryGetValue("Size", out var value))
            {
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return ParseInt(parts[0]);
                }
            }

            return _mapRows.Count > 0 ? _mapRows[0].Length : 40;
        }

        private int SizeHeight()
        {
            if (_attributes.TryGetValue("Size", out var value))
            {
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    return ParseInt(parts[1]);
                }
            }

            return _mapRows.Count;
        }

        private void ApplyMap(Element[] grid, int width, int height)
        {
            for (int y = 0; y < _mapRows.Count && y < height; y++)
            {
                var row = _mapRows[y];
                for (int x = 0; x < row.Length && x < width; x++)
                {
                    (var element, var facing, var consumed) = _mapCodes.Decode(row, x);
                    grid[y * width + x] = element;
                    if (element.IsFly())
                    {
                        _initialFacing![y * width + x] = facing;
                    }

                    x += consumed - 1;
                }
            }
        }

        private void ApplyRandomFill(Element[] grid, int width, int height)
        {
            if (_attributes.TryGetValue("RandomFill", out var value))
            {
                ApplyRandomFill(grid, width, height, value);
            }
        }

        private void ApplyRandomFill(Element[] grid, int width, int height, string value)
        {
            var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var fills = new List<(Element Element, int Threshold)>();
            for (int i = 0; i + 1 < tokens.Length; i += 2)
            {
                fills.Add((MapElement(tokens[i]), ParseInt(tokens[i + 1])));
            }

            fills.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));

            for (int y = 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var draw = _random.Next();
                    foreach (var (element, threshold) in fills)
                    {
                        if (draw < threshold)
                        {
                            grid[y * width + x] = element;
                            break;
                        }
                    }
                }
            }
        }

        private void ApplyObject(Element[] grid, int width, int height, string kind, string[] args)
        {
            Element element;
            try
            {
                switch (kind)
                {
                    case "randomfill":
                        ApplyRandomFill(grid, width, height, string.Join(' ', args));
                        return;
                    case "point":
                        Set(grid, _initialFacing, width, height, ParseInt(args[0]), ParseInt(args[1]), MapElement(args[2]), FacingFor(args[2]));
                        return;
                    case "line":
                        DrawLine(
                            grid,
                            _initialFacing,
                            width,
                            height,
                            ParseInt(args[0]),
                            ParseInt(args[1]),
                            ParseInt(args[2]),
                            ParseInt(args[3]),
                            MapElement(args[4]));
                        return;
                    case "fillrect":
                    {
                        var x1 = ParseInt(args[0]);
                        var y1 = ParseInt(args[1]);
                        var x2 = ParseInt(args[2]);
                        var y2 = ParseInt(args[3]);
                        element = MapElement(args[4]);
                        var facing = FacingFor(args[4]);
                        Element? second = args.Length > 5 ? MapElement(args[5]) : null;
                        for (int y = y1; y <= y2; y++)
                        {
                            for (int x = x1; x <= x2; x++)
                            {
                                if (IsInside(width, height, x, y))
                                {
                                    bool onBorder = x == x1 || x == x2 || y == y1 || y == y2;
                                    Set(grid, _initialFacing, width, height, x, y, second.HasValue && !onBorder ? second.Value : element, second.HasValue && !onBorder ? Direction.Up : facing);
                                }
                            }
                        }

                        return;
                    }
                    case "rectangle":
                    {
                        var x1 = ParseInt(args[0]);
                        var y1 = ParseInt(args[1]);
                        var x2 = ParseInt(args[2]);
                        var y2 = ParseInt(args[3]);
                        element = MapElement(args[4]);
                        DrawLine(grid, _initialFacing, width, height, x1, y1, x2, y1, element);
                        DrawLine(grid, _initialFacing, width, height, x1, y2, x2, y2, element);
                        DrawLine(grid, _initialFacing, width, height, x1, y1, x1, y2, element);
                        DrawLine(grid, _initialFacing, width, height, x2, y1, x2, y2, element);
                        return;
                    }
                    case "raster":
                    {
                        var x = ParseInt(args[0]);
                        var y = ParseInt(args[1]);
                        var numberX = ParseInt(args[2]);
                        var numberY = ParseInt(args[3]);
                        var stepX = ParseInt(args[4]);
                        var stepY = ParseInt(args[5]);
                        element = MapElement(args[6]);
                        var facing = FacingFor(args[6]);
                        for (int j = 0; j < numberY; j++)
                        {
                            for (int i = 0; i < numberX; i++)
                            {
                                Set(grid, _initialFacing, width, height, x + i * stepX, y + j * stepY, element, facing);
                            }
                        }

                        return;
                    }
                    case "add":
                    {
                        var incX = ParseInt(args[0]);
                        var incY = ParseInt(args[1]);
                        var search = MapElement(args[2]);
                        var add = MapElement(args[3]);
                        var addFacing = FacingFor(args[3]);
                        for (int cell = 0; cell < grid.Length; cell++)
                        {
                            if (grid[cell] != search)
                            {
                                continue;
                            }

                            var cx = cell % width + incX;
                            var cy = cell / width + incY;
                            Set(grid, _initialFacing, width, height, cx, cy, add, addFacing);
                        }

                        return;
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new FormatException($"Cave '{Name}' uses an unknown element in '{kind} {string.Join(' ', args)}'.");
            }
        }

        private bool BoolAttr(string key)
        {
            return _attributes.TryGetValue(key, out var value) && bool.Parse(value);
        }

        private double DoubleAttr(string key, double fallback)
        {
            return _attributes.TryGetValue(key, out var value)
                ? double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
                : fallback;
        }

        private int ColorAt(string[] colors, int index)
        {
            return index < colors.Length && Colors.TryGetValue(colors[index], out var value) ? value : 0;
        }

        private int Bonus(string? value)
        {
            if (value == null)
            {
                return 100;
            }

            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? ParseInt(parts[1]) : FirstInt(value, 100);
        }
    }

    private static Element MapElement(string name)
    {
        if (Elements.TryGetValue(name, out var element))
        {
            return element;
        }

        var upper = name.ToUpperInvariant();
        if (upper.StartsWith("EXPLOSION", StringComparison.Ordinal))
        {
            return Element.Space;
        }

        if (upper.StartsWith("SCANNED_", StringComparison.Ordinal))
        {
            var baseName = upper["SCANNED_".Length..];
            if (Elements.TryGetValue(baseName, out element))
            {
                return element;
            }
        }

        throw new ArgumentException($"Unknown BDCFF element '{name}'.");
    }

    private static int FirstInt(string? value, int fallback)
    {
        if (value == null)
        {
            return fallback;
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? ParseInt(parts[0]) : fallback;
    }

    private static int ParseInt(string token)
    {
        return int.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool IsInside(int width, int height, int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private static Direction FacingFor(string name)
    {
        return name.ToUpperInvariant() switch
        {
            "FIREFLYL" or "BUTTERFLYL" or "SCANNED_FIREFLYL" or "SCANNED_BUTTERFLYL" => Direction.Left,
            "FIREFLYR" or "BUTTERFLYR" or "SCANNED_FIREFLYR" or "SCANNED_BUTTERFLYR" => Direction.Right,
            "FIREFLYU" or "BUTTERFLYU" or "SCANNED_FIREFLYU" or "SCANNED_BUTTERFLYU" => Direction.Up,
            "FIREFLYD" or "BUTTERFLYD" or "SCANNED_FIREFLYD" or "SCANNED_BUTTERFLYD" => Direction.Down,
            _ => Direction.Up,
        };
    }

    private static void Set(Element[] grid, Direction[]? facing, int width, int height, int x, int y, Element element, Direction direction = Direction.Up)
    {
        if (IsInside(width, height, x, y))
        {
            grid[y * width + x] = element;
            if (facing != null && element.IsFly())
            {
                facing[y * width + x] = direction;
            }
        }
    }

    private static void DrawBorder(Element[] grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            grid[x] = Element.TitaniumWall;
            grid[(height - 1) * width + x] = Element.TitaniumWall;
        }

        for (int y = 0; y < height; y++)
        {
            grid[y * width] = Element.TitaniumWall;
            grid[y * width + width - 1] = Element.TitaniumWall;
        }
    }

    private static void DrawLine(Element[] grid, Direction[]? facing, int width, int height, int x1, int y1, int x2, int y2, Element element)
    {
        var dx = Math.Abs(x2 - x1);
        var dy = -Math.Abs(y2 - y1);
        var sx = x1 < x2 ? 1 : -1;
        var sy = y1 < y2 ? 1 : -1;
        var error = dx + dy;
        while (true)
        {
            Set(grid, facing, width, height, x1, y1, element);
            if (x1 == x2 && y1 == y2)
            {
                return;
            }

            var twice = 2 * error;
            if (twice >= dy)
            {
                error += dy;
                x1 += sx;
            }

            if (twice <= dx)
            {
                error += dx;
                y1 += sy;
            }
        }
    }
}

/// <summary>Result of a BDCFF parse that keeps playable caves and reports per-cave errors.</summary>
public sealed class BdcffParseResult
{
    /// <summary>Creates a result holding playable caves and the reported errors.</summary>
    public BdcffParseResult(IReadOnlyList<CaveDefinition> caves, IReadOnlyList<string> errors)
    {
        Caves = caves;
        Errors = errors;
    }

    /// <summary>Playable caves of the document.</summary>
    public IReadOnlyList<CaveDefinition> Caves { get; }

    /// <summary>Human-readable errors for caves that could not be played.</summary>
    public IReadOnlyList<string> Errors { get; }
}
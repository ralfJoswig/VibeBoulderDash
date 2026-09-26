namespace VibeBoulderDash.Core;

internal static class CaveCatalog
{
    public static IReadOnlyList<CaveDefinition> AllCaves() => OriginalCaves.Load();

    public static CaveDefinition DemoCave()
    {
        return CaveDefinition.Parse(
            new[]
            {
                "name = Demo Cave",
                "time = 120",
                "diamonds = 8",
                "diamondvalue = 25",
                "bonusvalue = 150",
                "amoebamaxsize = 30",
                "grid:",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
                "W......................................W",
                "W.R##..DD..###................B........W",
                "W.####..D...###.......MMM....BB......F.W",
                "W.#......D.........TTT.................W",
                "W..............B...TTT..........YYYY...W",
                "W...#####.........B...F................W",
                "W...#...#.............W.Y.W............W",
                "W...#...#.............W...W............W",
                "W##..#..#......MMMM...T...T............W",
                "W#...#...#......M..M...T...T...........W",
                "W###...###......MMMM...T...T...........W",
                "W...#####.....................#......E.W",
                "W..#....#.....................#........W",
                "W.#......#....DDDDDDD.....#...#..AAA...W",
                "W##......################....#...AAA.A.W",
                "W........#...........T..#..........AAA.W",
                "W......T.#.......B....T..#.............W",
                "W......................................W",
                "W....Y.................................W",
                "W.F...........................T........W",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
            });
    }

    public static CaveDefinition FireflyMaze()
    {
        return CaveDefinition.Parse(
            new[]
            {
                "name = Firefly Maze",
                "time = 100",
                "diamonds = 6",
                "diamondvalue = 25",
                "bonusvalue = 200",
                "grid:",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
                "W......................................W",
                "W.R......F....DD..F....Y..............WW",
                "W......#####....##....#................W",
                "W..F..........M..........F............WW",
                "W.......D....M.M...DD..................W",
                "W..####......M.M......##...............W",
                "W..#..####......M......................W",
                "W..#....####.........########..........W",
                "W..#........#...............Y.........WW",
                "W..#####....#.F..........#.............W",
                "W...........####......###..DDD.........W",
                "W....Y..............####...E...........W",
                "W......................................W",
                "W..T....T....##....#..D....Y.....F....WW",
                "W..T....T....#.....###.................W",
                "W..T....T....##.......................WW",
                "W......................................W",
                "W..B..B..B...DDDD..B..BB..DDDD........WW",
                "W......................................W",
                "W....F............Y...........F........W",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
            });
    }

    public static CaveDefinition AmoebaLab()
    {
        return CaveDefinition.Parse(
            new[]
            {
                "name = Amoeba Lab",
                "time = 90",
                "diamonds = 10",
                "diamondvalue = 25",
                "bonusvalue = 300",
                "amoebamaxsize = 40",
                "seed = 7",
                "grid:",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
                "W......................................W",
                "W.R######......DDDD.....AAA....D.......W",
                "W.##....#......D..D.....A.A....D.D.....W",
                "W.#..B..#..............AAA............WW",
                "W.##....#.................AAA..........W",
                "W..####.................D.D...D.......WW",
                "W................M.....#.#.....#.......W",
                "W................M.M....###..F........WW",
                "W........DD.....M.M......#...F........WW",
                "W................M.......####..........W",
                "W..TT.....DD....MMM......#............WW",
                "W..TT..DD........T#T....T.............WW",
                "W.......DD......T..#....T.....Y....E...W",
                "W.....DD....B..T..........T...........WW",
                "W................T....####............WW",
                "W...B...B........T....#..DD..Y........WW",
                "W................T....#...............WW",
                "W.....DDD....T.....T.D.D..DD..........WW",
                "W......................T..............WW",
                "W........F.....D.....Y.......D...F.....W",
                "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
            });
    }
}

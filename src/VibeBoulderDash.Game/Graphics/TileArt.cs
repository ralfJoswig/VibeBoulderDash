using System.Collections.Generic;
using VibeBoulderDash.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBoulderDash.Graphics;

internal sealed class TileArt
{
    private const int Size = 32;
    private const int Cols = 8;
    private const int Rows = 16;
    private const int BlockW = Size / Cols;
    private const int BlockH = Size / Rows;

    private readonly Dictionary<Element, Texture2D[]> _tiles;

    public TileArt(GraphicsDevice device)
    {
        _tiles = new Dictionary<Element, Texture2D[]>
        {
            [Element.Dirt] = new[] { Build(device, Grids.Dirt, DirtPalette) },
            [Element.Wall] = new[] { Build(device, Grids.Wall, WallPalette) },
            [Element.TitaniumWall] = new[] { Build(device, Grids.Titanium, SteelPalette) },
            [Element.ExpandingWall] = new[] { Build(device, Grids.Wall, WallPalette) },
            [Element.MagicWall] = new[]
            {
                Build(device, Grids.MagicWallRest, MagicPalette),
                Build(device, Grids.MagicWallMill1, MagicPalette),
                Build(device, Grids.MagicWallMill2, MagicPalette),
            },
            [Element.Rockford] = new[]
            {
                Build(device, Grids.RockfordA, HeroPalette),
                Build(device, Grids.RockfordWalk1, HeroPalette),
                Build(device, Grids.RockfordWalk2, HeroPalette),
                Build(device, Grids.RockfordWalk3, HeroPalette),
                Build(device, Grids.RockfordWalk4, HeroPalette),
                Build(device, Grids.RockfordBlink, HeroPalette),
                Build(device, Grids.RockfordTap, HeroPalette),
                Build(device, Grids.RockfordTapBlink, HeroPalette),
            },
            [Element.Boulder] = new[] { Build(device, Grids.Boulder, BoulderPalette) },
            [Element.Diamond] = new[] { Build(device, Grids.DiamondA, DiamondPalette), Build(device, Grids.DiamondB, DiamondPalette) },
            [Element.Firefly] = new[] { Build(device, Grids.FireflyA, FireflyPalette), Build(device, Grids.FireflyB, FireflyPalette) },
            [Element.Butterfly] = new[] { Build(device, Grids.ButterflyA, ButterflyPalette), Build(device, Grids.ButterflyB, ButterflyPalette) },
            [Element.Amoeba] = new[] { Build(device, Grids.AmoebaA, AmoebaPalette), Build(device, Grids.AmoebaB, AmoebaPalette) },
            [Element.Slime] = new[] { Build(device, Grids.AmoebaA, SlimePalette), Build(device, Grids.AmoebaB, SlimePalette) },
            [Element.Exit] = new[]
            {
                Build(device, Grids.Titanium, SteelPalette),
                Build(device, Grids.ExitOpen, SteelPalette),
                Build(device, Grids.ExitOpen, SteelPalette),
            },
        };
    }

    public Texture2D[] Get(Element element) => _tiles[element];

    internal static readonly Color[] DirtPalette = new[] { Opaque, Rgb(0xa0, 0x67, 0x2f), Rgb(0xc9, 0x8a, 0x4a), Opaque };
    internal static readonly Color[] WallPalette = new[] { Opaque, Rgb(0x6e, 0x76, 0x82), Rgb(0xc9, 0xcf, 0xd4), Opaque };
    internal static readonly Color[] SteelPalette = new[] { Opaque, Rgb(0x8a, 0x93, 0xa0), Rgb(0xff, 0xff, 0xff), Rgb(0x66, 0x6e, 0x7a) };
    internal static readonly Color[] MagicPalette = new[] { Opaque, Rgb(0xd8, 0xb4, 0x5a), Rgb(0xff, 0xf0, 0xb0), Opaque };
    internal static readonly Color[] BoulderPalette = new[] { Opaque, Rgb(0x8a, 0x93, 0xa0), Rgb(0xff, 0xff, 0xff), Rgb(0x56, 0x5d, 0x66) };
    internal static readonly Color[] DiamondPalette = new[] { Rgb(0x3a, 0x2a, 0x10), Rgb(0xe8, 0xc6, 0x4a), Rgb(0xff, 0xff, 0xff), Rgb(0xc8, 0x9a, 0x40) };
    internal static readonly Color[] HeroPalette = new[] { Clear, Rgb(0x4a, 0x6c, 0xd8), Rgb(0xf0, 0xd0, 0xa8), Rgb(0x9a, 0x5a, 0x30) };
    internal static readonly Color[] FireflyPalette = new[] { Clear, Rgb(0xd8, 0x43, 0x2a), Rgb(0xff, 0xff, 0xff), Rgb(0x6e, 0x1c, 0x0e) };
    internal static readonly Color[] ButterflyPalette = new[] { Clear, Rgb(0xa8, 0x5a, 0xa8), Rgb(0xff, 0xff, 0xff), Rgb(0x6f, 0x2e, 0x7b) };
    internal static readonly Color[] AmoebaPalette = new[] { Rgb(0x10, 0x35, 0x1a), Rgb(0x3f, 0x9c, 0x47), Rgb(0x86, 0xc0, 0x6a), Rgb(0x1b, 0x4a, 0x28) };
    internal static readonly Color[] SlimePalette = new[] { Rgb(0x0a, 0x16, 0x30), Rgb(0x24, 0x4f, 0x8a), Rgb(0x4f, 0x82, 0xc9), Rgb(0x9f, 0xd0, 0xff) };
    internal static readonly Color[] ExplosionPalette = new[] { Clear, Rgb(0xff, 0xff, 0xff), Rgb(0xb8, 0x58, 0xf0), Rgb(0x6e, 0x2e, 0x9a) };
    internal static readonly Color[] PathwayPalette = new[] { Clear, Rgb(0xff, 0xff, 0xff), Rgb(0xff, 0xff, 0xff), Rgb(0xff, 0xff, 0xff) };
    internal static readonly Color[] StreakPalette = new[] { Clear, Rgb(0x52, 0x4b, 0x45), Rgb(0x9e, 0x97, 0x90), Rgb(0xe0, 0xd8, 0xd2) };

    private static readonly Color Opaque = Color.Black;
    private static readonly Color Clear = Color.Transparent;

    private static Color Rgb(byte r, byte g, byte b) => new Color(r, g, b);

    internal static Color[] ToColors(string[] grid, Color[] palette)
    {
        var pixels = new Color[Size * Size];
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                FillBlock(pixels, col, row, palette[grid[row][col] - '0']);
            }
        }

        return pixels;
    }

    private static void FillBlock(Color[] pixels, int col, int row, Color color)
    {
        for (int dy = 0; dy < BlockH; dy++)
        {
            for (int dx = 0; dx < BlockW; dx++)
            {
                pixels[(row * BlockH + dy) * Size + col * BlockW + dx] = color;
            }
        }
    }

    internal static Texture2D Build(GraphicsDevice device, string[] grid, Color[] palette) => ToTexture(device, ToColors(grid, palette));

    internal static Texture2D FromColors(GraphicsDevice device, Color[] colors) => ToTexture(device, colors);

    private static Texture2D ToTexture(GraphicsDevice device, Color[] pixels)
    {
        var texture = new Texture2D(device, Size, Size);
        texture.SetData(pixels);
        return texture;
    }

    internal static class Grids
    {
        public static readonly string[] Dirt =
        {
            "00100101",
            "11001001",
            "01210120",
            "11011101",
            "10111011",
            "02101210",
            "21122011",
            "10111120",
            "01112111",
            "12011120",
            "12102101",
            "02110121",
            "10101010",
            "11001101",
            "10120100",
            "01001010",
        };

        public static readonly string[] Wall =
        {
            "02220222",
            "01220122",
            "01110111",
            "00000000",
            "22022202",
            "22012201",
            "11011101",
            "00000000",
            "02220222",
            "01220122",
            "01110111",
            "00000000",
            "22022202",
            "22012201",
            "11011101",
            "00000000",
        };

        public static readonly string[] Titanium =
        {
            "11111111",
            "11111111",
            "10011001",
            "11011101",
            "12011201",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "10011001",
            "11011101",
            "12011201",
            "11111111",
            "11111111",
            "11111111",
        };

        public static readonly string[] MagicWallRest = Wall;

        public static readonly string[] MagicWallMill1 =
        {
            "22222222",
            "21222122",
            "21112111",
            "20002000",
            "22022202",
            "22022202",
            "11021102",
            "00020002",
            "22222222",
            "21222122",
            "21112111",
            "20002000",
            "22022202",
            "22022202",
            "11021102",
            "00020002",
        };

        public static readonly string[] MagicWallMill2 =
        {
            "02220222",
            "01220122",
            "01210121",
            "00200020",
            "22022202",
            "22012201",
            "12011201",
            "02000200",
            "02220222",
            "01220122",
            "01210121",
            "00200020",
            "22022202",
            "22012201",
            "12011201",
            "02000200",
        };

        public static readonly string[] ExitOpen =
        {
            "11111111",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "10000001",
            "11111111",
        };

        public static readonly string[] FireflyA =
        {
            "11111111",
            "11111111",
            "10000001",
            "10000001",
            "10111101",
            "10111101",
            "10122101",
            "10122101",
            "10122101",
            "10122101",
            "10111101",
            "10111101",
            "10000001",
            "10000001",
            "11111111",
            "11111111",
        };

        public static readonly string[] FireflyB =
        {
            "00000000",
            "00000000",
            "01111110",
            "01111110",
            "01222210",
            "01222210",
            "01211210",
            "01211210",
            "01211210",
            "01211210",
            "01222210",
            "01222210",
            "01111110",
            "01111110",
            "00000000",
            "00000000",
        };

        public static readonly string[] Boulder =
        {
            "00222000",
            "02123200",
            "23212220",
            "12313222",
            "11131222",
            "13111312",
            "11331122",
            "11111312",
            "10111113",
            "11011313",
            "10111111",
            "11011111",
            "11101110",
            "01111110",
            "00103100",
            "00011000",
        };

        public static readonly string[] DiamondA =
        {
            "00012000",
            "00031000",
            "00122200",
            "00322100",
            "01111120",
            "03333310",
            "10000002",
            "30000001",
            "13333332",
            "31111111",
            "01222220",
            "03222210",
            "00111200",
            "00333100",
            "00031000",
            "00033000",
        };

        public static readonly string[] DiamondB =
        {
            "00012000",
            "00031000",
            "00111200",
            "00333100",
            "01000020",
            "03000010",
            "13333332",
            "31111111",
            "12222222",
            "32222221",
            "01111120",
            "03333310",
            "00100200",
            "00300100",
            "00031000",
            "00033000",
        };

        public static readonly string[] ButterflyA =
        {
            "20000001",
            "10000003",
            "22000012",
            "21000032",
            "11200111",
            "33100333",
            "00021000",
            "00013000",
            "33321333",
            "11113111",
            "22200122",
            "22100322",
            "12000011",
            "31000033",
            "10000003",
            "30000003",
        };

        public static readonly string[] ButterflyB =
        {
            "02000010",
            "02000010",
            "01000030",
            "01000030",
            "01200110",
            "03100330",
            "00021000",
            "00013000",
            "03321330",
            "01113110",
            "02200120",
            "02100320",
            "02000010",
            "01000030",
            "01000010",
            "03000030",
        };

        public static readonly string[] AmoebaA =
        {
            "11112111",
            "11111111",
            "21111111",
            "02111112",
            "02111112",
            "02111112",
            "21111111",
            "11111111",
            "11111111",
            "21111112",
            "02111120",
            "02111120",
            "02111120",
            "21111112",
            "11111111",
            "11111111",
        };

        public static readonly string[] AmoebaB =
        {
            "11200021",
            "11120211",
            "11112111",
            "11111112",
            "21111120",
            "11111112",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "11112111",
            "11120211",
            "11200021",
        };

        public static readonly string[] RockfordA =
        {
            "00000000",
            "00100100",
            "00111100",
            "01011010",
            "01011010",
            "00111100",
            "00011000",
            "00111100",
            "01022010",
            "02011020",
            "00022000",
            "00011000",
            "00322300",
            "00300300",
            "00300300",
            "02200220",
        };

        public static readonly string[] RockfordWalk1 =
        {
            "00000000",
            "00011000",
            "00111100",
            "01011100",
            "01011100",
            "00111100",
            "00011000",
            "00011000",
            "00022000",
            "00211000",
            "00022000",
            "00011000",
            "00322300",
            "00320030",
            "00320030",
            "02200220",
        };

        public static readonly string[] RockfordWalk2 =
        {
            "00000000",
            "00011000",
            "00111100",
            "01011100",
            "01011100",
            "00111100",
            "00011000",
            "00011000",
            "00022000",
            "00211000",
            "00022000",
            "00011000",
            "00322330",
            "03000002",
            "03000002",
            "22000000",
        };

        public static readonly string[] RockfordWalk3 =
        {
            "00000000",
            "00011000",
            "00111100",
            "01011100",
            "01011100",
            "00111100",
            "00011000",
            "00011000",
            "00022000",
            "00211000",
            "00022000",
            "00011000",
            "00322300",
            "00322030",
            "00322030",
            "02200220",
        };

        public static readonly string[] RockfordWalk4 =
        {
            "00000000",
            "00011000",
            "00111100",
            "01011100",
            "01011100",
            "00111100",
            "00011000",
            "00011000",
            "00022000",
            "00211000",
            "00022000",
            "00011000",
            "00322300",
            "00320002",
            "00320002",
            "02200000",
        };

        public static readonly string[] RockfordBlink =
        {
            "00000000",
            "00000000",
            "00111100",
            "01011010",
            "01011010",
            "00111100",
            "00011000",
            "00111100",
            "01022010",
            "02011020",
            "00022000",
            "00011000",
            "00322300",
            "00300300",
            "00300300",
            "02200220",
        };

        public static readonly string[] RockfordTap =
        {
            "00000000",
            "00100100",
            "00111100",
            "01011010",
            "01011010",
            "00111100",
            "00011000",
            "00111100",
            "01022010",
            "02011020",
            "00022000",
            "00011000",
            "00322300",
            "00300300",
            "00300300",
            "02200000",
        };

        public static readonly string[] RockfordTapBlink =
        {
            "00000000",
            "00000000",
            "00111100",
            "01011010",
            "01011010",
            "00111100",
            "00011000",
            "00111100",
            "01022010",
            "02011020",
            "00022000",
            "00011000",
            "00322300",
            "00300300",
            "00300300",
            "02200000",
        };

        public static readonly string[] Explosion1 =
        {
            "00000000",
            "00000000",
            "01011010",
            "00010202",
            "00000000",
            "00000000",
            "00002020",
            "22220100",
            "01030202",
            "00000101",
            "00000000",
            "00002010",
            "30100020",
            "00000000",
            "00000000",
            "00000000",
        };

        public static readonly string[] Explosion2 =
        {
            "00000000",
            "00000101",
            "20200001",
            "00001010",
            "00000000",
            "00000000",
            "20200000",
            "11010200",
            "00000010",
            "02021010",
            "01010000",
            "00000002",
            "00002202",
            "00200000",
            "00000000",
            "00000000",
        };

        public static readonly string[] Explosion3 =
        {
            "00000000",
            "01010000",
            "00020000",
            "00000000",
            "00000000",
            "00001010",
            "00000202",
            "03012020",
            "10000000",
            "00200202",
            "20200101",
            "00000000",
            "00010000",
            "20000111",
            "00000000",
            "00000000",
        };

        public static readonly string[] Explosion4 =
        {
            "00000101",
            "00000010",
            "00000000",
            "00000000",
            "00000000",
            "02020000",
            "01010000",
            "01000000",
            "20000000",
            "00000202",
            "00002020",
            "01010000",
            "00010000",
            "00002000",
            "00000000",
            "00000000",
        };

        public static readonly string[] Pathway =
        {
            "00100100",
            "10001000",
            "00100100",
            "00000000",
            "01000010",
            "00000000",
            "01000010",
            "00000000",
            "00100100",
            "10001000",
            "00100100",
            "00000000",
            "01000010",
            "00000000",
            "01000010",
            "00000000",
        };

        public static readonly string[] Streak =
        {
            "00000000",
            "00000000",
            "00000000",
            "00000000",
            "00000000",
            "00111100",
            "01222210",
            "01333321",
            "01333321",
            "01222210",
            "00111110",
            "00000000",
            "00000000",
            "00000000",
            "00000000",
            "00000000",
        };
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace VibeBoulderDash.Graphics;

internal static class ArtPreview
{
    public static string RenderAll()
    {
        var sb = new StringBuilder();
        foreach (var (name, colors) in AllTiles())
        {
            sb.AppendLine(name + ":");
            sb.Append(Render(colors));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static IEnumerable<(string Name, Color[] Colors)> AllTiles()
    {
        yield return ("Dirt", TileArt.ToColors(TileArt.Grids.Dirt, TileArt.DirtPalette));
        yield return ("Wall", TileArt.ToColors(TileArt.Grids.Wall, TileArt.WallPalette));
        yield return ("Steel", TileArt.ToColors(TileArt.Grids.Titanium, TileArt.SteelPalette));
        yield return ("MagicRest", TileArt.ToColors(TileArt.Grids.MagicWallRest, TileArt.MagicPalette));
        yield return ("MillA", TileArt.ToColors(TileArt.Grids.MagicWallMill1, TileArt.MagicPalette));
        yield return ("MillB", TileArt.ToColors(TileArt.Grids.MagicWallMill2, TileArt.MagicPalette));
        yield return ("ExitOpen", TileArt.ToColors(TileArt.Grids.ExitOpen, TileArt.SteelPalette));
        yield return ("Boulder", TileArt.ToColors(TileArt.Grids.Boulder, TileArt.BoulderPalette));
        yield return ("DiamondA", TileArt.ToColors(TileArt.Grids.DiamondA, TileArt.DiamondPalette));
        yield return ("DiamondB", TileArt.ToColors(TileArt.Grids.DiamondB, TileArt.DiamondPalette));
        yield return ("RockfordA", TileArt.ToColors(TileArt.Grids.RockfordA, TileArt.HeroPalette));
        yield return ("RockfordWalk1", TileArt.ToColors(TileArt.Grids.RockfordWalk1, TileArt.HeroPalette));
        yield return ("RockfordWalk2", TileArt.ToColors(TileArt.Grids.RockfordWalk2, TileArt.HeroPalette));
        yield return ("RockfordWalk3", TileArt.ToColors(TileArt.Grids.RockfordWalk3, TileArt.HeroPalette));
        yield return ("RockfordWalk4", TileArt.ToColors(TileArt.Grids.RockfordWalk4, TileArt.HeroPalette));
        yield return ("RockfordBlink", TileArt.ToColors(TileArt.Grids.RockfordBlink, TileArt.HeroPalette));
        yield return ("RockfordTap", TileArt.ToColors(TileArt.Grids.RockfordTap, TileArt.HeroPalette));
        yield return ("RockfordTapBlink", TileArt.ToColors(TileArt.Grids.RockfordTapBlink, TileArt.HeroPalette));
        yield return ("Explosion1", TileArt.ToColors(TileArt.Grids.Explosion1, TileArt.ExplosionPalette));
        yield return ("Explosion2", TileArt.ToColors(TileArt.Grids.Explosion2, TileArt.ExplosionPalette));
        yield return ("Explosion3", TileArt.ToColors(TileArt.Grids.Explosion3, TileArt.ExplosionPalette));
        yield return ("Explosion4", TileArt.ToColors(TileArt.Grids.Explosion4, TileArt.ExplosionPalette));
        yield return ("Pathway", TileArt.ToColors(TileArt.Grids.Pathway, TileArt.PathwayPalette));
        yield return ("Streak", TileArt.ToColors(TileArt.Grids.Streak, TileArt.StreakPalette));
        yield return ("FireflyA", TileArt.ToColors(TileArt.Grids.FireflyA, TileArt.FireflyPalette));
        yield return ("FireflyB", TileArt.ToColors(TileArt.Grids.FireflyB, TileArt.FireflyPalette));
        yield return ("ButterflyA", TileArt.ToColors(TileArt.Grids.ButterflyA, TileArt.ButterflyPalette));
        yield return ("ButterflyB", TileArt.ToColors(TileArt.Grids.ButterflyB, TileArt.ButterflyPalette));
        yield return ("AmoebaA", TileArt.ToColors(TileArt.Grids.AmoebaA, TileArt.AmoebaPalette));
        yield return ("AmoebaB", TileArt.ToColors(TileArt.Grids.AmoebaB, TileArt.AmoebaPalette));
    }

    private static string Render(Color[] colors)
    {
        const int size = 32;
        const string ramp = " .:-=+*#%@";
        var sb = new StringBuilder();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var c = colors[y * size + x];
                if (c.A == 0)
                {
                    sb.Append(' ');
                }
                else
                {
                    var lum = (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;
                    sb.Append(ramp[Math.Clamp((int)(lum * 10), 0, ramp.Length - 1)]);
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
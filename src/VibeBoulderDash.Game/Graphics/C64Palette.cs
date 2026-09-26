using System;
using Microsoft.Xna.Framework;

namespace VibeBoulderDash.Graphics;

internal static class C64Palette
{
    public static readonly Color[] Colors =
    {
        new(0, 0, 0),
        new(255, 255, 255),
        new(136, 0, 0),
        new(170, 255, 238),
        new(204, 68, 204),
        new(0, 204, 85),
        new(0, 0, 170),
        new(238, 238, 87),
        new(221, 136, 85),
        new(102, 68, 0),
        new(255, 119, 119),
        new(68, 68, 68),
        new(153, 153, 153),
        new(119, 255, 221),
        new(85, 255, 255),
        new(221, 221, 221),
    };

    public static Color Get(int index) => Colors[Math.Clamp(index, 0, Colors.Length - 1)];
}
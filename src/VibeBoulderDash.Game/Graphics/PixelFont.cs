using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBoulderDash.Graphics;

internal sealed class PixelFont
{
    private const int Cell = 8;
    private static readonly Dictionary<char, int[]> Glyphs = new()
    {
        ['A'] = new[] { 14, 17, 17, 31, 17, 17, 17 },
        ['B'] = new[] { 30, 17, 17, 30, 17, 17, 30 },
        ['C'] = new[] { 14, 17, 16, 16, 16, 17, 14 },
        ['D'] = new[] { 30, 17, 17, 17, 17, 17, 30 },
        ['E'] = new[] { 31, 16, 16, 30, 16, 16, 31 },
        ['F'] = new[] { 31, 16, 16, 30, 16, 16, 16 },
        ['G'] = new[] { 14, 17, 16, 23, 17, 17, 14 },
        ['H'] = new[] { 17, 17, 17, 31, 17, 17, 17 },
        ['I'] = new[] { 31, 4, 4, 4, 4, 4, 31 },
        ['J'] = new[] { 15, 2, 2, 2, 2, 18, 12 },
        ['K'] = new[] { 17, 18, 20, 24, 20, 18, 17 },
        ['L'] = new[] { 16, 16, 16, 16, 16, 16, 31 },
        ['M'] = new[] { 17, 27, 21, 17, 17, 17, 17 },
        ['N'] = new[] { 17, 25, 21, 19, 17, 17, 17 },
        ['O'] = new[] { 14, 17, 17, 17, 17, 17, 14 },
        ['P'] = new[] { 30, 17, 17, 30, 16, 16, 16 },
        ['Q'] = new[] { 14, 17, 17, 17, 21, 18, 13 },
        ['R'] = new[] { 30, 17, 17, 30, 20, 18, 17 },
        ['S'] = new[] { 15, 16, 16, 14, 1, 1, 30 },
        ['T'] = new[] { 31, 4, 4, 4, 4, 4, 4 },
        ['U'] = new[] { 17, 17, 17, 17, 17, 17, 14 },
        ['V'] = new[] { 17, 17, 17, 17, 17, 10, 4 },
        ['W'] = new[] { 17, 17, 17, 17, 21, 27, 17 },
        ['X'] = new[] { 17, 17, 10, 4, 10, 17, 17 },
        ['Y'] = new[] { 17, 17, 10, 4, 4, 4, 4 },
        ['Z'] = new[] { 31, 1, 2, 4, 8, 16, 31 },
        ['0'] = new[] { 14, 17, 19, 21, 25, 17, 14 },
        ['1'] = new[] { 4, 12, 4, 4, 4, 4, 31 },
        ['2'] = new[] { 14, 17, 1, 2, 4, 8, 31 },
        ['3'] = new[] { 14, 17, 1, 6, 1, 17, 14 },
        ['4'] = new[] { 2, 6, 10, 18, 31, 2, 2 },
        ['5'] = new[] { 31, 16, 30, 1, 1, 17, 14 },
        ['6'] = new[] { 6, 8, 16, 30, 17, 17, 14 },
        ['7'] = new[] { 31, 1, 2, 4, 8, 8, 8 },
        ['8'] = new[] { 14, 17, 17, 14, 17, 17, 14 },
        ['9'] = new[] { 14, 17, 17, 15, 1, 2, 12 },
        [':'] = new[] { 0, 0, 4, 0, 4, 0, 0 },
        ['-'] = new[] { 0, 0, 0, 31, 0, 0, 0 },
        ['/'] = new[] { 1, 1, 2, 4, 8, 16, 16 },
        ['!'] = new[] { 4, 4, 4, 4, 4, 0, 4 },
        ['.'] = new[] { 0, 0, 0, 0, 0, 4, 0 },
        [' '] = new[] { 0, 0, 0, 0, 0, 0, 0 },
    };

    private readonly Texture2D _atlas;
    private readonly Dictionary<char, Rectangle> _rects = new();

    public PixelFont(GraphicsDevice device)
    {
        var order = BuildOrder();
        _atlas = new Texture2D(device, Cell * order.Count, Cell);
        var pixels = new Color[Cell * order.Count * Cell];

        for (int i = 0; i < order.Count; i++)
        {
            var glyph = Glyphs[order[i]];
            for (int y = 0; y < 7; y++)
            {
                var bits = glyph[y];
                for (int x = 0; x < 5; x++)
                {
                    if ((bits & (16 >> x)) != 0)
                    {
                        pixels[(y + 1) * _atlas.Width + i * Cell + x + 1] = Color.White;
                    }
                }
            }

            _rects[order[i]] = new Rectangle(i * Cell, 0, Cell, Cell);
        }

        _atlas.SetData(pixels);
    }

    public float Measure(string text) => text.Length * Cell;

    public void Draw(SpriteBatch batch, Vector2 position, string text, Color color, float scale = 1f)
    {
        var x = position.X;
        foreach (var character in text)
        {
            var glyph = char.ToUpperInvariant(character);
            if (!_rects.TryGetValue(glyph, out var source))
            {
                source = _rects[' '];
            }

            batch.Draw(_atlas, new Vector2(x, position.Y), source, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            x += Cell * scale;
        }
    }

    private static List<char> BuildOrder()
    {
        var chars = Glyphs.Keys.ToList();
        chars.Sort();
        return chars;
    }
}
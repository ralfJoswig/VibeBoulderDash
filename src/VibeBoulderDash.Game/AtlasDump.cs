using System;
using System.IO;
using VibeBoulderDash.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeBoulderDash;

internal sealed class AtlasDump : Game
{
    private const int Scale = 8;
    private const int TileSize = 32;

    private readonly string _outDir;
    private readonly GraphicsDeviceManager _graphics;
    private readonly string[] _names;
    private readonly Color[][] _tiles;

    public AtlasDump(string outDir)
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 64,
            PreferredBackBufferHeight = 64,
        };
        Content.RootDirectory = "Content";
        _outDir = Directory.CreateDirectory(outDir).FullName;
        var all = ArtPreview.AllTiles();
        var listNames = new System.Collections.Generic.List<string>();
        var listTiles = new System.Collections.Generic.List<Color[]>();
        foreach (var (name, colors) in all)
        {
            listNames.Add(name);
            listTiles.Add(colors);
        }

        _names = listNames.ToArray();
        _tiles = listTiles.ToArray();
    }

    protected override void LoadContent()
    {
        for (int i = 0; i < _names.Length; i++)
        {
            var src = TileArt.FromColors(GraphicsDevice, _tiles[i]);
            var scaled = new Texture2D(GraphicsDevice, TileSize * Scale, TileSize * Scale);
            var pixels = new Color[TileSize * Scale * (TileSize * Scale)];
            var raw = _tiles[i];
            for (int y = 0; y < TileSize * Scale; y++)
            {
                for (int x = 0; x < TileSize * Scale; x++)
                {
                    pixels[y * TileSize * Scale + x] = raw[(y / Scale) * TileSize + (x / Scale)];
                }
            }

            scaled.SetData(pixels);
            var path = Path.Combine(_outDir, _names[i] + ".png");
            using (var stream = File.Create(path))
            {
                scaled.SaveAsPng(stream, scaled.Width, scaled.Height);
            }
        }

        Exit();
    }
}
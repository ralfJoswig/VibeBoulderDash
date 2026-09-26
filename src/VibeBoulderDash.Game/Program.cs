using System;
using VibeBoulderDash.Graphics;

if (args.Length > 0 && args[0] == "--art")
{
    Console.WriteLine(ArtPreview.RenderAll());
    return;
}

if (args.Length > 0 && args[0] == "--atlas")
{
    using var dump = new VibeBoulderDash.AtlasDump(args.Length > 1 ? args[1] : "art");
    dump.Run();
    return;
}

using var game = new VibeBoulderDash.BoulderDashGame();
game.Run();
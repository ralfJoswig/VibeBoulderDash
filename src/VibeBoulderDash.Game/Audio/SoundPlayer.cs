using System.Collections.Generic;
using System.IO;
using VibeBoulderDash.Core;
using Microsoft.Xna.Framework.Audio;

namespace VibeBoulderDash.Audio;

internal sealed class SoundPlayer
{
    private readonly Dictionary<GameEventType, SoundEffect> _sounds = new();
    private readonly List<(string Name, NoteSpec[] Spec)> _specs = new();

    public void Load()
    {
        Add(GameEventType.Dug, "Dig", Sfx.Dig());
        Add(GameEventType.Walked, "Walk", Sfx.Walk());
        Add(GameEventType.Pushed, "Boulder", Sfx.Boulder());
        Add(GameEventType.SlimePassed, "Thud", Sfx.Thud());
        Add(GameEventType.ExpandingWallGrew, "Thud", Sfx.Thud());
        Add(GameEventType.DiamondCollected, "Collect", Sfx.Collect());
        Add(GameEventType.StartedFalling, "Boulder", Sfx.Boulder());
        Add(GameEventType.Landed, "Boulder", Sfx.Boulder());
        Add(GameEventType.MagicWallConverted, "Sparkle", Sfx.Sparkle());
        Add(GameEventType.Explosion, "Explosion", Sfx.Explosion());
        Add(GameEventType.RockfordDied, "Death", Sfx.Death());
        Add(GameEventType.ExitOpened, "ExitOpen", Sfx.ExitOpen());
        Add(GameEventType.CaveCompleted, "CaveComplete", Sfx.CaveComplete());
        Add(GameEventType.ExtraLife, "ExtraLife", Sfx.ExtraLife());
        Add(GameEventType.OutOfTime, "OutOfTime", Sfx.OutOfTime());

        DumpIfRequested();
    }

    public void Handle(IEnumerable<GameEvent> events)
    {
        foreach (var gameEvent in events)
        {
            if (_sounds.TryGetValue(gameEvent.Type, out var sound))
            {
                sound.Play();
            }
        }
    }

    private void Add(GameEventType type, string name, NoteSpec[] spec)
    {
        _sounds[type] = Synth.Render(spec);
        _specs.Add((name, spec));
    }

    private void DumpIfRequested()
    {
        var target = System.Environment.GetEnvironmentVariable("BD_SFX_DUMP");
        if (string.IsNullOrEmpty(target))
        {
            return;
        }

        Directory.CreateDirectory(target);
        foreach (var (name, spec) in _specs)
        {
            var path = Path.Combine(target, name + ".wav");
            using var stream = File.Create(path);
            Synth.WriteTo(stream, Synth.CreateSamples(spec));
        }
    }
}
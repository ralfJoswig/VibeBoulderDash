namespace VibeBoulderDash.Audio;

internal static class Sfx
{
    public static NoteSpec[] Dig() => new[] { new NoteSpec(2576f, 0.09f, 0f, Wave.Noise, 0.18f, AttackSeconds: 0.024f, SustainLevel: 0.8f) };

    public static NoteSpec[] Walk() => new[] { new NoteSpec(827f, 0.08f, 0f, Wave.Noise, 0.18f, AttackSeconds: 0.024f, SustainLevel: 0.8f) };

    public static NoteSpec[] Boulder() => new[] { new NoteSpec(143.5f, 0.10f, 0f, Wave.Noise, 0.28f) };

    public static NoteSpec[] Collect() => new[] { new NoteSpec(319.5f, 0.12f, 0f, Wave.Triangle, 0.30f) };

    public static NoteSpec[] Thud() => new[] { new NoteSpec(0f, 0.08f, 0f, Wave.Noise, 0.18f) };

    public static NoteSpec[] Sparkle() => new[]
    {
        new NoteSpec(520f, 0.04f, 0f, Wave.Square, 0.15f),
        new NoteSpec(780f, 0.04f, 0f, Wave.Square, 0.15f),
    };

    public static NoteSpec[] Explosion() =>
        new[] { new NoteSpec(0f, 0.30f, 0f, Wave.Noise, 0.40f) };

    public static NoteSpec[] Death() => new[]
    {
        new NoteSpec(500f, 0.12f, 250f, Wave.Square, 0.25f),
        new NoteSpec(300f, 0.25f, 100f, Wave.Square, 0.25f),
    };

    public static NoteSpec[] ExitOpen() => new[]
    {
        new NoteSpec(440f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(554f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(659f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(880f, 0.16f, 0f, Wave.Square, 0.20f),
    };

    public static NoteSpec[] ExtraLife() => new[]
    {
        new NoteSpec(523f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(659f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(784f, 0.08f, 0f, Wave.Square, 0.18f),
        new NoteSpec(1047f, 0.22f, 0f, Wave.Square, 0.20f),
    };

    public static NoteSpec[] CaveComplete() => new[]
    {
        new NoteSpec(659f, 0.12f, 0f, Wave.Square, 0.18f),
        new NoteSpec(784f, 0.12f, 0f, Wave.Square, 0.18f),
        new NoteSpec(880f, 0.12f, 0f, Wave.Square, 0.18f),
        new NoteSpec(1047f, 0.12f, 0f, Wave.Square, 0.18f),
        new NoteSpec(1319f, 0.34f, 0f, Wave.Square, 0.20f),
    };

    public static NoteSpec[] OutOfTime() => new[] { new NoteSpec(90f, 0.40f, 45f, Wave.Square, 0.30f) };
}
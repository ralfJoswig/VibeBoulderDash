using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace VibeBoulderDash.Audio;

internal enum Wave : byte
{
    Square,
    Triangle,
    Noise,
}

internal readonly record struct NoteSpec(
    float Frequency,
    float Duration,
    float FrequencyEnd = 0f,
    Wave Wave = Wave.Square,
    float Volume = 0.2f,
    float AttackSeconds = 0.002f,
    float DecaySeconds = 0.006f,
    float SustainLevel = 1f,
    float ReleaseSeconds = 0.006f);

internal static class Synth
{
    public const int SampleRate = 22050;

    public static SoundEffect Render(NoteSpec note)
    {
        var samples = new float[(int)(note.Duration * SampleRate) + 1];
        var phase = 0.0;
        var rng = 12345u;
        var noisePrev = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            var t = (float)i / SampleRate;
            var progress = samples.Length <= 1 ? 0f : (float)i / (samples.Length - 1);
            var frequency = note.FrequencyEnd > 0f
                ? note.Frequency + (note.FrequencyEnd - note.Frequency) * progress
                : note.Frequency;
            var value = WaveSample(note.Wave, ref phase, frequency, ref rng, ref noisePrev);
            samples[i] = value * Envelope(t, note.Duration, note) * note.Volume;
        }

        return FromSamples(samples);
    }

    public static SoundEffect Render(NoteSpec[] sequence)
    {
        return FromSamples(CreateSamples(sequence));
    }

    internal static float[] CreateSamples(NoteSpec[] sequence)
    {
        var total = 0.0f;
        foreach (var spec in sequence)
        {
            total += spec.Duration;
        }

        var samples = new float[(int)(total * SampleRate) + 1];
        var offsetSample = 0;
        var phase = 0.0;
        var rng = 555u;
        var noisePrev = 0f;
        foreach (var spec in sequence)
        {
            var count = (int)(spec.Duration * SampleRate);
            for (int i = 0; i < count && offsetSample < samples.Length; i++, offsetSample++)
            {
                var t = (float)i / SampleRate;
                var progress = count <= 1 ? 0f : (float)i / (count - 1);
                var frequency = spec.FrequencyEnd > 0f
                    ? spec.Frequency + (spec.FrequencyEnd - spec.Frequency) * progress
                    : spec.Frequency;
                var value = WaveSample(spec.Wave, ref phase, frequency, ref rng, ref noisePrev);
                samples[offsetSample] = value * Envelope(t, spec.Duration, spec) * spec.Volume;
            }

            phase = 0.0;
        }

        return samples;
    }

    public static void WriteTo(Stream stream, float[] samples)
    {
        var pcm = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            var value = (short)(Math.Clamp(samples[i], -1f, 1f) * short.MaxValue);
            pcm[i * 2] = (byte)(value & 0xFF);
            pcm[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        var dataSize = pcm.Length;
        var bytes = new byte[44 + dataSize];
        WriteAscii(bytes, 0, "RIFF");
        WriteInt32(bytes, 4, 36 + dataSize);
        WriteAscii(bytes, 8, "WAVE");
        WriteAscii(bytes, 12, "fmt ");
        WriteInt32(bytes, 16, 16);
        WriteInt16(bytes, 20, 1);
        WriteInt16(bytes, 22, 1);
        WriteInt32(bytes, 24, SampleRate);
        WriteInt32(bytes, 28, SampleRate * 2);
        WriteInt16(bytes, 32, 2);
        WriteInt16(bytes, 34, 16);
        WriteAscii(bytes, 36, "data");
        WriteInt32(bytes, 40, dataSize);
        Buffer.BlockCopy(pcm, 0, bytes, 44, dataSize);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static SoundEffect FromSamples(float[] samples)
    {
        using var stream = new MemoryStream();
        WriteTo(stream, samples);
        stream.Position = 0;
        return SoundEffect.FromStream(stream);
    }

    private static float WaveSample(Wave wave, ref double phase, float frequency, ref uint rng, ref float noisePrev)
    {
        if (wave == Wave.Noise)
        {
            var clock = frequency > 0f ? frequency : SampleRate;
            phase += clock / SampleRate;
            while (phase >= 1.0)
            {
                phase -= 1.0;
                rng ^= rng << 13;
                rng ^= rng >> 17;
                rng ^= rng << 5;
            }

            var held = ((rng & 0xFFFF) / 32768f) - 1f;
            var diff = held - noisePrev;
            noisePrev = held;
            return diff;
        }

        phase += frequency / SampleRate;
        if (phase >= 1.0)
        {
            phase -= 1.0;
        }

        return wave switch
        {
            Wave.Triangle => 4f * (float)Math.Abs(phase - 0.5f) - 1f,
            _ => phase < 0.25f ? 1f : -1f,
        };
    }

    private static float Envelope(float t, float duration, NoteSpec note)
    {
        var attack = note.AttackSeconds;
        if (attack > 0f && t < attack)
        {
            return t / attack;
        }

        var decayEnd = attack + note.DecaySeconds;
        if (note.DecaySeconds > 0f && t < decayEnd)
        {
            return 1f - (1f - note.SustainLevel) * ((t - attack) / note.DecaySeconds);
        }

        var holdEnd = note.ReleaseSeconds > 0f
            ? Math.Max(decayEnd, duration - note.ReleaseSeconds)
            : duration;
        if (t < holdEnd)
        {
            return note.SustainLevel;
        }

        return note.ReleaseSeconds > 0f
            ? note.SustainLevel * Math.Max(0f, 1f - (t - holdEnd) / note.ReleaseSeconds)
            : note.SustainLevel;
    }

    private static void WriteAscii(byte[] buffer, int offset, string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            buffer[offset + i] = (byte)text[i];
        }
    }

    private static void WriteInt16(byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
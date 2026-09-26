namespace VibeBoulderDash.Core;

/// <summary>Predictable two-byte random generator of the original C64 Boulder Dash.</summary>
internal struct C64Random
{
    public byte Seed1;
    public byte Seed2;

    public C64Random(byte seed2)
    {
        Seed1 = 0;
        Seed2 = seed2;
    }

    public byte Next()
    {
        int tempRand1 = (Seed1 & 0x01) * 0x80;
        int tempRand2 = (Seed2 >> 1) & 0x7F;
        int result = Seed2 + (Seed2 & 0x01) * 0x80;
        int carry = result > 0xFF ? 1 : 0;
        result = (result & 0xFF) + carry + 0x13;
        carry = result > 0xFF ? 1 : 0;
        Seed2 = (byte)(result & 0xFF);
        result = Seed1 + carry + tempRand1;
        carry = result > 0xFF ? 1 : 0;
        result = (result & 0xFF) + carry + tempRand2;
        Seed1 = (byte)(result & 0xFF);
        return Seed1;
    }
}
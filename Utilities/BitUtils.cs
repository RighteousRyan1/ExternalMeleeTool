namespace ExternalMeleeTool.Utilities; 
public static class BitUtils {
    /// <summary>
    /// Unpacks the bits of a byte into an array of bytes, where each entry is either 1 or 0 representing each bit.
    /// </summary>
    /// <param name="value">The byte to unpack.</param>
    /// <returns>1 or 0 in each array entry, representing each bit.</returns>
    public static byte[] Unpack(byte value) {
        var bits = new byte[8];
        for (int i = 0; i < 8; i++) {
            bits[i] = (byte)(value >> i & 1);
        }
        return bits;
    }

    public static byte[] Unpack(ushort value) {
        var bits = new byte[16];
        for (int i = 0; i < 16; i++)
            bits[i] = (byte)(value >> i & 1);
        return bits;
    }

    public static byte[] Unpack(uint value) {
        var bits = new byte[32];
        for (int i = 0; i < 32; i++)
            bits[i] = (byte)(value >> i & 1);
        return bits;
    }

    public static byte PackU8(ReadOnlySpan<byte> bits) {
        if (bits.Length < 8)
            throw new ArgumentException("Need at least 8 bits");

        byte value = 0;
        for (int i = 0; i < 8; i++)
            value |= (byte)((bits[i] & 1) << i);

        return value;
    }

    public static ushort PackU16(ReadOnlySpan<byte> bits) {
        if (bits.Length < 16)
            throw new ArgumentException("Need at least 16 bits");

        ushort value = 0;
        for (int i = 0; i < 16; i++)
            value |= (ushort)((bits[i] & 1) << i);

        return value;
    }

    public static uint PackU32(ReadOnlySpan<byte> bits) {
        if (bits.Length < 32)
            throw new ArgumentException("Need at least 32 bits");

        uint value = 0;
        for (int i = 0; i < 32; i++)
            value |= (uint)(bits[i] & 1) << i;

        return value;
    }

    public static bool HasFlag<T, TEnum>(this T flags, TEnum flag) where T : struct where TEnum : Enum {
        // Convert to long to handle any underlying enum type (byte, short, int)
        long flagsValue = Convert.ToInt32(flags);
        long flagValue = Convert.ToInt32(flag);

        return (flagsValue & flagValue) == flagValue;
    }
}

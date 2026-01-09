using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Utilities;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct U8Bitfield {
    byte _value;

    public byte RawValue {
        readonly get => _value;
        set => _value = value;
    }

    public readonly bool b0 => (_value & (1 << 0)) != 0;
    public readonly bool b1 => (_value & (1 << 1)) != 0;
    public readonly bool b2 => (_value & (1 << 2)) != 0;
    public readonly bool b3 => (_value & (1 << 3)) != 0;
    public readonly bool b4 => (_value & (1 << 4)) != 0;
    public readonly bool b5 => (_value & (1 << 5)) != 0;
    public readonly bool b6 => (_value & (1 << 6)) != 0;
    public readonly bool b7 => (_value & (1 << 7)) != 0;

    public override readonly string ToString() {
        // return $"0x{_value:X2} | b7={b7} b6={b6} b5={b5} b4={b4} b3={b3} b2={b2} b1={b1} b0={b0}";
        return Convert.ToString(_value, 2);
    }
}

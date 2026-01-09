using System.Drawing;

namespace ExternalMeleeTool.Melee; 

public struct GXColor(byte r, byte g, byte b, byte a) {
    public byte R = r;
    public byte G = g;
    public byte B = b;
    public byte A = a;

    // why does System.Drawing.Color not have a parameter'd ctor
    public static implicit operator Color(GXColor param) => Color.FromArgb(param.A, param.R, param.G, param.B);
    public override readonly string ToString() => $"GXColor(R={R}, G={G}, B={B}, A={A})";
}

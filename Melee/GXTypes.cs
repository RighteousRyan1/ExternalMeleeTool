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

public struct HSD_Material {
    public GXColor ambient;
    public GXColor diffuse;
    public GXColor specular;
    public f32 alpha;
    public f32 shininess;

    public override readonly string ToString() => $"amb={ambient}, diff={diffuse}, spec={specular}, alpha={alpha:F2}, shiny={shininess}";
}

public struct HSD_VtxDescList {
    public enum_t attr; // GXAttr enum
    public enum_t attr_type; // GXAttrType
    public enum_t comp_cnt; // GXCompCnt
    public enum_t comp_type; // GXCompType
    public u8 frac;
    public u16 stride;
    public Ptr32 vertex; // void*
}
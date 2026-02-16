using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data that defines a light object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct LObj {
    /* 0x00 - 0x04 */
    public HSD_Obj parent;
    /* 0x08 */
    public u16 flags;
    /* 0x0A */
    public u16 priority;
    /* 0x0C */
    public Struct_t next; // lobj*
    /* 0x10 */
    public GXColor color;
    /* 0x14 */
    public GXColor hw_color;
    /* 0x18 */
    public Struct_t position; // wobj*
    /* 0x1C */
    public Struct_t interest; // wobj*
    /* 0x20 - 0x34 */
    //union {
    //    HSD_LightPoint point;
    //    HSD_LightSpot spot;
    //    HSD_LightAttn attn;
    //} // max size = 24
    // implement at some point but rn laziness
    public fixed byte light_union_bytes[24];

    /* 0x38 */
    public f32 shininess; // relating to phong?
    /* 0x3C - 0x44 */
    public Vec3 lvec;
    /* 0x48 */
    public Struct_t aobj; // aobj*
    /* 0x4C */
    public GXLightID id;
    /* 0x50 */
    // public GXLightObj lightobj;
    public fixed byte lightobj_bytes[64]; // GXLightObj has a 16-length of u32, size 64... idk what it is
    /* 0x90 */
    public GXLightID spec_id;
    /* 0x94 */
    // public GXLightObj spec_lightobj;
    public fixed byte spec_lightobj_bytes[64];
}

// ENUM

public enum GXLightID {
    GX_LIGHT0 = 0x001,
    GX_LIGHT1 = 0x002,
    GX_LIGHT2 = 0x004,
    GX_LIGHT3 = 0x008,
    GX_LIGHT4 = 0x010,
    GX_LIGHT5 = 0x020,
    GX_LIGHT6 = 0x040,
    GX_LIGHT7 = 0x080,
    GX_MAX_LIGHT = 0x100,
    GX_LIGHT_NULL = 0,
}
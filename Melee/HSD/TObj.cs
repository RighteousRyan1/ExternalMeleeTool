using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data representing a texture object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct HSD_TObj {
    /* 0x00 */
    public HSD_Obj parent;
    /* 0x08 */
    public Struct_t next; // HSD_TObj*
    /* 0x0C */
    public GXTexMapID id;
    /* 0x10 */
    public GXTexGenSrc src;
    /* 0x14 */
    public u32 mtxid;
    /* 0x18 */
    public Quaternion rotate;
    /* 0x28 */
    public Vec3 scale;
    /* 0x3C */
    public Vec3 translate;
    /* 0x40 */
    public GXTexWrapMode wrap_s;
    /* 0x44 */
    public GXTexWrapMode wrap_t;
    /* 0x48 */
    public u8 repeat_s;
    /* 0x49 */
    public u8 repeat_t;
    // 3 bytes padding
    /* 0x4C */
    public u32 flags;
    /* 0x50 */
    public f32 blending;
    /* 0x54 */
    public GXTexFilter magFilt;
    /* 0x58 */
    // struct HSD_ImageDesc* imagedesc;
    public Struct_t imagedesc; // HSD_ImageDesc*
    /* 0x5C */
    // struct _HSD_Tlut* tlut;
    public Struct_t tlut; // _HSD_Tlut*
    /* 0x60 */
    // struct _HSD_TexLODDesc* lod;
    public Struct_t lod; // _HSD_TexLODDesc*
    /* 0x64 */
    public Struct_t aobj; // HSD_AObj*
    /* 0x68 */
    // struct HSD_ImageDesc** imagetbl;
    public PtrPtr32 imagetbl; // HSD_ImageDesc**
    /* 0x6C */
    // struct _HSD_Tlut** tluttbl;
    public PtrPtr32 tluttbl; // _HSD_Tlut**
    /* 0x70 */
    public u8 tlut_no; // tluttbl count?
    // 3 bytes padding
    /* 0x74 */
    public Mtx mtx;
    /* 0xA4 */
    public GXTexCoordID coord;
    /* 0xA8 */
    public Struct_t tev; // _HSD_TObjTev*
}

// ENUMS
public enum GXTexCoordID : u32 {
    GX_TEXCOORD0,
    GX_TEXCOORD1,
    GX_TEXCOORD2,
    GX_TEXCOORD3,
    GX_TEXCOORD4,
    GX_TEXCOORD5,
    GX_TEXCOORD6,
    GX_TEXCOORD7,
    GX_MAX_TEXCOORD,
    GX_TEXCOORD_NULL = 0xFF,
}

public enum GXTexFilter : u32 {
    GX_NEAR,
    GX_LINEAR,
    GX_NEAR_MIP_NEAR,
    GX_LIN_MIP_NEAR,
    GX_NEAR_MIP_LIN,
    GX_LIN_MIP_LIN,
}

public enum GXTexMapID {
    GX_TEXMAP0,
    GX_TEXMAP1,
    GX_TEXMAP2,
    GX_TEXMAP3,
    GX_TEXMAP4,
    GX_TEXMAP5,
    GX_TEXMAP6,
    GX_TEXMAP7,
    GX_MAX_TEXMAP,
    GX_TEXMAP_NULL = 0xFF,
    GX_TEX_DISABLE = 0x100,
}

public enum GXTexGenSrc {
    GX_TG_POS,
    GX_TG_NRM,
    GX_TG_BINRM,
    GX_TG_TANGENT,
    GX_TG_TEX0,
    GX_TG_TEX1,
    GX_TG_TEX2,
    GX_TG_TEX3,
    GX_TG_TEX4,
    GX_TG_TEX5,
    GX_TG_TEX6,
    GX_TG_TEX7,
    GX_TG_TEXCOORD0,
    GX_TG_TEXCOORD1,
    GX_TG_TEXCOORD2,
    GX_TG_TEXCOORD3,
    GX_TG_TEXCOORD4,
    GX_TG_TEXCOORD5,
    GX_TG_TEXCOORD6,
    GX_TG_COLOR0,
    GX_TG_COLOR1,
}

public enum GXTexWrapMode {
    GX_CLAMP,
    GX_REPEAT,
    GX_MIRROR,
    GX_MAX_TEXWRAPMODE,
}
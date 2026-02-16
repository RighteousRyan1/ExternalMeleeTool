using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data that describes a frame object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FObj {
    // struct HSD_FObj* next;
    public Struct_t next; // fobj*
    public Ptr32 ad;      // u8*
    public Ptr32 ad_head; // u8*, ad = anim data?
    public u32 length;
    public u8 flags;
    public u8 op;
    public u8 op_intrp;
    public u8 obj_type;
    public u8 frac_value;
    public u8 frac_slope;
    public u16 nb_pack;
    public s16 startframe;
    public u16 fterm;
    public f32 time;
    public f32 p0;
    public f32 p1;
    public f32 d0;
    public f32 d1;
}
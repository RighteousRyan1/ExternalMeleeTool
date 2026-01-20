using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data that describes an animation object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct HSD_AObj {
    /* 0x00 */
    public u32 flags;
    /* 0x04 */
    public f32 curr_frame;
    /* 0x08 */
    public f32 rewind_frame;
    /* 0x0C */
    public f32 end_frame;
    /* 0x10 */
    public f32 framerate;
    /* 0x14 */
    // HSD_FObj* fobj;
    public Struct_t fobj;
    /* 0x18 */
    // struct HSD_Obj* hsd_obj;
    public Struct_t hsd_obj;
}
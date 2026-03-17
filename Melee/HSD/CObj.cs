using System.Drawing;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD; 
public unsafe struct CObj {
    public HSD_Obj parent;
    public CObjFlags flags;
    public RectangleF viewport;
    public Scissor scissor;

    // both WObjs
    public Ptr32 eye; 
    public Ptr32 interest;

    // if only the X has a value, more than likely it's 'f32 roll'
    public Vec3 up;

    public f32 near;
    public f32 far;

    public f32 fov; // or aspect. you'll need to deduce that yourself.

    // union containing perspective, frustum, and orthographic data
    fixed byte projection_param[16];

    public u8 projection_type;
    public Mtx view_mtx;
    public Ptr32 aobj;
    public Ptr32 proj_mtx;

    public override readonly string ToString() => $"up={up:F1}, eye={eye:F2}, int={interest:F2}, near={near:F2}, far={far:F2}, fov={fov}";

    public static CObj GetMainCObj() {
        return Dolphinterop.Read<CObj>(Dolphinterop.ReadPtr(MeleePointers.CURRENT_COBJ_START));
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct Scissor {
    public u16 left;
    public u16 right;
    public u16 top;
    public u16 bottom;

    public override readonly string ToString() => $"left={left}, right={right}, top={top}, bottom={bottom}";
}

// ENUMS

// there must be more... why would "flags" use 32 bits?
[Flags]
public enum CObjFlags : u32 {
    UseRoll,
    UseUp
}

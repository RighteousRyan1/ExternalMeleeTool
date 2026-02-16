using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data describing a world object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct WObj {
    public HSD_Obj parent;
    public u32 flags;
    public Vec3 pos;
    public Struct_t aobj; // aobj*
    public Struct_t robj; // robj*
}
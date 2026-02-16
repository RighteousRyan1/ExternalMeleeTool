using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.HSD;

/// <summary>
/// Data describing a reference object.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct RObj {
    public Struct_t next; // robj*
    public u32 flags;
    /*union {
        HSD_JObj* jobj;
        HSD_Exp exp;
        f32 limit;
        HSD_IKHint ik_hint;
    }*/
    public Struct_t aobj; // aobj*
};

// ENUMS

public enum HSD_IKHint {

}
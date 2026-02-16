namespace ExternalMeleeTool.Melee.HSD; 

public unsafe struct PObj {
    fixed byte parent[4];
    public Ptr32 next; // PObj*
    public Ptr32 verts; // HSD_VtxDescList*
    public PObjFlags flags;
    public u16 n_display;

    public Ptr32 display; // u8*

    // union, 4 pointers.
    // stores jobj, shape_set, envelope_list, and "unk" ("_unk_struct_pobj*")
    public Ptr32 pobj_union;
}

// ENUMS

[Flags]
public enum PObjFlags : u16 {
    ShapeSet_Average = 1 << 0,
    ShapeSet_Additive = 1 << 1,
    Unk2 = 1 << 2,
    Anim = 1 << 3,
    Unk4 = 1 << 4,
    Unk5 = 1 << 5,
    Unk6 = 1 << 6,
    Unk7 = 1 << 7,
    Unk8 = 1 << 8,
    Unk9 = 1 << 9,
    Unk10 = 1 << 10,
    Unk11 = 1 << 11,
    ShapeAnim = 1 << 12,
    Envelope = 1 << 13,
    CullBack = 1 << 14,
    CullFront = 1 << 15
}
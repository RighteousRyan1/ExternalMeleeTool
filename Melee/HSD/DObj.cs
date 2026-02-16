namespace ExternalMeleeTool.Melee.HSD; 
public unsafe struct DObj {
    fixed byte parent[4]; // HSD_Class, too fucking layered to bother copying
    public DObj_t next;
    public Ptr32 mobj;
    public Ptr32 pobj;
    public Ptr32 aobj;
    public DObjFlags flags;

    public const int SIZE = 0x18;
}

public struct DObjList {
    public u32 count;
    public PtrPtr32 data;
}

// ENUMS

[Flags]
public enum DObjFlags : u32 {
    Unk0 = 1u << 0,
    Visible = 1u << 1, // &= ~Hidden strips visibility. i feel like this is "Visible"?
    Unk2 = 1u << 2,
    Unk3 = 1u << 3,
    Unk4 = 1u << 4,
    Unk5 = 1u << 5,
    Unk6 = 1u << 6,
    Unk7 = 1u << 7,
    // literally nothing else. this sucks
}

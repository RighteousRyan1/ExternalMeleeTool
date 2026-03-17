using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Utilities;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.Collision;

/// <summary>
/// A struct containing many pieces of data relating to collision
/// </summary>
/// <remarks>
/// Anything denoted with 'flags' is a bitfield. Use <see cref="BitUtils.Unpack"/> to extract individual bits.
/// These particular flags are structured as follows: <br></br>
/// Bit 0: Flag 1 <br></br>
/// Bits 1-4: Flag 2 <br></br>
/// Bits 5-7 are each their own flags. <br></br>
/// Extract the bits in this manner and you will get their values.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CollData {
    /* fp+6F0 */
    public Ptr32 gobj; // HSD_GObj
    /* fp+6F4 */
    public Vector3 cur_pos;
    // position on the previous step of collision
    /* fp+700 */
    public Vector3 prev_pos;
    // position before collision routine started
    /* fp+70C */
    public Vector3 last_pos;
    /* fp+718 */
    public Vector3 x28_vec;
    /* fp+724 */
    public U8Bitfield x34_flags;
    // ECBFlagStruct x34_flags;
    /* fp+725 */
    public U8Bitfield x35_flags;
    // ECBFlagStruct x35_flags;
    /* fp+726 */
    public s16 facing_dir;
    /* fp+728 */
    public int x38; // some sort of timer???
    /* fp+72C */
    public int floor_skip;
    /* fp+730 */
    public int ledge_id_right;
    /* fp+734 */
    public int ledge_id_left;
    /* fp+738 */
    public int joint_id_skip;
    /* fp+73C */
    public int joint_id_only;
    /* fp+740 */
    public float x50;
    /* fp+744 */
    public float ledge_snap_x;
    /* fp+748 */
    public float ledge_snap_y;
    /* fp+74C */
    public float ledge_snap_height;
    /* fp+750 */
    // not updated when dashing??
    public float lstick_x;
    /* fp+754 */
    public ECB x64_ecb; // the widest the ecb has been?
    /* fp+774 */
    /// <summary>The fighter's desired Environmental Collision Box (ECB) on the next frame. Coordinates are relative to the fighter position.</summary>
    public ECB desired_ecb;
    /* fp+794 */
    /// <summary>The fighter's Environmental Collision Box (ECB). Coordinates are relative to the fighter position.</summary>
    public ECB ecb;
    // ECB on the previous step of collision
    /* fp+7B4 */
    /// <summary>The fighter's Environmental Collision Box (ECB) on the previous frame. Coordinates are relative to the fighter position.</summary>
    public ECB prev_ecb;
    /* fp+7D4 */
    public ECB xE4_ecb;
    /* fp+7F4 */
    // commented for now because somehow ECBSource is size 36... 2 words less than it should be
    // public ECBSource ecb_source;
    public fixed byte ecb_source_bytes[44];
    /* fp+820 */
    // first bit has something to do with moving up during a jump?
    public u32 x130_flags;
    /* fp+824 */
    public s32 env_flags;
    /* fp+828 */
    public s32 prev_env_flags;
    /* fp+82C */
    public s32 x13C;
    /* fp+830 */
    public Vector3 contact;
    /* fp+83C */
    public SurfaceData floor;
    /* fp+850 */
    public SurfaceData left_facing_wall;
    /* fp+864 */
    public SurfaceData right_facing_wall;
    /* fp+878 */
    public SurfaceData ceiling;

    /*public unsafe void test_print() {
        var size1 = sizeof(CollData);
        var size2 = sizeof(ECB);
        var size3 = sizeof(U8Bitfield);
        var size4 = sizeof(SurfaceData);
        var size5 = sizeof(ECBSource);
        var str = this.FieldsToString();
    }*/
}

public struct SurfaceData {
    public int index;
    public u32 flags;
    public Vector3 normal;

    public override readonly string ToString() => $"Index={index}, Flags={Convert.ToString(flags, 2)}, Normal={normal}";
}

/// <summary>
/// ECB Source data. Indicates how the ECB is being defined.
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 4)]
public unsafe struct ECBSource {
    /* fp+7F4 */
    [FieldOffset(0x0)] public ECBSourceKind kind;
    /* fp+7F8 */
    // START: union of two structs. structs separated by space.
    // first two are both HSD_JObj*
    [FieldOffset(0x4)] public Ptr32 x108_joint;

    // also a pointer. unfortunately cannot use the Ptr32 wrapper with it
    [FieldOffset(0x8)] public fixed uint x10C_joint[6]; // an array of 6 joints, 24 bytes (4 * 6)

    [FieldOffset(0x4)] public float up;
    [FieldOffset(0x8)] public float down;
    [FieldOffset(0xC)] public float front;
    [FieldOffset(0x10)] public float back;
    [FieldOffset(0x14)] public float angle;
    // END

    /* fp+814 */
    [FieldOffset(0x18)] public float x124;
    /* fp+818 */
    [FieldOffset(0x1C)] public float x128;
    /* fp+81C */
    [FieldOffset(0x20)] public float x12C;

    public override readonly string ToString() => $"Kind={kind}, Up={up}, Down={down}, Front={front}, Back={back}, Angle={angle}";
}

// to my knowledge, there is no list of these...
// just a pointer
public struct MapCollData {
    /*  +0 */
    public Struct_t verts; // Vector2*
    /*  +4 */
    public int vert_count;
    /*  +8 */
    public Struct_t lines; // MapLine*
    /*  +C */
    public int line_count;
    /* +10 */
    public s16 floor_start;
    /* +12 */
    public s16 floor_count;
    /* +14 */
    public s16 ceiling_start;
    /* +16 */
    public s16 ceiling_count;
    /* +18 */
    public s16 right_wall_start;
    /* +1A */
    public s16 right_wall_count;
    /* +1C */
    public s16 left_wall_start;
    /* +1E */
    public s16 left_wall_count;
    /* +20 */
    public s16 dynamic_start;
    /* +22 */
    public s16 dynamic_count;
    /* +24 */
    public Struct_t joints; // MapJoint*
    /* +28 */
    public int coll_group_count;
    /* +2C */
    public int x2C; /* inferred */
}

// is this unused?
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct CollJoint {
    public Struct_t next; // CollJoint*
    public Struct_t inner; // CollLineGroup*
    public u32 flags;
    public s16 xC;
    public u8 xE; // 0xE, move to 0x10 with padding
    public Vector2 bounding_min;
    public Vector2 bounding_max;
    public Struct_t jobj; // HSD_JObj*
    public Func_t x24_callback;
    public Func_t x28_ground_data;
    public Func_t x2C_callback;
    public Func_t x30_ground_data;
}

public struct MapLine(ushort start, ushort end) {
    // 0x0
    public u16 StartIdx = start;
    // 0x2
    public u16 EndIdx = end;

    // next line data?
    // 0x4
    public s16 prev_id0;
    // 0x6
    public s16 next_id0;
    // 0x8
    public s16 prev_id1;
    // 0xA
    public s16 next_id1;

    public CollKind coll_type; // top, bottom, right, left
    public CollProperty coll_property;
    public CollMaterial material_type;

    public const nint SIZE = 0x10;

    public override readonly string ToString() => $"coll={coll_type}, int={coll_property}, mat={material_type}";
    // public static void Construct
}

// pointer/linkedlist of map joints!
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct CollLineGroup {
    /*  +0 */
    public s16 floor_start;
    /*  +2 */
    public s16 floor_count;
    /*  +4 */
    public s16 ceiling_start;
    /*  +6 */
    public s16 ceiling_count;
    /*  +8 */
    public s16 right_wall_start;
    /*  +A */
    public s16 right_wall_count;
    /*  +C */
    public s16 left_wall_start;
    /*  +E */
    public s16 left_wall_count;
    /* +10 */
    public s16 dynamic_start;
    /* +12 */
    public s16 dynamic_count;
    // structure doesn't match BoundingRect lol so 4 floats it is
    /* +14 */
    public float left_bound;
    /* +18 */
    public float bottom_bound;
    /* +1C */
    public float right_bound;
    /* +20 */
    public float top_bound;
    /* +24 */
    public s16 vtx_start;
    /* +26 */
    public s16 vtx_count;

    public const int SIZE = 0x28; // 40
};

// ENUMS:

public enum CollKind : u16 {
    Disabled = 0,
    Top      = 1,
    Bottom   = 2,
    Right    = 4,
    Left     = 8, // maybe cuz it's actual flags
    // Disabled = 16
}

public enum CollProperty : u8 {
    None        = 0,
    DropThrough = 1,
    LedgeGrab   = 2,
    Unknown     = 4 // ? idk
}

public enum CollMaterial : u8 {
    Basic,
    Rock,
    Grass,
    Dirt,
    Wood,
    LightMetal,
    HeavyMetal,
    Cloth,
    AlienGoop,
    Felt,
    Water,
    Unknown11, // i will figure it out!
    Glass,
    TurtleShell, // for great bay specifically?
    Snow,
    Ice,
    FlatZone,
    Swamp,
    Cardboard
}

public enum HitSFXKind : u32 { // these belong to items *and* fighters
    NONE,
    PUNCH,
    KICK,
    SWORD,
    COIN,
    BAT,
    FAN,
    ELEC,
    FIRE,
    CHEW,
    SHELL,
    ENERGY,
    PEACHITEM,
    ICE,
    SFX_14,
    SFX_15,
}
using ExternalMeleeTool.Utilities;
using System.Numerics;
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
    public ECB desired_ecb;
    /* fp+794 */
    public ECB ecb;
    // ECB on the previous step of collision
    /* fp+7B4 */
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

// ENUMS:

public enum MaterialType : u8 {
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
public enum CollisionType : u16 {
    Top      = 1,
    Bottom,
    Right,
    Left,
    Disabled
}

public enum InteractType : u8 {
    None = 0,
    DropThrough,
    LedgeGrab,
    Unknown // ? idk
}
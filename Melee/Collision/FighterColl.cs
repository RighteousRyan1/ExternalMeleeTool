using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.Collision;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FighterHurtCapsule {
    public HurtCapsule capsule;
    public HurtHeight height; // 0x44. 0 = low, 1 = mid, 2 = high
    public bool is_grabbable; // 0x48

    public const uint SIZE = 0x4C;
};
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct HurtCapsule {
    public HurtCapsuleState state;
    public Vector3 a_offset;
    public Vector3 b_offset;
    public float scale;
    public Ptr32 bone; // 0x20, HSD_JObj*
    /*u8 skip_update_pos : 1;
    u8 x24_b1 : 1; // 0x24 0x40
    u8 x24_b2 : 1; // 0x24 0x20
    u8 x24_b3 : 1; // 0x24 0x10
    u8 x24_b4 : 1; // 0x24 0x08
    u8 x24_b5 : 1; // 0x24 0x04
    u8 x24_b6 : 1; // 0x24 0x02
    u8 x24_b7 : 1; // 0x24 0x01*/
    // public fixed u8 some_random_bits[8];
    public byte bits_as_byte;

    public Vector3 start;
    public Vector3 end;
    public int bone_idx; // 0x40

    public const uint SIZE = 0x44;
};

public unsafe struct HitCapsule {
    /*  +0 */ public HitCapsuleState state;
    /*  +4 */ public u32 x4;
    /*  +8 */ public u32 unk_count;
    /*  +C */ public float damage;
    /* +10 */ public Vector3 b_offset;
    /* +1C */ public float scale;
    /* +20 */ public int kb_angle;
    /* +24 */ public u32 x24;
    /* +28 */ public u32 x28;
    /* +2C */ public u32 x2C;
    /* +30 */ public HitElement element;
    /* +34 */ public int x34;
    /* +38 */ public int sfx_severity;
    /* +3C */ public HitSFXKind sfx_kind; // enum_t... find out what cool things it does sometime
    // /* +40 */ u16 x40_b0 : 1;
    // /* +40 */ u16 x40_b1 : 1;
    // /* +40 */ u16 x40_b2 : 1;
    // /* +40 */ u16 x40_b3 : 1;
    // i'm having a literal stroke
    public byte x40_bits_4;
    // /* +40 */ u16 x40_b4 : 8;
    public byte x40_5_bits_8;
    // /* +41:4 */ u16 x41_b4 : 1;
    // /* +41:5 */ u16 x41_b5 : 1;
    // /* +41:6 */ u16 x41_b6 : 1;
    // /* +41:7 */ u16 x41_b7 : 1;
    // /* +42:0 */ u8 x42_b0 : 1;
    // /* +42:1 */ u8 x42_b1 : 1;
    // /* +42:2 */ u8 x42_b2 : 1;
    // /* +42:3 */ u8 x42_b3 : 1;
    // /* +42:4 */ u8 x42_b4 : 1;
    // /* +42:5 */ u8 x42_b5 : 1;
    // /* +42:6 */ u8 x42_b6 : 1;
    // /* +42:7 */ u8 x42_b7 : 1;
    // /* +43:0 */ u8 x43_b0 : 1;
    // /* +43:1 */ u8 x43_b1 : 1;
    // /* +43:2 */ u8 x43_b2 : 1;
    // /* +43:3 */ u8 x43_b3 : 1;
    // more strokage!
    public u16 x_43_bits_16;
    // /* +43:4 */ u8 x43_b4 : 1;
    // /* +43:5 */ u8 x43_b5 : 1;
    // /* +43:6 */ u8 x43_b6 : 1;
    // /* +43:7 */ u8 x43_b7 : 1;

    /* +44 */ public u8 victims_1_count; // victims_1 count
    /* +45 */ public u8 victims_2_count; // victims_2 count
    // /* +46 */ u8 x46[0x48 - 0x46]; // random ass two bytes of padding?
    public fixed byte x46_padding[2];
    /* +48 */ public Ptr32 jobj; // HSD_JObj
    /* +4C */ public Vector3 start;
    /* +58 */ public Vector3 end; // end pos? x58
    /* +64 */ public Vector3 hurt_coll_pos; // i dont think this has anything to do with hurt collision
    /* +70 */ public float coll_distance;
    // guessing this works?
    /* +74 */ public HitVictimBuffer12 victims_1;
    /* +D4 */ public HitVictimBuffer12 victims_2;
    /* +134 */
    //union {
    //    HSD_GObj* owner;
    //    u8 hit_grabbed_victim_only : 1;
    //};
    // temporary just to satisfy union
    public Ptr32 owner; // HSD_GObj*

    public const uint SIZE = 0x138; // 312

    [InlineArray(12)]
    public struct HitVictimBuffer12 {
        HitVictim _victim;
    }
};
// size = 0x8
public struct HitVictim {
    public Unk_t victim; // prolly either Fighter* or HSD_GObj*
    public u32 x4; // whatever tf this is man
}
// ENUMS

public enum HurtCapsuleState : uint {
    Enabled,
    Disabled,
    Intangible
}
public enum HurtHeight : uint {
    Low,
    Mid,
    High,
}

public enum HitCapsuleState {
    Disabled,
    Enabled,
    Init,   // some kind of state as the attack's first frame?
    Wait, // some kind of state after the attack is out for a while?
}

public enum HitElement : uint {
    Normal,
    Fire,
    Electric,
    Slash,
    Coin,
    Ice,

    // Sleep for 103 frames
    Sleep103,

    // Sleep for 412 frames
    Sleep412,

    Catch,
    Ground,
    Cape,
    // e.g. falcon side b
    Inert,
    Disable,
    Dark,

    // Screw Attack
    Screw,

    Lipstick,

    // Formerly presumed empty, this hitbox element is used by
    // ReDead grab attacks
    Leadead,

    Max = Leadead
}
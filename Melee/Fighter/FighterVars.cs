using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ExternalMeleeTool.Melee.Fighter; 

//public static class FighterVarsHelper {
    // public static 
//}

// not every fighter, yet, but a few that are typically played

// booleans are 4 bytes in C, but 4 in C#/.NET
public unsafe struct FtVarsMario {
    // ID of color combo
    public int vitamin_curr;
    public int vitamin_prev;

    public bool tornado_charge;
    //fixed byte pad1[3];

    public bool is_cape_boost;
    //fixed byte pad2[3];

    public GObj_t capeGObj;

    public u32 x18; // x2240

    // fixed u8 _[0xE0];
}

public unsafe struct FtVarsLuigi {
    // since in melee this value is literally unset, this is usually garbage data by design
    // until luigi uses cyclone
    public bool cyclone_charge;
    //fixed byte pad1[3];

    // unused?
    public u32 x4; // x2230

    // used on luigi's death... somehow
    public u32 x8; // x2234

    // fixed u8 _[0xEC];
}

public unsafe struct FtVarsCaptain {
    public u32 x0; // x222C, during specials starting?
    public u32 x4; // x2230, during specials?

    // fixed u8 _[0xF0];
}

public unsafe struct FtVarsGameWatch {
    public s32 judge_var1;
    public s32 judge_var2;
    public u32 xC; // x2230
    public s32 panic_charge;
    public s32 panic_damage;
    public s32 chef_var1;
    public s32 chef_var2;

    public GObj_t manhole_gobj;
    public GObj_t greenhouse_gobj1;
    public GObj_t greenhouse_gobj2;
    public GObj_t fire_gobj;
    public GObj_t parachute_gobj;
    public GObj_t turtle_gobj;
    public GObj_t sparky_gobj;
    public GObj_t judgement_gobj;
    public GObj_t panic_gobj;
    public GObj_t rescue_gobj;
}

public struct FtVarsFox {
    public GObj_t blaster_gobj;
}

public unsafe struct FtVarsLink {
    public bool used_boomerang;
    public bool x4; // x2230
    public GObj_t boomerang_gobj;
    public GObj_t xC;
    public GObj_t arrow_gobj;
    public GObj_t x14;
    public GObj_t x18;
    public u32 x1C;
}

public unsafe struct FtVarsPurin {
    public u32 x0; // x222C
    public Vec3 x4; // x2230;
    public JObj_t x8; // x223C
    public fixed byte dobj_list[8]; // don't have a DObjList struct yet
    public u32 x20;
}

public unsafe struct FtVarsMarth {
    public u32 x0; // x222C
    // fixed u8 _[0xF4];
}

public struct FtVarsDonkey {
    public s32 x0; // x222C
    public s32 x4; // x2230
}

public struct FtVarsSheik {
    public int x0;
    public GObj_t x4; // some item, maybe needles
    public GObj_t x8; // also another item, probably rope thingy
    public Vec3Buffer4 xC;
    public Vec3Buffer4 x3C;
    public Vec3 lstick_delta;

    [InlineArray(4)]
    public struct Vec3Buffer4 { Vec3 _instance; }
}
// how they're typed on the GameCube
global using u8 = byte;
global using s8 = sbyte;
global using u16 = ushort;
global using s16 = short;
global using u32 = uint;
global using s32 = int;
global using u64 = ulong;
global using s64 = long;

// how they're named in melee's code
global using Mtx = ExternalMeleeTool.Melee.Matrix3x4;
global using Vec3 = System.Numerics.Vector3;
global using Vec2 = System.Numerics.Vector2;
global using S32Vec2 = System.Drawing.Point;

global using f32 = float;

// types analagous to GC
global using HSD_Pad = uint;

// naming clarity
global using Unk_t = ExternalMeleeTool.Ptr32;
global using Func_t = ExternalMeleeTool.Ptr32;
global using GObj_t = ExternalMeleeTool.Ptr32;
global using JObj_t = ExternalMeleeTool.Ptr32;
global using DObj_t = ExternalMeleeTool.Ptr32;

// semantically wrong i think
global using Struct_t = ExternalMeleeTool.Ptr32;
global using PtrPtr32 = ExternalMeleeTool.Ptr32; // pointer to a pointer
global using enum_t = uint;

// function callback types
global using Callback32 = ExternalMeleeTool.Ptr32;
using ExternalMeleeTool.Melee.HSD;

namespace ExternalMeleeTool;

public readonly struct Ptr32(uint value) {
    readonly uint Value = value;

    public static implicit operator uint(Ptr32 p) => p.Value;
    public static implicit operator Ptr32(uint value) => new(value);

    public override string ToString() => $"0x{Value:X8}";
}

// TODO: change to pointers starting at game rom
/// <summary>A static class that contains important pointers to melee's memory.</summary>
public static class MeleeGlobals {
    // these are all offsets from GALE01!!!!
    public const uint CAM_START = 0x80453040;
    public const uint CAM_TYPE = 0x80452C6F;

    // PlayerMatchInfo = 8046b6d8.. look there soon. always 6 entries

    // maybe change to read from PLAYER_ONE + (playerIndex * sizeof(StaticPlayer))?
    public const uint PLAYER_ONE = 0x80453080;
    public const uint PLAYER_TWO = 0x80453F10;
    public const uint PLAYER_THREE = 0x80454DA0;
    public const uint PLAYER_FOUR = 0x80455C30;
    public const uint PAUSE_BIT = 0x80479D68;

    public const uint START_MELEE_RULES = 0x8046DB68;

    public const uint MINOR_SCENE = 0x80479D30;
    public const uint MAJOR_SCENE = 0x80479D33;
    public const byte MAJOR_SCENE_MAINMENU = 0;
    public const byte MAJOR_SCENE_STAGESELECT = 1;
    public const byte MAJOR_SCENE_INGAME = 2;

    // size of GC memory, where all code lies for any GC game
    public const uint RAM_SIZE = 0x02000000;
    public const uint ROM_SIZE = 0x80000000;

    // what is R13?
    public const uint R13 = 0x804DB6A0;
    // important for bone mapping!
    public const uint CHR_SKEL_INFO_TABLE = 0x804D6544;

    public const uint STAGE_INFO = 0x8049E6C8;


    // this is a linked list
    public const uint MAP_COLL_JOINT_HEAD = 0x804D64C0;// C8 is count?

    public const uint MATCH_INFO = 0x8046B6A0; // TODO: look here later
    public const uint MATCH_CAM = 0x80452C68;
    public const uint MATCH_HUD = 0x804A0FD8;

    public const uint MATCH_HUD_HIDDEN = 0x804D6D6C;
    // uh.... hardcore mode?
    public const uint MATCH_DEV_HUD_HIDDEN = 0x804D6D58;


    // lookup tables
    public const uint GOBJ_LOOKUP_TABLE = 0x804D782C; // R13 - 0x3E74; // GOBJ**, or PLinkList
    // ReadPtr(), loop through MATCHPLINK max, ReadPtr()

    public static IEnumerable<GObj> GetGObjList(PLink plink) {
        // PLinkList addr
        var plinkoffset = (s64)plink * sizeof(int);
        var collection_ptr = Dolphinterop.ReadPtr(GOBJ_LOOKUP_TABLE);
        var link_ptr = Dolphinterop.ReadPtr(collection_ptr + plinkoffset);

        var curAddr = link_ptr;
        while (curAddr != 0) {
            var gobj = Dolphinterop.Read<GObj>(curAddr);
            yield return gobj;
            curAddr = gobj.next;
        }
    }
}
/// <summary>A static class that contains important pointers to Slippi Netplay memory.</summary>
public static class SlippiGlobals {
    // thanks, Altafen!
    public const uint ONLINE_DATA_BLOCK = MeleeGlobals.R13 - 0x49E4;
}
// assists with offset changes/value changes in training mode (CE)
public static class TMConstants {
    // training lab
    public const byte MINOR_SCENE_TM = 43;
}
// STATIC STRUCTS
public struct CharSkeletonInfo {
    public Ptr32 joint_to_part; // byte*
    public Ptr32 part_to_joint; // supposedly byte*, but i think HSD_JObj*, but realistically could be joint index
    public uint parts_count; // _num?
}

// ENUMS
[Flags]
public enum HSDPadButton : uint {
    None         = 0,

    DPadLeft     = 0x0001,
    DPadRight    = 0x0002,
    DPadDown     = 0x0004,
    DPadUp       = 0x0008,

    TriggerZ     = 0x0010,
    TriggerR     = 0x0020,
    TriggerL     = 0x0040,

    A            = 0x0100,
    B            = 0x0200,
    X            = 0x0400,
    Y            = 0x0800,
    Start        = 0x1000,

    Up           = 0x10000,
    Down         = 0x20000,
    Left         = 0x40000,
    Right        = 0x80000,
}
public enum CameraKind : byte {
    Normal = 0x00,
    Develop = 0x08
}
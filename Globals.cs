// how they're typed on the GameCube
global using s16 = short;
global using s32 = int;
global using s8 = sbyte;
global using u16 = ushort;
global using u32 = uint;
global using u8 = byte;
global using HSD_Pad = uint;
global using UNK_T = uint;

namespace ExternalMeleeTool;

public readonly struct Ptr32(uint value) {
    readonly uint Value = value;

    public static implicit operator uint(Ptr32 p) => p.Value;
    public static implicit operator Ptr32(uint value) => new(value);

    public override string ToString() => $"0x{Value:X8}";
}

/// <summary>A static class that contains important pointers to melee's memory.</summary>
public static class MeleeGlobals {
    // these are all offsets from GALE01!!!!
    public const uint CAM_START = 0x453040;
    public const uint CAM_TYPE = 0x452C6F;

    // PlayerMatchInfo = 8046b6d8.. look there soon. always 6 entries

    public const uint PLAYER_ONE = 0x453080;
    public const uint PLAYER_TWO = 0x453F10;
    public const uint PLAYER_THREE = 0x454DA0;
    public const uint PLAYER_FOUR = 0x455C30;
    public const uint PAUSE_BIT = 0x479D68;

    public const uint START_MELEE_RULES = 0x46DB68;
    public const uint MATCH_INFO = 0x46b6a0; // TODO: look here later

    public const uint MINOR_SCENE = 0x479D30;
    public const uint MAJOR_SCENE = 0x479D33;
    public const byte MAJOR_SCENE_MAINMENU = 0;
    public const byte MAJOR_SCENE_STAGESELECT = 1;
    public const byte MAJOR_SCENE_INGAME = 2;

    // size of GC memory, where all code lies for any GC game
    public const uint RAM_SIZE = 0x02000000;
    public const uint ROM_SIZE = 0x80000000;

    public const uint FTPART_SIZE = 0x10;

    // what is R13?
    public const uint R13 = 0x4DB6A0;

    public const uint STAGE_INFO = 0x49E6C8;


    // this is a linked list
    public const uint MAP_COLL_JOINT_HEAD = 0x4D64C0;
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
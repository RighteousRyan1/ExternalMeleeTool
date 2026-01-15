using ExternalMeleeTool.Melee.Collision;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee;

// DIRECT STRUCT COPIES

/// <summary>
/// Presumably a 'live' struct dictating camera info in real-time, versus GrGroundParam which is more static stage data.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct StageCameraInfo { // From gr/types.h, StageCameraInfo
    public BoundingRect CamBounds;   // 0x00
    public float OffsetX;        // 0x10
    public float OffsetY;        // 0x14
    public float TiltVertical;   // 0x18
    public float PanDegrees;     // 0x1C
    public float x20;                 // 0x20
    public float x24;                 // 0x24
    public float TrackRatio;     // 0x28
    public float FixedZoom;      // 0x2C
    public float TrackSmooth;    // 0x30
    public float ZoomRate;       // 0x34
    public float MaxDepth;       // 0x38
    public float x3C;                 // 0x3C
    public float PauseMinZ;   // 0x40
    public float PauzeInitialZ;  // 0x44
    public float PauseMaxZ;   // 0x48
    public float AngleUp;        // 0x4C
    public float AngleDown;      // 0x50
    public float AngleLeft;      // 0x54
    public float AngleRight;     // 0x58
    public Vector3 FixedPos;     // 0x5C
    public float FixedFOV;       // 0x68
    public float FixedAngleVertical;// 0x6C
    public float FixedAngleHorizontal;// 0x70
}

/// <summary>
/// Data related to stage parameters.
/// </summary>
/// <remarks>
/// Any fields starting 'x' have unknown/unclear purposes.
/// <br></br>
/// Any pointers are left as raw pointers due to lack of information on the pointed-to struct.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct GrParam { // From gr/types.h, UnkStage6B0
    public float StageScale; // stage scale
    public short ShadowAlpha; // Shadow Alpha
    // u8 x6_pad[2]; // ignore, padding managed by StructLayout?
    public short CamFov; // cam fov
    public short xA; // unk, is this also padding?? i don't see what this is related to
    public int DistMin; // cam dist min
    public int DistMax; // cam dist max
    public int TiltScale; // tilt scale (degs)
    public float RotationVertical; // vertical rotation
    public float RotationHorizontal; // horizontal rotation
    public float x20_fixedness; // "fixedness"?
    public float BubbleMultiplier; // BubbleMultiplier
    public float MovementSmoothness; // camera speed smoothness
    // u8 x2C_pad[0x2E - 0x2C]; // same here?
    public short x2E; // ?
    public int PauseMinZ; // PauseMinZ
    public int PauseInitialZ; // PauseInitialZ
    public int PauseMaxZ; // PauseMaxZ
    public float PauseMaxAngleUp; // PauseMaxAngleUp
    public float PauseMaxAngleDown; // PauseMaxAngleDown
    public float PauseMaxAngleLeft; // PauseMaxAngleLeft
    public float PauseMaxAngleRight; // PauseMaxAngleRight
    public bool x4C_fixed_cam_force; // something to do with fixed camera
    public float FixedCamX; // FixedCamX
    public float FixedCamY; // FixedCamY
    public float FixedCamZ; // FixedCamZ
    public float FixedFOV; // FixedFOV
    public float FixedVerticalAngle; // FixedVerticalAngle
    public float FixedHorizontalAngle; // FixedHorizontalAngle
    public short x68; // unknown
    // u8 x6C_pad[0xB0 - 0x6A]; // item spawn weights? this is not padding
    public Ptr32 bgmStructPtr; // left as ptr due to not knowing it
    // UnkBgmStruct* xB0; // BGM data
    /// <summary>Number of entries in <see cref="bgmStructPtr"/>.</summary>
    public int NumBgmVariants; // number of BGM variants
    public GXColor BubbleColorTopLeft; // top left bubble color
    public GXColor BubbleColorTopMiddle; // top middle "
    public GXColor BubbleColorTopRight; // top right "
    public GXColor BubbleColorSideTop; // side top "
    public GXColor BubbleColorSideMiddle; // side middle "
    public GXColor BubbleColorSideBottom; // side bottom "
    public GXColor BubbleColorBottomLeft; // bottom left "
    public GXColor BubbleColorBottomMiddle; // bottom middle "
    public GXColor BubbleColorBottomRight; // bottom right "
};

// Indirect Struct Copies

public struct StageLine(ushort start, ushort end) {
    public u16 StartIdx = start;
    public u16 EndIdx = end;

    // next line data?
    public s16 prev_id0;
    public s16 next_id0;
    public s16 prev_id1;
    public s16 next_id1;

    public CollisionType coll_type; // top, bottom, right, left
    public InteractType interact_type;
    public MaterialType material_type;

    public const nint SIZE = 0x10;

    public override readonly string ToString() => $"coll={coll_type}, int={interact_type}, mat={material_type}";
    // public static void Construct
}

// ENUMS:

public enum ExternalStageId {
    DUMMY = 0,
    TEST = 1,
    IZUMI = 2, // FoD
    PSTADIUM = 3,
    CASTLE = 4,
    KONGO = 5,
    ZEBES = 6,
    CORNERIA = 7,
    STORY = 8,
    ONETT = 9,
    MUTECITY = 10,
    RCRUISE = 11,
    GARDEN = 12,
    GREATBAY = 13,
    SHRINE = 14, // Temple
    KRAID = 15, // Depths
    YOSTER = 16, // Yoshi's Island
    GREENS = 17,
    FOURSIDE = 18,
    INISHIE1 = 19, // Kingdom 1
    INISHIE2 = 20, // Kingdom 2
    AKANEIA = 21,  // debug only?
    VENOM = 22,
    PURA = 23, // Poke Floats
    BIGBLUE = 24,
    ICEMT = 25, // Ice Mountain
    ICETOP = 26, // debug only?
    FLATZONE = 27,
    OLD_PPP = 28, // Dreamland 64
    OLD_YOSH = 29, // Yoshi's Island
    OLD_KONG = 30,
    BATTLE = 31,
    LAST = 32,

    // T = Training, plus character name
    TMARIO = 33,
    TCAPTAIN = 34,
    TCLINK = 35,
    TDONKEY = 36,
    TDRMARIO = 37,
    TFALCO = 38,
    TFOX = 39,
    TICECLIM = 40,
    TKIRBY = 41,
    TKOOPA = 42,
    TLINK = 43,
    TLUIGI = 44,
    TMARS = 45,
    TMEWTWO = 46,
    TNESS = 47,
    TPEACH = 48,
    TPICHU = 49,
    TPIKACHU = 50,
    TPURIN = 51,
    TSAMUS = 52,
    TSEAK = 53,
    TYOSHI = 54,
    TZELDA = 55,
    TGAMEWAT = 56,
    TEMBLEM = 57,
    TGANON = 58,

    _1_1KINOKO = 59, // Adventure Kingdom
    _1_2CASTLE = 60,
    _2_1KONGO = 61,
    _2_2GARDEN = 62,
    _3_1MEIKYU = 63, // Underground Maze
    _3_2SHRINE = 64,
    _4_1ZEBES = 65,
    _4_2DASSYUT = 66, // Brinstar Escape
    _5_1GREENS = 67,
    _5_2GREENS = 68,
    _5_3GREENS = 69,
    _6_1CORNERI = 70,
    _6_2CORNERI = 71,
    _7PSTADIUM = 72,
    _8_1BBROUTE = 73, // F-Zero GP
    _8_2MUTECIT = 74,
    _9_1ONETT = 75,
    _10_1ICEMT = 76,
    _10_2 = 77,
    _11_1BATTLE = 78,
    _11_2BATTLE = 79,
    _12_1LAST = 80,
    _12_2LAST = 81,

    TUKISUSUME = 82, // Race to finish
    FIGUREGET = 83, // trophies
    HOMERUN = 84,
    HEAL = 85 // All-Star rest
}
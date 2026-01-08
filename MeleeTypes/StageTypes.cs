using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.MeleeTypes;

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
public unsafe struct GrGroundParam { // From gr/types.h, UnkStage6B0
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
    public int* bgmStructPtr; // left as ptr due to not knowing it
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

public struct StageLineMap(ushort start, ushort end) {
    //public Vector2 Start = start;
    //public Vector2 End = end;

    public ushort StartIdx = start;
    public ushort EndIdx = end;

    public const nint SIZE = 0x10;
    // public static void Construct
}
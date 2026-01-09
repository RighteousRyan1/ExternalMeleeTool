using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Utilities;
using System.Drawing;
using System.Numerics;

namespace ExternalMeleeTool;

public struct Joint {
    public Vector3 Position;
    public Vector3 Rotation;
}

// will eventually need sequential if i decide to copy over every Fighter struct item
public struct FighterData {
    // only use if you're skilled!!!
    public Ptr32 GObjPtr;
    public Ptr32 FighterPtr;
    public Ptr32 BonesPtr;

    public byte Port;

    public Ptr32 CollDataPtr;
    /// <summary>The fighter's Environmental Collision Box (ECB). Coordinates are relative to the fighter position.</summary>
    public CollData CollData;
    public FtCommonAttr Attr;

    // some other time. FighterHurtCapsule @ offset 11A0 of Fighter
    // public FighterHurtbox[] Hurtboxes;

    /// <summary>The position of the fighter. If the character is transformed, it returns the sub-character position.</summary>
    public Vector3 Position;
    public Vector3 VelocitySelf;
    public Vector3 Knockback;
    /// <summary>The character type.</summary>
    public CKind CharKind;
    public FtAnimState AnimState;

    /// <summary>The kind of slot of this fighter's memory block.</summary>
    public SlotKind SlotKind;
    /// <summary>The team this fighter belongs to.</summary>
    public SlotTeam Team;

    // why did HAL make direction a float? the world will forever be wondering
    // maybe i should change it to a s8 myself
    /// <summary>Either -1.0 for left-facing or 1.0 for right-facing.</summary>
    public float Direction;
    /// <summary>The damage percent of this fighter.</summary>
    public short Percent;

    // and why did HAL allow stocks to be negative semantically???
    /// <summary>How many stocks this fighter has remaining.</summary>
    public sbyte Stocks;

    /// <summary><c>true</c> if the fighter is transformed from their original. (i.e: Sheik from Zelda)</summary>
    public bool IsTransformed;

    public GCInput Input;

    public readonly bool IsDead =>
        AnimState == FtAnimState.DeadUpStar ||
        AnimState == FtAnimState.DeadUpStarIce ||
        AnimState == FtAnimState.DeadLeft ||
        AnimState == FtAnimState.DeadRight ||
        AnimState == FtAnimState.DeadDown ||
        AnimState == FtAnimState.DeadUpFall ||
        AnimState == FtAnimState.DeadUpFallHitCamera ||
        AnimState == FtAnimState.DeadUpFallHitCameraFlat ||
        AnimState == FtAnimState.DeadUpFallHitCameraIce;

    public readonly string FriendlyString() => $"Fighter: {CharKind} | {Position}";
    public override readonly string ToString() => $"FighterBlock(CKind={CharKind}, Pos={Position}, SKind={SlotKind}, Team={Team}, Dir={Direction}, %={Percent}, Stocks={Stocks})";

    internal static readonly Dictionary<CKind, CKind> SubCharMap = new() {
        [CKind.Zelda] = CKind.Sheik
        // [CKind.PopoNana] = CKind.
    };

    // CommonPart when finished.
    /// <summary>
    /// Returns the transform matrix of the given bone part.
    /// </summary>
    /// <param name="part">The part of the body.</param>
    public readonly Matrix3x4 GetBoneTransform(FtPart part) {
        // nint part_jobj = Slippinterop.ReadPtr(Bones + (uint)part * MeleeConstants.FTPART_SIZE); // the jobj is 0x0 from FighterBone so we can skip that offset


        // whatever this offset is. Thanks Altafen!
        //nint charSkelInfo = Slippinterop.ReadPtr(MeleeConstants.R13 - 0x515C);
        //nint commonBoneMap = Slippinterop.ReadPtr(charSkelInfo + (uint)CharKind * 4);


        //byte part = Slippinterop.ReadU8(commonBoneMap + (uint)bone);
        Ptr32 parts = Dolphinterop.ReadPtr(FighterPtr + 0x5E8);
        Ptr32 jobj = Dolphinterop.ReadPtr(parts + (uint)part * MeleeConstants.FTPART_SIZE);



        var mtx = Dolphinterop.Read<Matrix3x4>(jobj + 0x44); //Slippinterop.ReadMatrix3x4(jobj + 0x44); // 0x44 is the matrix offset in HSD_JObj
        // Console.WriteLine(mtx);

        return mtx;
    }
}
/// <summary>A structure representing the match's settings.</summary>
public struct MatchData {
    public FighterData[] Fighters;
    /// <summary>If there is an active teams match, <c>true</c>, else, <c>false</c>.</summary>
    public bool IsTeams;
}
public struct StageData {
    /// <summary>The ID of the stage being played on.</summary>
    public ExternalStageId StageId;
    // public float Scale;
    public GrGroundParam GroundParams;

    public int LineCount;
    public int VertexCount;
    public Vector2[] Vertices;
    public StageLine[] MapLines;

    public BoundingRect BlastZone;

    public StageCameraInfo CameraInfo;

    public BoundingRect GetRealBlastZone() {
        return new BoundingRect() {
            Top = BlastZone.Top + CameraInfo.OffsetY,
            Bottom = BlastZone.Bottom + CameraInfo.OffsetY,
            Left = BlastZone.Left + CameraInfo.OffsetX,
            Right = BlastZone.Right + CameraInfo.OffsetX
        };
    }
    public BoundingRect GetRealCameraBounds() {
        // GroundParams.
        return new BoundingRect() {
            Top = CameraInfo.CamBounds.Top + CameraInfo.OffsetY,
            Bottom = CameraInfo.CamBounds.Bottom + CameraInfo.OffsetY,
            Left = CameraInfo.CamBounds.Left + CameraInfo.OffsetX,
            Right = CameraInfo.CamBounds.Right + CameraInfo.OffsetX
        };
    }
}

public struct SlippiOnlineData {
    public byte ClientPort;
    public byte ClientControllerPort;
    public bool InOnlineMatch;
    public byte Frame;

    public static bool IsSlippiOnline(GlobalMeleeData gmd) {
        // for whatever reason, this indicates online melee
        return gmd.MinorScene == 8 && gmd.MajorScene == 2;
    }
    // fails if IsSlippiOnline is false
    public static byte GetClientPort(GlobalMeleeData gmd) {
        if (!IsSlippiOnline(gmd)) return 255;

        var odb_ptr = Dolphinterop.ReadPtr(SlippiConstants.ONLINE_DATA_BLOCK);

        var cli_port = Dolphinterop.ReadU8(odb_ptr);
        // var guh = $"{port_ptr:X} {Slippinterop.GALE01:X}";

        return cli_port;
    }
}
/// <summary>A structure holding data relating to common melee data that isn't bound to gameplay.</summary>
public struct GlobalMeleeData {
    /// <summary>The 'minor' scene data ID. Typically involves sub-menus.</summary>
    public byte MinorScene;
    /// <summary>The 'major' scene data ID. Typically involves different game states.</summary>
    public byte MajorScene;

    public readonly bool IsIngame => MajorScene == 2;
}

using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Runtime.CompilerServices;

namespace ExternalMeleeTool.GameComponents;

// will eventually need sequential if i decide to copy over every Fighter struct item
// however this struct is huge... like literally reading almost 10k bytes *every* fetch
public unsafe struct FighterData {
    // only use if you're skilled!!!
    public GObj GObj;
    public Struct_t FighterPtr;
    public Struct_t BonesPtr;

    public byte Port;

    public Struct_t CollDataPtr;
    /// <summary>The fighter's Environmental Collision Box (ECB). Coordinates are relative to the fighter position.</summary>
    public CollData CollData;

    public FigATree AnimTree;

    // public FighterBone

    // experimental
    public StructHint<FtCommonAttr> Attr;

    // todo: add special attr

    // hal made this an enum and thankfully i debloated it!
    public int Grounded;

    // some other time. FighterHurtCapsule @ offset 11A0 of Fighter
    //public FighterHurtbox[] Hurtboxes;
    // 15 found at address Fighter + 0x11A0
    public FighterHurtCapsuleBuffer15 Hurtboxes;

    // two separate arrays... one with 4 hitboxes @ x914, one with 2 hitboxes @ xDF4
    // gonna leave the last 2 out for now
    public HitCapsuleBuffer6 Hitboxes;

    public Struct_t PositionPtr;
    /// <summary>The position of the fighter. If the character is transformed, it returns the sub-character position.</summary>
    public Vec3 Position;
    public Vec3 VelocitySelf;
    public Vec3 Knockback;
    /// <summary>The character type.</summary>
    public FtKind CharKind;
    public FtAnimState AnimState;

    /// <summary>The kind of slot of this fighter's memory block.</summary>
    public SlotKind SlotKind;
    /// <summary>The team this fighter belongs to.</summary>
    public SlotTeam Team;

    // why did HAL make direction a float? the world will forever be wondering
    // maybe i should change it to a s8 myself
    /// <summary>Either -1.0 for left-facing or 1.0 for right-facing.</summary>
    public float Direction;

    /// <summary>To get a percentage, divide this value by 60.</summary>
    public float ShieldHealth;
    public float AnimFrame;
    /// <summary>The damage percent of this fighter.</summary>
    public short Percent;

    // and why did HAL allow stocks to be negative semantically???
    /// <summary>How many stocks this fighter has remaining.</summary>
    public sbyte Stocks;

    /// <summary><c>true</c> if the fighter is transformed from their original. (i.e: Sheik from Zelda)</summary>
    public bool IsTransformed;

    public FighterInput Input;

    public readonly bool IsShielding =>
        AnimState == FtAnimState.Guard ||
        AnimState == FtAnimState.GuardOn ||
        AnimState == FtAnimState.GuardOff;
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

    public readonly bool IsKnockedBack =>
        AnimState == FtAnimState.DamageAir1 ||
        AnimState == FtAnimState.DamageAir2 ||
        AnimState == FtAnimState.DamageAir3 ||
        AnimState == FtAnimState.DamageFlyHi ||
        AnimState == FtAnimState.DamageFlyLw ||
        AnimState == FtAnimState.DamageFlyN ||
        AnimState == FtAnimState.DamageFlyRoll ||
        AnimState == FtAnimState.DamageFlyTop;
    public readonly bool IsOnLedge =>
        AnimState == FtAnimState.CliffCatch ||
        AnimState == FtAnimState.CliffWait;

    public readonly string FriendlyString() {
        // 1. PadRight(12) ensures the Name always takes up 12 spaces.
        // 2. {Position.X,7:F2} means "allocate 7 spaces for this number".
        return $"{CharKind,-12} | <{Position.X,5:F2}, {Position.Y,5:F2}, {Position.Z:F2}>";
    }
    public override readonly string ToString() => $"FighterBlock(CKind={CharKind}, Pos={Position}, SKind={SlotKind}, Team={Team}, Dir={Direction}, %={Percent}, Stocks={Stocks})";

    internal static readonly Dictionary<FtKind, FtKind> SubCharMap = new() {
        [FtKind.Zelda] = FtKind.Sheik
        // [CKind.PopoNana] = CKind.
    };

    /// <summary>
    /// Returns the transform matrix of the given bone part, with appropriate mapping.
    /// </summary>
    /// <param name="part">The part of the body.</param>
    public readonly FighterBone GetBone(FtPart part) {
        var tbl = GetPartTable();
        // no multiplication because byte is size 1
        var mappedIndex = Dolphinterop.ReadU8(tbl.part_to_joint + (uint)part);

        Struct_t parts = Dolphinterop.ReadPtr(FighterPtr + 0x5E8);
        var bone = Dolphinterop.Read<FighterBone>(parts + (mappedIndex * FighterBone.SIZE));

        return bone;
    }

    // 80C76E10 = ganon's FtPartsTable
    // 80C76D84 = ganon's joint_to_part
    // 80C76DD8 = ganon's part_to_joint
    public readonly FtPart GetPartFromJoint(int joint_idx) {
        var tbl = GetPartTable();
        // no multiplication because byte is size 1
        var mappedIndex = Dolphinterop.ReadU8(tbl.joint_to_part + joint_idx);

        return (FtPart)mappedIndex;
    }

    public readonly FighterBone GetUnmappedBone(int joint) {
        Struct_t parts = Dolphinterop.ReadPtr(FighterPtr + 0x5E8);
        var bone = Dolphinterop.Read<FighterBone>(parts + (joint * FighterBone.SIZE));

        return bone;
    }

    /// <summary>
    /// Gets this fighter's mapped parts table.
    /// </summary>
    public readonly FtPartsTable GetPartTable() {
        var kind = CharKind;

        var tblPtr = Dolphinterop.ReadPtr(MeleeGlobals.CHR_SKEL_INFO_TABLE);
        var charBoneMap = Dolphinterop.ReadPtr(tblPtr + (uint)kind * 4);

        return Dolphinterop.Read<FtPartsTable>(charBoneMap);
    }

    [InlineArray(15)]
    public struct FighterHurtCapsuleBuffer15 {
        FighterHurtCapsule _capsule;

        public const uint LENGTH = 15;
    }

    // 4 hitbox array, then a 2 hitbox array, then a "thrown hitbox"
    // however, i don't want to include the thrown hitbox as it's always present (but inactive) at XRotN
    [InlineArray(6)]
    public struct HitCapsuleBuffer6 {
        HitCapsule _capsule;

        public const uint LENGTH = 6;
    }
}

using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using System.Runtime.CompilerServices;

namespace ExternalMeleeTool.GameComponents;

// will eventually need sequential if i decide to copy over every Fighter struct item
// however this struct is huge... like literally reading almost 10kb *every* fetch
/// <summary>
/// Describes a fighter. Not a direct struct copy- do not write.
/// </summary>
public unsafe struct FighterData {
    /// <summary>This data is not fetched by default. Call <see cref="TryGetPlCo"/> to get this data.</summary>
    public static FtCommonData PlCo;

    // only use if you're skilled!!!
    public GObj GObj;
    public Struct_t FighterPtr;
    public Struct_t BonesPtr;

    /// <summary>The port of this fighter.</summary>
    public byte Port;

    public Struct_t CollDataPtr;

    /// <summary>Contains data relating to this fighter's collision.</summary>
    public CollData CollData;

    /// <summary>Contains data relating to this fighter's animation tree.</summary>
    public FigATree AnimTree;

    // public FighterBone

    // experimental
    /// <summary>The fighter's common attributes (i.e: dash speed, jump height, etc).</summary>
    public FtCommonAttr Attr;

    // todo: add special attr

    // hal made this an enum and thankfully i debloated it!
    public bool Grounded;

    // some other time. FighterHurtCapsule @ offset 11A0 of Fighter
    //public FighterHurtbox[] Hurtboxes;
    // 15 found at address Fighter + 0x11A0
    /// <summary>A buffer/list of the 15 (<see cref="FighterHurtCapsuleBuffer15.LENGTH"/>) hurtboxes used by this fighter.</summary>
    public FighterHurtCapsuleBuffer15 Hurtboxes;

    // two separate arrays... one with 4 hitboxes @ x914, one with 2 hitboxes @ xDF4
    // gonna leave the last 2 out for now
    /// <summary>A buffer/list of the 6 (<see cref="HitCapsuleBuffer6.LENGTH"/>) hitboxes used by this fighter during attacks.</summary>
    public HitCapsuleBuffer6 Hitboxes;

    public Ptr32 PositionPtr;
    /// <summary>The position of the fighter. If the character is transformed, it returns the sub-character position.</summary>
    public Vec3 Position;

    /// <summary>The self-imposed velocity of the fighter.</summary>
    public Vec3 VelocitySelf;

    public Vec3 VelocityCombined;

    /// <summary>The current knockback value of the fighter.</summary>
    public Vec3 Knockback;

    /// <summary>The uniform scale of the fighter.</summary>
    public Vec3 Scale;
    /// <summary>The character type.</summary>
    public FtKind CharKind;

    public int ActionId;
    public FtAnimState AnimState;
    /// <summary>The kind of slot of this fighter's memory block.</summary>
    public SlotKind SlotKind;
    /// <summary>The team this fighter belongs to.</summary>
    public SlotTeam Team;

    /// <summary>Contains many references to this character's display objects (<see cref="DObj"/>).</summary>
    public DObjList DObjs;

    // why did HAL make direction a float? the world will forever be wondering
    // maybe i should change it to a s8 myself
    /// <summary>Either -1.0 for left-facing or 1.0 for right-facing.</summary>
    public float Direction;

    /// <summary>To get a percentage, divide this value by 60.</summary>
    public float ShieldHealth;

    /// <summary>The current frame of this character's animation.</summary>
    public float AnimFrame;

    /// <summary>The speed of this character's current animation.</summary>
    public float AnimRate;
    /// <summary>The staled_damage percent of this fighter.</summary>
    public short Percent;

    // and why did HAL allow stocks to be negative semantically???
    /// <summary>How many stocks this fighter has remaining.</summary>
    public sbyte Stocks;

    /// <summary><c>true</c> if the fighter is transformed from their original. (i.e: Sheik from Zelda)</summary>
    public bool IsTransformed;

    /// <summary>Contains various data about this fighter's port input.</summary>
    public FighterInput Input;

    /// <summary>Checks if this fighter is in a shielding animation.</summary>
    public readonly bool IsShielding =>
        AnimState == FtAnimState.Guard ||
        AnimState == FtAnimState.GuardOn ||
        AnimState == FtAnimState.GuardOff;

    /// <summary>Checks if this fighter is dead.</summary>
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
    /// <summary>Checks if this fighter is in a state caused by knockback.</summary>
    public readonly bool IsKnockedBack =>
        AnimState == FtAnimState.DamageHi1 ||
        AnimState == FtAnimState.DamageHi2 ||
        AnimState == FtAnimState.DamageHi3 ||

        AnimState == FtAnimState.DamageLw1 ||
        AnimState == FtAnimState.DamageLw3 ||
        AnimState == FtAnimState.DamageLw3 ||

        AnimState == FtAnimState.DamageAir1 ||
        AnimState == FtAnimState.DamageAir2 ||
        AnimState == FtAnimState.DamageAir3 ||
        AnimState == FtAnimState.DamageFlyHi ||
        AnimState == FtAnimState.DamageFlyLw ||
        AnimState == FtAnimState.DamageFlyN ||
        AnimState == FtAnimState.DamageFlyRoll ||
        AnimState == FtAnimState.DamageFlyTop;

    /// <summary>Checks if this fighter has any sort of knockback.</summary>
    public readonly bool HasKnockback => Knockback.LengthSquared() > 0;

    // public readonly float KnockbackAngle => MathF.Atan2(Knockback.Y, Knockback.X);

    /// <summary>Checks if this fighter is on a ledge.</summary>
    public readonly bool IsOnLedge =>
        AnimState == FtAnimState.CliffCatch ||
        AnimState == FtAnimState.CliffWait;

    public readonly string FriendlyString() {
        // PadRight(12) ensures the Name always takes up 12 spaces.
        // {Position.X,7:F2} means "allocate 7 spaces for this number"
        return $"{CharKind,-12} | <{Position.X,5:F2}, {Position.Y,5:F2}, {Position.Z:F2}>";
    }
    public override readonly string ToString() => $"FighterBlock(CKind={CharKind}, Pos={Position}, SKind={SlotKind}, Team={Team}, Dir={Direction}, %={Percent}, Stocks={Stocks})";

    /// <summary>
    /// A conversion of character to their sub-character.
    /// </summary>
    internal static readonly Dictionary<FtKind, FtKind> SubCharMap = new() {
        [FtKind.Zelda] = FtKind.Sheik
        // [CKind.PopoNana] = CKind.
    };

    /// <summary>
    /// Gets the *full* action symbol name of the given action ID.
    /// </summary>
    /// <param name="actionId"></param>
    /// <returns>The full symbol name of the action.</returns>
    public readonly string GetActionNameFull(int actionId) {
        var action_table = Dolphinterop.ReadPtr(FighterPtr + 0x24);

        var action = Dolphinterop.Read<FtAction>(action_table + actionId * FtAction.SIZE);

        var action_name = Dolphinterop.ReadString(action.anim_symbol);

        return action_name;
    }
    /// <summary>
    /// Gets strictly the name of the action as opposed to the entire symbol of the given action ID.
    /// </summary>
    /// <returns>The truncated name.</returns>
    public readonly string GetActionNameTrunc(int actionId) {
        var action_table = Dolphinterop.ReadPtr(FighterPtr + 0x24);

        var action = Dolphinterop.Read<FtAction>(action_table + actionId * FtAction.SIZE);

        var action_name = Dolphinterop.ReadString(action.anim_symbol);

        if (string.IsNullOrEmpty(action_name)) return "N/A";

        var split = action_name.Split('_');
        if (split.Length < 4) return "N/A";

        return split[3];
    }

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
    public readonly FtPart GetPartFromBoneIndex(int joint_idx) {
        var tbl = GetPartTable();
        // no multiplication because byte is size 1
        var mappedIndex = Dolphinterop.ReadU8(tbl.joint_to_part + joint_idx);

        return (FtPart)mappedIndex;
    }
    public readonly int GetBoneCount() {
        var start = BonesPtr;
        var curPtr = start;
        // var curBone = Dolphinterop.Read<FighterBone>(start);
        int numBones = 0;
        while (Dolphinterop.ReadU32(curPtr + FighterBone.SIZE) != 0) {
            numBones++;
            curPtr += FighterBone.SIZE;
            // curBone = Dolphinterop.Read<FighterBone>(curPtr);
        }

        return numBones;
    }
    public readonly FighterBone GetUnmappedBone(int joint) {
        Struct_t parts = Dolphinterop.ReadPtr(FighterPtr + 0x5E8);
        var bone = Dolphinterop.Read<FighterBone>(parts + (joint * FighterBone.SIZE));

        return bone;
    }

    /// <summary>Call once, loads into <see cref="PlCo"/>. This data is shared amongst the cast, and is static.</summary>
    /// <returns><c>true</c> if successful, <c>false</c> if not.</returns>
    public static bool TryGetPlCo() {
        PlCo = Dolphinterop.Read<FtCommonData>(Dolphinterop.ReadPtr(MeleePointers.PLCO_PTR));
        if (PlCo.Equals(default)) return false;
        return true;
    }

    /// <summary>
    /// Gets this fighter's mapped parts table.
    /// </summary>
    public readonly FtPartsTable GetPartTable() {
        var kind = CharKind;

        var tblPtr = Dolphinterop.ReadPtr(MeleePointers.CHR_SKEL_INFO_TABLE);
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

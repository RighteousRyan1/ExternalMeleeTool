using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee;

/// <summary>
/// Attributes that are common to the Melee cast.
/// </summary>
/// <remarks>
/// Yes, this is a direct struct copy from the Melee decompilation.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct FtCommonAttr { // loaded from .dat files.
    /* +000 fp+110 */
    public float walk_init_vel;
    /* +004 fp+114 */
    public float walk_accel;
    /* +008 fp+118 */
    public float walk_max_vel;
    /* +00C fp+11C */
    public float slow_walk_max;
    /* +010 fp+120 */
    public float mid_walk_point;
    /* +014 fp+124 */
    public float fast_walk_min;
    /* +018 fp+128 */
    public float gr_friction;
    /* +01C fp+12C */
    public float dash_initial_velocity;
    /* +020 fp+130 */
    public float dash_run_acceleration_a;
    /* +024 fp+134 */
    public float dash_run_acceleration_b;
    /* +028 fp+138 */
    public float dash_run_terminal_velocity;
    /* +02C fp+13C */
    public float run_animation_scaling;
    /* +030 fp+140 */
    public float max_run_brake_frames;
    /* +034 fp+144 */
    public float ground_max_horizontal_velocity;
    /* +038 fp+148 */
    public float jump_startup_time;
    /* +03C fp+14C */
    public float jump_h_initial_velocity;
    /* +040 fp+150 */
    public float jump_v_initial_velocity;
    /* +044 fp+154 */
    public float ground_to_air_jump_momentum_multiplier;
    /* +048 fp+158 */
    public float jump_h_max_velocity;
    /* +04C fp+15C */
    public float hop_v_initial_velocity;
    /* +050 fp+160 */
    public float air_jump_v_multiplier;
    /* +054 fp+164 */
    public float air_jump_h_multiplier;
    /* +058 fp+168 */
    public int max_jumps;
    /* +05C fp+16C */
    public float grav;
    /* +060 fp+170 */
    public float terminal_vel;
    /* +064 fp+174 */
    public float air_drift_stick_mul;
    /* +068 fp+178 */
    public float aerial_drift_base;
    /* +06C fp+17C */
    public float air_drift_max;
    /* +070 fp+180 */
    public float aerial_friction;
    /* +074 fp+184 */
    public float fast_fall_velocity;
    /* +078 fp+188 */
    public float air_max_horizontal_velocity;
    /* +07C fp+18C */
    public float jab_2_input_window;
    /* +080 fp+190 */
    public float jab_3_input_window;
    /* +084 fp+194 */
    public float frames_to_change_direction_on_standing_turn;
    /* +088 fp+198 */
    public float weight;
    /* +08C fp+19C */
    public float model_scaling;
    /* +090 fp+1A0 */
    public float initial_shield_size;
    /* +094 fp+1A4 */
    public float shield_break_initial_velocity;
    /* +098 fp+1A8 */
    public int rapid_jab_window;
    /* +09C fp+1AC */
    public float x9C;
    /* +0A0 fp+1B0 */
    public int xA0;
    /* +0A4 fp+1B4 */
    public int xA4;
    /* +0A8 fp+1B8 */
    public float ledge_jump_horizontal_velocity;
    /* +0AC fp+1BC */
    public float ledge_jump_vertical_velocity;
    /* +0B0 fp+1C0 */
    public float item_throw_velocity_multiplier;
    /* +0B4 fp+1C4 */
    public float xB4;
    /* +0B8 fp+1C8 */
    public float xB8;
    /* +0BC fp+1CC */
    public FtCommonAttr_xBC xBC;
    //ftCo_DatAttrs_xBC_t xBC;
    /* +0DC fp+1EC */
    public float xDC;
    /* +0E0 fp+1F0 */
    public float kirby_b_star_damage;
    /* +0E4 fp+1F4 */
    public float normal_landing_lag;
    /* +0E8 fp+1F8 */
    public float landingairn_lag;
    /* +0EC fp+1FC */
    public float landingairf_lag;
    /* +0F0 fp+200 */
    public float landingairb_lag;
    /* +0F4 fp+204 */
    public float landingairhi_lag;
    /* +0F8 fp+208 */
    public float landingairlw_lag;
    /* +0FC fp+20C */
    public float name_tag_height;
    /* +100 fp+210 */
    public float passivewall_vel_x;
    /* +104 fp+214 */
    public float wall_jump_horizontal_velocity;
    /* +108 fp+218 */
    public float wall_jump_vertical_velocity;
    /* +10C fp+21C */
    public float passiveceil_vel_x;
    /* +110 fp+220 */
    public float trophy_scale;
    /* +114 fp+224 */
    public Vector3 x114;
    /* +120 fp+230 */
    public Vector3 x120;
    /* +12C fp+23C */
    public float x12C;
    /* +130 fp+240 */
    public Vector3 x130;
    /* +13C fp+24C */
    public float x13C;
    /* +140 fp+250 */
    public float x140;
    /* +144 fp+254 */
    public float x144;
    /* +148 fp+258 */
    public float x148;
    /* +14C fp+25C */
    public float damageice_ice_size;
    /* +150 fp+260 */
    public float x150_damageice_unk;
    /* +154 fp+264 */
    public float x154_damageice_unk;
    /* +158 fp+268 */
    public float damageicejump_vel_y;
    /* +15C fp+26C */
    public float damageicejump_vel_x_mult;
    /* +160 fp+270 */
    public float respawn_platform_scale;
    /* +164 fp+274 */
    public float x164;
    /* +168 fp+278 */
    public float x168;
    /* +16C fp+27C */
    public int camera_zoom_target_bone;
    /* +170 fp+280 */
    public Vector3 x170;
    /* +17C fp+28C */
    public float x17C;
    /* +180 fp+290 */
    public byte weight_independent_throws_mask;
}

/// <summary>
/// Fighter Animation Tree, I think.
/// </summary>
public struct FigATree {
    public int type;
    public u32 flags;
    public f32 frames;
    public Ptr32 nodes; // s8*
    public Ptr32 tracks; // FigaTrack*
}

// unsure what these are, but they're necessary to commplete FtCommonAttr
public struct FtCommonAttr_xBC {
    public float size; // size of what? who knows
    public Vector3 x4;
    public Vector3 x10;
    public float x1C;
}

// ENUMS:

public enum ECBSourceKind : int { None, JObj, Fixed }

public enum FighterMemorySlot : uint {
    IndexOne = MeleeGlobals.PLAYER_ONE,
    IndexTwo = MeleeGlobals.PLAYER_TWO,
    IndexThree = MeleeGlobals.PLAYER_THREE,
    IndexFour = MeleeGlobals.PLAYER_FOUR
}

public enum SlotTeam { Red, Blue, Green }
// unsure of the other kinds...
public enum SlotKind { Human, CPU, Demo, None, Boss }

// hal smoking crack to make these different as per usual.
public enum FtKind : u32 {
    Mario = 0x00,
    Fox = 0x01,
    CaptainFalcon = 0x02,
    DonkeyKong = 0x03,
    Kirby = 0x04,
    Bowser = 0x05,
    Link = 0x06,
    Sheik = 0x07,
    Ness = 0x08,
    Peach = 0x09,
    Popo = 0x0A,
    Nana = 0x0B,
    Pikachu = 0x0C,
    Samus = 0x0D,
    Yoshi = 0x0E,
    Jigglypuff = 0x0F,
    Mewtwo = 0x10,
    Luigi = 0x11,
    Marth = 0x12,
    Zelda = 0x13,
    YoungLink = 0x14,
    DrMario = 0x15,
    Falco = 0x16,
    Pichu = 0x17,
    GameWatch = 0x18,
    Ganondorf = 0x19,
    Roy = 0x1A,
    MasterHand = 0x1B,
    CrazyHand = 0x1C,
    MaleWireframe = 0x1D,
    FemaleWireframe = 0x1E,
    GigaBowser = 0x1F,
    Sandbag = 0x20,
    None = 0x21
}

// holy smokes this is a big enum
public enum FtAnimState {
    DeadDown,
    DeadLeft,
    DeadRight,
    DeadUp,
    DeadUpStar,
    DeadUpStarIce,
    DeadUpFall,
    DeadUpFallHitCamera,
    DeadUpFallHitCameraFlat,
    DeadUpFallIce,
    DeadUpFallHitCameraIce,
    Sleep,
    Rebirth,
    RebirthWait,
    Wait,
    WalkSlow,
    WalkMiddle,
    WalkFast,
    Turn,
    TurnRun,
    Dash,
    Run,
    RunDirect,
    RunBrake,
    KneeBend,
    JumpF,
    JumpB,
    JumpAerialF,
    JumpAerialB,
    Fall,
    FallF,
    FallB,
    FallAerial,
    FallAerialF,
    FallAerialB,
    FallSpecial,
    FallSpecialF,
    FallSpecialB,
    DamageFall,
    Squat,
    SquatWait,
    SquatRV,
    Landing,
    LandingFallSpecial,
    Attack11,
    Attack12,
    Attack13,
    Attack100Start,
    Attack100Loop,
    Attack100End,
    AttackDash,
    AttackS3Hi,
    AttackS3HiS,
    AttackS3S,
    AttackS3LwS,
    AttackS3Lw,
    AttackHi3,
    AttackLw3,
    AttackS4Hi,
    AttackS4HiS,
    AttackS4S,
    AttackS4LwS,
    AttackS4Lw,
    AttackHi4,
    AttackLw4,
    AttackAirN,
    AttackAirF,
    AttackAirB,
    AttackAirHi,
    AttackAirLw,
    LandingAirN,
    LandingAirF,
    LandingAirB,
    LandingAirHi,
    LandingAirLw,
    DamageHi1,
    DamageHi2,
    DamageHi3,
    DamageN1,
    DamageN2,
    DamageN3,
    DamageLw1,
    DamageLw2,
    DamageLw3,
    DamageAir1,
    DamageAir2,
    DamageAir3,
    DamageFlyHi,
    DamageFlyN,
    DamageFlyLw,
    DamageFlyTop,
    DamageFlyRoll,
    LightGet,
    HeavyGet,
    LightThrowF,
    LightThrowB,
    LightThrowHi,
    LightThrowLw,
    LightThrowDash,
    LightThrowDrop,
    LightThrowAirF,
    LightThrowAirB,
    LightThrowAirHi,
    LightThrowAirLw,
    HeavyThrowF,
    HeavyThrowB,
    HeavyThrowHi,
    HeavyThrowLw,
    LightThrowF4,
    LightThrowB4,
    LightThrowHi4,
    LightThrowLw4,
    LightThrowAirF4,
    LightThrowAirB4,
    LightThrowAirHi4,
    LightThrowAirLw4,
    HeavyThrowF4,
    HeavyThrowB4,
    HeavyThrowHi4,
    HeavyThrowLw4,
    SwordSwing1,
    SwordSwing3,
    SwordSwing4,
    SwordSwingDash,
    BatSwing1,
    BatSwing3,
    BatSwing4,
    BatSwingDash,
    ParasolSwing1,
    ParasolSwing3,
    ParasolSwing4,
    ParasolSwingDash,
    HarisenSwing1,
    HarisenSwing3,
    HarisenSwing4,
    HarisenSwingDash,
    StarRodSwing1,
    StarRodSwing3,
    StarRodSwing4,
    StarRodSwingDash,
    LipstickSwing1,
    LipstickSwing3,
    LipstickSwing4,
    LipstickSwingDash,
    ItemParasolOpen,
    ItemParasolFall,
    ItemParasolFallSpecial,
    ItemParasolDamageFall,
    LGunShoot,
    LGunShootAir,
    LGunShootEmpty,
    LGunShootAirEmpty,
    FireFlowerShoot,
    FireFlowerShootAir,
    ItemScrew,
    ItemScrewAir,
    DamageScrew,
    DamageScrewAir,
    ItemScopeStart,
    ItemScopeRapid,
    ItemScopeFire,
    ItemScopeEnd,
    ItemScopeAirStart,
    ItemScopeAirRapid,
    ItemScopeAirFire,
    ItemScopeAirEnd,
    ItemScopeStartEmpty,
    ItemScopeRapidEmpty,
    ItemScopeFireEmpty,
    ItemScopeEndEmpty,
    ItemScopeAirStartEmpty,
    ItemScopeAirRapidEmpty,
    ItemScopeAirFireEmpty,
    ItemScopeAirEndEmpty,
    LiftWait,
    LiftWalk1,
    LiftWalk2,
    LiftTurn,
    GuardOn,
    Guard,
    GuardOff,
    GuardSetOff,
    GuardReflect,
    DownBoundU,
    DownWaitU,
    DownDamageU,
    DownStandU,
    DownAttackU,
    DownForwardU,
    DownBackU,
    DownSpotU,
    DownBoundD,
    DownWaitD,
    DownDamageD,
    DownStandD,
    DownAttackD,
    DownForwardD,
    DownBackD,
    DownSpotD,
    Passive,
    PassiveStandF,
    PassiveStandB,
    PassiveWall,
    PassiveWallJump,
    PassiveCeil,
    ShieldBreakFly,
    ShieldBreakFall,
    ShieldBreakDownU,
    ShieldBreakDownD,
    ShieldBreakStandU,
    ShieldBreakStandD,
    FuraFura,
    Catch,
    CatchPull,
    CatchDash,
    CatchDashPull,
    CatchWait,
    CatchAttack,
    CatchCut,
    ThrowF,
    ThrowB,
    ThrowHi,
    ThrowLw,
    CapturePulledHi,
    CaptureWaitHi,
    CaptureDamageHi,
    CapturePulledLw,
    CaptureWaitLw,
    CaptureDamageLw,
    CaptureCut,
    CaptureJump,
    CaptureNeck,
    CaptureFoot,
    EscapeF,
    EscapeB,
    Escape,
    EscapeAir,
    ReboundStop,
    Rebound,
    ThrownF,
    ThrownB,
    ThrownHi,
    ThrownLw,
    ThrownLwWomen,
    Pass,
    OttOtto,
    OttOttoWait,
    FlyReflectWall,
    FlyReflectCeil,
    StopWall,
    StopCeil,
    MissFoot,
    CliffCatch,
    CliffWait,
    CliffClimbSlow,
    CliffClimbQuick,
    CliffAttackSlow,
    CliffAttackQuick,
    CliffEscapeSlow,
    CliffEscapeQuick,
    CliffJumpSlow1,
    CliffJumpSlow2,
    CliffJumpQuick1,
    CliffJumpQuick2,
    AppealR,
    AppealL,
    ShoulderedWait,
    ShoulderedWalkSlow,
    ShoulderedWalkMiddle,
    ShoulderedWalkFast,
    ShoulderedTurn,
    ThrownFF,
    ThrownFB,
    ThrownFHi,
    ThrownFLw,
    CaptureCaptain,
    CaptureYoshi,
    YoshiEgg,
    CaptureKoopa,
    CaptureDamageKoopa,
    CaptureWaitKoopa,
    ThrownKoopaF,
    ThrownKoopaB,
    CaptureKoopaAir,
    CaptureDamageKoopaAir,
    CaptureWaitKoopaAir,
    ThrownKoopaAirF,
    ThrownKoopaAirB,
    CaptureKirby,
    CaptureWaitKirby,
    ThrownKirbyStar,
    ThrownCopyStar,
    ThrownKirby,
    BarrelWait,
    Bury,
    BuryWait,
    BuryJump,
    DamageSong,
    DamageSongWait,
    DamageSongRV,
    DamageBind,
    CaptureMewtwo,
    CaptureMewtwoAir,
    ThrownMewtwo,
    ThrownMewtwoAir,
    WarpStarJump,
    WarpStarFall,
    HammerWait,
    HammerWalk,
    HammerTurn,
    HammerKneeBend,
    HammerFall,
    HammerJump,
    HammerLanding,
    KinokoGiantStart,
    KinokoGiantStartAir,
    KinokoGiantEnd,
    KinokoGiantEndAir,
    KinokoSmallStart,
    KinokoSmallStartAir,
    KinokoSmallEnd,
    KinokoSmallEndAir,
    Entry,
    EntryStart,
    EntryEnd,
    DamageIce,
    DamageIceJump,
    CaptureMasterHand,
    CaptureDamageMasterHand,
    CaptureWaitMasterHand,
    ThrownMasterHand,
    CaptureKirbyYoshi,
    KirbyYoshiEgg,
    CaptureLeadDead,
    CaptureLikeLike,
    DownReflect,
    CaptureCrazyHand,
    CaptureDamageCrazyHand,
    CaptureWaitCrazyHand,
    ThrownCrazyHand,
    BarrelCannonWait
}
public enum FtPart {
    TopN,
    TransN,
    XRotN,
    YRotN,
    HipN,
    WaistN,
    LLegJA,
    LLegJ,
    LKneeJ,
    LFootJA,
    LFootJ,
    RLegJA,
    RLegJ,
    RKneeJ,
    RFootJA,
    RFootJ,
    BustN,
    LShoulderN,
    LShoulderJA,
    LShoulderJ,
    LArmJ,
    LHandN,
    L1stNa,
    L1stNb,
    L2ndNa,
    L2ndNb,
    L3rdNa,
    L3rdNb,
    L4thNa,
    L4thNb,
    LThumbNa,
    LThumbNb,
    LHandNb,
    NeckN,
    HeadN,
    RShoulderN,
    RShoulderJA,
    RShoulderJ,
    RArmJ,
    RHandN,
    R1stNa,
    R1stNb,
    R2ndNa,
    R2ndNb,
    R3rdNa,
    R3rdNb,
    R4thNa,
    R4thNb,
    RThumbNa,
    RThumbNb,
    RHandNb,
    ThrowN,
    TransN2,
    // there is a 53 somewhere????
    Max = 109,
}
public enum CommonBone : uint {
    TopN,
    TransN,
    XRotN,
    YRotN,
    HipN,
    WaistN,
    NeckN = 0x22,
    HeadN = 0x23,
    ThrowN = 0x35,
    Extra = 0x36
}
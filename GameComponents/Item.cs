using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool.GameComponents;

// this struct is also huge, so fetching roughly 4k bytes every time is huge?
// this struct is 20 bytes off from the original (orig = 4068, this = 4048)
// reading this every farme might be egregious
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct ItemData {
    public Ptr32 x0;

    /// @at{4} @sz{4}
    public Struct_t gobj;

    public s32 x8;

    /// @at{C} @sz{4}
    public enum_t spawn_kind;

    /// @at{10} @sz{4}
    public ItemKind kind;

    /// @at{14} @sz{4}
    public ItHoldKind hold_kind;

    public s32 x18;
    public s32 x1C;

    /// @at{20} @sz1
    public u8 x20_team_id;

    public u8 x21;
    public u8 x22;
    public u8 x23;

    /// @at{24} @sz{4}
    public enum_t msid;

    /// @at{28} @sz{4}
    public enum_t anim_id;

    /// @at{2C} @sz{4}
    public f32 facing_dir;

    /// @at{30} @sz{4}
    public f32 init_facing_dir;

    /// @at{34} @sz{4}
    public f32 spin_spd;

    /// @at{38} @sz{4}
    public f32 scl;

    /// @at{3C} @sz{4}
    public f32 x3C;

    /// @at{40} @sz{C}
    public Vec3 x40_vel;

    /// @at{4C} @sz{C}
    public Vec3 pos;

    /// @at{58} @sz{C}
    public Vec3 x58_vec_unk;

    /// @at{64} @sz{C}
    public Vec3 x64_vec_unk2;

    /// @at{70} @sz{C}
    public Vec3 x70_nudge;

    public Vec3 x7C;
    public Vec3 x88;                           // From it_80277040
    public Vec3 x94;                           // From it_80277040
    public Vec3 xA0;                           // From it_802734B4
    public Vec3 xAC_unk;                       // From it_80276CEC
    //ItemLogicTable* xB8_itemLogicTable; // Global item callbacks
    public Func_t item_logic_table;
    //ItemStateTable* xBC_itemStateContainer;
    public Func_t item_state_container;
    //GroundOrAir ground_or_air;
    public u32 ground_or_air;
    //Article* xC4_article_data;
    public Ptr32 xC4_article;
    //HSD_Joint* xC8_joint;
    public Ptr32 xC8_joint;
    //ItemAttr* xCC_item_attr;
    public Ptr32 xCC_item_attr; // has repeated things from lower down in this struct 
    //ItemStateDesc* xD0_itemStateDesc;
    public Ptr32 xD0_itemStateDesc;
    //Item_DynamicBones xD4_dynamicBones[24];
    public fixed byte xD4_dynamicBones[672]; // describes the 24-length Item_DynamicBones above
    public s32 x374_dynamicBonesNum;
    /*  ip+378 */ 
    public CollData x378_itemColl;
    public s32 ecb_lock;

    /// @at{518} @sz{4}
    /// @brief Item's current owner
    public Struct_t owner_gobj;

    public Struct_t x51C_gobj;            // Related to the owner gobj
    // CmSubject* x520_cameraBox; // CmSubject
    public Struct_t x520_cameraBox;
    // CommandInfo x524_cmd;       // should this be CommandInfo* instead?
    public fixed byte cmd_info[36];
                               // f32 x528;
                               // void* x52C_item_script; // Script parse?
                               // u32 x530;
                               // u32 x534;
                               // u32 x538;
                               // u32 x53C;
                               // u32 x540;
                               // u32 x544;
    // ColorOverlay x548_colorOverlay;
    public fixed byte x548_colorOverlay[128];
    public u8 x5C8;
    public u8 x5C9; // used heavily in it_80278108
    public u8 x5CA;
    public u8 x5CB;
    public f32 x5CC_currentAnimFrame;
    public f32 x5D0_animFrameSpeed;

    public struct HitboxDesc {
        public HitCapsule hit;
        public s32 x138;
    }

    [InlineArray(4)]
    public struct HitboxDesc4 { HitboxDesc _instance; public const int LENGTH = 4; }
    //struct {
    //    HitCapsule hit; // x5D4, x710, x84C, x988
    //    s32 x138;       // x70C, x848, x984, xAC0
    //} x5D4_hitboxes[4];
    public HitboxDesc4 x5D4_hitboxes;

    public u32 xAC4_ignoreItemID;           // Cannot hit items with this index?
    public u8 xAC8_hurtboxNum;              // Number of hurtboxes this item has

    // when Pack = 1
    // public fixed byte xAC8_pad[3];

    [InlineArray(2)]
    public struct HurtCapsuleBuffer2 { HurtCapsule _instance; public const int LENGTH = 2; }

    public HurtCapsuleBuffer2 xACC_itemHurtbox; // xACC, xB10
    // struct lb_80014638_arg1_t xB54;
    public fixed byte lb_80014638_arg1[20];
    public u8 xB68; // int for ItemDynamics->count?
    public u8 xB69;
    public u8 xB6A;
    public u8 xB6B;

    struct DynamicsData_xB6C_t {
        public Vec3 xB6C;
        // u32 xB6C; // struct DynamicsData* for DynamicsDesc->data?
        // u32 xB70; // int for DynamicsDesc->count?
        // u32 xB74; // pos.x?
        public f32 xB78;       // pos.y? scale?
        public Struct_t xB7C_jobj; // HSD_JObj* for bone?
        public u32 xB80;
        public Vec3 xB84;
        // u32 xB88;
        // u32 xB8C;
        public u32 xB90; // enum_t for BoneDynamicsDesc->bone_id?
    }
    [InlineArray(2)]
    public struct DynamicsDataBuffer2 { DynamicsData_xB6C_t _instance; }

    public DynamicsDataBuffer2 xB6C;
    // u32 xB94;
    // u32 xB98;
    // u32 xB9C;
    // u32 xBA0;
    // u32 xBA4;
    // u32 xBA8;
    // u32 xBAC;
    // u32 xBB0;
    // u32 xBB4;
    // u32 xBB8;
    // DynamicBoneTable* xBBC_dynamicBoneTable;
    public Struct_t xBBC_dynamicBoneTable;
    public UNK_T xBC0;
    public u8 xBC4;
    public u8 xBC5;
    public u8 xBC6;
    public u8 xBC7;
    public u32 xBC8;
    public Vec2 xBCC_unk;
    public Vec2 xBD4_grabRange;

    public LocalECB ecb;
    public LocalECB ecb_prev;
    public LocalECB xBFC; // the next 3 ecbs appear to be the same, idk why
    public LocalECB xC0C;
    public LocalECB xC1C;

    public s32 xC2C;
    public s32 xC30;

    public s32 xC34_damageDealt; // Rounded down
    public s32 xC38;             // 0xc38 - ItemKind?
    public f32 xC3C;             // 0xc3c
    public f32 xC40;             // 0xc40
    public f32 xC44;             // 0xc44
    public s32 xC48;             // 0xc48
    public s32 xC4C;  // Something to do with staled_damage. 0x80077464 checks this against
               // reflectors' maximum staled_damage threshold
    public s32 xC50;  // 0xc50
    public f32 xC54;  // 0xc54
    public Vec3 xC58; // 0xc58
    public Struct_t xC64_reflectGObj; // GObj that reflected this item?
    public f32 xC68;                   // 0xc68
    public f32 xC6C;                   // 0xc6c
    public f32 xC70;                   // 0xc70
    public s32 xC74;
    public Vec2 xC78;                   // 0xc78
    public S32Vec2 xC80;                // 0xc80
    public s32 xC88;                    // 0xc88
    public u16 xC8C;                    // 0xc8c
    public Struct_t xC90_absorbGObj;   // 0xc90
    public s32 xC94;                    // 0xc94
    public f32 xC98;                    // 0xc98
    public s32 xC9C;                    // Total staled_damage taken?
    public s32 xCA0;                    // Last amount of staled_damage taken?
    public s32 xCA4;                    // 0xca4
    public s32 xCA8;                    // 0xca8, hitlag related
    public s32 xCAC_angle;              // 0xcac
    public s32 xCB0_source_ply;         // 0xcb0, staled_damage source ply number
    public s32 xCB4;                    // 0xcb4
    public f32 xCB8_outDamageDirection; // 0xcb8, updated @ 80078184
    public f32 xCBC_hitlagFrames;       // 0xcbc, hitlag frames remaining
    public f32 xCC0;                    // 0xcc0
    public s32 xCC4;           // 0xcc4, switch statement for this in it_8027CBFC
    public f32 xCC8_knockback; // 0xcc8
    public f32 xCCC_incDamageDirection; // Direction from which staled_damage was applied?
    public f32 xCD0;                    // 0xcd0
    public Vec3 xCD4;                   // 0xcd4
    public Vec3 xCE0;                   // 0xce0
    public Struct_t xCEC_fighterGObj;  // 0xcec
    public Struct_t xCF0_itemGObj; // 0xcf0, is a fp GObj, but is the owner of the

    public Struct_t xCF4_fighterGObjUnk;

    /// @at{CF8} @sz{4}
    /// @brief The entity that was detected by this item's inert hitbox.
    public Struct_t toucher_gobj;

    public Struct_t xCFC_gobj;

    /// @at{D00} @sz{4}
    /// @brief The entity that got grabbed by this item.
    public Struct_t grab_victim_gobj;

    /// @at{D04} @sz{4}
    /// @brief The entity that collided with this item's hitbox?
    public Struct_t atk_victim_gobj;

    public u8 xD08;
    public u8 xD09;
    public u8 xD0A;
    public u8 xD0B;

    /// @at{D0C} @sz{4}
    public enum_t xD0C;

    public f32 xD10;

    /// @at{D14} @sz{4}
    // HSD_GObjPredicate animated;
    public int animated_gobjpredicate;

    /// @at{D18} @sz{4}
    // HSD_GObjEvent physics_updated;
    public int physics_updated_gobjevent;

    /// @at{D1C} @sz{4}
    // HSD_GObjPredicate collided;
    public int collided_gobjpredicate;

    /// @at{D20} @sz{4}
    /// @todo What does this mean?
    // HSD_GObjEvent on_accessory;
    public int on_accessory_gobjevent;

    /// @at{D24} @sz{4}
    /// @brief Runs when an entity is detected by this item's inert hibox.
    // HSD_GObjPredicate touched;
    public int touched_gobjpredicate;

    /// @at{D28} @sz{4}
    /// @brief Runs after applying hitlag in staled_damage.
    /// @todo What function is @c 8026a62c?
    // HSD_GObjEvent entered_hitlag;
    public int entered_hitlag_gobjevent;

    // 0xd2c, runs after exiting hitlag in hitlag, update proc 8026a200
    // HSD_GObjEvent exited_hitlag;
    public int exited_hitlag_gobjevent;

    /// @at{D28} @sz{4}
    /// @brief Runs when the item is jumped on.
    /// @todo What function is @c 80269bac?
    // HSD_GObjPredicate jumped_on;
    public int jumped_on_gobjpredicate;

    /// @at{D34} @sz{4}
    /// @brief When grabbing a fighter, run this function on self.
    // HSD_GObjEvent grab_dealt;
    public int grab_dealt_gobjevent;

    /** @at{D38} @sz{4}
     * @brief When grabbing a fighter, run this function on them.
     *
     * @p gobj0 - The victim of the grab. \n
     * @p gobj1 - This item's entity.
     */
    // HSD_GObjInteraction grabbed_for_victim;
    public int grabbed_for_victim_gobjinteraction;

    public f32 xD3C_spinSpeed;
    public f32 xD40;
    public f32 xD44_lifeTimer;
    public f32 xD48_halfLifeTimer; // Not radioactive, just the item's original
                                   // lifetime halved
    public int xD4C; // Number of ammo remaining (Checked by shootable items in
                     // it_8026B594)
    public u32 xD50_landNum;  // Number of times this item has landed
    public u32 xD54_throwNum; // Number of times this item has been thrown
    public u32 xD58;
    public s32 xD5C;

    /// @at{D60} @sz{4}
    public enum_t destroy_type;

    /// @at{D64} @sz{4}
    public enum_t sfx_unk1;

    /// @at{D68} @sz{4}
    public enum_t sfx_unk2;

    public s32 xD6C;
    public s32 xD70;
    public s32 xD74;
    public s32 xD78;

    /// @at{D7C} @sz{4}
    /// @brief SFX that plays when this item is destroyed
    public enum_t destroy_sfx;

    public s32 xD80;
    public s32 xD84;
    public s32 xD88_attackID;
    public u16 xD8C_attack_instance;
    public s16 xD8E;
    // union Struct2070 xD90; // some bit struct/union
    public u32 xD90_union;

    public Vec2 xD94;
    public S32Vec2 xD9C;
    public u32 xDA4_word;
    public u16 xDA8_short; // this is the final correct offset, after there are misalignment issues
    //union {
    //    UnkFlagStruct xDAA_flag; // Develop mode stuff?
    //    u8 xDAA_byte;
    //};
    public u32 xDAA_flagUnion; // offset *should* be xDAA, but in actuality it's xDAC. Changing "pack" seems to do nothing

    public u32 xDAC_itcmd_var0;
    public u32 xDB0_itcmd_var1;
    public u32 xDB4_itcmd_var2;
    public u32 xDB8_itcmd_var3;
    public u32 xDBC_itcmd_var4;
    public u32 xDC0;
    public u32 xDC4;
    // flag32 xDC8_word;
    public u32 xDC8_flag32;
    /*struct {
        u8 b0 : 1;
        u8 b1 : 1;
        u8 b2 : 1;
        u8 b3 : 1;
        u8 b4567 : 4;
    } xDCC_flag;*/
    public u8 xDCC_flag;
    // CameraBoxFlags xDCD_flag;
    public U8Bitfield xDCD_camboxflag;
    //UnkFlagStruct xDCE_flag;
    public U8Bitfield xDCE_flag;
    //UnkFlagStruct xDCF_flag;
    public U8Bitfield xDCF_flag;
    //UnkFlagStruct xDD0_flag;
    public U8Bitfield xDD0_flag;
    //UnkFlagStruct xDD1_flag;
    public U8Bitfield xDD1_flag;
    //UnkFlagStruct xDD2_flag;
    public U8Bitfield xDD2_flag;
    //UnkFlagStruct xDD3_flag;
    public U8Bitfield xDD3_flag;
    /*union Item_ItemVars {
        itBombHei_ItemVars bombhei;
        itBox_ItemVars box;
        itCapsule_ItemVars capsule;
        itDosei_ItemVars dosei;
        itChicorita_ItemVars chicorita;
        itClimbersBlizzard_ItemVars climbersblizzard;
        itCoin_ItemVars coin;
        itDrMarioPill_ItemVars drmariopill;
        itEgg_ItemVars egg;
        itFFlower_ItemVars fflower;
        itFFlowerFlame_ItemVars fflowerflame;
        itFlipper_ItemVars flipper;
        itFoods_ItemVars foods;
        itFoxBlaster_ItemVars foxblaster;
        itFoxIllusion_ItemVars foxillusion;
        itFoxLaser_ItemVars foxlaser;
        itFreeze_ItemVars freeze;
        itGShell_ItemVars gshell, rshell, zgshell, zrshell;
        itHassam_ItemVars hassam;
        itHeart_ItemVars heart;
        itHeiho_ItemVars heiho;
        it_266F_ItemVars it_266F;
        it_279D_ItemVars it_279D;
        it_27B5_ItemVars it_27B5;
        it_27CE_ItemVars it_27CE;
        it_27CF_ItemVars it_27CF;
        it_2E5A_ItemVars it_2E5A;
        it_2E6A_ItemVars_1 it_2E6A_1;
        it_2F28_ItemVars it_2F28;
        itKinoko_ItemVars kinoko;
        itKirbyHammer_ItemVars kirbyhammer;
        itKlap_ItemVars klap;
        itLGun_ItemVars lgun;
        itLGunBeam_ItemVars lgunbeam;
        itLGunRay_ItemVars lgunray;
        itLinkArrow_ItemVars linkarrow;
        itLinkBomb_ItemVars linkbomb;
        itLinkBoomerang_ItemVars linkboomerang;
        itLinkBow_ItemVars linkbow;
        itLinkHookshot_ItemVars linkhookshot;
        itMBall_ItemVars mball;
        itMato_ItemVars mato;
        itMsBomb_ItemVars msbomb;
        itNokoNoko_ItemVars nokonoko;
        itOctarock_ItemVars octarock;
        itPeachTurnip_ItemVars peachturnip;
        itPikachutJoltGround_ItemVars pikachujoltground;
        itPikachutJoltAir_ItemVars pikachujoltair;
        itPKFlush_ItemVars pkflush;
        itPKFlushExplode_ItemVars pkflushexplode;
        itPKThunder_ItemVars pkthunder;
        itPokemon_ItemVars pokemon;
        itSamusBomb_ItemVars samusbomb;
        itSamusGrapple_ItemVars samusgrapple;
        itSeakNeedleThrown_ItemVars seakneedlethrown;
        itSonans_ItemVars sonans;
        itStar_ItemVars star;
        itSword_ItemVars sword;
        itTaru_ItemVars taru;
        itTincle_ItemVars tincle;
        itTomato_ItemVars tomato;
        itWhispyApple_ItemVars whispyapple;
        itWhiteBea_ItemVars whitebea;
        itZeldaDinFireExplode_ItemVars zeldadinfireexplode;
        itMasterHandBullet_ItemVars masterhandbullet;
        itMasterHandLaser_ItemVars masterhandlaser;
        itUnk4_ItemVars unk4;
        itStarRodStar_ItemVars starrodstar;
        itZeldaDinFire_ItemVars zeldadinfire;
        itTosakinto_ItemVars tosakinto;
        itMDisable_ItemVars mdisable;
        itKoopaFlame_ItemVars koopaflame;
        u8 _[0xFCC - 0xDD4];
    } xDD4_itemVar;*/
    // public fixed byte itvars[456]; // placeholder cuz jesus christ
    public fixed byte itvars[504]; // <-- this is the padding found at the end of the union :|

    public fixed byte alignment_temporary_do_not_change[20];

    public override readonly string ToString() => $"Kind={kind}, HoldKind={hold_kind}, Owner={owner_gobj}";
}

public enum ItHoldKind {
    None,      // no hand change
    OpenIn,    // open palm, facing inwards (towards fighter)
    Sword,     // closed palm, holding thin long object
    OpenDown,  // open palm, facing down
    OpenFront, // open palm, facing forward
}
// ENUMS
public enum ItemKind {
    // COMMON ITEMS

    Capsule,
    Box,
    Barrel,
    Egg,
    PartyBall,
    BarrelCannon,
    BobOmb,
    MrSaturn,
    HeartContainer,
    MaximTomato,
    Starman,
    HomeRunBat,
    BeamSword,
    Parasol,
    GreenShell,
    RedShell,
    RayGun,
    Freezie,
    Food,
    MotionSensorBomb,
    Flipper,
    SuperScope,
    StarRod,
    LipStick,
    Fan,
    FireFlower,
    SuperMushroom,
    PoisonMushroom,
    Hammer,
    WarpStar,
    ScrewAttack,
    BunnyHood,
    MetalBox,
    CloakingDevice,
    PokeBall,

    // ITEM-RELATED

    RayGunRecoil,
    StarRodStar,
    LipStickDust,
    SuperScopeBeam,
    RayGunBeam,
    HammerHead,
    Flower,
    YoshiEggEvent,

    // MONSTERS

    Goomba,
    Redead,
    Octorok,
    Ottosea,
    OctorokStone,

    // CHARACTER-RELATED

    MarioFireball,
    DrMarioPill,
    KirbyCutterBeam,
    KirbyHammer,
    Unknown1,
    Unknown2,
    FoxLaser,
    FalcoLaser,
    FoxIllusion,
    FalcoPhantasm,
    LinkBomb,
    YoungLinkBomb,
    LinkBoomerang,
    YoungLinkBoomerang,
    LinkHookshot,
    YoungLinkHookshot,
    LinkArrow,
    YoungLinkFireArrow,
    NessPkFire,
    NessPkFireFlame,
    NessPkFlash,
    NessPkThunder,
    NessPkThunderTrail1,
    NessPkThunderTrail2,
    NessPkThunderTrail3,
    NessPkThunderTrail4,
    FoxBlaster,
    FalcoBlaster,
    LinkBow,
    YoungLinkBow,
    NessPkFlashExplosion,
    SheikNeedleThrown,
    SheikNeedleHeld,
    PikachuThunder,
    PichuThunder,
    MarioCape,
    DrMarioCape,
    SheikSmoke,
    YoshiEggThrown,
    YoshiEggLay,
    YoshiStar,
    PikachuThunderJoltGround,
    PikachuThunderJoltAir,
    PichuThunderJoltGround,
    PichuThunderJoltAir,
    SamusBomb,
    SamusChargeShot,
    SamusMissile,
    SamusGrappleBeam,
    SheikChain,
    PeachBomberExplosion,
    PeachTurnip,
    BowserFlame,
    NessBat,
    NessYoyo,
    PeachParasol,
    PeachToad,
    LuigiFireball,
    IceClimberIce,
    IceClimberBlizzard,
    ZeldaDinsFire,
    ZeldaDinsFireExplosion,
    MewtwoDisable,
    PeachToadSpore,
    MewtwoShadowBall,
    IceClimberBelay,
    GameWatchInsecticide,
    GameWatchManhole,
    GameWatchFire,
    GameWatchParachute,
    GameWatchTurtle,
    GameWatchSparky,
    GameWatchJudge,
    GameWatchOilPanic,
    GameWatchChef,
    YoungLinkMilk,
    GameWatchFirefighter,
    MasterHandLaser,
    MasterHandBullet,
    CrazyHandLaser,
    CrazyHandBullet,
    CrazyHandBomb,
    KirbyMarioFire,
    KirbyDrMarioPill,
    KirbyLuigiFire,
    KirbyIceClimberIce,
    KirbyPeachToad,
    KirbyPeachToadSpore,
    KirbyFoxLaser,
    KirbyFalcoLaser,
    KirbyFoxBlaster,
    KirbyFalcoBlaster,
    KirbyLinkArrow,
    KirbyYoungLinkArrow,
    KirbyLinkBow,
    KirbyYoungLinkBow,
    KirbyMewtwoShadowBall,
    KirbyNessPkFlash,
    KirbyNessPkFlashExplosion,
    KirbyPikachuThunderJoltGround,
    KirbyPikachuThunderJoltAir,
    KirbyPichuThunderJoltGround,
    KirbyPichuThunderJoltAir,
    KirbySamusChargeShot,
    KirbySheikNeedleThrown,
    KirbySheikNeedleHeld,
    KirbyBowserFlame,
    KirbyGameWatchChef,
    KirbyGameWatchChefPan,
    KirbyYoshiEggLay,
    Unknown4,
    Coin,

    // POKEMON

    PokemonRandom,
    Goldeen,
    Chikorita,
    Snorlax,
    Blastoise,
    Weezing,
    Charizard,
    Moltres,
    Zapdos,
    Articuno,
    Wobbuffet,
    Scizor,
    Unown,
    Entei,
    Raikou,
    Suicune,
    Bellossom,
    Electrode,
    Lugia,
    HoOh,
    Ditto,
    Clefairy,
    Togepi,
    Mew,
    Celebi,
    Staryu,
    Chansey,
    Porygon2,
    Cyndaquil,
    Marill,
    Venusaur,

    // POKEMON-RELATED

    ChikoritaLeaf,
    BlastoiseHydroPump,
    WeezingGas1,
    WeezingGas2,
    CharizardFlame1,
    CharizardFlame2,
    CharizardFlame3,
    CharizardFlame4,
    UnownSwarm,
    LugiaAeroblast1,
    LugiaAeroblast2,
    LugiaAeroblast3,
    HoOhSacredFire,
    StaryuStar,
    ChanseyEgg,
    CyndaquilFlame,
    PokemonUnknown,

    // MONSTERS 2

    OldGoomba,
    Target,
    ShyGuy,
    KoopaGreen,
    KoopaRed,
    LikeLike,
    OldRedead,
    OldOctorok,
    OldOttosea,
    PolarBear,
    Klaptrap,
    GreenShellAlt,
    RedShellAlt,

    // STAGE-SPECIFIC

    Tingle,
    Invalid1,
    Invalid2,
    Invalid3,
    WhispyApple,
    WhispyHealApple,
    Invalid4,
    Invalid5,
    Invalid6,
    Tools,
    Invalid7,
    Invalid8,
    Birdo,
    ArwingLaser,
    GreatFoxLaser,
    BirdoEgg
}
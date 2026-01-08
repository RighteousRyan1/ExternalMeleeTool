
// GObj structure
struct HSD_GObj {
    /*  +0 */ u16 classifier;
    /*  +2 */ u8 p_link;
    /*  +3 */ u8 gx_link;
    /*  +4 */ u8 p_priority;
    /*  +5 */ u8 render_priority;
    /*  +6 */ u8 obj_kind;
    /*  +7 */ u8 user_data_kind;
    /*  +8 */ HSD_GObj* next;
    /*  +C */ HSD_GObj* prev;
    /* +10 */ HSD_GObj* next_gx;
    /* +14 */ HSD_GObj* prev_gx;
    /* +18 */ HSD_GObjProc* proc;
    /* +1C */ GObj_RenderFunc render_cb;
    /* +20 */ u64 gxlink_prios;
    /* +28 */ void* hsd_obj;
    /* +2C */ void* user_data;
    /* +30 */ void (*user_data_remove_func)(void* data);
    /* +34 */ void* x34_unk;
};

// FighterBlock
typedef struct _StaticPlayer {
    /// @at{0} @sz{4}
    /// @todo 0x02 In-Game (includes dead). 0x00 Otherwise.
    enum_t player_state;

    /// @at{4} @sz{4}
    /// @todo External ID.
    CharacterKind player_character;

    /// @at{8} @sz{4}
    Gm_PKind slot_type;

    /*0x0C*/ u8 transformed[2]; // 0x0001 for normal, 0x0100 for transformed
    // (Probably Zelda/Sheik only)
    /*0x0E*/ s16 unk0E;

    union {
        struct {
            /*0x10-0x1B*/ Vec3 nametag_pos; /// Horizontal, Vertical, Depth (floats)
            /*0x1C-0x27*/ Vec3 transformed_player_pos;
            /*0x28-0x33*/ Vec3 spawn_platform_final_pos;
            /*0x34-0x3f*/ Vec3 some_other_player_pos;
        } byVecName;

        Vec3 byIndex[4];
    } player_poses;

    /*0x40*/ f32 facing_direction;

    /*0x44*/ u8 costume_id; // 00 = normal, 01 = red, 02 = blue, 03 = green
    // (reflected in icon immediately)
    /*0x45*/ u8 unk45;
    /*0x46*/ s8 controller_index;
    /*0x47*/ u8 team; /// 00 = red, 01 = blue, 02 = green
    /*0x48*/ u8 player_id;
    /*0x49*/ u8 cpu_level;
    /*0x4A*/ u8 cpu_type;
    /*0x4B*/ u8 handicap;

    /*0x4C*/ s8 unk4C;
    /*0x4D*/ s8 unk4D;
    /*0x4E*/ s8 unk4E;
    /*0x4F*/ s8 unk4F;

    /*0x50*/ f32 unk50;

    /*0x54*/ f32 attack_ratio;
    /*0x58*/ f32 defense_ratio;

    /*0x5C*/ f32 model_scale;

    union {
        struct {
            /*0x60*/ s16 damage_percent;
            /*0x62*/ s16 damage_percent_alt_or_start_hp;
            /*0x64*/ s16 stamina;
            /*0x66*/ s16 unk66;
        } byName;
        s16 byIndex[4];
    } staminas;

    /*0x68 - 0x6C*/ s32 falls[2]; /// other index for nana falls

    /*0x70-0x84*/ u32 kos_by_player[6];

    /// @at{88} @sz{4}
    /// @remarks If -1 in zz_0035184, then it's set to MatchInfo->frame_count
    u32 match_frame_count;

    /*0x8C*/ u16 suicide_count;

    /*0x8E*/ s8 stocks;
    /*0x8F*/ s8 unk8F;

    /*0x90*/ int current_coins;
    /*0x94*/ s32 total_coins;

    /*0x98*/ s32 unk98;
    /*0x9C*/ s32 unk9C;

    /*0xA0-A4*/ s32
        joystick_direction_input_count[2]; // Incremented every time you move
    // the joystick a different
    // direction from neutral.

    /*0xA8*/ int nametag_slot_id;

    /*0xAC*/ struct {
        u8 b0 : 1; // rumble enabled
        u8 b1 : 1;
        u8 b2 : 1;
        u8 b3 : 1;
        u8 b4 : 1;
        u8 is_metal : 1;
        u8 b6 : 1;
        u8 b7 : 1;
    } flags;

    /*0xAD*/ struct {
        u8 b0 : 1;
        u8 b1 : 1;
        u8 b2 : 1;
        u8 b3 : 1;
        u8 b4 : 1;
        u8 b5 : 1;
        u8 b6 : 2;
    } more_flags;

    /*0xAE*/ struct {
        u8 b0 : 1;
        u8 b1 : 1;
        u8 b2 : 1;
        u8 b3 : 1;
        u8 b4 : 1;
        u8 b5 : 1;
        u8 b6 : 1;
        u8 b7 : 1;
    } flagsAE;

    /*0xAF*/ s8 unkAF;

    /*0xB0*/ HSD_GObj* player_entity[2];
    /*0xB4*/ /*void* sub_character_entity;*/ // Used for followers, such as
    // Nana

    /*0xB8*/ void (*struct_func)(s32 slot);

    /*0xBC*/ StaleMoveTable stale_moves;

    /*0xDB0*/ u8 xDB0[0xE90 - 0xDB0];

} StaticPlayer;

struct StartMeleeRules {
    u32 x0_0 : 3; // match mode? 1 = stock mode, 2 = coin mode?
    u32 x0_3 : 3;
    u32 x0_6 : 1;
    u32 x0_7 : 1; ///< timer counts up

    u32 x1_0 : 1;
    u32 x1_1 : 1;
    u32 x1_2 : 1;
    u32 x1_3 : 1;
    u32 x1_4 : 1;
    u32 x1_5 : 1;
    u32 timer_shows_hours : 1; // false=65:00.00, true=1:05:00.00

    u32 x1_7 : 1; ///< friendly fire on

    u32 x2_0 : 1;
    u32 x2_1 : 1;
    u32 x2_2 : 1;
    u32 x2_3 : 1; ///< single-button mode enabled
    u32 x2_4 : 1;
    u32 x2_5 : 1;
    u32 x2_6 : 1;
    u32 x2_7 : 1;

    u32 x3_0 : 1;
    u32 x3_1 : 1;
    u32 x3_2 : 1;
    u32 x3_3 : 1;
    u32 x3_4 : 1;
    u32 x3_5 : 1;
    u32 x3_6 : 1;
    u32 x3_7 : 1;

    u32 x4_0 : 1;
    u32 x4_1 : 1;
    u32 x4_2 : 1;
    u32 x4_3 : 1;
    u32 x4_4 : 1;
    u32 x4_5 : 1;
    u32 x4_6 : 1;
    u32 x4_7 : 1;

    u32 x5_0 : 1;
    u32 x5_1 : 1;
    u32 x5_2 : 1;
    u32 x5_3 : 1;
    u32 x5_4 : 1;
    u32 x5_5 : 1;
    u32 x5_6 : 1;
    u32 x5_7 : 1;

    u8 x6;
    u8 x7; // end graphic / SFX type
    u8 is_teams;
    u8 x9;
    u8 xA;
    s8 xB; // item frequency
    s8 xC; // SD penalty
    u8 xD;
    u16 xE; // InternalStageId

    u32 x10; // time limit
    u8 x14;
    u32 x18;
    u32 x1C_pad[(0x20 - 0x1C) / 4];

    u64 x20; // item mask
    int x28;
    float x2C;
    float x30; // damage ratio
    float x34; // game speed
    void (*x38)(int);  // on unpause callback
    void (*x3C)(int);  // on pause callback (conditional?)
    int (*x40)(void);  // on pause callback
    void (*x44)(void); // on VS match start callback
    void (*x48)(void); // ingame pre-frame callback
    void (*x4C)(void); // ingame post-frame callback
    void (*x50)(int);  // on VS match end callback
    struct {
        u8 pad_x0[0x10];
        u8 x10_b0 : 1;
        u8 x10_b1 : 1;
    }*x54;
    int x58;
    u8 pad_x5C[0x60 - 0x5C];
};

// holy massive
struct Fighter {
    /*    fp+0 */ HSD_GObj* gobj;
    /*    fp+4 */ FighterKind kind;
    /*    fp+8 */ s32 x8_spawnNum;
    /*    fp+C */ u8 player_id;
    /*   fp+10 */ FtMotionId motion_id;
    /*   fp+14 */ enum_t anim_id;
    /*   fp+18 */ s32 x18;
    /*   fp+1C */ MotionState* x1C_actionStateList;
    /*   fp+20 */ MotionState* x20_actionStateList;
    /*   fp+24 */ struct S_TEMP4* x24;
    /*   fp+28 */ u8* x28;
    /*   fp+2C */ float facing_dir;
    /*   fp+30 */ float facing_dir1;
    /*   fp+34 */ Vec3 x34_scale;
    /*   fp+40 */ float x40;
    /*   fp+44 */ Mtx x44_mtx;
    /*   fp+74 */ Vec3 x74_anim_vel;
    /*   fp+80 */ Vec3 self_vel;
    /*   fp+8C */ Vec3 x8c_kb_vel;
    /*   fp+98 */ Vec3 x98_atk_shield_kb;
    /*   fp+A4 */ Vec3 xA4_unk_vel;
    /*   fp+B0 */ Vec3 cur_pos;
    /*   fp+BC */ Vec3 prev_pos;
    /*   fp+C8 */ Vec3 pos_delta;
    /*   fp+D4 */ Vec3 xD4_unk_vel;
    /*   fp+E0 */ GroundOrAir ground_or_air;
    /*   fp+E4 */ float xE4_ground_accel_1;
    /*   fp+E8 */ float xE8_ground_accel_2;
    /*   fp+EC */ float gr_vel;
    /*   fp+F0 */ float xF0_ground_kb_vel;
    /*   fp+F4 */ float xF4_ground_attacker_shield_kb_vel;
    /*   fp+F8 */ Vec2 xF8_playerNudgeVel;
    /*  fp+100 */ float x100;
    /*  fp+104 */ u8 x104;
    /*  fp+105 */ s8 x105;
    /*  fp+106 */ s8 x106;
    /*  fp+107 */ s8 x107;
    /*  fp+108 */ HSD_Joint* x108_costume_joint;
    /*  fp+10C */ ftData* ft_data;
    /*  fp+110 */ ftCo_DatAttrs co_attrs;
    /*  fp+294 */ itPickup x294_itPickup;
    /*  fp+2C4 */ Vec2 x2C4;
    /*  fp+2CC */ ftDonkeyAttributes* x2CC;
    /*  fp+2D0 */ struct Fighter_x2D0_t {
        /// @warning i didnt confirm these comments, they come from altimors ghidra db
        int x0;         ///< turn frames
        float x4;       ///< turn threshold
        float x8;       ///< x impulse
        float xC;       ///< accel mult
        float x10;      ///< speed mult
        float x14[5];      ///< y impulse
        int x28;        ///< state count
        enum_t x2C;     ///< start state
        enum_t x30;     ///< start state helmet
    }*x2D0; ///< multi jump stats
    /*  fp+2D4 */ void* dat_attrs;
    /*  fp+2D8 */ void* dat_attrs_backup;
    /*  fp+2DC */ float x2DC;
    /*  fp+2E0 */ float x2E0;
    /*  fp+2E4 */ float x2E4;
    /*  fp+2E8 */ float x2E8;
    /*  fp+2EC */ float x2EC;
    /*  fp+2F0 */ BoneDynamicsDesc dynamic_bone_sets[Ft_Dynamics_NumMax];
    /*  fp+3E0 */ int dynamics_num;
    /*  fp+3E4 */ CommandInfo x3E4_fighterCmdScript;
    /*  fp+408 */ ColorOverlay x408;
    /*  fp+488 */ ColorOverlay x488;
    /*  fp+508 */ ColorOverlay x508;
    /*  fp+588 */ HSD_LObj* x588;
    /*  fp+58C */ u32 x58C;
    /*  fp+590 */ FigaTree* x590;
    /*  fp+594 */ union {
        struct {
            /* fp+594:0 */ u8 x594_b0 : 1;
            /* fp+594:1 */ u8 x594_b1 : 1;
            /* fp+594:2 */ u8 x594_b2 : 1;
            /* fp+594:3 */ u8 x594_b3 : 1;
            /* fp+594:4 */ u8 x594_b4 : 1;
            /* fp+594:5 */ u8 x594_b5 : 1;
            /* fp+594:6 */ u8 x594_b6 : 1;
            /* fp+594:7 */ u8 x594_b7 : 1;
            /* fp+596 */ struct {
                /* fp+596:0 */ u8 x0 : 7;
                /* fp+596:7 */ u16 x7 : 3;
            } x596_bits;
        };
        struct {
            u32 x594_pad : 10;
            u32 x594_bits : 13;
            u32 x594_pad2 : 3;
            u32 x597_bits : 6; // FighterKind of this fighter's x590 FigaTree
        };
        /* fp+594 */ s32 x594_s32;
    };
    /*  fp+598 */ FigaTree* x598;
    /*  fp+59C */ struct Fighter_x59C_t* x59C;
    /*  fp+5A0 */ struct Fighter_x59C_t* x5A0;
    /*  fp+5A4 */ UNK_T x5A4;
    /*  fp+5A8 */ UNK_T x5A8;
    /*  fp+5AC */ u32 x5AC;
    /*  fp+5B0 */ u8 _5B0[0x5B8 - 0x5B0];
    /*  fp+5B8 */ s32 x5B8;
    /*  fp+5BC */ UNK_T x5BC;
    /*  fp+598 */ u8 filler_x598[0x5C8 - 0x5C0];
    /*  fp+5A0 */ void* x5C8;
    /*  fp+5CC */ u32 n_costume_tobjs;
    /*  fp+5D0 */ u16* x5D0;
    /*  fp+5D4 */ HSD_TObj* costume_tobjs[5];
    /*  fp+5E8 */ FighterBone* parts;
    /*  fp+5EC */ DObjList dobj_list;
    /*  fp+5F4 */ struct {
        s8 x0, x1;
    } x5F4_arr[2];
    /*  fp+5F8 */ s8 x5F8;
    /*  fp+5FC */ u8 filler_x5FC[0x60C - 0x5F9];
    /*  fp+60C */ void* x60C;
    /*  fp+610 */ GXColor x610_color_rgba[2];
    /*  fp+618 */ u8 x618_player_id;
    /*  fp+619 */ u8 x619_costume_id;
    /*  fp+61A */ u8 x61A_controller_index;
    /*  fp+61B */ u8 team;
    /*  fp+61C */ s8 x61C;
    /*  fp+61D */ u8 x61D;
    /*  fp+61E */ u8 filler_x61E[0x620 - 0x61E];
    /*  fp+620 */ struct {
        /*  fp+620 */ Vec2 lstick;
        /*  fp+628 */ Vec2 lstick1;
        /*  fp+630 */ float x630;
        /*  fp+634 */ float x634;
        /*  fp+638 */ Vec2 cstick;
        /*  fp+640 */ Vec2 cstick1;
        /*  fp+648 */ float x648;
        /*  fp+64C */ float x64C;
        /*  fp+650 */ float x650;
        /*  fp+654 */ float x654;
        /*  fp+658 */ float x658;
        /*  fp+65C */ HSD_Pad held_inputs;
        /*  fp+660 */ HSD_Pad x660; ///< previous held inputs
        /*  fp+664 */ HSD_Pad x664;
        /*  fp+668 */ HSD_Pad x668; ///< pressed inputs
        /*  fp+66C */ HSD_Pad x66C; ///< released inputs
    } input;
    /*  fp+670 */ u8 x670_timer_lstick_tilt_x;
    /*  fp+671 */ u8 x671_timer_lstick_tilt_y;
    /*  fp+672 */ u8 x672_input_timer_counter;
    /*  fp+673 */ u8 x673;
    /*  fp+674 */ u8 x674;
    /*  fp+674 */ u8 x675;
    /*  fp+676 */ u8 x676_x;
    /*  fp+677 */ u8 x677_y;
    /*  fp+678 */ u8 x678;
    /*  fp+679 */ u8 x679_x;
    /*  fp+67A */ u8 x67A_y;
    /*  fp+67B */ u8 x67B;
    /*  fp+67C */ u8 x67C;
    /*  fp+67D */ u8 x67D;
    /*  fp+67E */ u8 x67E;
    /*  fp+67F */ u8 x67F;
    /*  fp+680 */ u8 x680;
    /*  fp+681 */ u8 x681;
    /*  fp+682 */ u8 x682;
    /*  fp+683 */ u8 x683;
    /*  fp+684 */ u8 x684;
    /*  fp+685 */ u8 x685;
    /*  fp+686 */ u8 x686;
    /*  fp+687 */ u8 x687;
    /*  fp+688 */ u8 x688;
    /*  fp+689 */ u8 x689;
    /*  fp+68A */ u8 x68A;
    /*  fp+68B */ u8 x68B;
    /*  fp+68C */ Vec3 x68C_transNPos;
    /*  fp+698 */ Vec3 x698;
    /*  fp+6A4 */ Vec3 x6A4_transNOffset;
    /*  fp+6B0 */ Vec3 x6B0;
    /*  fp+6BC */ float lstick_angle;
    /*  fp+6C0 */ Vec3 x6C0;
    /*  fp+6CC */ Vec3 x6CC;
    /*  fp+6D8 */ Vec3 x6D8;
    /*  fp+6E4 */ Vec3 x6E4;
    /*  fp+6F0 */ CollData coll_data;
    /*  fp+88C */ s32 ecb_lock;
    /*  fp+890 */ CmSubject* x890_cameraBox;
    /*  fp+894 */ float cur_anim_frame;
    /*  fp+898 */ float x898_unk;
    /*  fp+89C */ float frame_speed_mul;
    /*  fp+8A0 */ float x8A0_unk;
    /*  fp+8A4 */ float x8A4_animBlendFrames;
    /*  fp+8A8 */ float x8A8_unk;
    /*  fp+8AC */ HSD_JObj* x8AC_animSkeleton;
    /*  fp+8B0 */ struct Fighter_x8B0_t {
        int x0;
        float x4;
        float x8;
        float xC;
        s8 x10;
        s8 x11;
    } x8B0[5];
    /*  fp+914 */ HitCapsule x914[4];
    /*  fp+DF4 */ HitCapsule xDF4[2];
    /* fp+1064 */ HitCapsule x1064_thrownHitbox;
    /* fp+119C */ u8 x119C_teamUnk;
    /* fp+119D */ u8 grabber_unk1;
    /* fp+119E */ u8 hurt_capsules_len;
    /* fp+119F */ u8 x119F;
    /* fp+11A0 */ FighterHurtCapsule hurt_capsules[15];
    /* fp+1614 */ struct Fighter_x1614_t {
        f32 x0;
        HSD_JObj* x4;
        Vec3 x8;
        Vec3 x14;
        Vec3 x20;
    } x1614[2];
    /* fp+166C */ u8 x166C; ///< number of valid entries in x1670 array
    /* fp+1670 */ struct Fighter_x1670_t {
        /* +00 */ Vec3 v1;
        /* +0C */ float v2;
        /* +10 */ HSD_JObj* jobj;
        /* +14 */ float x14;
        /* +18 */ Vec3 x18;
        /* +24 */ u8 pad[0x28 - 0x24];
    } x1670[1]; ///< @todo figure out proper size
    /* fp+1674 */ u8 filler_x1674[0x1828 - 0x1670 - 0x28];
    /* fp+1828 */ enum_t x1828;
    /* fp+182C */ struct dmg {
        /* fp+182C */ float x182c_behavior;
        /* fp+1830 */ float x1830_percent;
        /* fp+1834 */ float x1834;
        /* fp+1838 */ float x1838_percentTemp;
        /* fp+183C */ int x183C_applied;
        /* fp+1840 */ int x1840;
        /* fp+1844 */ float facing_dir_1;
        /* fp+1848 */ int x1848_kb_angle;
        /* fp+184C */ int x184c_damaged_hurtbox;
        /* fp+1850 */ float kb_applied;
        /* fp+1854 */ Vec3 x1854_collpos;
        /* fp+1860 */ u32 x1860_element;
        /* fp+1864 */ int x1864;
        /* fp+1868 */ HSD_GObj* x1868_source;
        /* fp+186C */ int x186c;
        /* fp+1870 */ struct DmgLogEntry* x1870;
        /* fp+1874 */ int x1874;
        /* fp+1878 */ int x1878;
        /* fp+187C */ float x187c;
        /* fp+1880 */ int x1880;
        /* fp+1884 */ int x1884;
        /* fp+1888 */ int x1888;
        /* fp+188C */ int x188c;
        /* fp+1890 */ int x1890;
        /* fp+1894 */ int x1894;
        /* fp+1898 */ int x1898;
        /* fp+189C */ float x189C_unk_num_frames;
        /* fp+18A0 */ float x18a0;
        /// kb magnitude
        /* fp+18A4 */ float x18A4_knockbackMagnitude;
        /* fp+18A8 */ float x18A8;
        /// in frames
        /* fp+18AC */ int x18ac_time_since_hit;
        /* fp+18B0 */ float armor0;
        /* fp+18B4 */ float armor1;
        /* fp+18B8 */ float x18B8;
        /* fp+18BC */ float x18BC;
        /* fp+18C0 */ int x18C0;
        /// damage source ply number
        /* fp+18C4 */ int x18c4_source_ply;
        /* fp+18C8 */ int x18C8;
        /* fp+18CC */ int x18CC;
        /* fp+18D0 */ int x18D0;
        /* fp+18D4 */ UnkPlBonusBits x18d4;
        /* fp+18D8 */ ft_800898B4_t x18d8;
        /// Last Move Instance This Player Was Hit by
        /* fp+18EC */ u16 x18ec_instancehitby;
        /* fp+18F0 */ int x18F0;
        /* fp+18F4 */ int x18F4;
        /* fp+18F8 */ u8 x18F8;
        /* fp+18F9 */ u8 x18f9;
        /* fp+18FA */ u16 x18fa_model_shift_frames;
        /* fp+18FC */ u8 x18FC;
        /* fp+18FD */ u8 x18FD;
        /* fp+1900 */ float x1900;
        /* fp+1904 */ float x1904;
        /* fp+1908 */ enum_t x1908;
        /* fp+190C */ UNK_T x190C;
        /* fp+1910 */ int x1910;
        /* fp+1914 */ int x1914;
        /* fp+1918 */ int int_value;
        /* fp+191C */ float x191C;
        /* fp+1920 */ float facing_dir;
        /* fp+1924 */ int x1924;
        /* fp+1928 */ float x1928;
        /* fp+192C */ float x192c;
        /* fp+1930 */ struct lb_80014638_arg0_t x1930;
        /* fp+1948 */ int x1948;
        /* fp+194C */ int x194C;
        /* fp+1950 */ bool x1950;
        /* fp+1954 */ float x1954;
        /* fp+1958 */ float x1958;
        /* fp+195C */ float x195c_hitlag_frames;
    } dmg;
    /* fp+1960 */ float x1960_vibrateMult;
    /* fp+1964 */ float x1964;
    /* fp+1968 */ u8 x1968_jumpsUsed;
    /* fp+1969 */ u8 x1969_walljumpUsed;
    /* fp+196C */ float hitlag_mul;
    /* fp+1970 */ enum_t unk_msid;
    /* fp+1974 */ Item_GObj* item_gobj;
    /* fp+1978 */ Item_GObj* x1978; // held item
    /* fp+197C */ HSD_GObj* x197C; ///< bunny hood
    /* fp+1980 */ HSD_GObj* x1980;
    /* fp+1984 */ Item_GObj* x1984_heldItemSpec;
    /* fp+1988 */ enum_t x1988;
    /* fp+198C */ s32 x198C;
    /* fp+1990 */ s32 x1990;
    /* fp+1994 */ bool x1994;
    /* fp+1998 */ float shield_health;
    /* fp+199C */ float lightshield_amount;
    /* fp+19A0 */ s32 x19A0_shieldDamageTaken;
    /* fp+19A4 */ int x19A4;
    /* fp+19A8 */ HSD_GObj* x19A8;
    /* fp+19AC */ float specialn_facing_dir;
    /* fp+19B0 */ enum_t x19B0;
    /* fp+19B4 */ float shield_unk0;
    /* fp+19B8 */ float shield_unk1;
    /* fp+19BC */ s32 x19BC_shieldDamageTaken3;
    /* fp+19C0 */ HitResult shield_hit;
    /* fp+19E4 */ HitResult reflect_hit;
    /* fp+1A08 */ HitResult absorb_hit;
    /* fp+1A2C */ struct {
        /* fp+1A2C */ float x1A2C_reflectHitDirection;
        /* fp+1A30 */ s32 x1A30_maxDamage;
        /* fp+1A34 */ float x1A34_damageMul;
        /* fp+1A38 */ float x1A38_speedMul;
        /// % damage over the maximum reflectable damage threshold
        /* fp+1A3C */ s32 x1A3C_damageOver;
    } ReflectAttr;
    /* fp+1A40 */ struct {
        /* fp+1A40 */ float x1A40_absorbHitDirection;
        /// unconfirmed?
        /* fp+1A44 */ s32 x1A44_damageTaken;
        /// unconfirmed?
        /* fp+1A48 */ s32 x1A48_hitsTaken;
    } AbsorbAttr;
    /* fp+1A4C */ float grab_timer;
    /* fp+1A50 */ s8 x1A50;
    /* fp+1A51 */ s8 x1A51;
    /* fp+1A52 */ u8 x1A52;
    /* fp+1A53 */ u8 x1A53;
    /* fp+1A54 */ s32 x1A54;
    /* fp+1A58 */ Fighter_GObj* victim_gobj;
    /* fp+1A5C */ Fighter_GObj* x1A5C;
    /* fp+1A60 */ Item_GObj* target_item_gobj;
    /* fp+1A64 */ UNK_T x1A64;
    /* fp+1A68 */ u16 x1A68;
    /* fp+1A6A */ u16 x1A6A;
    /* fp+1A6C */ float x1A6C;
    /* fp+1A70 */ Vec3 x1A70;
    /* fp+1A7C */ Vec3 x1A7C;
    /* fp+x1A88 */ struct Fighter_x1A88_t x1A88;
    /* fp+2004 */ int x2004;
    /* fp+2008 */ s32 x2008;
    /* fp+200C */ s32 x200C;
    /* fp+2010 */ s32 x2010;
    /* fp+2014 */ s32 x2014;
    /* fp+2018 */ s32 x2018;
    /* fp+201C */ s32 x201C;
    /* fp+2020 */ s8 x2020;
    /* fp+2021 */ s8 x2021;
    /* fp+2022 */ s8 x2022;
    /* fp+2024 */ s32 x2024;
    /* fp+2028 */ int metal_timer;
    /* fp+202C */ int metal_health;
    /* fp+2030 */ s32 x2030;
    /* fp+2034 */ s32 x2034;
    /* fp+2038 */ s32 x2038;
    /* fp+203C */ u32 x203C;
    /* fp+2040 */ HSD_DObj** x2040;
    /* fp+203C */ u8 filler_x203C[0x2064 - 0x2044];
    /* fp+2064 */ int x2064_ledgeCooldown;
    /* fp+2068 */ s32 x2068_attackID;
    /* fp+206C */ u16 x206C_attack_instance;
    /* fp+206E */ short x206E;
    /* fp+2070 */ union Struct2070 x2070;
    /* fp+2074 */ struct Struct2074 x2074;
    /* fp+208C */ s32 x208C;
    /* fp+2090 */ u16 x2090;
    /* fp+2092 */ u16 x2092;
    /// GObj pointer of combo victim?
    /* fp+2094 */ Fighter_GObj* x2094;
    /* fp+2098 */ u16 x2098;
    /* fp+209A */ u16 x209A;
    /* fp+209C */ u16 x209C;
    /* fp+20A0 */ HSD_JObj* x20A0_accessory;
    /* fp+20A4 */ LbShadow x20A4;
    /* fp+20AC */ HSD_GObj* unk_gobj;
    /* fp+20B0 */ struct Fighter_x20B0_t {
        Vec3 x0;
        Vec3 xC;
    } x20B0[3];
    /* fp+20F8 */ float x20F8;
    /* fp+20FC */ float x20FC;
    /* fp+2100 */ s8 x2100;
    /* fp+2101:0 */ u8 x2101_bits_0to6 : 7;
    /* fp+2101:7 */ u8 x2101_bits_8 : 1;
    /* fp+2102 */ s8 x2102;
    /* fp+2103 */ s8 x2103;
    /* fp+2104 */ int x2104;
    /* fp+2108 */ int capture_timer;
    /* fp+210C */ u8 wall_jump_input_timer;
    /* fp+210C */ u8 filler_x210C[3];
    /* fp+2110 */ float x2110_walljumpWallSide;
    /* fp+2114 */ SmashAttr smash_attrs;
    /* fp+213C */ s32 x213C;
    /* fp+2140 */ float x2140;
    /* fp+2144 */ int x2144;
    /* fp+2148 */ s32 x2148;
    /* fp+214C */ s32 x214C;
    /* fp+2150 */ s32 x2150;
    /* fp+2154 */ s32 x2154;
    /* fp+2158 */ s32 x2158;
    /* fp+215C */ s32 x215C;
    /* fp+2160 */ s32 x2160;
    /* fp+2164 */ int x2164;
    /* fp+2168 */ int x2168;
    /* fp+216C */ float unk_grab_val;
    /* fp+2170 */ float x2170;
    /* fp+2174 */ Vec x2174;
    /* fp+2180 */ s32 x2180;
    /* fp+2184 */ HSD_JObj* x2184;
    /* fp+2188 */ S32Vec2 x2188;
    /// callback struct. Not all of them used by fighter.c.
    /* fp+2190 */ HSD_GObjEvent grab_cb;
    /* fp+2194 */ HSD_GObjEvent x2194;
    /* fp+2198 */ HSD_GObjInteraction grabbed_cb;
    /* fp+219C */ HSD_GObjEvent input_cb;
    /* fp+21A0 */ HSD_GObjEvent anim_cb;
    /* fp+21A4 */ HSD_GObjEvent phys_cb;
    /* fp+21A8 */ HSD_GObjEvent coll_cb;
    /* fp+21AC */ HSD_GObjEvent cam_cb;
    /* fp+21B0 */ HSD_GObjEvent accessory1_cb;
    /* fp+21B4 */ HSD_GObjEvent accessory2_cb;
    /* fp+21B8 */ HSD_GObjEvent accessory3_cb;
    /* fp+21BC */ HSD_GObjEvent accessory4_cb;
    /* fp+21C0 */ HSD_GObjEvent deal_dmg_cb;
    /* fp+21C4 */ HSD_GObjEvent shield_hit_cb;
    /* fp+21C8 */ HSD_GObjEvent reflect_hit_cb;
    /* fp+21CC */ HSD_GObjEvent x21CC;
    /* fp+21D0 */ HSD_GObjEvent hitlag_cb;
    /* fp+21D4 */ HSD_GObjEvent pre_hitlag_cb;
    /* fp+21D8 */ HSD_GObjEvent post_hitlag_cb;
    /* fp+21DC */ HSD_GObjEvent take_dmg_cb;
    /* fp+21E0 */ HSD_GObjEvent death1_cb;
    /// @remarks Used. Internally Dead_Proc as evidenced by 800F5430.
    /* fp+21E4 */ HSD_GObjEvent death2_cb;
    /* fp+21E8 */ HSD_GObjEvent death3_cb;
    /* fp+21EC */ HSD_GObjEvent x21EC;
    /* fp+21F0 */ HSD_GObjEvent take_dmg_2_cb;
    /* fp+21F4 */ HSD_GObjEvent hurtbox_detect_cb;
    /* fp+21F8 */ HSD_GObjEvent x21F8;
    /* fp+21FC */ UnkFlagStruct x21FC_flag;
    /* fp+21FC */ u8 filler_x21FC[0x2200 - 0x21FD];
    /* fp+2200 */ u32 cmd_vars[4];
    /* fp+2210 */ union {
        u32 throw_flags;
        struct {
            u8 throw_flags_b0 : 1;
            u8 throw_flags_b1 : 1;
            u8 throw_flags_b2 : 1;
            u8 throw_flags_b3 : 1;
            u8 throw_flags_b4 : 1;
            u8 throw_flags_b5 : 1;
            u8 throw_flags_b6 : 1;
            u8 throw_flags_b7 : 1;
        };
    };
    /* fp+2214 */ float cmd_timer;
    /* fp+2218:0 */ u8 allow_interrupt : 1;
    /* fp+2218:1 */ u8 x2218_b1 : 1;
    /* fp+2218:2 */ u8 x2218_b2 : 1;
    /* fp+2218:3 */ u8 reflecting : 1;
    /* fp+2218:4 */ u8 x2218_b4 : 1;
    /* fp+2218:5 */ u8 x2218_b5 : 1;
    /* fp+2218:6 */ u8 x2218_b6 : 1;
    /* fp+2218:7 */ u8 x2218_b7 : 1;

    /* fp+2219:0 */ u8 x2219_b0 : 1;
    /* fp+2219:1 */ u8 x2219_b1 : 1;
    /* fp+2219:2 */ u8 x2219_b2 : 1;
    /* fp+2219:3 */ u8 x2219_b3 : 1;
    /* fp+2219:4 */ u8 x2219_b4 : 1;
    /* fp+2219:5 */ u8 x2219_b5 : 1;
    /* fp+2219:6 */ u8 x2219_b6 : 1;
    /* fp+2219:7 */ u8 x2219_b7 : 1;

    /* fp+221A:0 */ u8 x221A_b0 : 1;
    /* fp+221A:1 */ u8 x221A_b1 : 1;
    /* fp+221A:2 */ u8 x221A_b2 : 1;
    /* fp+221A:3 */ u8 x221A_b3 : 1;
    /* fp+221A:4 */ u8 fall_fast : 1;
    /* fp+221A:5 */ u8 x221A_b5 : 1;
    /* fp+221A:6 */ u8 x221A_b6 : 1;
    /* fp+221A:7 */ u8 x221A_b7 : 1;

    /* fp+221B */ struct {
        /* fp+221B:0 */ u8 x221B_b0 : 1;
        /* fp+221B:1 */ u8 x221B_b1 : 1;
        /* fp+221B:2 */ u8 x221B_b2 : 1;
        /* fp+221B:3 */ u8 x221B_b3 : 1;
        /* fp+221B:4 */ u8 x221B_b4 : 1;
        /* fp+221B:5 */ u8 x221B_b5 : 1;
        /* fp+221B:6 */ u8 x221B_b6 : 1;
        /* fp+221B:7 */ u8 x221B_b7 : 1;
    };

    /* fp+221C:0 */ u16 x221C_b0 : 1;
    /* fp+221C:1 */ u16 x221C_b1 : 1;
    /* fp+221C:2 */ u16 x221C_b2 : 1;
    /* fp+221C:3 */ u16 x221C_b3 : 1;
    /* fp+221C:4 */ u16 x221C_b4 : 1;
    /* fp+221C:5 */ u16 x221C_b5 : 1;
    /* fp+221C:6 */ u16 x221C_b6 : 1;
    /* fp+221C:7 */ u16 x221C_u16_y : 3;
    /* fp+221D:2 */ u16 x221D_b2 : 1;
    /* fp+221D:3 */ u16 x221D_b3 : 1;
    /* fp+221D:4 */ u16 x221D_b4 : 1;
    /* fp+221D:5 */ u16 x221D_b5 : 1;
    /* fp+221D:6 */ u16 x221D_b6 : 1;
    /* fp+221D:7 */ u16 x221D_b7 : 1;

    /* fp+221E:0 */ u8 invisible : 1;
    /* fp+221E:1 */ u8 x221E_b1 : 1;
    /* fp+221E:2 */ u8 x221E_b2 : 1;
    /* fp+221E:3 */ u8 x221E_b3 : 1;
    /* fp+221E:4 */ u8 x221E_b4 : 1;
    /* fp+221E:5 */ u8 x221E_b5 : 1;
    /* fp+221E:6 */ u8 x221E_b6 : 1;
    /* fp+221E:7 */ u8 x221E_b7 : 1;

    /* fp+221F:0 */ u8 x221F_b0 : 1;
    /* fp+221F:1 */ u8 x221F_b1 : 1;
    /* fp+221F:2 */ u8 x221F_b2 : 1;
    /* fp+221F:3 */ u8 x221F_b3 : 1;
    /* fp+221F:4 */ u8 x221F_b4 : 1;
    /* fp+221F:5 */ u8 x221F_b5 : 1;
    /* fp+221F:6 */ u8 x221F_b6 : 1;
    /* fp+221F:7 */ u8 x221F_b7 : 1;

    /* fp+2220:0 */ u8 x2220_b0 : 3;
    /* fp+2220:3 */ u8 x2220_b3 : 1;
    /* fp+2220:4 */ u8 x2220_b4 : 1;
    /* fp+2220:5 */ u8 x2220_b5 : 1;
    /* fp+2220:6 */ u8 x2220_b6 : 1;
    /* fp+2220:7 */ u8 x2220_b7 : 1;

    /* fp+2221:0 */ u8 x2221_b0 : 1;
    /* fp+2221:1 */ u8 x2221_b1 : 1;
    /* fp+2221:2 */ u8 x2221_b2 : 1;
    /* fp+2221:3 */ u8 x2221_b3 : 1;
    /* fp+2221:4 */ u8 x2221_b4 : 1; ///< parasol-related
    /* fp+2221:5 */ u8 x2221_b5 : 1; ///< parasol-related
    /* fp+2221:6 */ u8 x2221_b6 : 1; ///< parasol-related
    /* fp+2221:7 */ u8 x2221_b7 : 1;

    /* fp+2222:0 */ u8 x2222_b0 : 1;      ///< can cargo grab? only set for DK
    /* fp+2222:1 */ u8 can_multijump : 1; ///< set for Kirby and Jigglypuff
    /* fp+2222:2 */ u8 x2222_b2 : 1;
    /* fp+2222:3 */ u8 x2222_b3 : 1;
    /* fp+2222:4 */ u8 x2222_b4 : 1;
    /* fp+2222:5 */ u8 x2222_b5 : 1;
    /* fp+2222:6 */ u8 x2222_b6 : 1;
    /* fp+2222:7 */ u8 x2222_b7 : 1;

    /* fp+2223:0 */ u8 x2223_b0 : 1;
    /* fp+2223:1 */ u8 x2223_b1 : 1;
    /* fp+2223:2 */ u8 x2223_b2 : 1;
    /* fp+2223:3 */ u8 x2223_b3 : 1;
    /* fp+2223:4 */ u8 x2223_b4 : 1;
    /* fp+2223:5 */ u8 x2223_b5 : 1;
    /* fp+2223:6 */ u8 is_always_metal : 1; ///< e.g. classic metal mario
    /* fp+2223:7 */ u8 is_metal : 1;

    /* fp+2224:0 */ u8 x2224_b0 : 1;
    /* fp+2224:1 */ u8 x2224_b1 : 1;
    /* fp+2224:2 */ u8 x2224_b2 : 1;
    /* fp+2224:3 */ u8 x2224_b3 : 1;
    /* fp+2224:4 */ u8 x2224_b4 : 1;
    /* fp+2224:5 */ u8 x2224_b5 : 1;
    /* fp+2224:6 */ u8 x2224_b6 : 1;
    /* fp+2224:7 */ u8 can_walljump : 1;

    /* fp+2225:0 */ u8 x2225_b0 : 1;
    /* fp+2225:1 */ u8 x2225_b1 : 1;
    /* fp+2225:2 */ u8 x2225_b2 : 1;
    /* fp+2225:3 */ u8 x2225_b3 : 1;
    /* fp+2225:4 */ u8 x2225_b4 : 1;
    /* fp+2225:5 */ u8 x2225_b5 : 1;
    /* fp+2225:6 */ u8 x2225_b6 : 1;
    /* fp+2225:7 */ u8 x2225_b7 : 1;

    /* fp+2226:0 */ u8 x2226_b0 : 1;
    /* fp+2226:1 */ u8 x2226_b1 : 1;
    /* fp+2226:2 */ u8 x2226_b2 : 1;
    /* fp+2226:3 */ u8 x2226_b3 : 1;
    /* fp+2226:4 */ u8 x2226_b4 : 1;
    /* fp+2226:5 */ u8 x2226_b5 : 1;
    /* fp+2226:6 */ u8 x2226_b6 : 1;
    /* fp+2226:7 */ u8 x2226_b7 : 1;

    /* fp+2227:0 */ u8 x2227_b0 : 1;
    /* fp+2227:1 */ u8 x2227_b1 : 1;
    /* fp+2227:2 */ u8 x2227_b2 : 1;
    /* fp+2227:3 */ u8 x2227_b3 : 1;
    /* fp+2227:4 */ u8 x2227_b4 : 1;
    /* fp+2227:5 */ u8 x2227_b5 : 1;
    /* fp+2227:6 */ u8 x2227_b6 : 1;
    /* fp+2227:7 */ u8 x2227_b7 : 1;

    /* fp+2228:0 */ u8 x2228_b0 : 1;
    /* fp+2228:1 */ u8 x2228_b1 : 1;
    /* fp+2228:2 */ u8 x2228_b2 : 1;
    /* fp+2228:3 */ u8 x2228_b3 : 2;
    /* fp+2228:5 */ u8 x2228_b5 : 1;
    /* fp+2228:6 */ u8 used_tether : 1;
    /* fp+2228:7 */ u8 x2228_b7 : 1;

    /* fp+2229:0 */ u8 x2229_b0 : 1;
    /* fp+2229:1 */ u8 x2229_b1 : 1;
    /* fp+2229:2 */ u8 x2229_b2 : 1;
    /* fp+2229:3 */ u8 x2229_b3 : 1;
    /* fp+2229:4 */ u8 x2229_b4 : 1;
    /* fp+2229:5 */ u8 no_normal_motion : 1;
    /* fp+2229:6 */ u8 x2229_b6 : 1;
    /* fp+2229:7 */ u8 no_kb : 1;

    /* fp+222A:0 */ u8 x222A_b0 : 1;
    /* fp+222A:1 */ u8 x222A_b1 : 1;
    /* fp+222A:2 */ u8 x222A_b2 : 1;
    /* fp+222A:3 */ u8 x222A_b3 : 2;
    /* fp+222A:5 */ u8 x222A_b5 : 1;
    /* fp+222A:6 */ u8 x222A_b6 : 1;
    /* fp+222A:7 */ u8 x222A_b7 : 1;

    /* fp+222C */ union Fighter_FighterVars {
        /* fp+222C */ struct ftCaptain_FighterVars ca, gn;
        /* fp+222C */ struct ftDonkey_FighterVars dk;
        /* fp+222C */ struct ftFox_FighterVars fx, fc;
        /* fp+222C */ struct ftGameWatch_FighterVars gw;
        /* fp+222C */ struct ftKb_FighterVars kb;
        /* fp+222C */ struct ftKoopa_FighterVars kp, gk;
        /* fp+222C */ struct ftLk_FighterVars lk;
        /* fp+222C */ struct ftLuigi_FighterVars lg;
        /* fp+222C */ struct ftMario_FighterVars mr;
        /* fp+222C */ struct ftMars_FighterVars ms;
        /* fp+222C */ struct ftMasterhand_FighterVars mh, ch;
        /* fp+222C */ struct ftMewtwo_FighterVars mt;
        /* fp+222C */ struct ftNess_FighterVars ns;
        /* fp+222C */ struct ftPeach_FighterVars pe;
        /* fp+222C */ struct ftPikachu_FighterVars pk, pc;
        /* fp+222C */ struct ftPopo_FighterVars pp, nn;
        /* fp+222C */ struct ftPurin_FighterVars pr;
        /* fp+222C */ struct ftSamus_FighterVars ss;
        /* fp+222C */ struct ftSandbag_FighterVars sb;
        /* fp+222C */ struct ftSeak_FighterVars sk;
        /* fp+222C */ struct ftYoshi_FighterVars ys;
        /* fp+222C */ struct ftZakoBoy_FighterVars bo, gl;
        /* fp+222C */ struct ftZelda_FighterVars zd;
    } fv;
    /* fp+2324 */ InternalStageId bury_stage_kind;
    /* fp+2328 */ u32 bury_timer_1;
    /* fp+232C */ u32 bury_timer_2;
    /* fp+2330 */ IntVec2 x2330;
    /* fp+2338 */ IntVec2 x2338;
    /// @at{2340} @sz{AC}
    /* fp+2340 */ union Fighter_MotionVars {
        /* fp+2340 */ u8 _[0x23EC - 0x2340];
        /* fp+2340 */ union ftCaptain_MotionVars ca, gn;
        /* fp+2340 */ union ftCommon_MotionVars co;
        /* fp+2340 */ union ftDonkey_MotionVars dk;
        /* fp+2340 */ union ftFox_MotionVars fx, fc;
        /* fp+2340 */ union ftGameWatch_MotionVars gw;
        /* fp+2340 */ union ftKb_MotionVars kb;
        /* fp+2340 */ union ftKoopa_MotionVars kp;
        /* fp+2340 */ union ftLk_MotionVars lk;
        /* fp+2340 */ union ftLuigi_MotionVars lg;
        /* fp+2340 */ union ftMario_MotionVars mr, dr;
        /* fp+2340 */ union ftMars_MotionVars ms, fe;
        /* fp+2340 */ union ftMasterHand_MotionVars mh, ch;
        /* fp+2340 */ union ftMewtwo_MotionVars mt;
        /* fp+2340 */ union ftNess_MotionVars ns;
        /* fp+2340 */ union ftPe_MotionVars pe;
        /* fp+2340 */ union ftPikachu_MotionVars pk, pc;
        /* fp+2340 */ union ftPp_MotionVars pp;
        /* fp+2340 */ union ftPurin_MotionVars pr;
        /* fp+2340 */ union ftSamus_MotionVars ss;
        /* fp+2340 */ union ftSeak_MotionVars sk;
        /* fp+2340 */ union ftYoshi_MotionVars ys;
        /* fp+2340 */ union ftZelda_MotionVars zd;
    } mv;
};

struct CollData {
    /* fp+6F0 */ HSD_GObj* x0_gobj;
    /* fp+6F4 */ Vec3 cur_pos;
    // position on the previous step of collision
    /* fp+700 */ Vec3 prev_pos;
    // position before collision routine started
    /* fp+70C */ Vec3 last_pos;
    /* fp+718 */ Vec3 x28_vec;
    /* fp+724 */ ECBFlagStruct x34_flags;
    /* fp+725 */ ECBFlagStruct x35_flags;
    /* fp+726 */ s16 facing_dir;
    /* fp+728 */ int x38;
    /* fp+72C */ int floor_skip;
    /* fp+730 */ int ledge_id_right;
    /* fp+734 */ int ledge_id_left;
    /* fp+738 */ int joint_id_skip;
    /* fp+73C */ int joint_id_only;
    /* fp+740 */ float x50;
    /* fp+744 */ float ledge_snap_x;
    /* fp+748 */ float ledge_snap_y;
    /* fp+74C */ float ledge_snap_height;
    /* fp+750 */ float lstick_x;
    /* fp+754 */ ftECB x64_ecb;
    /* fp+774 */ ftECB desired_ecb;
    /* fp+794 */ ftECB ecb;
    // ECB on the previous step of collision
    /* fp+7B4 */ ftECB prev_ecb;
    /* fp+7D4 */ ftECB xE4_ecb;
    /* fp+7F4 */ ECBSource ecb_source;
    /* fp+820 */ u32 x130_flags;
    /* fp+824 */ s32 env_flags;
    /* fp+828 */ s32 prev_env_flags;
    /* fp+82C */ s32 x13C;
    /* fp+830 */ Vec3 contact;
    /* fp+83C */ SurfaceData floor;
    /* fp+850 */ SurfaceData left_facing_wall;
    /* fp+864 */ SurfaceData right_facing_wall;
    /* fp+878 */ SurfaceData ceiling;
};
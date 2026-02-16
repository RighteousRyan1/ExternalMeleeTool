using System.Runtime.InteropServices;

namespace ExternalMeleeTool.Melee.Fighter;

/// <summary>
/// Attributes that are common to the cast.
/// </summary>
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
    public Vec3 x114;
    /* +120 fp+230 */
    public Vec3 x120;
    /* +12C fp+23C */
    public float x12C;
    /* +130 fp+240 */
    public Vec3 x130;
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
    public Vec3 x170;
    /* +17C fp+28C */
    public float x17C;
    /* +180 fp+290 */
    public byte weight_independent_throws_mask;

    // unsure what these are, but they're necessary to commplete FtCommonAttr
    public struct FtCommonAttr_xBC {
        public float size; // size of what? who knows
        public Vec3 x4;
        public Vec3 x10;
        public float x1C;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct FtCommonData {
    /*   +0 */
    public float deadzone_x;
    /*   +4 */
    public float deadzone_y;
    /*   +8 */
    public float stick_hold_thresh_x;
    /*   +C */
    public float stick_hold_thresh_y;
    /*  +10 */
    public float lr_deadzone;
    /*  +14 */
    public float z_analog_value;
    /*  +18 */
    public float lr_hold_thresh;
    /*  +1C */
    public int tech_lockout;
    /*  +20 */
    public float x20_angle;
    /*  +24 */
    public float walk_thresh;
    /*  +28 */
    public float walk_middle_speed_fraction;
    /*  +2C */
    public float walk_slow_speed_fraction;
    /*  +30 */
    public float walk_accel_moving_mult;
    /*  +34 */
    public float turn_thresh;
    /*  +38 */
    public float turn_run_thresh;
    /*  +3C */
    public float x_smash_thresh;
    /*  +40 */
    public int x_smash_frames;
    /*  +44 */
    public float dash_forward_frames;
    /*  +48 */
    public float dash_forward_roll_frames;
    /*  +4C */
    public float max_initial_dash_frames;
    /*  +50 */
    public float dash_attack_traction_mult;
    /*  +54 */
    public float dash_stop_vel_mult;
    /*  +58 */
    public float run_thresh;
    /*  +5C */
    public float x5C;
    /*  +60 */
    public float dash_traction_mult;
    /*  +64 */
    public float grab_traction_mult;
    /*  +68 */
    public float shield_dash_grab_frames;
    /*  +6C */
    public float double_traction;
    /*  +70 */
    public float y_smash_thresh;
    /*  +74 */
    public int y_smash_frames;
    /*  +78 */
    public float jump_back_thresh;
    /*  +7C */
    public float tap_jump_full_hop_thresh;
    /*  +80 */
    public float running_tap_jump_thresh;
    /*  +84 */
    public float x84;
    /*  +88 */
    public float fastfall_stick_thresh;
    /*  +8C */
    public int fastfall_stick_frames;
    /*  +90 */
    public float squat_thresh;
    /*  +94 */
    public float max_squatwait_thresh;
    /*  +98 */
    public float ftilt_thresh;
    /*  +9C */
    public float uftilt_angle;
    /*  +A0 */
    public float uftilt_mid_angle;
    /*  +A4 */
    public float dftilt_mid_angle;
    /*  +A8 */
    public float dftilt_angle;
    /*  +AC */
    public float utilt_thresh;
    /*  +B0 */
    public float dtilt_thresh;
    /*  +B4 */
    public float xB4;
    /*  +B8 */
    public float xB8_radians;
    /*  +BC */
    public float xBC_radians;
    /*  +C0 */
    public float xC0_radians;
    /*  +C4 */
    public float xC4_radians;
    /*  +C8 */
    public float xC8;
    /*  +CC */
    public float usmash_thresh;
    /*  +D0 */
    public float usmash_frames; // breaks the consistency of int
    /*  +D4 */
    public float dsmash_thresh;
    /*  +D8 */
    public float dsmash_frames;
    /*  +DC */
    public float aerial_thresh_x;
    /*  +E0 */
    public float aerial_thresh_y;
    /*  +E4 */
    public int l_cancel_window;
    /*  +E8 */
    public float l_cancel_lag_divisor;
    /*  +EC */
    public float xEC;
    /*  +F0 */
    public float xF0;
    /*  +F4 */
    public float kb_weight_scale;
    /*  +F8 */
    public float kb_weight_factor;
    /*  +FC */
    public int no_kb_stack_frames;
    /* +100 */
    public float kb_vel_mult;
    /* +104 */
    public float kb_min;
    /* +108 */
    public float kb_max; // arbitrarily set to 2500 by default, lol
    /* +10C */
    public float x10C;
    /* +110 */
    public float kb_percent_factor;
    /* +114 */
    public float kb_percent_times_dmg_factor;
    /* +118 */
    public float fixed_kb_percent;
    /* +11C */
    public float knockback_mult;
    /* +120 */
    public float knockback_addend;
    /* +124 */
    public float crouch_kb_mult;
    /* +128 */
    public float wobble_damage_mult; // damage taken from other sources while grabbed
    /* +12C */
    public float absurd_kb_thresh; // 65000.0 weirdly
    /* +130 */
    public int minigame_iframes;
    /* +134 */
    public int damage_rumble_mult;
    /* +138 */
    public float damage_rumble_addend;
    /* +13C */
    public float grab_break_kb;
    /* +140 */
    public float sakurai_air_angle;
    /* +144 */
    public float sakurai_high_kb_angle;
    /* +148 */
    public float sakurai_low_kb;
    /* +14C */
    public float sakurai_high_kb;
    /* +150 */
    public float hitstun_mult;
    /* +154 */
    public float kb_level_1;
    /* +158 */
    public float kb_level_2;
    /* +15C */
    public float kb_level_3;
    /* +160 */
    public float max_ground_kb_vel;
    /* +164 */
    public float damage_shake_mult;
    /* +168 */
    public float damage_shake_addend; // 0 lol, but cool for config
    /* +16C */
    public float x16C;
    /* +170 */
    public float x170;
    /* +174 */
    public float x174;
    /* +178 */
    public float x178;
    /* +17C */
    public float x17C;
    /* +180 */
    public float x180;
    /* +184 */
    public float x184;
    /* +188 */
    public float x188;
    /* +18C */
    public int v_cancel_window;
    /* +190 */
    public float v_cancel_kb_mult;
    /* +194 */
    public float hitstop_max;
    /* +198 */
    public float hitstop_mult;
    /* +19C */
    public float hitstop_addend;
    /* +1A0 */
    public float hitstop_crouch_mult;
    /* +1A4 */
    public float hitstop_electric_mult; // electric mult?
    /* +1A8 */
    public float tdi_angle;
    /* +1AC */
    public float x1AC;
    /* +1B0 */
    public float tumble_bounce_thresh;
    /* +1B4 */
    public float x1B4;
    /* +1B8 */
    public int x1B8;
    /* +1BC */
    public float x1BC;
    /* +1C0 */
    public float x1C0;
    /* +1C4 */
    public float x1C4;
    /* +1C8 */
    public float x1C8;
    /* +1CC */
    public float x1CC;
    /* +1D0 */
    public float x1D0;
    /* +1D4 */
    public float x1D4;
    /* +1D8 */
    public float x1D8;
    /* +1DC */
    public UNK_T x1DC;
    /* +1E0 */
    public float x1E0;
    /* +1E4 */
    public float x1E4;
    /* +1E8 */
    public float x1E8_radians;
    /* +1EC */
    public float x1EC;
    /* +1F0 */
    public float x1F0;
    /* +1F4 */
    public float x1F4;
    /* +1F8 */
    public float x1F8;
    /* +1FC */
    public float x1FC;
    /* +200 */
    public float x200;
    /* +204 */
    public float x204_knockbackFrameDecay;
    /* +208 */
    public float x208;
    /* +20C */
    public float x20C;
    /* +210 */
    public float x210;
    /* +214 */
    public int x214;
    /* +218 */
    public float x218;
    /* +21C */
    public float x21C;
    /* +220 */
    public float x220;
    /* +224 */
    public int x224;
    /* +228 */
    public float x228;
    /* +22C */
    public float x22C;
    /* +230 */
    public float x230;
    /* +234 */
    public float x234_radians;
    /* +238 */
    public float x238_radians;
    /* +23C */
    public UNK_T x23C;
    /* +240 */
    public float x240;
    /* +244 */
    public float x244;
    /* +248 */
    public float x248;
    /* +24C */
    public float x24C;
    /* +250 */
    public float tech_window; // in frames
    /* +254 */
    public float tech_roll_thresh;
    /* +258 */
    public float x258;
    /* +25C */
    public float x25C;
    /* +260 */
    public float shield_max_health;
    /* +264 */
    public float shield_scale_base;
    /* +268 */
    public float x268;
    /* +26C */
    public float x26C;
    /* +270 */
    public float x270;
    /* +274 */
    public UNK_T x274;
    /* +278 */
    public float shield_decay_scale;
    /* +27C */
    public float shield_regen;
    /* +280 */
    public float shield_size_after_break;
    /* +284 */
    public float shield_damage_mult;
    /* +288 */
    public float shield_damage_addend;
    /* +28C */
    public float shield_stun_mult;
    /* +290 */
    public float shield_stun_addend;
    /* +294 */
    public float shield_kb_vel_mult;
    /* +298 */
    public float shield_max_kb_vel;
    /* +29C */
    public float x29C;
    /* +2A0 */
    public int x2A0;
    /* +2A4 */
    public float x2A4;
    /* +2A8 */
    public float x2A8;
    /* +2AC */
    public float x2AC;
    /* +2B0 */
    public float x2B0;
    /* +2B4 */
    public float x2B4;
    /* +2B8 */
    public int x2B8;
    /* +2BC */
    public float x2BC;
    /* +2C0 */
    public float x2C0;
    /* +2C4 */
    public float x2C4;
    /* +2C8 */
    public float x2C8;
    /* +2CC */
    public float x2CC;
    /* +2D0 */
    public float x2D0;
    /* +2D4 */
    public float x2D4;
    /* +2D8 */
    public float x2D8;
    /* +2DC */
    public float x2DC;
    /* +2E0 */
    public float x2E0;
    /* +2E4 */
    public float x2E4;
    /* +2E8 */
    public float x2E8;
    /* +2EC */
    public float x2EC;
    /* +2F0 */
    public float x2F0;
    /* +2F4 */
    public float x2F4;
    /* +2F8 */
    public float x2F8;
    /* +2FC */
    public float x2FC;
    /* +300 */
    public float x300;
    /* +304 */
    public float x304;
    /* +308 */
    public float x308;
    /* +30C */
    public float x30C;
    /* +310 */
    public float x310;
    /* +314 */
    public float x314;
    /* +318 */
    public int x318;
    /* +31C */
    public float x31C;
    /* +320 */
    public int x320;
    /* +324 */
    public int roll_kara_item_throw_frames;
    /* +328 */
    public float x328;
    /* +32C */
    public Vec2 airdodge_deadzone;
    /* +334 */
    public int x334;
    /* +338 */
    public float escapeair_force;
    /* +33C */
    public float escapeair_decay;
    /* +340 */
    public float x340;
    /* +344 */
    public float x344;
    /* +348 */
    public int x348;
    /* +34C */
    public float x34C;
    /* +350 */
    public float x350;
    /* +354 */
    public float x354;
    /* +358 */
    public float x358;
    /* +35C */
    public float x35C;
    /* +360 */
    public float x360;
    /* +364 */
    public float x364;
    /* +368 */
    public float x368;
    /* +36C */
    public float x36C;
    /* +370 */
    public float grab_break_ground_vel;
    /* +374 */
    public Vec2 grab_break_air_vel;
    /* +37C */
    public float throw_speed_weight_scale;
    /* +380 */
    public fixed byte x380_lbcoll[36];
    /* +3A4 */
    public float grab_passive_decay;
    /* +3A8 */
    public float grab_mash_decay;
    /* +3AC */
    public float x3AC;
    /* +3B0 */
    public float x3B0;
    /* +3B4 */
    public float shouldered_anim_rate;
    /* +3B8 */
    public float x3B8;
    /* +3BC */
    public float x3BC;
    /* +3C0 */
    public int x3C0;
    /* +3C4 */
    public float x3C4;
    /* +3C8 */
    public float x3C8;
    /* +3CC */
    public int x3CC;
    /* +3D0 */
    public float clank_frames_mult;
    /* +3D4 */
    public float clank_frames_addend;
    /* +3D8 */
    public float clank_push_mult;
    /* +3DC */
    public float x3DC;
    /* +3E0 */
    public float x3E0;
    /* +3E4 */
    public float x3E4;
    /* +3E8 */
    public float x3E8_shieldKnockbackFrameDecay;
    /* +3EC */
    public float x3EC_shieldGroundFrictionMultiplier;
    /* +3F0 */
    public float x3F0;
    /* +3F4 */
    public UNK_T x3F4;
    /* +3F8 */
    public UNK_T x3F8;
    /* +3FC */
    public int x3FC;
    /* +400 */
    public float x400;
    /* +404 */
    public float x404;
    /* +408 */
    public float x408;
    /* +40C */
    public float x40C;
    /* +410 */
    public int x410;
    /* +414 */
    public int x414;
    /* +418 */
    public int x418;
    /* +41C */
    public int x41C;
    /* +420 */
    public float x420;
    /* +424 */
    public float downwait_maxframes;
    /* +428 */
    public int downdamage_thresh;
    /* +42C */
    public float x42C;
    /* +430 */
    public float x430;
    /* +434 */
    public float x434;
    /* +438 */
    public float x438;
    /* +43C */
    public float x43C;
    /* +440 */
    public float x440;
    /* +444 */
    public float x444;
    /* +448 */
    public float x448;
    /* +44C */
    public float x44C;
    /* +450 */
    public float x450;
    /* +454 */
    public float x454;
    /* +458 */
    public float x458;
    /* +45C */
    public float x45C;
    /* +460 */
    public float x460;
    /* +464 */
    public float x464;
    /* +468 */
    public float x468;
    /* +46C */
    public float x46C;
    /* +470 */
    public float x470;
    /* +474 */
    public float x474;
    /* +478 */
    public float x478;
    /* +47C */
    public float x47C;
    /* +480 */
    public float x480;
    /* +484 */
    public float x484;
    /* +488 */
    public int x488;
    /* +48C */
    public float x48C;
    /* +490 */
    public float x490;
    /* +494 */
    public float x494;
    /* +498 */
    public int ledge_cooldown;
    /* +49C */
    public int ledge_iframes;
    /* +4A0 */
    public float x4A0;
    /* +4A4 */
    public float x4A4;
    /* +4A8 */
    public float x4A8;
    /* +4AC */
    public float x4AC;
    /* +4B0 */
    public float x4B0;
    /* +4B4 */
    public int sdi_stick_thresh;
    /* +4B8 */
    public float sdi_stick_frames;
    /* +4BC */
    public float sdi_dist;
    /* +4C0 */
    public float asdi_dist;
    /* +4C4 */
    public int shield_sdi_dist;
    /* +4C8 */
    public int x4C8;
    /* +4CC */
    public int x4CC;
    /* +4D0 */
    public float x4D0;
    /* +4D4 */
    public float x4D4;
    /* +4D8 */
    public u32 x4D8;
    /* +4DC */
    public Vec2 x4DC;
    /* +4E4 */
    public Vec3 x4E4;
    /* +4F0 */
    public float x4F0;
    /* +4F4 */
    public float x4F4;
    /* +4F8 */
    public u32 x4F8;
    /* +4FC */
    public u32 x4FC;
    /* +500 */
    public int respawn_timer;
    /* +504 */
    public int respawn_timer_starko;
    /* +508 */
    public UNK_T x508;
    /* +50C */
    public UNK_T x50C;
    /* +510 */
    public float x510;
    /* +514 */
    public float x514;
    /* +518 */
    public UNK_T x518;
    /* +51C */
    public float x51C_radians;
    /* +520 */
    public int screen_ko_chance;
    /* +524 */
    public int screen_ko_respawn_timer;
    /* +528 */
    public UNK_T x528;
    /* +52C */
    public UNK_T x52C;
    /* +530 */
    public UNK_T x530;
    /* +534 */
    public UNK_T x534;
    /* +538 */
    public UNK_T x538;
    /* +53C */
    public float x53C;
    /* +540 */
    public float x540;
    /* +544 */
    public UNK_T x544;
    /* +548 */
    public float x548;
    /* +54C */
    public float x54C;
    /* +550 */
    public float x550;
    /* +554 */
    public float x554;
    /* +558 */
    public float x558;
    /* +55C */
    public float x55C;
    /* +560 */
    public float x560_radians;
    /* +564 */
    public float x564;
    /* +568 */
    public float x568;
    /* +56C */
    public float x56C;
    /* +570 */
    public float x570;
    /* +574 */
    public float x574;
    /* +578 */
    public float x578;
    /* +57C */
    public int x57C;
    /* +580 */
    public int x580;
    /* +584 */
    public int x584;
    /* +588 */
    public int x588;
    /* +58C */
    public float x58C;
    /* +590 */
    public float x590;
    /* +594 */
    public float open_parasol_threshold;
    /* +598 */
    public float close_parasol_threshold;
    /* +59C */
    public float x59C;
    /* +5A0 */
    public float x5A0;
    /* +5A4 */
    public int x5A4;
    /* +5A8 */
    public float x5A8;
    /* +5AC */
    public float x5AC;
    /* +5B0 */
    public float x5B0;
    /* +5B4 */
    public int x5B4;
    /* +5B8 */
    public float x5B8;
    /* +5BC */
    public UNK_T x5BC;
    /* +5C0 */
    public float x5C0;
    /* +5C4 */
    public UNK_T x5C4;
    /* +5C8 */
    public int x5C8;
    /* +5CC */
    public float x5CC;
    /* +5D0 */
    public UNK_T x5D0;
    /* +5D4 */
    public UNK_T x5D4;
    /* +5D8 */
    public UNK_T x5D8;
    /* +5DC */
    public u32 bury_timer_unk1;
    /* +5E0 */
    public u32 bury_timer_unk2;
    /* +5E4 */
    public u32 bury_timer_unk3;
    /* +5E8 */
    public float x5E8;
    /* +5EC */
    public UNK_T x5EC;
    /* +5F0 */
    public u32 x5F0;
    /* +5F4 */
    public int x5F4;
    /* +5F8 */
    public float x5F8;
    /* +5FC */
    public float x5FC;
    /* +600 */
    public float x600;
    /* +604 */
    public float x604;
    /* +608 */
    public float x608;
    /* +60C */
    public float x60C;
    /* +610 */
    public float x610;
    /* +614 */
    public float x614;
    /* +618 */
    public float x618;
    /* +61C */
    public float x61C;
    /* +620 */
    public int x620;
    /* +624 */
    public float x624;
    /* +628 */
    public float x628;
    /* +62C */
    public float x62C;
    /* +630 */
    public float x630;
    /* +634 */
    public float x634;
    /* +638 */
    public float x638;
    /* +63C */
    public float x63C;
    /* +640 */
    public float x640;
    /* +644 */
    public float x644;
    /* +648 */
    public int x648;
    /* +64C */
    public float cape_kb_vel_air;
    /* +650 */
    public float cape_kb_vel_gr;
    /* +654 */
    public float cape_kb_vel_shield;
    /* +658 */
    public float x658;
    /* +65C */
    public float x65C;
    /* +660 */
    public float x660;
    /* +664 */
    public float x664;
    /* +668 */
    public float x668;
    /* +66C */
    public float x66C;
    /* +670 */
    public float x670;
    /* +674 */
    public float x674;
    /* +678 */
    public float super_mushroom_scl_tinymelee;
    /* +67C */
    public float super_mushroom_scale;
    /* +680 */
    public float x680; // could be poison mushroom related
    /* +684 */
    public float x684; // same
    /* +688 */
    public int mushroom_duration;
    /* +68C */
    public int x68C;
    /* +690 */
    public int x690;
    /* +694 */
    public float x694;
    /* +698 */
    public float x698;
    /* +69C */
    public float warpstarfall_drift_scaling;
    /* +6A0 */
    public float warpstarfall_drift_flat;
    /* +6A4 */
    public float warpstarfall_drift_max;
    /* +6A8 */
    public float x6A8;
    /* +6AC */
    public int x6AC;
    /* +6B0 */
    public int x6B0;
    /* +6B4 */
    public int x6B4;
    /* +6B8 */
    public int x6B8;
    /* +6BC */
    public int x6BC;
    /* +6C0 */
    public int x6C0;
    /* +6C4 */
    public float x6C4;
    /* +6C8 */
    public int x6C8;
    /* +6CC */
    public int x6CC;
    /* +6D0 */
    public float x6D0;
    /* +6D4 */
    public int stamina_kb_percent;
    /* +6D8 */
    public Ptr32 x6D8;
    /* +6DC */
    public GXColor col_pl1, col_pl2, col_pl3, col_pl4;
    /* +6EC */
    public fixed byte x6EC[0x6F0 - 0x6EC];
    /* +6F0 */
    public float metal_armor;
    /* +6F4 */
    public int x6F4_unkDamage;
    /* +6F8 */
    public int x6F8;
    /* +6FC */
    public int x6FC;
    /* +700 */
    public int x700;
    /* +704 */
    public float x704;
    /* +708 */
    public float x708;
    /* +70C */
    public float x70C;
    /* +710 */
    public float x710;
    /* +714 */
    public float x714;
    /* +718 */
    public float kb_ice_mul;
    /* +71C */
    public float x71C;
    /* +720 */
    public float x720;
    /* +724 */
    public float x724;
    /* +728 */
    public float x728;
    /* +72C */
    public float x72C;
    /* +730 */
    public float x730;
    /* +734 */
    public float x734; //< leadead capture timer decrement
    /* +738 */
    public float x738; //< leadead grab break threshold
    /* +73C */
    public int x73C;
    /* +740 */
    public float x740;
    /* +744 */
    public float x744;
    /* +748 */
    public float x748;
    /* +74C */
    public float x74C;
    /* +750 */
    public float x750;
    /* +754 */
    public float x754;
    /* +758 */
    public float x758;
    /* +75C */
    public float x75C;
    /* +760 */
    public int wall_tech_freeze_frames;
    /* +764 */
    public int walljump_intangibility;
    /* +768 */
    public float walljump_window;
    /* +76C */
    public float walljump_stick_thresh;
    /* +770 */
    public float walljump_freeze_frames;
    /* +774 */
    public int x774;
    /* +778 */
    public float passive_wall_vel_y_base;
    /* +77C */
    public float damageice_gravity_mult;
    /* +780 */
    public float damageice_min_speed;
    /* +784 */
    public float damageice_speed_mult_on_break;
    /* +788 */
    public float damageice_rot_speed_min;
    /* +78C */
    public float damageice_rot_speed_max;
    /* +790 */
    public float x790_damageice_unk;
    /* +794 */
    public float x794_damageice_unk;
    /* +798 */
    public float x798_damageice_unk;
    /* +79C */
    public float damageice_dmg_time_reduction_mult;
    /* +7A0 */
    public float damageice_ice_size;
    /* +7A4 */
    public float damageicejump_escape_time;
    /* +7A8 */
    public float phantom_thresh;
    /* +7AC */
    public int x7AC;
    /* +7B0 */
    public int x7B0;
    /* +7B4 */
    public int x7B4_unkDamage;
    /* +7B8 */
    public float x7B8;
    /* +7BC */
    public float x7BC;
    /* +7C0 */
    public float x7C0;
    /* +7C4 */
    public float smash_charge_kb_mult;
    /* +7C8 */
    public float x7C8;
    /* +7CC */
    public int x7CC;
    /* +7D0 */
    public int x7D0;
    /* +7D4 */
    public float hit_weight_mul;
    /* +7D8 */
    public GXColor x7D8;
    /* +7DC */
    public int x7DC;
    /* +7E0 */
    public int x7E0;
    /* +7E4 */
    public float flatzone_scale_z;
    /* +7E8 */
    public u32 unk_kb_angle_min;
    /* +7EC */
    public u32 unk_kb_angle_max;
    /* +7F0 */
    public int meteor_cancel_delay;
    /* +7F4 */
    public float x7F4;
    /* +7F8 */
    public float x7F8;
    /* +7FC */
    public float x7FC;
    /* +800 */
    public float x800;
    /* +804 */
    public float max_ground_rotation;
    /* +808 */
    public Vec3 x808;
    /* +814 */
    public int x814;
    // potentially more size?
}

public unsafe struct FtAction {
    public Ptr32 anim_symbol; // char*, aka string
    public int anim_offset;
    public int anim_size;
    public Ptr32 script; // script data for this action (void*)
    public int flags;
    public Ptr32 anim_data; // anim data in aram

    public const uint SIZE = 0x18;
}


/// <summary>
/// Fighter Animation Tree, I think.
/// </summary>
public struct FigATree {
    public int type;
    public u32 flags;
    public f32 frames;
    public Struct_t nodes; // s8*
    public Struct_t tracks; // FigaTrack*
}

public struct FtPartsTable {
    public Ptr32 joint_to_part; // byte*
    public Ptr32 part_to_joint; // byte*
    public int parts_num; // amount of parts this fighter has
}

public struct FighterBone {
    public Struct_t jobj; // JObj*
    public Struct_t jobj_interpolate; // JObj*
    public u8 u1_flags1; // u = union
    public u8 u1_flags2;
    public u16 u1_flags8;

    // public u8 u2_flags1; // first bit = flag 1, other 7 bits = flag 2
    public u32 u2_t; // has two u8s and a u32, total size = 4

    public const int SIZE = 0x10;
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
    None = 0x21,
    Max = Sandbag
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
    BarrelCannonWait,

    Count = BarrelCannonWait
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
    WaistN2,
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

    Max = 109,

    Invalid = 255
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
// how they're typed on the GameCube
global using u8 = byte;
global using s8 = sbyte;
global using u16 = ushort;
global using s16 = short;
global using u32 = uint;
global using s32 = int;
global using u64 = ulong;
global using s64 = long;

// how they're named in melee's code
global using Mtx = ExternalMeleeTool.Melee.Matrix3x4;
global using Vec3 = System.Numerics.Vector3;
global using Vec2 = System.Numerics.Vector2;
global using S32Vec2 = System.Drawing.Point;

global using f32 = float;

// types analagous to GC
global using HSD_Pad = uint;

// naming clarity
global using UNK_T = ExternalMeleeTool.Ptr32;
global using Func_t = ExternalMeleeTool.Ptr32;
global using GObj_t = ExternalMeleeTool.Ptr32;
global using JObj_t = ExternalMeleeTool.Ptr32;
global using DObj_t = ExternalMeleeTool.Ptr32;

// semantically wrong i think
global using Struct_t = ExternalMeleeTool.Ptr32;
global using PtrPtr32 = ExternalMeleeTool.Ptr32; // pointer to a pointer
global using enum_t = uint;

// function callback types
global using Callback32 = ExternalMeleeTool.Ptr32;


using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Utilities;

namespace ExternalMeleeTool;

/// <summary>
/// Data containing a pointer to a 32-bit address. If you wish to de-reference a <see cref="Ptr32"/>, use <see cref="Dolphinterop.Read{T}(s64)"/> with <see cref="Dolphinterop.ReadPtr(s64)"/> as the parameter.
/// </summary>
/// <param name="value"></param>
public readonly struct Ptr32(uint value) {
    readonly uint Value = value;

    public static implicit operator uint(Ptr32 p) => p.Value;
    public static implicit operator Ptr32(uint value) => new(value);

    public override string ToString() => $"0x{Value:X8}";
}

public static class MeleeEvents {
    /// <summary>
    /// Currently non-functional/useless. Will have a use in the future.
    /// </summary>
    public ref struct ComboSettings {
        public bool MustKill;
        public bool MinPercent;
        public bool MinHits;
        /// <summary>Number of frames maximum between sequential hits.</summary>
        public int  Leniency;
    }

    // General match events
    public delegate void StockLost(FighterData fighter); // fire when a change in the stock value occurs
    public delegate void MatchOver(FighterData[] winners); // separate based on team or FFA logic? FFA = 1 player remains, teams = one team remains
    public delegate void GamePause(FighterData fighter, bool paused);
    public delegate void LRAStart (FighterData fighter); // check for pause, then find if a player has LRA-started... track player to have last pressed their start button and only check that player for LRA-start
    public delegate void MatchGo(); // when the match timer decrements for the first time?

    /// <summary>Called when a fighter loses a stock in any manner.</summary>
    public static event StockLost? OnStockLost;
    /// <summary>Called when a match is over.</summary>
    public static event MatchOver? OnMatchOver;
    /// <summary>Currently non-functional.</summary>
    public static event GamePause? OnGamePause; // Nonfunctional.
    /// <summary>Currently non-functional.</summary>
    public static event  LRAStart?  OnLRAStart; // Nonfunctional.
    /// <summary>Called after the "GO!" text appears on match start.</summary>
    public static event   MatchGo?   OnMatchGo;

    // Fighter events
    public delegate void FighterHit    (FighterData assailant, FighterData victim, HitCapsule hitbox);
    public delegate void EnterKnockback(FighterData fighter);
    public delegate void MeteorCancel  (FighterData fighter);
    public delegate void EnterAttack   (FighterData fighter);
    public delegate void LeaveAttack   (FighterData fighter);
    public delegate void FighterGrab   (FighterData grabber, FighterData victim);

    /// <summary>Called when a fighter is hit by another fighter.</summary>
    public static event FighterHit?         OnFighterHit;
    /// <summary>Called when a fighter enters a knockback state.</summary>
    public static event EnterKnockback? OnEnterKnockback;
    /// <summary>Called when a fighter successfully performs a meteor cancel.</summary>
    public static event MeteorCancel?     OnMeteorCancel;
    /// <summary>Called when a fighter activates an attack's hitboxes.</summary>
    public static event EnterAttack?       OnEnterAttack;
    /// <summary>Called when a fighter deactivates an attack's hitboxes.</summary>
    public static event LeaveAttack?       OnLeaveAttack;
    /// <summary>
    /// Call once within your update loop to ensure that all events contained by <see cref="MeleeEvents"/> are fired at the appropriate times.
    /// </summary>
    public static void PollEvents(MatchData md, SlippiOnlineData sod, SceneData sd) {

        if (!sd.IsIngame || md.ActiveFighters.All(x => x.AnimState == FtAnimState.EntryStart)) {
            firstFrame1 = false;
        }

        // fresh pause/unpause
        /*if (md.IsPaused != mdPrev.IsPaused) {
            var idx = Array.FindIndex(md.Fighters, x => {
                Console.WriteLine($"Port {x.Port}:" +
                    $"\nPressed: {x.Input.Pressed}" +
                    $"\nHeld:    {x.Input.Held}" +
                    $"\nPreserv: {x.Input.Preserved}" +
                    $"\nPrev:    {x.Input.Prev}" +
                    $"\nRel:     {x.Input.Released}");

                    return x.Input.Preserved.HasFlag(HSDPadButton.Start);
                });

            if (idx > -1) {
                OnGamePause?.Invoke(md.Fighters[idx].Port, md.IsPaused);

                if (md.IsPaused) pausePort = idx;
                else pausePort = -1;
            }
        }

        if (md.IsPaused && pausePort > -1) {
            var pauser = md.Fighters[pausePort];

            // Console.WriteLine(pauser.Input.Preserved);

            if (pauser.Input.Preserved.HasFlag(HSDPadButton.TriggerL | HSDPadButton.TriggerR | 
                HSDPadButton.A | HSDPadButton.Start)) {

                OnLRAStart?.Invoke(pausePort);
            }
        }*/

        // game is over?
        // only if not a quit
        if (!md.IsPaused && md.Frame == mdPrev.Frame) {
            var winners = new List<FighterData>();

            foreach (var fighter in md.ActiveFighters) {
                if (fighter.Stocks > 0) winners.Add(fighter);
            }

            OnMatchOver?.Invoke([.. winners]);
        }

        // frame sits on 59 until go happens
        if (!firstFrame1 && md.Frame == 0) {
            OnMatchGo?.Invoke();
            firstFrame1 = true;
        }

        if (mdPrev.Fighters != null) {
            for (int i = 0; i < md.Fighters.Length; i++) {
                var cFighter = md.Fighters[i];
                var pFighter = mdPrev.Fighters[i];

                // check IsDead to prevent stock steal from firing the event
                if (cFighter.Stocks < pFighter.Stocks && cFighter.IsDead) {
                    OnStockLost?.Invoke(cFighter);
                }

                if (cFighter.Knockback.Y == 0 && cFighter.VelocitySelf.Y > 0 && pFighter.Knockback.Y < 0 && pFighter.VelocitySelf.Y < 0) {
                    OnMeteorCancel?.Invoke(cFighter);
                }

                if (cFighter.HasKnockback && !pFighter.HasKnockback) {
                    OnEnterKnockback?.Invoke(cFighter);
                }

                bool anyJustActive = false, anyJustDeactive = false;
                bool hitInvoked = false;
                for (int j = 0; j < FighterData.HitCapsuleBuffer6.LENGTH; j++) {
                    var cHb = md.Fighters[i].Hitboxes[j];
                    var pHb = mdPrev.Fighters[i].Hitboxes[j];

                    for (int k = 0; k < HitCapsule.HitVictimBuffer12.LENGTH; k++) {
                        var vict = cHb.hit_objects[k];
                        var oldVict = pHb.hit_objects[k];

                        // writes are semi-expensive. 
                        if (OnFighterHit != null) {
                            /*if (cHb.state == HitCapsuleState.Disabled) {
                                // vict.victim = 0;

                                // var writeOff = cFighter.FighterPtr + 0x914 + (HitCapsule.SIZE * i);

                                // var writeOff = cFighter.FighterPtr + 0x914 + (HitCapsule.SIZE * i) + 0x74 + (HitVictim.SIZE * k);
                                // this forces the hitbox victim to be 0 when it is deactivated.
                                // Dolphinterop.WriteU32(writeOff, vict.victim);

                                // Dolphinterop.WriteU32(cFighter.FighterPtr + 0x1a58, 0);
                            }*/

                            // praying this keeps working...!!!!

                            // multihits are not factored.

                            // Console.WriteLine($"c: {vict.victim}, o: {oldVict.victim}");
                            bool freshHit = vict.victim > oldVict.victim; // > vs != creates a difference?

                            // hackiness ftw
                            var validFighter = md.Fighters[i].CharKind > FtKind.Mario && md.Fighters[i].CharKind < (FtKind)100;
                            if (freshHit && !hitInvoked && validFighter) {
                                var victim = new FighterData();
                                Dolphinterop.ReadFromFighterPtr(ref victim, vict.victim);


                                OnFighterHit?.Invoke(md.Fighters[i], victim, cHb);
                                hitInvoked = true;
                            }
                        }

                        if (!anyJustActive && cHb.state == HitCapsuleState.Init && pHb.state == HitCapsuleState.Disabled) anyJustActive = true;
                        if (!anyJustDeactive && cHb.state == HitCapsuleState.Disabled && pHb.state == HitCapsuleState.Wait) anyJustDeactive = true;
                    }
                }

                if (anyJustActive) OnEnterAttack?.Invoke(cFighter);
                if (anyJustDeactive) OnLeaveAttack?.Invoke(cFighter);
            }
        }

        // ComboEvents();

        mdPrev = md;
        sodPrev = sod;
        sdPrev = sd;
    }

    static void ComboEvents() {
        // TODO: Implement
    }

    static MatchData mdPrev;
    static SlippiOnlineData sodPrev;
    static SceneData sdPrev;

    static bool firstFrame1;
    // static int pausePort; // -1 = no pauser
}

// TODO: change to pointers starting at game rom
/// <summary>A static class that contains important pointers to melee's memory.</summary>
public static class MeleePointers {
    // these are all offsets from GALE01!!!!
    public const uint DEVELOP_CAM_START = 0x80453040;
    public const uint STD_CAM_START = 0x80452C68;  // Camera
    public const uint STD_COBJ_START = 0x804D6464; // CObj
    public const uint CURRENT_COBJ_START = 0x804D765C;
    public const uint CAM_TYPE = 0x80452C6F;

    // PlayerMatchInfo = 8046b6d8.. look there soon. always 6 entries

    // maybe change to read from PLAYER_ONE + (playerIndex * sizeof(StaticPlayer))?
    public const uint PLAYER_ONE = 0x80453080;
    public const uint PLAYER_TWO = 0x80453F10;
    public const uint PLAYER_THREE = 0x80454DA0;
    public const uint PLAYER_FOUR = 0x80455C30;
    public const uint PAUSE_BIT = 0x80479D68;

    public const uint START_MELEE_RULES = 0x8046DB68;

    public const uint MINOR_SCENE = 0x80479D30;
    public const uint MAJOR_SCENE = 0x80479D33;
    public const byte MAJOR_SCENE_MAINMENU = 0;
    public const byte MAJOR_SCENE_STAGESELECT = 1;
    public const byte MAJOR_SCENE_INGAME = 2;

    // size of GC memory, where all code lies for any GC game
    public const uint RAM_SIZE = 0x02000000;
    public const uint ROM_SIZE = 0x80000000;

    // what is R13?
    public const uint R13 = 0x804DB6A0;
    // important for bone mapping!
    public const uint CHR_SKEL_INFO_TABLE = 0x804D6544;

    public const uint STAGE_INFO = 0x8049E6C8;


    // this is a linked list
    public const uint MAP_COLL_JOINT_HEAD = 0x804D64C0;// C8 is count?

    public const uint MATCH_INFO = 0x8046B6A0; // TODO: look here later
    public const uint MATCH_CAM = 0x80452C68;
    public const uint MATCH_HUD = 0x804A0FD8;

    public const uint MATCH_HUD_HIDDEN = 0x804d6D58;

    // uh.... hardcore mode?
    public const uint MATCH_DEV_HUD_HIDDEN = 0x804D6D58;

    public const uint OFFSCREEN_BUBBLE_TABLE = 0x804A1DE0;

    // idk what this was
    // public const uint PLCO_START = 0x80C54C80;
    public const uint PLCO_PTR = 0x804D6554; // this is the ptr to the real plco
    // lookup tables
    public const uint GOBJ_LOOKUP_TABLE = 0x804D782C; // R13 - 0x3E74; // GOBJ**, or PLinkList
    // ReadPtr(), loop through MATCHPLINK max, ReadPtr()

    public static IEnumerable<GObj> GetGObjList(PLink plink) {
        // PLinkList addr
        var plinkoffset = (s64)plink * sizeof(int);
        var collection_ptr = Dolphinterop.ReadPtr(GOBJ_LOOKUP_TABLE);
        var link_ptr = Dolphinterop.ReadPtr(collection_ptr + plinkoffset);

        var curAddr = link_ptr;
        while (curAddr != 0) {
            var gobj = Dolphinterop.Read<GObj>(curAddr);
            yield return gobj;
            curAddr = gobj.next;
        }
    }
}
/// <summary>A static class that contains important pointers to Slippi Netplay memory.</summary>
public static class SlippiGlobals {
    // thanks, Altafen!
    public const uint ONLINE_DATA_BLOCK = MeleePointers.R13 - 0x49E4;
}
// assists with offset changes/value changes in training mode (CE)
public static class TMConstants {
    // training lab
    public const byte MINOR_SCENE_TM = 43;
}
// STATIC STRUCTS
public struct CharSkeletonInfo {
    public Ptr32 joint_to_part; // byte*
    public Ptr32 part_to_joint; // supposedly byte*, but i think HSD_JObj*, but realistically could be joint index
    public uint parts_count; // _num?
}

// ENUMS
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
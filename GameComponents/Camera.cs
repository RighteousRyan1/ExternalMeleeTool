using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static ExternalMeleeTool.Utilities.UnsafeUtils;

namespace ExternalMeleeTool.GameComponents;

// TODO: FIND!!! FIND!!! where the camera's up-vector is. it's very important.

// size = 944
/// <summary>
/// A structure that describes a camera specific to Melee.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 924)]
public unsafe struct Camera {
    /* 0x000 */ 
    public GObj_t gobj;
    /* 0x004 */ 
    public CameraType mode;
    /* 0x008 */ 
    public u8 background_r;
    /* 0x009 */
    public u8 background_g;
    /* 0x00A */
    public u8 background_b;
    /* 0x00B */
    public s8 xB;
    /* 0x00C */
    public f32 nearz;
    /* 0x010 */
    public f32 farz;
    /* 0x014 */
    public CameraTransformState transform;
    /* 0x04C */ 
    public CameraTransformState transform_copy; // this runs the same tween logic, but isnt used for anything?
    /* 0x084 */
    public Vec2 translation;
    /* 0x08C */
    public fixed s32 _8C[5]; /* maybe part of translation[4]? */
    /* 0x0A0 */
    public GObj_t xA0_gobj;
    /* 0x0A4 */ f32 xA4;
    /* 0x0A8 */ f32 xA8;
    /* 0x0AC */ f32 xAC; /* inferred */
    // Vec3?

    // quakes are 16 big
    // /* 0x0B0 */ struct CameraQuake _B0[2][8];
    //fixed byte cam_quakes_xb0[256];
    public CameraQuakeBuffer2x8 cam_quakes_xb0;
    // /* 0x1B0 */ struct CameraQuake _1B0[2][8];
    //fixed byte cam_quakes_x1b0[256]; // doing inlinearray of an inlinearray in c# is fucking hell. no point, so just add padding
    public CameraQuakeBuffer2x8 cam_quakes_x1b0;

    [InlineArray(2)]
    public struct CameraQuakeBuffer2x8 {
        [InlineArray(8)]
        public struct CameraQuakeBuffer8 {
            CameraQuake _instance;
        }
        CameraQuakeBuffer8 instance;

        public const int H = 2;
        public const int W = 8;
    }

    /* 0x2B0 */
    // one of these could have to do with the training mode pause menu
    public float x2B0; // <-- constantly set to 1
    /* 0x2B4 */
    public float x2B4; // <-- ... started at 341, but sets to 1 for some reason?
    /* 0x2B8 */
    public s16 x2B8;
    /* 0x2BA */
    public s16 x2BA;
    /* 0x2BC */
    public f32 focus_dist_mult; // <-- distance from player in real-time, 1 = what it would be normally
    /* 0x2C0 */
    public f32 x2C0; // <-- starts at *241* and does the same thing
    /* 0x2C4 */
    public s8 x2C4; /* unk player slot */
    /* 0x2C5 */
    public s8 x2C5;
    /* 0x2C6 */
    fixed byte pad_2C6[2]; // unknown stuff, i guess?
    /* 0x2C8 */
    public float pitch_offset;
    /* 0x2CC */
    public float yaw_offset;
    /* 0x2D0 */
    public f32 x2D0;
    /* 0x2D4 */
    public f32 x2D4;
    /* 0x2D8 */
    public f32 x2D8;
    /* 0x2DC */
    public f32 x2DC;
    /* 0x2E0 */
    public f32 x2E0;
    /* 0x2E4 */
    public f32 x2E4;
    /* 0x2E8 */
    public f32 x2E8;
    /* 0x2EC */
    public f32 x2EC;
    /* 0x2F0 */
    public f32 x2F0;
    /* 0x2F4 */
    public f32 x2F4;
    /* 0x2F8 */
    public f32 min_distance;
    /* 0x2FC */
    public f32 max_distance;
    /* 0x300 */
    public s32 x300;
    /* 0x304 */
    public s8 x304;
    /* 0x305 */
    public s8 x305;
    /* 0x306 */
    public s8 x306;
    /* 0x307 */
    public s8 x307;
    /* 0x308 */
    public Vec3 x308;
    /* 0x314 */
    public Vec3 x314;
    /* 0x320 */
    public Vec3 pause_eye_offset; /* offset from focused player */
    /* 0x32C */
    public f32 x32C;
    /* 0x330 */
    public f32 pause_eye_distance; /* distance to focused player */
    /* 0x334 */
    public Vec3 pause_up;          /* up vector */
    /* 0x340 */
    public u8 x340;
    // /* 0x341:0 */ u8 x341_b0 : 1;
    // /* 0x341:1 */ u8 x341_b1_b2 : 2;
    // /* 0x341:3 */ u8 x341_b3_b4 : 2;
    // /* 0x341:5 */ u8 x341_b5_b6 : 2;
    // /* 0x341:7 */ u8 x341_b7 : 1;
    public byte x341_flags; // 1, 2, 2, 2, 1. weird split if you ask me
    /* 0x342 */ fixed byte pad_342[14]; /* maybe part of unk_341[0x57]? another 8 bits */
    /* 0x350 */
    public Vec3 x350;
    // size = 0xC
    // /* 0x35C */ union {
    //    Vec3 vec;
    //    s32 (*cb)(Vec3*);
    // } x35C; 
    fixed byte pad_x35c[0xC];
    /* 0x368 */
    public Vec3 x368;
    /* 0x374 */
    public f32 x374;
    // size = 0x4
    // /* 0x378 */ union {
    //    f32 f32;
    //    s32 s32;
    //} x378;
    fixed byte pad_x378[0x4];
    // /* 0x378 */ f32 x378;
    /* 0x37C */
    public s32 x37C;
    /* 0x380 */
    fixed u8 x380[0x18]; // more padding for unknowns?
    // /* 0x398:0 */ u8 x398_b0 : 1;
    // /* 0x398:1 */ u8 x398_b1 : 1;
    // /* 0x398:2 */ u8 x398_b2 : 1;
    // /* 0x398:3 */ u8 x398_b3 : 1;
    // /* 0x398:4 */ u8 x398_b4 : 1;
    // /* 0x398:5 */ u8 x398_b5 : 1;
    // /* 0x398:6 */ u8 x398_b6_b7 : 2;
    public byte x398_flags;

    ///* 0x399:0 */ u8 x399_b0_b1 : 2;
    // /* 0x399:2 */ u8 x399_b2 : 1;
    // /* 0x399:3 */ u8 x399_b3 : 1;
    // /* 0x399:4 */ u8 x399_b4 : 1;
    // /* 0x399:5 */ u8 x399_b5 : 1;
    // /* 0x399:6 */ u8 x399_b6 : 1;
    // /* 0x399:7 */ u8 x399_b7 : 1;
    public byte x399_flags;

    // /* 0x39A:0 */ u8 x39A_b0 : 1;
    // /* 0x39A:1 */ u8 x39A_b1 : 1;
    // /* 0x39A:2 */ u8 x39A_b2 : 1;
    // /* 0x39A:3 */ u8 x39A_b3 : 1;
    // /* 0x39A:4 */ u8 x39A_b4 : 1;
    // /* 0x39A:5 */ u8 x39A_b5 : 1;
    // /* 0x39A:6 */ u8 x39A_b6 : 1;
    // /* 0x39A:7 */ u8 x39A_b7 : 1;
    public byte x39A_flags;

    byte pad_39b;

    public static Camera GetMeleeCamera() {
        var mem = MeleePointers.STD_CAM_START;

        return Dolphinterop.Read<Camera>(mem);
    }
    /// <summary>
    /// Sets the type of camera melee will use.
    /// </summary>
    /// <param name="type">The kind of camera melee will use to set its render matrices to.</param>
    public static void SetCameraType(CameraType type) {
        // 0x08 = develop camera offset
        Dolphinterop.WriteU8(MeleePointers.CAM_TYPE, (byte)type);
    }
    /// <summary>
    /// A function to set Melee's Develop camera position, focus, and FOV.
    /// </summary>
    /// <param name="eye">The origin of the camera.</param>
    /// <param name="focus">The location for the camera to look at.</param>
    /// <param name="fov">The field-of-view of the camera.</param>
    /// <remarks>This function is typically called from <see cref="MeleeFreeCamera.ApplyToMelee"/>.</remarks>
    public static void SetDevelopCam(Vector3 eye, Vector3 focus, float fov) {
        // the payload of bytes to send into melee's memory
        List<byte> payload = [];

        // important to write the focus first since it's *before* the eye in memory
        payload.AddRange(Dolphinterop.FloatToBigEndian(focus.X));
        payload.AddRange(Dolphinterop.FloatToBigEndian(focus.Y));
        payload.AddRange(Dolphinterop.FloatToBigEndian(focus.Z * -1)); // invert z to match melee

        // eye/origin, written after
        payload.AddRange(Dolphinterop.FloatToBigEndian(eye.X));
        payload.AddRange(Dolphinterop.FloatToBigEndian(eye.Y));
        payload.AddRange(Dolphinterop.FloatToBigEndian(eye.Z * -1));

        // camera fov
        payload.AddRange(Dolphinterop.FloatToBigEndian(fov));

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(Dolphinterop.Handle, (IntPtr)(Dolphinterop.GameCube + MeleePointers.DEVELOP_CAM_START), data, data.Length, out _);
    }

    /// <summary>
    /// Provides a quick and efficient way to change melee's cameras.
    /// </summary>
    public static void QuickManip(RefAction<CObj> manip) {
        var cam = GetMeleeCamera();
        var cgobj = cam.gobj.As<GObj>();
        var cobj = cgobj.hsd_obj.As<CObj>();

        manip.Invoke(ref cobj);
        Dolphinterop.Write(cgobj.hsd_obj, cobj);
    }
}

/// <summary>
/// Describes multiple properties of a <see cref="Camera"/>.
/// </summary>
public struct CameraTransformState {
    public Vec3 interest;
    public Vec3 target_interest;
    public Vec3 position;
    public Vec3 target_position;
    public float fov;
    public float target_fov;

    public override readonly string ToString() => $"[int={interest:F2}, t_int={target_interest:F2}, pos={position:F2}, t_pos={target_position:F2}, fov={fov:F2}, t_fov={target_fov:F2}]";
}

/// <summary>
/// Describes a camera "quake" or "shake."
/// </summary>
public struct CameraQuake {
    public Vec3 amount;
    public int type;
}


// ENUMS

/// <summary>
/// Kinds of cameras used in Melee.
/// </summary>
public enum CameraType {
    Standard = 0,      //< mode used during normal gameplay
    Pause = 1,         //< mode used during pause menu
    TrainingMenu = 2, //< mode used when the training menu is open
    Clear = 3,         //< camera zooms in on the target. gets set when
                              // clearing a stage 1p modes
    Fixed = 4,
    Free = 5,        //< used in training mode, and special melee "Camera Mode"
    BossIntro = 6,  //< used during master/crazy hand match spawn. rotates
                            // around the player then the boss
    DebugFollow = 7, //< follows the player, but can change pos/rotation offset
    DebugFree = 8,
}
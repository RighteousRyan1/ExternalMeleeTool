using System.Numerics;

namespace ExternalMeleeTool.GameComponents; 

/// <summary>
/// Not a direct copy from Camera in Melee. Do not write this struct to memory.
/// </summary>
public struct Camera {
    public GObj_t gobj;
    public CameraType type;

    public float near, far;

    public CameraTransformState transform;

    public Vec2 translation;

    public float pitch_offset;
    public float yaw_offset;

    public static Camera GetMeleeCamera() {
        var camStart = MeleeGlobals.STD_CAM_START;
        var cam = new Camera {
            gobj = Dolphinterop.ReadPtr(camStart),
            type = (CameraType)Dolphinterop.ReadU32(camStart + 0x4),
            near = Dolphinterop.ReadF32(camStart + 0xC),
            far = Dolphinterop.ReadF32(camStart + 0x10),
            transform = Dolphinterop.Read<CameraTransformState>(camStart + 0x14),
            pitch_offset = Dolphinterop.ReadF32(camStart + 0x2C8),
            yaw_offset = Dolphinterop.ReadF32(camStart + 0x2CC)
        };

        return cam;
    }
    /// <summary>
    /// Sets the type of camera melee will use.
    /// </summary>
    /// <param name="type">The kind of camera melee will use to set its render matrices to.</param>
    public static void SetCameraType(CameraType type) {
        // 0x08 = develop camera offset
        Dolphinterop.WriteU8(MeleeGlobals.CAM_TYPE, (byte)type);
    }
    /// <summary>
    /// A function to set Melee's Develop camera position, focus, and FOV.
    /// </summary>
    /// <param name="eye">The origin of the camera.</param>
    /// <param name="focus">The location for the camera to look at.</param>
    /// <param name="fov">The field-of-view of the camera.</param>
    /// <remarks>This function is typically called from <see cref="MeleeFreeCamera.SetCam"/>.</remarks>
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
        SysLib.WriteProcessMemory(Dolphinterop.Handle, (IntPtr)(Dolphinterop.GameCube + MeleeGlobals.DEVELOP_CAM_START), data, data.Length, out _);
    }
}

public struct CameraTransformState {
    public Vec3 interest;
    public Vec3 target_interest;
    public Vec3 position;
    public Vec3 target_position;
    public float fov;
    public float target_fov;

    public override readonly string ToString() => $"[int={interest:F2}, t_int={target_interest:F2}, pos={position:F2}, t_pos={target_position:F2}, fov={fov:F2}, t_fov={target_fov:F2}]";
}


// ENUMS

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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EMTDisplay.Utils;

public enum ViewMode {
    View2D,
    View3D
}

public static class SceneView {
    public static ViewMode Mode = ViewMode.View2D;
    public static bool Is3D => Mode == ViewMode.View3D;

    public const float FOV_Y = MathHelper.PiOver4;
    public const float YAW_DEF = 0.6f;
    public const float PITCH_DEF = 0.3f;
    const float PITCH_MAX = 1.5f; // just under 90 degrees so the up vector never degenerates

    public static bool MatchGameCamera = false;
    public static bool GameCamValid { get; private set; }
    public static Vector3 GameEye { get; private set; }
    public static Vector3 GameInterest { get; private set; }
    public static float GameFovDeg { get; private set; }

    /// <summary>True while the 3D view is actually showing the game's camera.</summary>
    public static bool UsingGameCamera => Mode == ViewMode.View3D && MatchGameCamera && GameCamValid;

    /// <summary>Vertical fov of whatever camera is active this frame (radians).</summary>
    public static float CurrentFovY { get; private set; } = FOV_Y;

    /// <summary>Orbit angles for the 3D camera. Yaw rotates around the vertical axis.</summary>
    public static float Yaw = YAW_DEF;
    public static float Pitch = PITCH_DEF;

    // XNA-space outputs
    public static Matrix View = Matrix.Identity;
    public static Matrix Projection = Matrix.Identity;
    public static Matrix ViewProjection = Matrix.Identity;
    public static Vector3 EyeXna = Vector3.Zero;
    public static Vector3 RightXna = Vector3.UnitX;
    public static Vector3 UpXna = Vector3.UnitY;

    public static Viewport Viewport;
    public static float Zoom = 1f;

    public static Vector3 Target;

    public static Vector3 GetOrbitEyeMelee(Vector3 targetMelee, float viewportHeight, float zoom) {
        zoom = MathF.Max(float.IsFinite(zoom) ? zoom : 1f, 0.0001f);
        float dist = MathF.Max(viewportHeight, 1f) / zoom / (2f * MathF.Tan(FOV_Y / 2f));

        float cp = MathF.Cos(Pitch);
        var dirXna = new Vector3(MathF.Sin(Yaw) * cp, MathF.Sin(Pitch), MathF.Cos(Yaw) * cp);
        return targetMelee + ToXna(dirXna) * dist;
    }

    public static float ZSign = 1f;

    public static Vector3 ToXna(Vector3 melee) => new(melee.X, melee.Y, melee.Z * ZSign);

    public static void Toggle() => SetMode(Is3D ? ViewMode.View2D : ViewMode.View3D);
    public static void SetMode(ViewMode mode) => Mode = mode;

    public static void SetGameCamera(Vector3 eye, Vector3 interest, float fovDeg) {
        GameCamValid = float.IsFinite(eye.X + eye.Y + eye.Z + interest.X + interest.Y + interest.Z + fovDeg)
            && Vector3.DistanceSquared(eye, interest) > 0.0001f
            && fovDeg > 1f && fovDeg < 170f;
        if (!GameCamValid) return;

        GameEye = eye;
        GameInterest = interest;
        GameFovDeg = fovDeg;
    }

    public static bool TryGetOrbitFromCamera(Vector3 eye, Vector3 interest, float viewportHeight,
        out float yaw, out float pitch, out float zoom) {
        yaw = pitch = 0f;
        zoom = 1f;

        var toEye = ToXna(eye - interest);
        float dist = toEye.Length();
        if (!float.IsFinite(dist) || dist < 0.001f) return false;

        var dir = toEye / dist;
        yaw = MathF.Atan2(dir.X, dir.Z);
        pitch = MathHelper.Clamp(MathF.Asin(MathHelper.Clamp(dir.Y, -1f, 1f)), -PITCH_MAX, PITCH_MAX);
        zoom = MathF.Max(viewportHeight, 1f) / (dist * 2f * MathF.Tan(FOV_Y / 2f));
        return float.IsFinite(zoom);
    }

    public static void ResetOrbit() {
        Yaw = YAW_DEF;
        Pitch = PITCH_DEF;
    }

    public static void Orbit(float deltaYaw, float deltaPitch) {
        Yaw += deltaYaw;
        Pitch = MathHelper.Clamp(Pitch + deltaPitch, -PITCH_MAX, PITCH_MAX);
        Yaw = MathHelper.WrapAngle(Yaw);
    }

    public static Vector3 ScreenDragToWorld(float dxPixels, float dyPixels, float zoom) {
        var d = (-RightXna * dxPixels + UpXna * dyPixels) / MathF.Max(zoom, 0.0001f);
        return new Vector3(d.X, d.Y, d.Z * ZSign);
    }

    public static void Update(Viewport viewport, Vector2 target2D, float targetZ, float zoom) {
        Viewport = viewport;
        Zoom = MathF.Max(float.IsFinite(zoom) ? zoom : 1f, 0.0001f);
        Target = new Vector3(target2D, targetZ);

        float w = MathF.Max(viewport.Width, 1);
        float h = MathF.Max(viewport.Height, 1);

        if (Mode == ViewMode.View2D) {
            float hw = w / (2f * Zoom);
            float hh = h / (2f * Zoom);

            // Melee Y is up and so is XNA Y, so no flip is needed here
            Projection = Matrix.CreateOrthographicOffCenter(
                target2D.X - hw, target2D.X + hw,
                target2D.Y - hh, target2D.Y + hh,
                -10000f, 10000f);
            CurrentFovY = FOV_Y;
            View = Matrix.Identity;
            RightXna = Vector3.UnitX;
            UpXna = Vector3.UnitY;
            EyeXna = new Vector3(target2D.X, target2D.Y, 1000f);
        }
        else {
            float fovY = FOV_Y;
            Vector3 targetXna;

            if (UsingGameCamera) {
                // exactly what Melee is rendering (keeps the game's fov; roll is ignored)
                Target = GameInterest;
                targetXna = ToXna(GameInterest);
                EyeXna = ToXna(GameEye);
                fovY = MathHelper.ToRadians(GameFovDeg);
            }
            else {
                targetXna = ToXna(Target);
                EyeXna = ToXna(GetOrbitEyeMelee(Target, h, Zoom));
            }

            CurrentFovY = fovY;
            View = Matrix.CreateLookAt(EyeXna, targetXna, Vector3.UnitY);
            Projection = Matrix.CreatePerspectiveFieldOfView(fovY, w / h, 0.1f, 100000f);

            RightXna = new Vector3(View.M11, View.M21, View.M31);
            UpXna = new Vector3(View.M12, View.M22, View.M32);
        }

        ViewProjection = View * Projection;
    }

    public static bool Project(Vector3 melee, out Vector2 screen, out float pixelsPerUnit) {
        var clip = Vector4.Transform(ToXna(melee), ViewProjection);

        screen = default;
        pixelsPerUnit = Zoom;

        if (Mode == ViewMode.View3D) {
            if (clip.W <= 0.001f) return false;
            pixelsPerUnit = Viewport.Height / (2f * MathF.Tan(CurrentFovY / 2f) * clip.W);
        }

        float invW = 1f / clip.W;
        float ndcX = clip.X * invW;
        float ndcY = clip.Y * invW;

        screen = new Vector2(
            (ndcX * 0.5f + 0.5f) * Viewport.Width,
            (1f - (ndcY * 0.5f + 0.5f)) * Viewport.Height);

        return float.IsFinite(screen.X) && float.IsFinite(screen.Y);
    }
}

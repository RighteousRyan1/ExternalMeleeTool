using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Fighter;
using System.Numerics;

namespace ExternalMeleeTool.Melee.Mechanics;

public readonly record struct DiInput(Vector2 lstick, Vector2 cstick, bool lr);

/// <summary>
/// Everything about DI ported from the decomp (AI assisted)
/// </summary>
public static class KnockbackInfluence {
    const float DegToRad = MathF.PI / 180f;

    // uses fallback constants
    const float DefMinMag = 0.7f, DefSdiScale = 6f, DefAsdiScale = 3f, DefTdiDeg = 18f, DefLrMult = 0.95f;
    static bool HavePlCo => !FighterData.PlCo.Equals(default);
    public static float MinStickMag => HavePlCo ? FighterData.PlCo.sdi_min_stick_mag : DefMinMag;
    public static float SdiScale => HavePlCo ? FighterData.PlCo.sdi_pos_scale : DefSdiScale;
    public static float AsdiScale => HavePlCo ? FighterData.PlCo.asdi_pos_scale : DefAsdiScale;
    public static float TdiAngleRad => (HavePlCo ? FighterData.PlCo.tdi_angle_degs : DefTdiDeg) * DegToRad;
    public static float LrKbMult => HavePlCo ? FighterData.PlCo.lr_hold_kb_mult : DefLrMult;
    public static int SdiWindow => HavePlCo ? FighterData.PlCo.sdi_stick_window : 4;

    /// <summary>
    /// SDI for ONE hitlag frame. <paramref name="stickAgeX"/>/<paramref name="stickAgeY"/> are how many frames the
    /// stick has been held on each axis (the game requires either to be &lt; the window, i.e. a fresh flick).
    /// </summary>
    public static void ApplySDI(ref Vec3 pos, Vector2 lstick, int stickAgeX, int stickAgeY) {
        float mag = MinStickMag;
        if (lstick.LengthSquared() < mag * mag) return;
        int win = SdiWindow;
        if (!(stickAgeX < win || stickAgeY < win)) return;
        pos.X += lstick.X * SdiScale;
        pos.Y += lstick.Y * SdiScale;
    }

    /// <summary>ASDI at hitlag exit. C-stick takes priority if it passes the magnitude gate.</summary>
    public static void ApplyASDI(ref Vec3 pos, Vector2 lstick, Vector2 cstick) {
        float mag = MinStickMag;
        bool cOk = cstick.LengthSquared() >= mag * mag;
        bool lOk = lstick.LengthSquared() >= mag * mag;
        if (!cOk && !lOk) return;
        var s = cOk ? cstick : lstick;
        pos.X += s.X * AsdiScale;
        pos.Y += s.Y * AsdiScale;
    }

    /// <summary>
    /// Trajectory DI (ftCo_8008E5A4). The kb angle is rotated by tdi_angle_degs * sign * cross^2 / |kb|^2,
    /// where cross = kb x stick (z component). Fixed vs. old code: that used (-kb.X, kb.Y) as the "perpendicular",
    /// which isn't a perpendicular and gave wrong signs/magnitudes.
    /// </summary>
    public static void ApplyTDI(ref Vec3 kb, Vector2 lstick) {
        if (lstick.X == 0 && lstick.Y == 0) return;
        float magSq = kb.X * kb.X + kb.Y * kb.Y;
        if (magSq < 0.00001f) return;
        float cross = kb.X * lstick.Y - kb.Y * lstick.X;
        float amount = cross * cross / magSq;
        if (cross < 0) amount = -amount;
        float len = MathF.Sqrt(magSq);
        float ang = MathF.Atan2(kb.Y, kb.X) + TdiAngleRad * amount;
        kb.X = len * MathF.Cos(ang);
        kb.Y = len * MathF.Sin(ang);
    }

    /// <summary>
    /// If L or R is held when hitlag ends, kb *= x1AC (0.95).
    /// There is no animation-state or tech-lockout condition in the real code.
    /// </summary>
    public static void ApplyVCancel(ref Vec3 kb, bool lrHeld) {
        if (!lrHeld) return;
        if (kb.X == 0 && kb.Y == 0) return;
        float m = LrKbMult;
        kb.X *= m;
        kb.Y *= m;
    }

    /// <summary>Everything the game does when hitlag ends, in order: ASDI, TDI, V-cancel.</summary>
    public static void ApplyHitlagExit(ref Vec3 pos, ref Vec3 kb, in DiInput input) {
        ApplyASDI(ref pos, input.lstick, input.cstick);
        ApplyTDI(ref kb, input.lstick);
        ApplyVCancel(ref kb, input.lr);
    }

    public static void ApplyCrouchCancel(ref Vec3 kb, in FighterData fd) {
        if (fd.AnimState == FtAnimState.Squat || fd.AnimState == FtAnimState.SquatWait) {
            float mult = FighterData.PlCo.Equals(default) ? 0.66666666f : FighterData.PlCo.crouch_kb_mult;
            kb.X *= mult;
            kb.Y *= mult;
            kb.Z *= mult;
        }
    }
}

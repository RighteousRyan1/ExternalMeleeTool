using ExternalMeleeTool.GameComponents;
using System.Numerics;

namespace ExternalMeleeTool.Melee.Mechanics;

public enum DIDirection { 
    None,
    Current, 
    DIIn, 
    DIOut 
}

public struct TrajectoryResult {
    public List<Vector3> Points;
    /// <summary>Index into <see cref="Points"/> where hitlag ends, -1 if not.</summary>
    public int HitlagEndIndex;
    /// <summary>Index into <see cref="Points"/> where hitstun ends, -1 if not.</summary>
    public int HitstunEndIndex;
    /// <summary>Index into <see cref="Points"/> of the point where the fighter lands on a floor, -1 if not.</summary>
    public int LandingIndex;
    /// <summary>Knockback after hitlag-exit modifications.</summary>
    public Vec3 FinalKb;
    public Vector2 Stick;
}

/// <summary>
/// Accounts for ASDI, TDI, V-Cancel, SDI, CC, KB Decay.
/// Thanks to TM-CE repo for having what I needed
/// </summary>
public static class TrajectorySim {
    /// <summary>The stick vector TM-CE uses for DI in/out, which is perpendicular to KB.</summary>
    public static Vector2 DiStick(Vec3 kb, float opponentDir, bool towardAttacker) {
        float kbAngle = MathF.Atan2(kb.Y, kb.X);
        while (kbAngle < 0) kbAngle += MathF.Tau;
        while (kbAngle >= MathF.Tau) kbAngle -= MathF.Tau;
        float dir = opponentDir < 0 ? -1f : 1f;
        float quarter = MathF.PI * 0.5f * dir;
        float ang;
        // DI in
        if (towardAttacker) ang = kbAngle <= MathF.PI ? kbAngle - quarter : kbAngle + quarter;
        // DI out
        else ang = kbAngle <= MathF.PI ? kbAngle + quarter : kbAngle - quarter;
        return new Vector2(MathF.Cos(ang), MathF.Sin(ang));
    }

    public static TrajectoryResult Simulate(in FighterData fd, DIDirection kind, float opponentDir, int maxFrames = 120,
        Func<Vector2, Vector2, Vector2?> findLanding = null) {
        var res = new TrajectoryResult { Points = new List<Vector3>(maxFrames + 2), HitlagEndIndex = -1, HitstunEndIndex = -1, LandingIndex = -1 };

        var pos = fd.Position;
        var kb = fd.Knockback;
        var self = fd.VelocitySelf;
        float hitlag = fd.HitlagFrames;
        float hitstun = fd.HitstunFrames;
        bool lr = fd.Input.Held.HasFlag(HSDPadButton.TriggerL) || fd.Input.Held.HasFlag(HSDPadButton.TriggerR);
        var input = kind switch {
            DIDirection.DIIn => new DiInput(DiStick(kb, opponentDir, true), Vector2.Zero, lr),
            DIDirection.DIOut => new DiInput(DiStick(kb, opponentDir, false), Vector2.Zero, lr),
            DIDirection.None => new DiInput(Vec2.Zero, Vector2.Zero, false),
            _ => new DiInput(fd.Input.LeftStick, fd.Input.CStick, lr),
        };
        res.Stick = input.lstick;

        var a = fd.Attr;
        float decay = FighterData.PlCo.Equals(default) ? 0.051f : FighterData.PlCo.x204_knockbackFrameDecay;

        res.Points.Add(new Vector3(pos.X, pos.Y, pos.Z));

        bool sdiDone = false;
        bool inHitlag = hitlag > 0;
        for (int f = 0; f < maxFrames; f++) {
            if (inHitlag) {
                // sdi
                if (!sdiDone && kind == DIDirection.Current && fd.AllowSDI) {
                    KnockbackInfluence.ApplySDI(ref pos, input.lstick, 0, 0);
                    sdiDone = true;
                }
                hitlag -= 1f;
                if (hitlag <= 0f) {
                    inHitlag = false;
                    if (kind == DIDirection.Current) KnockbackInfluence.ApplyHitlagExit(ref pos, ref kb, input);
                    else {
                        KnockbackInfluence.ApplyTDI(ref kb, input.lstick);
                        KnockbackInfluence.ApplyVCancel(ref kb, input.lr);
                    }
                    res.HitlagEndIndex = res.Points.Count;
                }
                res.Points.Add(new Vector3(pos.X, pos.Y, pos.Z));
                continue;
            }

            // gravity
            self.Y -= a.grav;
            if (self.Y < -a.terminal_vel) self.Y = -a.terminal_vel;

            // ftCommon_CalcSelfAccel_DriftFrom for air accel
            float lsx = input.lstick.X;
            float target = lsx * a.air_drift_max;
            float accel;
            if (target == 0f) {
                // deaccel, aerial_friction
                float v = self.X;
                accel = MathF.Abs(v) <= a.aerial_friction ? -v : -MathF.Sign(v) * a.aerial_friction;
            } else {
                accel = lsx * a.air_drift_stick_mul + (lsx > 0 ? a.aerial_drift_base : -a.aerial_drift_base);
                float v = self.X;
                if (!(v * accel < 0f)) {
                    if (accel > 0) {
                        if (v + accel > target) {
                            accel = -a.aerial_friction;
                            if (v + accel < target) accel = target - v;
                        }
                    } else if (v + accel < target) {
                        accel = a.aerial_friction;
                        if (v + accel > target) accel = target - v;
                    }
                }
            }
            self.X += accel;

            // ft.x204 = kb decay
            float kbLen = MathF.Sqrt(kb.X * kb.X + kb.Y * kb.Y);
            if (kbLen < decay) { kb.X = 0; kb.Y = 0; }
            else {
                float ang = MathF.Atan2(kb.Y, kb.X);
                kb.X -= decay * MathF.Cos(ang);
                kb.Y -= decay * MathF.Sin(ang);
            }

            var prev = new Vector2(pos.X, pos.Y);
            pos.X += self.X + kb.X;
            pos.Y += self.Y + kb.Y;
            if (findLanding != null && f > 0 && (self.Y + kb.Y) < 0f) {
                var hit = findLanding(prev, new Vector2(pos.X, pos.Y));
                if (hit.HasValue) {
                    res.Points.Add(new Vector3(hit.Value.X, hit.Value.Y, pos.Z));
                    res.LandingIndex = res.Points.Count - 1;
                    break;
                }
            }
            res.Points.Add(new Vector3(pos.X, pos.Y, pos.Z));

            if (hitstun > 0 && --hitstun <= 0) res.HitstunEndIndex = res.Points.Count - 1;
        }

        res.FinalKb = kb;
        return res;
    }
}

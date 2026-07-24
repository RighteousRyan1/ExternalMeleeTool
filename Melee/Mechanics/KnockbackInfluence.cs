using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Fighter;
using System.Numerics;

namespace ExternalMeleeTool.Melee.Mechanics; 
public class KnockbackInfluence {
    public static void ApplySDI(ref Vec3 simPos, in FighterData fd, int stickXHoldTime, int stickYHoldTime) {
        float stickMag = fd.Input.LeftStick.Length();

        if (stickMag >= 0.7f && (stickXHoldTime < 4 || stickYHoldTime < 4)) {
            float sdiDist = FighterData.PlCo.Equals(default) ? 6f : FighterData.PlCo.sdi_dist;
            simPos.X += fd.Input.LeftStick.X * sdiDist;
            simPos.Y += fd.Input.LeftStick.Y * sdiDist;
        }
    }

    public static void ApplyTDI(ref Vec3 simKb, in FighterData fd) {
        Vector2 stick = fd.Input.LeftStick;
        Vector2 kb = new Vector2(simKb.X, simKb.Y);

        float magSq = kb.LengthSquared();
        if (magSq == 0) return;

        Vector2 perpKb = new Vector2(-kb.X, kb.Y);
        float cross = (perpKb.X * stick.Y) - (perpKb.Y * stick.X);

        float tdiAmount = (cross * cross) / magSq;
        if (cross < 0) tdiAmount *= -1f;

        float currentAngle = MathF.Atan2(kb.Y, kb.X);
        float maxTdiAngle = FighterData.PlCo.Equals(default) ? (18f * MathF.PI / 180f) : FighterData.PlCo.tdi_angle;
        float newAngle = currentAngle + (maxTdiAngle * tdiAmount);

        float kbLength = kb.Length();
        simKb.X = MathF.Cos(newAngle) * kbLength;
        simKb.Y = MathF.Sin(newAngle) * kbLength;
    }

    public static void ApplyASDI(ref Vec3 simPos, in FighterData fd) {
        float cstickMag = fd.Input.CStick.Length();
        float stickMag = fd.Input.LeftStick.Length();

        float asdiDist = FighterData.PlCo.Equals(default) ? 3f : FighterData.PlCo.asdi_dist;

        if (cstickMag >= 0.7f) {
            simPos.X += fd.Input.CStick.X * asdiDist;
            simPos.Y += fd.Input.CStick.Y * asdiDist;
        }
        else if (stickMag >= 0.7f) {
            simPos.X += fd.Input.LeftStick.X * asdiDist;
            simPos.Y += fd.Input.LeftStick.Y * asdiDist;
        }
    }

    public static void ApplyVCancel(ref Vec3 simKb, in FighterData fd, int framesSinceLRPress, bool isTechLockedOut) {
        if (fd.Grounded) return;

        bool isValidState = fd.AnimState switch {
            FtAnimState.EscapeAir => true,
            FtAnimState.JumpF => true,
            FtAnimState.JumpB => true,
            FtAnimState.JumpAerialF => true,
            FtAnimState.JumpAerialB => true,
            FtAnimState.Fall => true,
            FtAnimState.FallF => true,
            FtAnimState.FallB => true,
            FtAnimState.FallAerial => true,
            FtAnimState.FallAerialF => true,
            FtAnimState.FallAerialB => true,
            FtAnimState.FallSpecial => true,
            FtAnimState.FallSpecialF => true,
            FtAnimState.FallSpecialB => true,
            FtAnimState.DamageFall => true,
            _ => false
        };

        if (isValidState && framesSinceLRPress <= 2 && !isTechLockedOut) {
            float mult = FighterData.PlCo.Equals(default) ? 0.95f : FighterData.PlCo.v_cancel_kb_mult;
            simKb.X *= mult;
            simKb.Y *= mult;
            simKb.Z *= mult;
        }
    }

    public static void ApplyCrouchCancel(ref Vec3 simKb, in FighterData fd) {
        if (fd.AnimState == FtAnimState.Squat || fd.AnimState == FtAnimState.SquatWait) {
            float mult = FighterData.PlCo.Equals(default) ? 0.66666666f : FighterData.PlCo.crouch_kb_mult;
            simKb.X *= mult;
            simKb.Y *= mult;
            simKb.Z *= mult;
        }
    }
}

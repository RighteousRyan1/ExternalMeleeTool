using System.Numerics;

namespace ExternalMeleeTool.MeleeTypes; 
public struct Matrix3x4 {
    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;

    public Vector3 Translation {
        // negative z because that's how melee works
        readonly get => new(M14, M24, -M34);
        set {
            M14 = value.X;
            M24 = value.Y;
            M34 = value.Z;
        }
    }

    // other entries are currently unknown

    public override readonly string ToString() {
        return
            $"[{M11,8:F3} {M12,8:F3} {M13,8:F3} {M14,8:F3}]\n" +
            $"[{M21,8:F3} {M22,8:F3} {M23,8:F3} {M24,8:F3}]\n" +
            $"[{M31,8:F3} {M32,8:F3} {M33,8:F3} {M34,8:F3}]";
    }
}

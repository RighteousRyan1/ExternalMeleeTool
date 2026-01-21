using System.Numerics;

namespace ExternalMeleeTool.Melee; 
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

    // diagonal = scale?
    // this is uncertain
    /*public Vector3 Scale {
        readonly get => new(M11, M22, M33);
        set {
            M11 = value.X;
            M22 = value.Y;
            M33 = value.Z;
        }
    }*/

    public Quaternion Rotation {
        readonly get {
            // normalize columns in case there's scaling
            var col0 = new Vector3(M11, M21, M31);
            var col1 = new Vector3(M12, M22, M32);
            var col2 = new Vector3(M13, M23, M33);

            col0 = Vector3.Normalize(col0);
            col1 = Vector3.Normalize(col1);
            col2 = Vector3.Normalize(col2);

            // Rebuild 3x3 rotation matrix
            Matrix4x4 rot = new(
                col0.X, col1.X, col2.X, 0f,
                col0.Y, col1.Y, col2.Y, 0f,
                col0.Z, col1.Z, col2.Z, 0f,
                0f,     0f,     0f,     1f
            );

            return Quaternion.CreateFromRotationMatrix(rot);
        }
        set {
            // Convert quaternion back to rotation matrix
            Matrix4x4 rot = Matrix4x4.CreateFromQuaternion(value);
            M11 = rot.M11; M12 = rot.M12; M13 = rot.M13;
            M21 = rot.M21; M22 = rot.M22; M23 = rot.M23;
            M31 = rot.M31; M32 = rot.M32; M33 = rot.M33;
        }
    }

    static readonly Matrix3x4 _identity = new() {
        M11 = 1f, M12 = 0f, M13 = 0f, M14 = 0f,
        M21 = 0f, M22 = 1f, M23 = 0f, M24 = 0f,
        M31 = 0f, M32 = 0f, M33 = 1f, M34 = 0f
    };
    public static Matrix3x4 Identity => _identity;
    public override readonly string ToString() {
        return
            $"[{M11,8:F3} {M12,8:F3} {M13,8:F3} {M14,8:F3}]\n" +
            $"[{M21,8:F3} {M22,8:F3} {M23,8:F3} {M24,8:F3}]\n" +
            $"[{M31,8:F3} {M32,8:F3} {M33,8:F3} {M34,8:F3}]";
    }
}

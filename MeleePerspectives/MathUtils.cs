using System;
using System.Numerics;

namespace MeleeThirdPerson;
public static class MathUtils {
    public static float Lerp(float a, float b, float t) {
        return a + (b - a) * t;
    }
    public static float InverseLerp(float a, float b, float v) {
        if (a == b) return 0f;
        return Clamp((v - a) / (b - a), 0, 1);
    }
    public static float Clamp(float val, float min, float max) {
        if (val < min) return min;
        if (val > max) return max;
        return val;
    }

    public static float ToRadians(float degrees) => degrees * (MathF.PI / 180f);
    public static float ToDegrees(float radians) => radians * (180f / MathF.PI);

    public static Vector2 FlattenZ(this Vector3 vector) => new(vector.X, vector.Z);
    public static Vector3 ExpandZ(this Vector2 vector) => new(vector.X, 0, vector.Y);

    public static Vector3 Rotate(this Vector3 vector, Vector3 axis, float angleInDegrees) {
        // Convert degrees to radians
        float radians = angleInDegrees * (MathF.PI / 180f);

        // Create the rotation quaternion
        // Note: Axis must be normalized
        Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), radians);

        // Transform the vector by the rotation
        return Vector3.Transform(vector, rotation);
    }
}
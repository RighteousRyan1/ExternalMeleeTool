using System.Numerics;

namespace ExternalMeleeTool.Utilities; 
public static class VectorExtensions {
    public static Vector2 Rotate(this Vector2 spinPoint, float radians, Vector2 center = default) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        Vector2 newPoint = spinPoint - center;
        Vector2 result = center;
        result.X += newPoint.X * cos - newPoint.Y * sin;
        result.Y += newPoint.X * sin + newPoint.Y * cos;
        return result;
    }

    public static Vector2 XY(this Vector3 vec) => new(vec.X, vec.Y);
}

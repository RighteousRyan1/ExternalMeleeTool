using System.Drawing;
using System.Numerics;

namespace ExternalMeleeTool.Melee;

public static class MathConstants {
    public const float DEG_TO_RAD = MathF.PI / 180f;
    public const float RAD_TO_DEG = 180f / MathF.PI;
}

public struct BoundingRect(float left, float top, float right, float bottom) {
    public float Left = left;
    public float Right = right;
    public float Top = top;
    public float Bottom = bottom;

    public static BoundingRect operator *(BoundingRect left, float scalar) {
        return new BoundingRect(
            left.Left * scalar,
            left.Top * scalar,
            left.Right * scalar,
            left.Bottom * scalar
        );
    }
    public static BoundingRect operator /(BoundingRect left, float scalar) {
        return new BoundingRect(
            left.Left / scalar,
            left.Top / scalar,
            left.Right / scalar,
            left.Bottom / scalar
        );
    }
}

public struct ECB(Vector2 top, Vector2 bottom, Vector2 right, Vector2 left) {
    public Vector2 Top = top;
    public Vector2 Bottom = bottom;
    public Vector2 Right = right;
    public Vector2 Left = left;

    public readonly bool Contains(Point point) {
        return point.X >= Left.X &&
               point.X <= Right.X &&
               point.Y >= Top.Y &&
               point.Y <= Bottom.Y;
    }
    public readonly bool Contains(float x, float y) {
        return x >= Left.X &&
               x <= Right.X &&
               y >= Top.Y &&
               y <= Bottom.Y;
    }

    public readonly Vector2 Center => (Top + Bottom + Left + Right) / 4;

    public override readonly string ToString() => $"[Top={Top:F2}, Right={Right:F2}, Bottom={Bottom:F2}, Left={Left:F2}";
}   

/// <summary>Describes an <see cref="ECB"/> but with offsets described from an implicit center.</summary>
public struct LocalECB(float top, float bottom, float right, float left) {
    public float Top = top;
    public float Bottom = bottom;
    public float Right = right;
    public float Left = left;

    public readonly ECB GetVectorDescribed() {
        return new ECB(
            new Vector2(0, Top),
            new Vector2(0, Bottom),
            new Vector2(Right, 0),
            new Vector2(Left, 0)
        );
    }

    public override readonly string ToString() => $"[Top={Top:F2}, Right={Right:F2}, Bottom={Bottom:F2}, Left={Left:F2}";
}
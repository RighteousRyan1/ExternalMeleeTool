using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EMTDisplay.Utils;

public enum Anchor {
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    TopCenter,
    BottomCenter,
    Center,
    LeftCenter,
    RightCenter,
}
public static class RenderUtils
{
    public static Vector2 Size(this Texture2D tex) => new(tex.Width, tex.Height);
    public static Vector2 GetAnchor(this Anchor a, Texture2D tex) {
        return a switch {
            Anchor.TopLeft => Vector2.Zero,
            Anchor.TopRight => new(tex.Width, 0),
            Anchor.BottomLeft => new(0, tex.Height),
            Anchor.BottomRight => new(tex.Width, tex.Height),
            Anchor.LeftCenter => new(0, tex.Height / 2),
            Anchor.RightCenter => new(tex.Width, tex.Height / 2),
            Anchor.Center => new(tex.Width / 2, tex.Height / 2),
            Anchor.TopCenter => new(tex.Width / 2, 0),
            Anchor.BottomCenter => new(tex.Width / 2, tex.Height),
            _ => default,
        };
    }
    public static Vector2 GetAnchor(this Anchor a, Vector2 vector) {
        return a switch {
            Anchor.TopLeft => Vector2.Zero,
            Anchor.TopRight => new(vector.X, 0),
            Anchor.BottomLeft => new(0, vector.Y),
            Anchor.BottomRight => new(vector.X, vector.Y),
            Anchor.LeftCenter => new(0, vector.Y / 2),
            Anchor.RightCenter => new(vector.X, vector.Y / 2),
            Anchor.Center => new(vector.X / 2, vector.Y / 2),
            Anchor.TopCenter => new(vector.X / 2, 0),
            Anchor.BottomCenter => new(vector.X / 2, vector.Y),
            _ => default,
        };
    }
}

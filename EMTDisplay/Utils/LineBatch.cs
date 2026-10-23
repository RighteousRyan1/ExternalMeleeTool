using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EMTDisplay.Utils;


// this was done by claude because i am so bad at graphics.

/// <summary>
/// Batches world-space lines and filled triangles and draws them with the current
/// <see cref="SceneView"/> matrices. Used for both the 2D (orthographic) and 3D (perspective)
/// views, so every shape only has to be described once. Lines are 1 pixel wide. Nothing is
/// depth tested, which keeps overlapping debug geometry (hitboxes inside hurtboxes etc.) visible.
///
/// Triangles are always flushed before lines, so outlines stay on top of fills.
/// Colors are premultiplied (XNA convention), so fade a color with <c>color * alpha</c>.
///
/// Positions are given in Melee space; the Z flip to XNA space happens here.
/// </summary>
public static class LineBatch {
    // must be a multiple of 2 (lines) and of 3 (triangles)
    const int MaxLineVerts = 1 << 15;
    const int MaxTriVerts = 3 * 8192;

    static readonly VertexPositionColor[] _lineVerts = new VertexPositionColor[MaxLineVerts];
    static readonly VertexPositionColor[] _triVerts = new VertexPositionColor[MaxTriVerts];
    static int _lineCount;
    static int _triCount;
    static BasicEffect _fx;
    static GraphicsDevice _gd;
    static bool _active;

    /// <summary>Lines submitted since the last Begin (handy for stats).</summary>
    public static int LineCount { get; private set; }
    /// <summary>Triangles submitted since the last Begin.</summary>
    public static int TriangleCount { get; private set; }

    public static void Begin(GraphicsDevice gd) {
        _gd = gd;
        _fx ??= new BasicEffect(gd) {
            VertexColorEnabled = true,
            TextureEnabled = false,
            LightingEnabled = false,
            World = Matrix.Identity
        };

        _fx.View = SceneView.View;
        _fx.Projection = SceneView.Projection;

        gd.BlendState = BlendState.AlphaBlend; // XNA colors are premultiplied
        gd.DepthStencilState = DepthStencilState.None;
        gd.RasterizerState = RasterizerState.CullNone;

        _lineCount = 0;
        _triCount = 0;
        LineCount = 0;
        TriangleCount = 0;
        _active = true;
    }

    public static void Line(Vector3 a, Vector3 b, Color color) {
        if (!_active) return;

        // NaN/Inf from garbage game memory would otherwise poison the whole batch
        if (!float.IsFinite(a.X + a.Y + a.Z + b.X + b.Y + b.Z)) return;

        if (_lineCount + 2 > MaxLineVerts) Flush();

        _lineVerts[_lineCount++] = new VertexPositionColor(SceneView.ToXna(a), color);
        _lineVerts[_lineCount++] = new VertexPositionColor(SceneView.ToXna(b), color);
        LineCount++;
    }

    public static void Triangle(Vector3 a, Vector3 b, Vector3 c, Color color) {
        if (!_active) return;

        if (!float.IsFinite(a.X + a.Y + a.Z + b.X + b.Y + b.Z + c.X + c.Y + c.Z)) return;

        if (_triCount + 3 > MaxTriVerts) Flush();

        _triVerts[_triCount++] = new VertexPositionColor(SceneView.ToXna(a), color);
        _triVerts[_triCount++] = new VertexPositionColor(SceneView.ToXna(b), color);
        _triVerts[_triCount++] = new VertexPositionColor(SceneView.ToXna(c), color);
        TriangleCount++;
    }

    public static void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color) {
        Triangle(a, b, c, color);
        Triangle(a, c, d, color);
    }

    // fills first, then lines, so outlines are never hidden under translucent fills
    static void Flush() {
        if (_triCount > 0) {
            _fx.CurrentTechnique.Passes[0].Apply();
            _gd.DrawUserPrimitives(PrimitiveType.TriangleList, _triVerts, 0, _triCount / 3);
            _triCount = 0;
        }
        if (_lineCount > 0) {
            _fx.CurrentTechnique.Passes[0].Apply();
            _gd.DrawUserPrimitives(PrimitiveType.LineList, _lineVerts, 0, _lineCount / 2);
            _lineCount = 0;
        }
    }

    public static void End() {
        if (!_active) return;
        Flush();
        _active = false;
    }
}

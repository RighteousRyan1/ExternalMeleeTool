using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Melee.Mechanics;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using static EMTDisplay.Utils.MeleeDrawing;

namespace EMTDisplay.Utils;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    IncludeFields = true)]
[JsonSerializable(typeof(DrawParams))]
internal partial class DrawParamsJsonContext : JsonSerializerContext;
public static class MeleeDrawing {
    public struct DrawParams {
        public bool DrawECBs = true;
        public bool DrawHitboxes = true;
        public bool DrawHurtboxes = true;
        public bool DrawShields = true;
        public bool DrawLedgeGrabBoxes = true;
        public bool DrawStatsForNerdsPlayer = true;
        public bool DrawStatsForNerdsItem = false;
        public bool Extrude3D = true;
        public bool DrawCenterLines = true;
        public bool DrawPrediction = true;
        public int PredictionFrames = 120;
        public bool FillMapLines = false;
        public bool FillHitboxes = false;
        public bool FillHurtboxes = false;
        public bool OutlineFilledBoxes = false;
        public float FillAlphaMap = 0.25f;
        public float FillAlphaBoxes = 0.35f;
        public float ExtrudeDepth = 12f;
        public bool ShowUnpopulatedBoxes = false;
        public bool ShowInactiveLines = false;
        public int DrawSchema = 0;
        public DrawParams() { }
    }
    public static DrawParams Params = new();
    const float ExtrudeFade = 0.35f;

    static bool IsInvalidCapsule(HitCapsule cap) => cap.state > HitCapsuleState.Wait;
    static bool IsInvalidCapsule(HurtCapsule cap) => cap.state > HurtCapsuleState.Intangible;

    static string[] _infoArr;

    public static Texture2D WhitePixel;

    public static SpriteFontBase MeleeFont;
    public static SpriteFontBase Cascadia;
    static readonly FontSystem _fs;
    static readonly FontSystem _fs2;
    static MeleeDrawing() {
        WhitePixel = new Texture2D(EMTDisplay.Graphics.GraphicsDevice, 1, 1);
        WhitePixel.SetData([Color.White]);

        _fs = new();
        _fs2 = new();
        _fs.AddFont(File.Open("Content/melee_font.ttf", FileMode.Open));
        MeleeFont = _fs.GetFont(30);
        _fs2.AddFont(File.Open("Content/cascadia.ttf", FileMode.Open));
        Cascadia = _fs2.GetFont(30);
    }

    #region Text (billboarded, screen space)

    struct TextItem {
        public SpriteFontBase Font;
        public string Text;
        public Vector3 Pos;
        public Color Color;
        public float WorldScale;
        public Anchor Anchor;
        public float Rotation;
    }
    static readonly List<TextItem> _text = [];

    public static void Text(SpriteFontBase font, string text, Vector3 pos, Color color, float worldScale,
        Anchor anchor = Anchor.TopLeft, float rotation = 0f) {
        _text.Add(new TextItem {
            Font = font, Text = text, Pos = pos, Color = color,
            WorldScale = worldScale, Anchor = anchor, Rotation = rotation
        });
    }

    public static void FlushText() {
        if (_text.Count == 0) return;

        var sb = EMTDisplay.SpriteBatch;
        sb.Begin();
        try {
            var vp = SceneView.Viewport;
            for (int i = 0; i < _text.Count; i++) {
                var t = _text[i];

                if (!SceneView.Project(t.Pos, out var screen, out var ppu)) continue;

                float scale = t.WorldScale * ppu;
                float approxPixelHeight = scale * 30f; // fonts are created at size 30
                // too small to read / absurdly large (very close to the 3D camera)
                if (approxPixelHeight < 3f || approxPixelHeight > 600f) continue;
                if (screen.X < -400 || screen.Y < -400 || screen.X > vp.Width + 400 || screen.Y > vp.Height + 400) continue;

                sb.DrawString(t.Font, t.Text, screen, t.Color,
                    rotation: t.Rotation,
                    origin: t.Anchor.GetAnchor(t.Font.MeasureString(t.Text)),
                    scale: new Vector2(scale));
            }
        }
        finally {
            sb.End();
            _text.Clear();
        }
    }

    public static void DiscardText() => _text.Clear();

    #endregion

    #region Primitives

    public static void DrawLine(Vector3 start, Vector3 end, Color color) => LineBatch.Line(start, end, color);

    public static void DrawArc(Vector3 center, Vector3 u, Vector3 v, float radius, float startAngle, float endAngle,
        Color color, int segments = 16) {
        float step = (endAngle - startAngle) / segments;
        var prev = center + (u * MathF.Cos(startAngle) + v * MathF.Sin(startAngle)) * radius;

        for (int i = 1; i <= segments; i++) {
            float a = startAngle + i * step;
            var next = center + (u * MathF.Cos(a) + v * MathF.Sin(a)) * radius;
            LineBatch.Line(prev, next, color);
            prev = next;
        }
    }

    public static void DrawRing(Vector3 center, Vector3 u, Vector3 v, float radius, Color color, int segments = 32)
        => DrawArc(center, u, v, radius, 0f, MathHelper.TwoPi, color, segments);

    /// <summary>Three perpendicular great circles.</summary>
    public static void DrawSphereOutline(Vector3 center, float radius, Color color, int segments = 32) {
        DrawRing(center, Vector3.UnitX, Vector3.UnitY, radius, color, segments);
        DrawRing(center, Vector3.UnitX, Vector3.UnitZ, radius, color, segments);
        DrawRing(center, Vector3.UnitY, Vector3.UnitZ, radius, color, segments);
    }

    /// <summary>A circle in 2D, a sphere in 3D.</summary>
    public static void DrawBubble(Vector3 center, float radius, Color color, int segments = 32) {
        if (SceneView.Is3D) DrawSphereOutline(center, radius, color, segments);
        else DrawRing(center, Vector3.UnitX, Vector3.UnitY, radius, color, segments);
    }

    /// <summary>A flat stadium outline in the XY plane (the old 2D look). Z is taken from start.</summary>
    static void DrawCapsuleFlat(Vector3 start, Vector3 end, float radius, Color color) {
        float z = start.Z;
        var s = new Vector2(start.X, start.Y);
        var e = new Vector2(end.X, end.Y);

        var dir = e - s;
        float length = dir.Length();

        if (length < 0.001f) {
            DrawRing(new Vector3(s, z), Vector3.UnitX, Vector3.UnitY, radius, color, 32);
            return;
        }

        var dn = dir / length;
        var right = new Vector2(-dn.Y, dn.X) * radius;

        LineBatch.Line(new Vector3(s + right, z), new Vector3(e + right, z), color);
        LineBatch.Line(new Vector3(e - right, z), new Vector3(s - right, z), color);

        float baseAngle = MathF.Atan2(dn.Y, dn.X);
        DrawArc(new Vector3(e, z), Vector3.UnitX, Vector3.UnitY, radius,
            baseAngle - MathHelper.PiOver2, baseAngle + MathHelper.PiOver2, color, 12);
        DrawArc(new Vector3(s, z), Vector3.UnitX, Vector3.UnitY, radius,
            baseAngle + MathHelper.PiOver2, baseAngle + MathHelper.Pi * 1.5f, color, 12);
    }

    /// <summary>
    /// Capsule (swept sphere) between two points. Flat outline in 2D, full wireframe capsule
    /// (end rings, 4 side lines, 2 arcs per hemisphere) in 3D.
    /// </summary>
    public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color) {
        if (!SceneView.Is3D) {
            DrawCapsuleFlat(start, end, radius, color);
            return;
        }

        var axis = end - start;
        float len = axis.Length();
        if (len < 0.001f) {
            DrawSphereOutline(start, radius, color, 24);
            return;
        }

        var d = axis / len;
        var helper = MathF.Abs(d.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(d, helper));
        var v = Vector3.Cross(d, u);

        DrawRing(start, u, v, radius, color, 24);
        DrawRing(end, u, v, radius, color, 24);

        LineBatch.Line(start + u * radius, end + u * radius, color);
        LineBatch.Line(start - u * radius, end - u * radius, color);
        LineBatch.Line(start + v * radius, end + v * radius, color);
        LineBatch.Line(start - v * radius, end - v * radius, color);

        // hemispherical caps: half circles in the (u, d) and (v, d) planes
        DrawArc(end, u, d, radius, 0f, MathHelper.Pi, color, 12);
        DrawArc(end, v, d, radius, 0f, MathHelper.Pi, color, 12);
        DrawArc(start, u, -d, radius, 0f, MathHelper.Pi, color, 12);
        DrawArc(start, v, -d, radius, 0f, MathHelper.Pi, color, 12);
    }

    #region Filled shapes

    const int CapSegs = 16;
    const int CapLat = 4; // latitude steps per hemisphere
    const int CapRings = (CapLat + 1) * 2;
    static readonly Vector3[] _capPts = new Vector3[CapRings * CapSegs];
    static readonly float[] _capCos = new float[CapSegs];
    static readonly float[] _capSin = new float[CapSegs];

    static void FillFanFlat(Vector2 center, float z, float radius, float a0, float a1, int segments, Color color) {
        float step = (a1 - a0) / segments;
        var c = new Vector3(center, z);
        var prev = new Vector3(center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius, z);

        for (int i = 1; i <= segments; i++) {
            float a = a0 + i * step;
            var next = new Vector3(center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius, z);
            LineBatch.Triangle(c, prev, next, color);
            prev = next;
        }
    }

    /// <summary>Filled stadium shape in the XY plane (Z taken from start).</summary>
    static void FillCapsuleFlat(Vector3 start, Vector3 end, float radius, Color color) {
        float z = start.Z;
        var s = new Vector2(start.X, start.Y);
        var e = new Vector2(end.X, end.Y);

        var dir = e - s;
        float length = dir.Length();

        if (length < 0.001f) {
            FillFanFlat(s, z, radius, 0f, MathHelper.TwoPi, 32, color);
            return;
        }

        var dn = dir / length;
        var right = new Vector2(-dn.Y, dn.X) * radius;

        LineBatch.Quad(new Vector3(s + right, z), new Vector3(e + right, z),
            new Vector3(e - right, z), new Vector3(s - right, z), color);

        float baseAngle = MathF.Atan2(dn.Y, dn.X);
        FillFanFlat(e, z, radius, baseAngle - MathHelper.PiOver2, baseAngle + MathHelper.PiOver2, 12, color);
        FillFanFlat(s, z, radius, baseAngle + MathHelper.PiOver2, baseAngle + MathHelper.Pi * 1.5f, 12, color);
    }

    /// <summary>
    /// Filled capsule. 2D: flat stadium. 3D: a tessellated solid capsule (also handles
    /// zero-length capsules, which become spheres).
    /// </summary>
    public static void DrawCapsuleFilled(Vector3 start, Vector3 end, float radius, Color color) {
        if (!SceneView.Is3D) {
            FillCapsuleFlat(start, end, radius, color);
            return;
        }

        if (_capCos[0] == 0f) {
            for (int j = 0; j < CapSegs; j++) {
                float a = j * MathHelper.TwoPi / CapSegs;
                _capCos[j] = MathF.Cos(a);
                _capSin[j] = MathF.Sin(a);
            }
        }

        var axis = end - start;
        float len = axis.Length();
        var d = len < 0.001f ? Vector3.UnitY : axis / len;

        var helper = MathF.Abs(d.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(d, helper));
        var v = Vector3.Cross(d, u);

        int ring = 0;
        void AddRing(Vector3 c, float rr) {
            int baseIdx = ring++ * CapSegs;
            for (int j = 0; j < CapSegs; j++)
                _capPts[baseIdx + j] = c + (u * _capCos[j] + v * _capSin[j]) * rr;
        }

        // start cap: pole -> equator, then end cap: equator -> pole
        for (int k = 0; k <= CapLat; k++) {
            float th = MathHelper.PiOver2 * (1f - (float)k / CapLat);
            AddRing(start - d * (radius * MathF.Sin(th)), radius * MathF.Cos(th));
        }
        for (int k = 0; k <= CapLat; k++) {
            float th = MathHelper.PiOver2 * k / CapLat;
            AddRing(end + d * (radius * MathF.Sin(th)), radius * MathF.Cos(th));
        }

        for (int r = 0; r < CapRings - 1; r++) {
            int a0 = r * CapSegs;
            int b0 = (r + 1) * CapSegs;
            for (int j = 0; j < CapSegs; j++) {
                int j2 = (j + 1) % CapSegs;
                LineBatch.Triangle(_capPts[a0 + j], _capPts[b0 + j], _capPts[b0 + j2], color);
                LineBatch.Triangle(_capPts[a0 + j], _capPts[b0 + j2], _capPts[a0 + j2], color);
            }
        }
    }

    static void DrawBoxCapsule(Vector3 start, Vector3 end, float radius, Color color, bool fill) {
        if (fill) {
            DrawCapsuleFilled(start, end, radius, color * Params.FillAlphaBoxes);
            if (!Params.OutlineFilledBoxes) return;
        }
        DrawCapsule(start, end, radius, color);
    }

    #endregion

    static void DrawRectAtZ(BoundingRect rect, Color color, float z) {
        var tl = new Vector3(rect.Left, rect.Top, z);
        var tr = new Vector3(rect.Right, rect.Top, z);
        var bl = new Vector3(rect.Left, rect.Bottom, z);
        var br = new Vector3(rect.Right, rect.Bottom, z);

        LineBatch.Line(tl, tr, color);
        LineBatch.Line(tr, br, color);
        LineBatch.Line(br, bl, color);
        LineBatch.Line(bl, tl, color);
    }

    public static void DrawBoundingRect(BoundingRect rect, Color color, bool drawText = false, float z = 0f, bool extrude = false) {
        var topLeft = new Vector3(rect.Left, rect.Top, z);
        var topRight = new Vector3(rect.Right, rect.Top, z);
        var bottomLeft = new Vector3(rect.Left, rect.Bottom, z);
        var bottomRight = new Vector3(rect.Right, rect.Bottom, z);

        DrawRectAtZ(rect, color, z);

        if (extrude && SceneView.Is3D && Params.Extrude3D) {
            var faint = color * ExtrudeFade;
            float zf = z - Params.ExtrudeDepth, zb = z + Params.ExtrudeDepth;
            DrawRectAtZ(rect, faint, zf);
            DrawRectAtZ(rect, faint, zb);
            LineBatch.Line(new Vector3(rect.Left, rect.Top, zf), new Vector3(rect.Left, rect.Top, zb), faint);
            LineBatch.Line(new Vector3(rect.Right, rect.Top, zf), new Vector3(rect.Right, rect.Top, zb), faint);
            LineBatch.Line(new Vector3(rect.Left, rect.Bottom, zf), new Vector3(rect.Left, rect.Bottom, zb), faint);
            LineBatch.Line(new Vector3(rect.Right, rect.Bottom, zf), new Vector3(rect.Right, rect.Bottom, zb), faint);
        }

        if (!drawText) return;

        var topStr = $"Top: {topLeft.Y:F1}";
        var botStr = $"Bottom: {bottomLeft.Y:F1}";
        var leftStr = $"Left: {topLeft.X:F1}";
        var rightStr = $"Right: {topRight.X:F1}";

        const float zoneTextScale = 0.25f;
        var font = MeleeFont;

        Text(font, topStr, (topLeft + topRight) / 2, color, zoneTextScale, Anchor.BottomCenter);
        Text(font, rightStr, (topRight + bottomRight) / 2, color, zoneTextScale, Anchor.BottomCenter, MathHelper.PiOver2);
        Text(font, botStr, (bottomRight + bottomLeft) / 2, color, zoneTextScale, Anchor.TopCenter);
        Text(font, leftStr, (bottomLeft + topLeft) / 2, color, zoneTextScale, Anchor.BottomCenter, -MathHelper.PiOver2);
    }

    public static void DrawECB(Vector3 source, ECB ecb, Color color) {
        var bottom = source + new Vector3(ecb.Bottom.X, ecb.Bottom.Y, 0);
        var right = source + new Vector3(ecb.Right.X, ecb.Right.Y, 0);
        var top = source + new Vector3(ecb.Top.X, ecb.Top.Y, 0);
        var left = source + new Vector3(ecb.Left.X, ecb.Left.Y, 0);

        LineBatch.Line(bottom, right, color);
        LineBatch.Line(right, top, color);
        LineBatch.Line(top, left, color);
        LineBatch.Line(left, bottom, color);
    }

    public static void DrawCross(Vector3 center, float halfLength, Color color) {
        LineBatch.Line(center - new Vector3(halfLength, 0, 0), center + new Vector3(halfLength, 0, 0), color);
        LineBatch.Line(center - new Vector3(0, halfLength, 0), center + new Vector3(0, halfLength, 0), color);
        if (SceneView.Is3D)
            LineBatch.Line(center - new Vector3(0, 0, halfLength), center + new Vector3(0, 0, halfLength), color);
    }

    public static void DrawCollisionLine(Vector2 start, Vector2 end, Color color) {
        var a = new Vector3(start, 0);
        var b = new Vector3(end, 0);
        if (Params.DrawCenterLines) LineBatch.Line(a, b, color);

        if (!SceneView.Is3D) {
            LineBatch.Line(a, b, color);
            return;
        }
        else if (Params.DrawCenterLines) LineBatch.Line(a, b, color);

        float d = Params.ExtrudeDepth;
        var af = new Vector3(start, -d);
        var ab = new Vector3(start, d);
        var bf = new Vector3(end, -d);
        var bb = new Vector3(end, d);

        // a wall is edge-on in 2D, so filling only means something in 3D
        if (Params.FillMapLines) LineBatch.Quad(af, bf, bb, ab, color * Params.FillAlphaMap);

        if (!Params.Extrude3D) return;

        var faint = color * ExtrudeFade;
        LineBatch.Line(af, bf, faint);
        LineBatch.Line(ab, bb, faint);
        LineBatch.Line(af, ab, faint);
        LineBatch.Line(bf, bb, faint);
    }

    #endregion

    #region Items

    public static void DrawItem(ItemData it, Color color, bool drawExtras = true) {
        var pos = new Vector3(it.pos.X, it.pos.Y, it.pos.Z);
        var ecb = it.ecb.GetVectorDescribed();

        if (Params.DrawECBs) {
            DrawECB(pos, ecb, color);
            // draw prev at some point
        }

        // for some reason these only get updated when *any* hitbox is active???
        if (Params.DrawHitboxes) {
            for (int i = 0; i < ItemData.HitboxDesc4.LENGTH; i++) {
                var desc = it.x5D4_hitboxes[i];

                if (desc.hit.element > HitElement.Leadead) continue;

                // degenerate hitbox?
                if (MathF.Abs(desc.hit.end.X) > 100000
                    || MathF.Abs(desc.hit.start.X) > 100000) continue;
                if (desc.hit.state > HitCapsuleState.Wait) continue;
                if (desc.hit.state == HitCapsuleState.Disabled) continue;

                var start = desc.hit.start.ToXNA();
                var end = desc.hit.end.ToXNA();
                if (IsInvalidCapsule(desc.hit)) continue;

                var hbColor = MeleeDisplayUtils.HitElementToColor[desc.hit.element];

                DrawBoxCapsule(start, end, desc.hit.scale, hbColor, Params.FillHitboxes);
            }
        }
        if (Params.DrawHurtboxes) {
            for (int i = 0; i < ItemData.HurtCapsuleBuffer2.LENGTH; i++) {
                var hurtbox = it.xACC_itemHurtbox[i];

                // degenerate, something's weird
                if (MathF.Abs(hurtbox.end.X) > 100000
                    || MathF.Abs(hurtbox.start.X) > 100000) continue;
                if (hurtbox.state > HurtCapsuleState.Intangible) continue;

                var start = hurtbox.start.ToXNA();
                var end = hurtbox.end.ToXNA();
                if (IsInvalidCapsule(hurtbox)) continue;

                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hurtbox.state];
                DrawBoxCapsule(start, end, hurtbox.scale, hbColor, Params.FillHurtboxes);
            }
        }

        DrawCross(pos, 1f, Color.White);

        if (!Params.DrawStatsForNerdsItem) return;
        if (!drawExtras) return;

        _infoArr = [
            $"kind: {it.kind}",
            $"pos: <{pos.X:F2}, {pos.Y:F2}, {pos.Z:F2}>",
            $"x0: {it.x0}",
            $"toucher: {it.toucher_gobj}",
            $"vel: {it.x40_vel:F2}",
        ];

        const float scale = 0.1f;
        float yOffset = it.ecb.Top;
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            Text(Cascadia, _infoArr[i],
                new Vector3(pos.X, pos.Y + i * 30 * scale + yOffset, pos.Z),
                color, scale, Anchor.BottomCenter);
        }
    }

    #endregion

    #region Fighters

    public static void DrawFighter(FighterData fd, StageData stDat, Color color, bool drawExtras = true) {
        var pos = new Vector3(fd.Position.X, fd.Position.Y, fd.Position.Z);
        var ecb = fd.CollData.ecb;

        // because of update order these end up being the same at the end of the frame
        if (Params.DrawECBs) {
            DrawECB(pos, fd.CollData.prev_ecb, Color.Sienna);
            DrawECB(pos, ecb, color);
        }

        if (Params.DrawHurtboxes) {
            for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
                var hb = fd.Hurtboxes[i];

                if (hb.capsule.state == HurtCapsuleState.Disabled || hb.capsule.state > HurtCapsuleState.Intangible) continue;
                if (hb.capsule.scale > 10) continue; // something has gone horribly wrong?
                if (hb.capsule.bone < MeleePointers.ROM_SIZE) continue; // something else has gone wrong)

                var end = hb.capsule.end.ToXNA();
                var start = hb.capsule.start.ToXNA();
                if (IsInvalidCapsule(hb.capsule)) continue;

                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hb.capsule.state];
                DrawBoxCapsule(start, end, hb.capsule.scale, hbColor, Params.FillHurtboxes);
            }
        }

        if (Params.DrawHitboxes) {
            for (int i = 0; i < FighterData.HitCapsuleBuffer6.LENGTH; i++) {
                var hb = fd.Hitboxes[i];

                if (hb.state == HitCapsuleState.Disabled) continue;
                if (hb.element > HitElement.Max) continue;
                if (hb.state > HitCapsuleState.Wait) continue;

                var start = hb.start.ToXNA();
                var end = hb.end.ToXNA();
                if (IsInvalidCapsule(hb)) continue;

                var hbColor = MeleeDisplayUtils.HitElementToColor[hb.element];

                DrawBoxCapsule(start, end, hb.scale, hbColor, Params.FillHitboxes);

                if (!Params.DrawStatsForNerdsPlayer) continue;

                Text(Cascadia, hb.state.ToString(), start, Color.IndianRed, 0.04f, Anchor.TopLeft);
            }
        }

        #region Shields
        if (Params.DrawShields && fd.IsShielding) {
            // lerp between initial size and 0.2f... or something?
            // this is not quite right but good enough
            var tgrScl = MathHelper.Lerp(0.75f, 1.5f, fd.Input.Triggers);
            var shieldSize = fd.Attr.initial_shield_size * (fd.ShieldHealth / 60) / tgrScl;
            // there's probably something in Fighter controlling this
            var shieldOrig = Dolphinterop.Read<JObj>(fd.GetBone(FtPart.ThrowN).jobj).mtx.Translation; // xrotn?
            DrawBubble(new Vector3(shieldOrig.X, shieldOrig.Y, shieldOrig.Z), shieldSize, Color.SkyBlue * fd.Input.Triggers, 32);
        }
        #endregion

        #region Ledgegrab Boxes
        if (Params.DrawLedgeGrabBoxes) {
            // subtracting magic numbers for now
            float visualSeparationOtherwiseYouCantSeeAColor = 0.025f;
            // right box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X + fd.CollData.ledge_snap_x + ecb.Right.X,
                Left = pos.X + visualSeparationOtherwiseYouCantSeeAColor,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Blue, false, pos.Z);
            // left box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X - visualSeparationOtherwiseYouCantSeeAColor,
                Left = pos.X - fd.CollData.ledge_snap_x + ecb.Left.X,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Red, false, pos.Z);
        }
        #endregion

        // draws a cross at the player's real position
        DrawCross(pos, 1f, Color.White);

        #region Player Danger
        var realCamBounds = stDat.GetRealCameraBounds();
        var realBlast = stDat.GetRealBlastZone();

        var colorWarn = Color.IndianRed;
        if (pos.X > realCamBounds.Right) {
            DrawLine(pos, new Vector3(realBlast.Right, pos.Y, pos.Z), colorWarn);
        }
        else if (pos.X < realCamBounds.Left) {
            DrawLine(pos, new Vector3(realBlast.Left, pos.Y, pos.Z), colorWarn);
        }
        if (pos.Y > realCamBounds.Top) {
            DrawLine(pos, new Vector3(pos.X, realBlast.Top, pos.Z), colorWarn);
        }
        else if (pos.Y < realCamBounds.Bottom) {
            DrawLine(pos, new Vector3(pos.X, realBlast.Bottom, pos.Z), colorWarn);
        }
        #endregion

        if (!Params.DrawStatsForNerdsPlayer) return;
        if (!drawExtras) return;

        #region Extra Details
        // AnimTree.frames is returning bogus...?
        float animFrameTotal = fd.AnimTree.frames;
        float animFrameCurr = fd.AnimFrame;
        float animSpeed = fd.AnimRate;
        var frameTotal = animFrameTotal / animSpeed;
        var frameCurr = animFrameCurr / animSpeed;

        string anim;

        if (fd.AnimState < FtAnimState.Count) anim = fd.AnimState.ToString();
        else anim = fd.GetActionNameTrunc(fd.ActionId) ?? fd.AnimState.ToString();

        _infoArr = [
            $"kind: {fd.CharKind}",
            $"pos:  <{pos.X:F2}, {pos.Y:F2}, {pos.Z:F2}>",
            $"anim: {anim}",
            // $"frame: {frameCurr} / {frameTotal}",
            $"sh:   {fd.ShieldHealth}",
            $"stun: {fd.HitstunFrames}/{fd.HitlagFrames}"
        ];

        const float scale = 0.1f;
        float yOffset = fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f;
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            Text(Cascadia, _infoArr[i],
                new Vector3(pos.X, pos.Y + i * 30 * scale + yOffset, pos.Z),
                color, scale, Anchor.BottomCenter);
        }
        #endregion
    }

    public static void DrawTrajectories(FighterData fd, float opponentDirX) {
        if (!Params.DrawPrediction) return;

        bool shouldDrawTraj = fd.IsKnockedBack; 

        if (shouldDrawTraj) {
            DrawTrajectory(TrajectorySim.Simulate(fd, DIDirection.DIIn, opponentDirX, Params.PredictionFrames), Color.LimeGreen, 0.4f);
            DrawTrajectory(TrajectorySim.Simulate(fd, DIDirection.None, opponentDirX, Params.PredictionFrames), Color.Yellow, 0.4f);
            DrawTrajectory(TrajectorySim.Simulate(fd, DIDirection.DIOut, opponentDirX, Params.PredictionFrames), Color.OrangeRed, 0.4f);
        }

        DrawTrajectory(TrajectorySim.Simulate(fd, DIDirection.Current, opponentDirX, Params.PredictionFrames), Color.White, 0.5f);
    }

    static void DrawTrajectory(TrajectoryResult r, Color col, float alpha) {
        var pts = r.Points;

        bool hitlagOver = r.HitlagEndIndex >= 0 && r.HitlagEndIndex < pts.Count;
        bool hitstunOver = r.HitstunEndIndex >= 0 && r.HitstunEndIndex < pts.Count;
        bool landed = r.LandingIndex >= 0 && r.LandingIndex < pts.Count;

        int last = hitstunOver ? r.HitstunEndIndex : pts.Count - 1;

        for (int k = 1; k <= last; k++) {
            float fade = 1f - 0.6f * k / (last + 1);
            DrawLine(pts[k - 1].ToXNA(), pts[k].ToXNA(), col * (alpha * fade));
        }

        if (hitlagOver && r.HitlagEndIndex <= last) DrawCross(pts[r.HitlagEndIndex].ToXNA(), 1.0f, col);
        if (hitstunOver) DrawCross(pts[r.HitstunEndIndex].ToXNA(), 2.0f, col * 0.6f);
        // landing before hitstun ends (e.g. a floor hit) is still worth showing
        if (landed && r.LandingIndex <= last) DrawCross(pts[r.LandingIndex].ToXNA(), 3.0f, col);
    }

    #endregion
}

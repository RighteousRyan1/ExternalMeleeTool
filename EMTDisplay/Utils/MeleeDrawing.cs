using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Utilities;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMTDisplay.Utils; 
public static class MeleeDrawing {
    public static bool DrawECBs = true;
    public static bool DrawHitboxes = true;
    public static bool DrawHurtboxes = true;
    public static bool DrawShields = true;
    public static bool DrawLedgeGrabBoxes = true;
    public static bool DrawStatsForNerdsPlayer = true;
    public static bool DrawStatsForNerdsItem = false;

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
    public static void DrawItem(ItemData it, Color color, float thickness, bool drawExtras = true) {
        var pos = new Vector2(it.pos.X, it.pos.Y);
        var ecb = it.ecb.GetVectorDescribed();

        #region ECBs
        if (DrawECBs) {
            DrawECB(pos, ecb, color, thickness);
            // draw prev at some point
        }
        #endregion

        #region Hurt/Hitboxes

        // for some reason these only get updated when *any* hitbox is active???
        if (DrawHitboxes) {
            for (int i = 0; i < ItemData.HitboxDesc4.LENGTH; i++) {
                var desc = it.x5D4_hitboxes[i];

                if (desc.hit.element > HitElement.Leadead) continue;

                // degenerate hitbox?
                if (desc.hit.end.X > 100000) continue;
                if (desc.hit.state > HitCapsuleState.Wait) continue;
                if (desc.hit.state == HitCapsuleState.Disabled) continue;

                var start = desc.hit.start.ToXNA().Flatten();
                var end = desc.hit.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HitElementToColor[desc.hit.element];

                DrawCapsuleOutline(start, end, desc.hit.scale, hbColor, thickness);
            }
        }
        if (DrawHurtboxes) {
            for (int i = 0; i < ItemData.HurtCapsuleBuffer2.LENGTH; i++) {
                var hurtbox = it.xACC_itemHurtbox[i];

                if (hurtbox.state > HurtCapsuleState.Intangible) continue; 

                var start = hurtbox.start.ToXNA().Flatten();
                var end = hurtbox.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hurtbox.state];
                DrawCapsuleOutline(start, end, hurtbox.scale, hbColor, thickness);
            }
        }
        #endregion

        #region Item Position
        float linesLength = 1;
        DrawLine(pos - new Vector2(linesLength, 0), pos + new Vector2(linesLength, 0), Color.White, thickness);
        DrawLine(pos - new Vector2(0, linesLength), pos + new Vector2(0, linesLength), Color.White, thickness);
        #endregion

        if (!DrawStatsForNerdsItem) return;

        if (!drawExtras) return;

        #region Extra Details
        _infoArr = [
            $"kind: {it.kind}",
            $"pos: <{pos.X:F2}, {pos.Y:F2}>",
            $"x0: {it.x0}",
            $"toucher: {it.toucher_gobj}",
            $"vel: {it.x40_vel:F2}",
            // $"{fd.GObj.FieldsToString()}"
        ];

        var scale = 0.1f;
        var yOffset = new Vector2(0, it.ecb.Top); // new Vector2(ecb.Top.X, ecb.Top.Y);
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            var info = _infoArr[i];
            EMTDisplay.SpriteBatch.DrawString(Cascadia, info,
                new Vector2(pos.X, pos.Y + i * 30 * scale) + yOffset, color,
                scale: new Vector2(scale, -scale),
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, Cascadia.MeasureString(info)));
        }
        #endregion
    }
    public static void DrawMeleePlayer(FighterData fd, StageData stDat, Color color, float thickness = 1f, bool drawExtras = true) {
        var pos = new Vector2(fd.Position.X, fd.Position.Y);
        var ecb = fd.CollData.ecb;

        #region ECBs
        // because of update order these end up being the same at the end of the frame
        if (DrawECBs) {
            DrawECB(pos, fd.CollData.prev_ecb, Color.Sienna, thickness);
            DrawECB(pos, ecb, color, thickness);
        }
        #endregion

        #region Hurt/Hitboxes
        if (DrawHurtboxes) {
            for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
                var hb = fd.Hurtboxes[i];

                if (hb.capsule.state == HurtCapsuleState.Disabled || hb.capsule.state > HurtCapsuleState.Intangible) continue;
                if (hb.capsule.scale > 10) continue; // something has gone horribly wrong?
                if (hb.capsule.bone < MeleeGlobals.ROM_SIZE) continue; // something else has gone wrong


                // var jobj = Dolphinterop.Read<HSD_JObj>(hb.capsule.bone);
                var end = hb.capsule.end.ToXNA().Flatten();
                var start = hb.capsule.start.ToXNA().Flatten();

                // why is is_grabbable always false?
                // var bone = Dolphinterop.Read<HSD_JObj>(hb.capsule.bone);
                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hb.capsule.state];
                DrawCapsuleOutline(start, end, hb.capsule.scale, hbColor, thickness);
            }
        }

        // debug ftpart name draw
        /*var names = Enum.GetNames<FtPart>();
        for (int i = 0; i < names.Length; i++) {
            var part = (FtPart)i;
            var jobj = fd.GetBoneJObj(part);

            var str = part.ToString();
            EMTDisplay.SpriteBatch.DrawString(Cascadia, str,
                    jobj.mtx.Translation.ToXNA().Flatten(),
                    color: Color.IndianRed,
                    scale: new Vector2(0.015f, -0.015f),
                    rotation: 0f,
                    origin: Cascadia.MeasureString(str) / 2);
        }*/

        if (DrawHitboxes) {
            for (int i = 0; i < FighterData.HitCapsuleBuffer4.LENGTH; i++) {
                var hb = fd.Hitboxes[i];

                if (hb.state == HitCapsuleState.Disabled) continue;

                //hb.element = HitElement.Cape;
                //Dolphinterop.Write<>

                if (hb.element > HitElement.Max) continue;

                var start = hb.start.ToXNA().Flatten();
                var end = hb.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HitElementToColor[hb.element];

                //EMTDisplay.SpriteBatch.Draw(WhitePixel, cpos, null, color, 0f, WhitePixel.Size() / 2, hb.scale, default, 0f);
                EMTDisplay.SpriteBatch.DrawString(Cascadia, /*hb.element.ToString()*/hb.kb_angle.ToString(),
                    start,
                    color: Color.IndianRed,
                    scale: new Vector2(0.04f, -0.04f),
                    rotation: 0f);

                // DrawCircleOutline(cpos, hb.scale, Color.IndianRed, 32, thickness);
                DrawCapsuleOutline(start, end, hb.scale, hbColor, thickness);
            }
        }
        #endregion

        #region Shields
        if (DrawShields && fd.IsShielding) {
            // const float magic_number = 1f;
            // lerp between initial size and 0.2f... or something?
            // this is not quite right but good enough
            var tgrScl = MathHelper.Lerp(0.5f, 1f, fd.Input.Triggers); // magic_number;
            var shieldSize = fd.Attr.initial_shield_size * (fd.ShieldHealth / 60) / tgrScl; // / (fd.Input.Triggers * magic_number);
            // i'm not entirely sure of the sauce behind this yet
            //var shieldSizeAdjusted = fd.Attr.initial_shield_size / (fd.Input.Triggers * magic_number);
            //var shieldSize = MathHelper.Lerp(2f, shieldSizeAdjusted, fd.ShieldHealth / 60);
            // there's probably something in Fighter controlling this
            DrawCircleOutline(pos + ecb.Center, shieldSize, Color.SkyBlue * fd.Input.Triggers, 32, thickness);
        }
        #endregion

        #region Ledgegrab Boxes

        if (DrawLedgeGrabBoxes) {
            // subtracting magic numbers for now
            float visualSeparationOtherwiseYouCantSeeAColor = 0.025f;
            // right box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X + fd.CollData.ledge_snap_x + ecb.Right.X,
                Left = pos.X + visualSeparationOtherwiseYouCantSeeAColor,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Red, thickness, false);
            // left box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X - visualSeparationOtherwiseYouCantSeeAColor,
                Left = pos.X - fd.CollData.ledge_snap_x + ecb.Left.X,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Blue, thickness, false);
        }
        #endregion

        #region Player Position
        // draws a cross at the player's real position
        float linesLength = 1;
        DrawLine(pos - new Vector2(linesLength, 0), pos + new Vector2(linesLength, 0), Color.White, thickness);
        DrawLine(pos - new Vector2(0, linesLength), pos + new Vector2(0, linesLength), Color.White, thickness);
        #endregion

        #region Player Danger

        var realCamBounds = stDat.GetRealCameraBounds();
        var realBlast = stDat.GetRealBlastZone();

        var colorWarn = Color.IndianRed;
        if (pos.X > realCamBounds.Right) {
            DrawLine(pos, new Vector2(realBlast.Right, pos.Y), colorWarn, thickness);
        }
        else if (pos.X < realCamBounds.Left) {
            DrawLine(pos, new Vector2(realBlast.Left, pos.Y), colorWarn, thickness);
        }
        if (pos.Y > realCamBounds.Top) {
            DrawLine(pos, new Vector2(pos.X, realBlast.Top), colorWarn, thickness);
        }
        else if (pos.Y < realCamBounds.Bottom) {
            DrawLine(pos, new Vector2(pos.X, realBlast.Bottom), colorWarn, thickness);
        }

        #endregion

        if (!DrawStatsForNerdsPlayer) return;

        if (!drawExtras) return;

        #region Extra Details
        _infoArr = [
            $"kind: {fd.CharKind}",
            $"pos:  <{pos.X:F2}, {pos.Y:F2}>",
            $"anim: {fd.AnimState}",
            $"sh:   {fd.ShieldHealth}",
            $"%:    {fd.Percent}",
            // $"lock: {Dolphinterop.ReadS32(fd.FighterPtr + 0x88C)}"
            // $"{fd.GObj.FieldsToString()}"
        ];

        var scale = 0.1f;
        var yOffset = new Vector2(0, fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f); // new Vector2(ecb.Top.X, ecb.Top.Y);
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            var info = _infoArr[i];
            EMTDisplay.SpriteBatch.DrawString(Cascadia, info,
                new Vector2(pos.X, pos.Y + i * 30 * scale) + yOffset, color,
                scale: new Vector2(scale, -scale),
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, Cascadia.MeasureString(info)));
        }
        #endregion
    }

    public static void DrawECB(Vector2 source, ECB ecb, Color color, float thickness = 1) {
        DrawLine(source + ecb.Bottom, source + ecb.Right, color, thickness);

        DrawLine(source + ecb.Right, source + ecb.Top, color, thickness);

        DrawLine(source + ecb.Top, source + ecb.Left, color, thickness);

        DrawLine(source + ecb.Left, source + ecb.Bottom, color, thickness);
    }

    public static void DrawBoundingRect(BoundingRect rect, Color color, float thickness = 1f, bool drawText = false) {
        var topLeft = new Vector2(rect.Left, rect.Top);
        var topRight = new Vector2(rect.Right, rect.Top);
        var bottomLeft = new Vector2(rect.Left, rect.Bottom);
        var bottomRight = new Vector2(rect.Right, rect.Bottom);

        DrawLine(topLeft, topRight, color, thickness); // top Edge
        DrawLine(topRight, bottomRight, color, thickness); // right Edge
        DrawLine(bottomRight, bottomLeft, color, thickness); // bottom Edge
        DrawLine(bottomLeft, topLeft, color, thickness); // left Edge

        if (!drawText) return;

        var topStr = $"Top: {topLeft.Y:F1}";
        var botStr = $"Bottom: {bottomLeft.Y:F1}";
        var leftStr = $"Left: {topLeft.X:F1}";
        var rightStr = $"Right: {topRight.X:F1}";

        var font = MeleeFont;
        // i have to flip vertical text along the X axis
        // horizontal text along the Y axis...
        float zoneTextScale = 0.25f;
        EMTDisplay.SpriteBatch.DrawString(font, topStr,
            (topLeft + topRight) / 2, color, scale: new Vector2(zoneTextScale, -zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, font.MeasureString(topStr)));
        EMTDisplay.SpriteBatch.DrawString(font, rightStr,
            (topRight + bottomRight) / 2, color, scale: new Vector2(-zoneTextScale, zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, font.MeasureString(rightStr)),
            rotation: MathHelper.PiOver2);
        EMTDisplay.SpriteBatch.DrawString(font, botStr,
            (bottomRight + bottomLeft) / 2, color, scale: new Vector2(zoneTextScale, -zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.TopCenter, font.MeasureString(botStr)));

        EMTDisplay.SpriteBatch.DrawString(font, leftStr,
            (bottomLeft + topLeft) / 2, color, scale: new Vector2(-zoneTextScale, zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, font.MeasureString(leftStr)),
            rotation: -MathHelper.PiOver2);
    }

    static void DrawCircleOutline(Vector2 center, float radius, Color color, int segments, float thickness = 1f) {
        float angleStep = (float)(Math.PI * 2.0 / segments);
        Vector2 lastPoint = new(center.X + radius, center.Y);

        for (int i = 1; i <= segments; i++) {
            float angle = i * angleStep;
            Vector2 nextPoint = new(
                center.X + (float)Math.Cos(angle) * radius,
                center.Y + (float)Math.Sin(angle) * radius
            );

            DrawLine(lastPoint, nextPoint, color, thickness);
            lastPoint = nextPoint;
        }
    }

    public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f) {
        EMTDisplay.SpriteBatch.Draw(WhitePixel, start, null, color,
            (end - start).ToRotation(),
            new Vector2(0, 0.5f),
            new Vector2(Vector2.Distance(start, end), thickness),
            SpriteEffects.None, 0);
    }

    public static void DrawCapsuleOutline(Vector2 start, Vector2 end, float radius, Color color, float thickness) {
        // 1. Handle Degenerate Case (Sphere)
        Vector2 dir = end - start;
        float length = dir.Length();

        if (length < 0.001f) {
            DrawCircleOutline(start, radius, color, 32, thickness);
            return;
        }

        // 2. Calculate the "Right" vector (perpendicular to axis)
        Vector2 directionNormalized = dir / length;
        Vector2 right = new Vector2(-directionNormalized.Y, directionNormalized.X) * radius;

        // 3. Draw the Body (Two Parallel Lines)
        // Left Side (from Start to End)
        MeleeDrawing.DrawLine(start + right, end + right, color, thickness);
        // Right Side (from End to Start)
        MeleeDrawing.DrawLine(end - right, start - right, color, thickness);

        // 4. Draw the End Caps (180-degree Arcs)
        // We calculate the base angle of the capsule to orient the arcs correctly
        float baseAngle = (float)Math.Atan2(directionNormalized.Y, directionNormalized.X);

        // Draw "Top" Cap (at End point) - usually -90 to +90 degrees relative to angle
        DrawArc(end, radius, baseAngle - MathHelper.PiOver2, baseAngle + MathHelper.PiOver2, color, thickness);

        // Draw "Bottom" Cap (at Start point) - usually +90 to +270 degrees
        DrawArc(start, radius, baseAngle + MathHelper.PiOver2, baseAngle + (MathHelper.Pi * 1.5f), color, thickness);
    }

    // ==========================================
    // Helper: Draw Arc using DrawLine
    // ==========================================
    private static void DrawArc(Vector2 center, float radius, float startAngle, float endAngle, Color color, float lineZoom, int segments = 12) {
        float angleStep = (endAngle - startAngle) / segments;
        Vector2 prevPoint = center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * radius;

        for (int i = 1; i <= segments; i++) {
            float angle = startAngle + i * angleStep;
            Vector2 nextPoint = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

            MeleeDrawing.DrawLine(prevPoint, nextPoint, color, lineZoom);
            prevPoint = nextPoint;
        }
    }
}

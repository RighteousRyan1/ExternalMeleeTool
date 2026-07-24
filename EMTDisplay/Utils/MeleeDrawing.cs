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
using System.IO;

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
    public static void DrawItem2D(ItemData it, Color color, float thickness, bool drawExtras = true) {
        var pos = new Vector2(it.pos.X, it.pos.Y);
        var ecb = it.ecb.GetVectorDescribed();

        #region ECBs
        if (DrawECBs) {
            DrawECB2D(pos, ecb, color, thickness);
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
                if (MathF.Abs(desc.hit.end.X) > 100000
                    || MathF.Abs(desc.hit.start.X) > 100000) continue;
                if (desc.hit.state > HitCapsuleState.Wait) continue;
                if (desc.hit.state == HitCapsuleState.Disabled) continue;

                var start = desc.hit.start.ToXNA().Flatten();
                var end = desc.hit.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HitElementToColor[desc.hit.element];

                DrawCapsuleOutline2D(start, end, desc.hit.scale, hbColor, thickness);
            }
        }
        if (DrawHurtboxes) {
            for (int i = 0; i < ItemData.HurtCapsuleBuffer2.LENGTH; i++) {
                var hurtbox = it.xACC_itemHurtbox[i];

                // degenerate, something's weird
                if (MathF.Abs(hurtbox.end.X) > 100000
                    || MathF.Abs(hurtbox.start.X) > 100000) continue;
                if (hurtbox.state > HurtCapsuleState.Intangible) continue; 

                var start = hurtbox.start.ToXNA().Flatten();
                var end = hurtbox.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hurtbox.state];
                DrawCapsuleOutline2D(start, end, hurtbox.scale, hbColor, thickness);
            }
        }
        #endregion

        #region Item Position
        float linesLength = 1;
        DrawLine2D(pos - new Vector2(linesLength, 0), pos + new Vector2(linesLength, 0), Color.White, thickness);
        DrawLine2D(pos - new Vector2(0, linesLength), pos + new Vector2(0, linesLength), Color.White, thickness);
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
    public static void DrawFighter2D(FighterData fd, StageData stDat, Color color, float thickness = 1f, bool drawExtras = true) {
        var pos = new Vector2(fd.Position.X, fd.Position.Y);
        var ecb = fd.CollData.ecb;

        #region ECBs
        // because of update order these end up being the same at the end of the frame
        if (DrawECBs) {
            DrawECB2D(pos, fd.CollData.prev_ecb, Color.Sienna, thickness);
            DrawECB2D(pos, ecb, color, thickness);
        }
        #endregion

        #region Hurt/Hitboxes
        if (DrawHurtboxes) {
            for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
                var hb = fd.Hurtboxes[i];

                if (hb.capsule.state == HurtCapsuleState.Disabled || hb.capsule.state > HurtCapsuleState.Intangible) continue;
                if (hb.capsule.scale > 10) continue; // something has gone horribly wrong?
                if (hb.capsule.bone < MeleePointers.ROM_SIZE) continue; // something else has gone wrong)


                // var bone = Dolphinterop.Read<HSD_JObj>(hb.capsule.bone);
                var end = hb.capsule.end.ToXNA().Flatten();
                var start = hb.capsule.start.ToXNA().Flatten();

                // why is is_grabbable always false?
                // var bone = Dolphinterop.Read<HSD_JObj>(hb.capsule.bone);
                var hbColor = MeleeDisplayUtils.HurtCapsuleStateToColor[hb.capsule.state];
                DrawCapsuleOutline2D(start, end, hb.capsule.scale, hbColor, thickness);

                if (!DrawStatsForNerdsPlayer) continue;

                /*var part = fd.GetPartFromBoneIndex(hb.capsule.bone_idx);

                var str = part.ToString(); // ((hb.capsule.start + hb.capsule.end) / 2).ToString("F2");
                EMTDisplay.SpriteBatch.DrawString(Cascadia, str,
                        (start + end) / 2,
                        color: Color.IndianRed,
                        scale: new Vector2(0.015f, -0.015f),
                        rotation: 0f,
                        origin: Cascadia.MeasureString(str) / 2);*/
            }
        }

        // debug ftpart name draw
        // var names = Enum.GetNames<FtPart>();
        /*var table = fd.GetPartTable();
        for (int i = 0; i < table.parts_num; i++) {
            var part = (FtPart)i;
            // if (part != FtPart.ThrowN) continue;
            var bone = fd.GetBone(part);
            var jobj = Dolphinterop.Read<JObj>(bone.jobj);

            var str = part.ToString(); // part.ToString();
            EMTDisplay.SpriteBatch.DrawString(Cascadia, str,
                    jobj.mtx.Translation.ToXNA().Flatten(),
                    color: Color.Lime,
                    scale: new Vector2(0.015f, -0.015f),
                    rotation: 0f,
                    origin: Cascadia.MeasureString(str) / 2);
        }*/

        if (DrawHitboxes) {
            for (int i = 0; i < FighterData.HitCapsuleBuffer6.LENGTH; i++) {
                var hb = fd.Hitboxes[i];

                if (hb.state == HitCapsuleState.Disabled) continue;

                //hb.element = HitElement.Cape;
                //Dolphinterop.Write<>

                if (hb.element > HitElement.Max) continue;

                var start = hb.start.ToXNA().Flatten();
                var end = hb.end.ToXNA().Flatten();

                var hbColor = MeleeDisplayUtils.HitElementToColor[hb.element];

                // DrawCircleOutline(cpos, hb.scale, Color.IndianRed, 32, thickness);
                DrawCapsuleOutline2D(start, end, hb.scale, hbColor, thickness);

                if (!DrawStatsForNerdsPlayer) continue;

                // origin later maybe
                EMTDisplay.SpriteBatch.DrawString(Cascadia, hb.state.ToString(),
                    start,
                    color: Color.IndianRed,
                    scale: new Vector2(0.04f, -0.04f),
                    rotation: 0f);
            }
        }
        #endregion

        #region Shields
        if (DrawShields && fd.IsShielding) {
            // const float magic_number = 1f;
            // lerp between initial size and 0.2f... or something?
            // this is not quite right but good enough
            var tgrScl = MathHelper.Lerp(0.75f, 1.5f, fd.Input.Triggers); // magic_number;
            var shieldSize = fd.Attr.initial_shield_size * (fd.ShieldHealth / 60) / tgrScl; // / (fd.Input.Triggers * magic_number);
            // i'm not entirely sure of the sauce behind this yet
            //var shieldSizeAdjusted = fd.Attr.initial_shield_size / (fd.Input.Triggers * magic_number);
            //var shieldSize = MathHelper.Lerp(2f, shieldSizeAdjusted, fd.ShieldHealth / 60);
            // there's probably something in Fighter controlling this
            var shieldOrig = Dolphinterop.Read<JObj>(fd.GetBone(FtPart.ThrowN).jobj).mtx.Translation; // xrotn?
            DrawCircleOutline(new Vector2(shieldOrig.X, shieldOrig.Y), shieldSize, Color.SkyBlue * fd.Input.Triggers, 32, thickness);
        }
        #endregion

        #region Ledgegrab Boxes

        if (DrawLedgeGrabBoxes) {
            // subtracting magic numbers for now
            float visualSeparationOtherwiseYouCantSeeAColor = 0.025f;
            // right box
            DrawBoundingRect2D(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X + fd.CollData.ledge_snap_x + ecb.Right.X,
                Left = pos.X + visualSeparationOtherwiseYouCantSeeAColor,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Blue, thickness, false);
            // left box
            DrawBoundingRect2D(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X - visualSeparationOtherwiseYouCantSeeAColor,
                Left = pos.X - fd.CollData.ledge_snap_x + ecb.Left.X,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Red, thickness, false);
        }
        #endregion

        #region Player Position
        // draws a cross at the player's real position
        float linesLength = 1;
        DrawLine2D(pos - new Vector2(linesLength, 0), pos + new Vector2(linesLength, 0), Color.White, thickness);
        DrawLine2D(pos - new Vector2(0, linesLength), pos + new Vector2(0, linesLength), Color.White, thickness);
        #endregion

        #region Player Danger

        var realCamBounds = stDat.GetRealCameraBounds();
        var realBlast = stDat.GetRealBlastZone();

        var colorWarn = Color.IndianRed;
        if (pos.X > realCamBounds.Right) {
            DrawLine2D(pos, new Vector2(realBlast.Right, pos.Y), colorWarn, thickness);
        }
        else if (pos.X < realCamBounds.Left) {
            DrawLine2D(pos, new Vector2(realBlast.Left, pos.Y), colorWarn, thickness);
        }
        if (pos.Y > realCamBounds.Top) {
            DrawLine2D(pos, new Vector2(pos.X, realBlast.Top), colorWarn, thickness);
        }
        else if (pos.Y < realCamBounds.Bottom) {
            DrawLine2D(pos, new Vector2(pos.X, realBlast.Bottom), colorWarn, thickness);
        }

        #endregion

        if (!DrawStatsForNerdsPlayer) return;

        if (!drawExtras) return;

        #region Extra Details
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
            $"pos:  <{pos.X:F2}, {pos.Y:F2}>",
            $"anim: {anim}",
            $"frame: {frameCurr} / {frameTotal}",
            $"sh:   {fd.ShieldHealth}",
            $"%:    {fd.Percent}",
            $"Port: {fd.Port}"
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

    public static void DrawFighterPrediction2D(FighterData fd, int framesSinceLRPress, bool isTechLockedOut, float thickness = 1f) {
        // this is rather poor 
        var simPos = fd.Position;
        var simKb = fd.Knockback;
        var simVelSelf = fd.VelocitySelf;

        // Apply hitstop influences to our prediction variables, keeping 'fd' completely untouched
        /*if (fd.HasKnockback) {
            //KnockbackInfluence.ApplyVCancel(ref simKb, in fd, framesSinceLRPress, isTechLockedOut);
            //KnockbackInfluence.ApplyCrouchCancel(ref simKb, in fd);

            //KnockbackInfluence.ApplySDI(ref simPos, in fd, 0, 0);
            //KnockbackInfluence.ApplyASDI(ref simPos, in fd);
            // KnockbackInfluence.ApplyTDI(ref simKb, in fd);
        }*/

        var simAnimFrame = fd.AnimFrame;
        // var maxAnimFrame = fd.AnimTree.frames;

        // is adding left stick really how it's done?
        var simVel = simVelSelf + simKb;

        // i'd have to know how many frames of hitstun the player's in and go based off of that
        // and do this with many other things
        const int NUM_FRAMES = 120; // # prediction frames

        for (int k = 0; k < NUM_FRAMES; k++) {
            var prevPos = simPos;

            simPos += simVel;

            var seg = new LineSegment(
                new Vector2(prevPos.X, prevPos.Y),
                new Vector2(simPos.X, simPos.Y)
            );

            /*for (int j = 0; j < MapLineSegments.Count; j++) {
                var mapSeg = MapLineSegments[i];

                if (seg.Intersects(mapSeg, out var pos, out var normal)) {
                    Console.WriteLine($"{mapSeg} {pos} {normal}");
                    // reflect velocity
                    var vel2D = new Vector2(simVel.X, simVel.Y);
                    var reflected = Vector2.Reflect(vel2D, normal) * 0.7f; // lose some speed on bounce
                    simVel.X = reflected.X;
                    simVel.Y = reflected.Y;
                    // move simpos to intersection point + small offset
                    simPos = new System.Numerics.Vector3(
                        seg.Start.X + normal.X * 0.1f,
                        seg.Start.Y + normal.Y * 0.1f,
                        0
                    );
                }
                // reflection is working shoddily
            }*/

            DrawLine2D(
                seg.Start,
                seg.End,
                Color.White * 0.1f,
                thickness
            );

            if (fd.Grounded)
                simVel.Y -= fd.Attr.grav;

            // terminal vel
            if (simVel.Y < -fd.Attr.terminal_vel) {
                simVel.Y = -fd.Attr.terminal_vel;
            }

            //if (fd.IsKnockedBack)
            //simVel += new System.Numerics.Vector3(fd.Input.LeftStick, 0);
            // aerial friction
            // if (!fd.IsKnockedBack) //else

            // current sim does not account for hitstun ending, landing, etc.
            /*if (fd.IsKnockedBack) {
                if (simAnimFrame >= maxAnimFrame) {

                }
            }*/

            simVel.X -= fd.Attr.aerial_friction * MathF.Sign(simVel.X);

            //var v = fd.Attr.Value;
            //v.jump_startup_time = 20;
            //fd.Attr.Value = v;

            simAnimFrame++;
        }
    }

    public static void DrawECB2D(Vector2 source, ECB ecb, Color color, float thickness = 1) {
        DrawLine2D(source + ecb.Bottom, source + ecb.Right, color, thickness);

        DrawLine2D(source + ecb.Right, source + ecb.Top, color, thickness);

        DrawLine2D(source + ecb.Top, source + ecb.Left, color, thickness);

        DrawLine2D(source + ecb.Left, source + ecb.Bottom, color, thickness);
    }

    public static void DrawBoundingRect2D(BoundingRect rect, Color color, float thickness = 1f, bool drawText = false) {
        var topLeft = new Vector2(rect.Left, rect.Top);
        var topRight = new Vector2(rect.Right, rect.Top);
        var bottomLeft = new Vector2(rect.Left, rect.Bottom);
        var bottomRight = new Vector2(rect.Right, rect.Bottom);

        DrawLine2D(topLeft, topRight, color, thickness); // top Edge
        DrawLine2D(topRight, bottomRight, color, thickness); // right Edge
        DrawLine2D(bottomRight, bottomLeft, color, thickness); // bottom Edge
        DrawLine2D(bottomLeft, topLeft, color, thickness); // left Edge

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

    public static void DrawCircleOutline(Vector2 center, float radius, Color color, int segments, float thickness = 1f) {
        float angleStep = (float)(Math.PI * 2.0 / segments);
        Vector2 lastPoint = new(center.X + radius, center.Y);

        for (int i = 1; i <= segments; i++) {
            float angle = i * angleStep;
            Vector2 nextPoint = new(
                center.X + (float)Math.Cos(angle) * radius,
                center.Y + (float)Math.Sin(angle) * radius
            );

            DrawLine2D(lastPoint, nextPoint, color, thickness);
            lastPoint = nextPoint;
        }
    }

    public static void DrawLine2D(Vector2 start, Vector2 end, Color color, float thickness = 1f) {
        EMTDisplay.SpriteBatch.Draw(WhitePixel, start, null, color,
            (end - start).ToRotation(),
            new Vector2(0, 0.5f),
            new Vector2(Vector2.Distance(start, end), thickness),
            SpriteEffects.None, 0);
    }

    public static void DrawCapsuleOutline2D(Vector2 start, Vector2 end, float radius, Color color, float thickness = 1f) {
        Vector2 dir = end - start;
        float length = dir.Length();

        if (length < 0.001f) {
            DrawCircleOutline(start, radius, color, 32, thickness);
            return;
        }

        Vector2 directionNormalized = dir / length;
        Vector2 right = new Vector2(-directionNormalized.Y, directionNormalized.X) * radius;

        DrawLine2D(start + right, end + right, color, thickness);
        // Right Side (from End to Start)
        DrawLine2D(end - right, start - right, color, thickness);

        float baseAngle = (float)Math.Atan2(directionNormalized.Y, directionNormalized.X);

        DrawArc2D(end, radius, baseAngle - MathHelper.PiOver2, baseAngle + MathHelper.PiOver2, color, thickness);

        DrawArc2D(start, radius, baseAngle + MathHelper.PiOver2, baseAngle + (MathHelper.Pi * 1.5f), color, thickness);
    }

    public static void DrawArc2D(Vector2 center, float radius, float startAngle, float endAngle, Color color, float lineZoom, int segments = 12) {
        float angleStep = (endAngle - startAngle) / segments;
        Vector2 prevPoint = center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * radius;

        for (int i = 1; i <= segments; i++) {
            float angle = startAngle + i * angleStep;
            Vector2 nextPoint = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

            DrawLine2D(prevPoint, nextPoint, color, lineZoom);
            prevPoint = nextPoint;
        }
    }

    // getting to this at some point maybe

    // 3d drawing
    // should text also be drawn in 3d?
    // also, allow to draw filled spheres/circles/capsules?
    public static void DrawItem3D(ItemData it, Color color, float thickness, bool drawExtras = true) {

    }
    public static void DrawFighter3D(FighterData fd, StageData stDat, Color color, float thickness = 1f, bool drawExtras = true) {

    }
    public static void DrawECB3D(Vector2 source, ECB ecb, Color color, float thickness = 1) {

    }
    public static void DrawBoundingRect3D(BoundingRect rect, Color color, float thickness = 1f, bool drawText = false) {

    }
    public static void DrawSphereOutline(Vector2 center, float radius, Color color, int segments, float thickness = 1f) {

    }
    public static void DrawLine3D(Vector2 start, Vector2 end, Color color, float thickness = 1f) {

    }
    public static void DrawCapsuleOutline3D(Vector2 start, Vector2 end, float radius, Color color, float thickness = 1f) {

    }
    public static void DrawArc3D(Vector2 center, float radius, float startAngle, float endAngle, Color color, float lineZoom, int segments = 12) {

    }
}

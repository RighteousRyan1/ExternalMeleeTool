using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Utilities;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// could add some cool things like "average death position"
// --> this respects the blast zone so it never goes inside of it (tangent to rectangle)
namespace EMTDisplay;

// readonly?
// TODO: make EMT grab grab "last hit" data so it grabs what the fighter was killed by
// include time of death
public struct KillData {
    public int Port;
    public Vector2 Position, Velocity;
    public float Angle;
}
public class EMTDisplay : Game {
    public static GraphicsDeviceManager Graphics;
    public static SpriteBatch SpriteBatch;

    public static MatchData Match;
    public static GlobalMeleeData GlDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    public static Texture2D WhitePixel;

    public Matrix CameraMatrix;

    public static SpriteFontBase MeleeFont;
    public static SpriteFontBase Cascadia;
    FontSystem _fs;
    FontSystem _fs2;

    static readonly List<KillData> _fighterDeathLog = [];

    public static uint UpdateCount;
    public static uint UpdateCount60;

    public EMTDisplay() {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferHeight = 800;
        Graphics.PreferredBackBufferWidth = 1280;
        Window.AllowUserResizing = true;

        // InactiveSleepTime = TimeSpan.FromSeconds(1.0 / 60.0);
        InactiveSleepTime = TimeSpan.Zero;
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here
        // IsFixedTimeStep = false;
        _fs = new();
        _fs2 = new();

        base.Initialize();
    }

    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
        WhitePixel.SetData([Color.White]);

        _fs.AddFont(File.Open("Content/melee_font.ttf", FileMode.Open));
        MeleeFont = _fs.GetFont(30);
        _fs2.AddFont(File.Open("Content/cascadia.ttf", FileMode.Open));
        Cascadia = _fs2.GetFont(30);

        // TODO: use this.Content to load your game content here
    }

    static short lastFrameNum;
    static bool curIngame;
    // whether or not to start the update cycle
    static bool _calcNow;
    protected override void Update(GameTime gameTime) {
        if (!Dolphinterop.IsConnected) {
            if (!Dolphinterop.Connect("GALE01", "GTME01")) {
                return;
            }
        }
        Match = Dolphinterop.GetMatchData();
        GlDat = Dolphinterop.GetGlobalData();
        OnDat = Dolphinterop.GetOnlineData(GlDat);
        StDat = Dolphinterop.GetStageData();

        // Console.WriteLine(Match.FieldsToString());

        UpdateCount++;
        UpdateCount60 %= 60;

        if (!float.IsFinite(targetZoom) || !float.IsFinite(zoom)) {
            zoom = 1;
            targetZoom = 1;
        }

        curIngame = GlDat.IsIngame || GlDat.IsSlippiReplay || GlDat.MinorScene == 5;

        var ms = Mouse.GetState();

        try {
            if (!curIngame) {
                for (int i = 0; i < 4; i++)
                    _prevDead[i] = true; // cuz default animstate is DeadDown
                _fighterDeathLog.Clear();
            }

            if (curIngame && !_oldIngame) {
                _crashDeterrent = 60;
            }
            if (_crashDeterrent >= 0) {
                _crashDeterrent--;

                if (_crashDeterrent == 0) {
                    FitToBlastZone(StDat);
                }
                // essentially work as an update (after a second)
            }
            if (_crashDeterrent < 0 && curIngame) {
                MainUpdate(gameTime, ms);
                // Match.Fighters[0].Input.LeftStick = new(1, 0);

                // if (UpdateCount % 60 == 0)
                // i'm moving in the right direction with this...
                //var rand = new Random();
                //var randf = (float)rand.NextDouble();
                // this causes a desync
                // Dolphinterop.WriteVec2(Match.Fighters[OnDat.ClientPort].FighterPtr + 0x620, new(1, 1));

                // Dolphinterop.Write<Vector2>(Match.Fighters[0].FighterPtr + 0x638, new(randf, randf));
            }
        } catch {
            // just catch errors...? maybe it will fix itself next frame
        }

        if (Match.Frame != lastFrameNum) {
            // Console.WriteLine("UpdateFixed: " + Match.Frame);
            FixedUpdate();
        }

        _oldMs = ms;
        _oldIngame = curIngame;
        lastFrameNum = Match.Frame;

        if (IsActive)
            InputUtils.PollKBM();
    }

    public void MainUpdate(GameTime gameTime, MouseState ms) {
        if (InputUtils.KeyJustPressed(Keys.F)) {
            _writeToGameCam = !_writeToGameCam;

            Dolphinterop.SetCameraType(_writeToGameCam ? CameraKind.Develop : CameraKind.Normal);
        }

        // var camType = Dolphinterop.ReadU8(MeleeConstants.CAM_TYPE);

        if (_writeToGameCam) {
            float baseDistance = 400f;
            float zoomSpeed = 0.1f;

            float zoomDepth = -baseDistance * MathF.Exp(-zoom * zoomSpeed);
            var sysVec = new System.Numerics.Vector3(_translation.X, _translation.Y, zoomDepth);
            Dolphinterop.SetMeleeCamera(
                sysVec,
                sysVec + new System.Numerics.Vector3(0, 0, 20),
                55
            );
        }

        if (ms.ScrollWheelValue != _oldMs.ScrollWheelValue) {
            var diff = ms.ScrollWheelValue - _oldMs.ScrollWheelValue;
            targetZoom += diff / 120 * 0.2f;
            targetZoom = MathF.Max(targetZoom, 0.6f);
        }
        zoom = MathHelper.Lerp(zoom, targetZoom, 10f * gameTime.DeltaTime());

        if (ms.LeftButton == ButtonState.Pressed && IsActive) {
            _translation.X += (_oldMs.Position.X - ms.Position.X) / zoom;
            _translation.Y -= (_oldMs.Position.Y - ms.Position.Y) / zoom;
        }

        // only constantly writes if it's enabled, otherwise toggle off once
        if (_writeToGameCam) {
            Dolphinterop.SetCameraType(CameraKind.Develop);
        }
    }
    public void FixedUpdate() {
        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            if (fd.IsDead && !_prevDead[i]) {
                // a buncha junk. maybe turn into state-based addition
                var data = new KillData() {
                    Port = fd.Port,
                    Position = new Vector2(fd.Position.X, fd.Position.Y),
                    Velocity = new Vector2(_prevKbs[i].X, _prevKbs[i].Y) + new Vector2(_prevVels[i].X, _prevVels[i].Y),
                };
                data.Angle = data.Velocity.ToRotation();

                _fighterDeathLog.Add(data);
            }

            /*var bnds = StDat.GetRealCameraBounds();
            if (fd.Knockback.Y > 0) {
                if (fd.Position.Y > bnds.Top) {
                    // for some reason downwards knockback is absurd
                    fd.SetKB(new Vector3(fd.Knockback.X, fd.Knockback.Y * -0.5f, 0).ToNumerics());
                }
            }
            else if (fd.Knockback.Y < 0) {
                if (fd.Position.Y < bnds.Bottom) {
                    fd.SetKB(new Vector3(fd.Knockback.X, fd.Knockback.Y * -1, 0).ToNumerics());
                }
            }
            if (fd.Knockback.X > 0) {
                if (fd.Position.X > bnds.Right) {
                    fd.SetKB(new Vector3(fd.Knockback.X * -1, fd.Knockback.Y, 0).ToNumerics());
                }
            }
            else if (fd.Knockback.X < 0) {
                if (fd.Position.X < bnds.Left) {
                    fd.SetKB(new Vector3(fd.Knockback.X * -1, fd.Knockback.Y, 0).ToNumerics());
                }
            }*/

            _prevKbs[i] = fd.Knockback;
            _prevVels[i] = fd.VelocitySelf;
            _prevDead[i] = fd.IsDead;
            //Dolphinterop.WriteVec3(fd.FighterPtr + 0x8C,
            //    new Vector3(0, 2.5f, 0).ToNumerics());

            /*if (fd.ECB.Contains(transMouse.X, transMouse.Y)) {
                _ftHover = i; // change to i?
            }*/
        }
    }

    static int _ftHover;
    static bool _writeToGameCam;
    readonly Color[] _portColors = [Color.Red, Color.Blue, Color.Yellow, Color.Green];
    Vector3 _translation;
    MouseState _oldMs;

    static Vector2 screenCenter;

    static float zoom;
    static float targetZoom;
    static bool _oldIngame;
    static int _crashDeterrent = -1;
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Transparent);

        if (!Dolphinterop.IsConnected) {
            SpriteBatch.Begin();

            SpriteBatch.DrawString(MeleeFont,
                $"Scanning for Melee...",
                Vector2.Zero, Color.White);

            SpriteBatch.End();
            return;
        }

        // todo: draw world origin?
        if (curIngame) {
            try {
                DrawScene();
            }
            // just try and ignore
            catch { }
        }

        // draws regular info
        SpriteBatch.Begin();

        if (curIngame && StDat.StageId != ExternalStageId.DUMMY)
        SpriteBatch.DrawString(MeleeFont,
            $"Zoom: {targetZoom:F2}\n" +
            $"StageScale: {StDat.GroundParams.StageScale}\n" +
            $"IsTeams: {Match.IsTeams}\n" +
            $"GameCamWrite: {_writeToGameCam}\n", // +
            //$"joints:\n{string.Join("\n", StDat.MapJoints.Select(x => x.FieldsToString()))}",
            Vector2.Zero, Color.White,
            scale: new Vector2(0.5f));

        var linesColor = Color.Gray;
        var linesLen = 8;
        DrawLine(screenCenter - new Vector2(linesLen, 0), screenCenter + new Vector2(linesLen, 0), linesColor);
        DrawLine(screenCenter - new Vector2(0, linesLen), screenCenter + new Vector2(0, linesLen), linesColor);

        SpriteBatch.End();

        base.Draw(gameTime);
    }

    public static int drawSchema;
    public void DrawScene() {
        var stageScale = StDat.GroundParams.StageScale;
        screenCenter = new(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
        var lineZoom = 1f / zoom;
        CameraMatrix =
            Matrix.CreateTranslation(-_translation.X, -_translation.Y, 0) *
            // -1 ensures melee coordinates
            Matrix.CreateScale(1, -1, 1) *
            Matrix.CreateScale(zoom) *
            Matrix.CreateTranslation(new(screenCenter, 0));

        SpriteBatch.Begin(transformMatrix: CameraMatrix, rasterizerState: RasterizerState.CullNone);

        // the zones already account for stage scale parameter...
        DrawBoundingRect(StDat.GetRealCameraBounds(), Color.CadetBlue, lineZoom, true);
        DrawBoundingRect(StDat.GetRealBlastZone(), Color.DarkRed, lineZoom, true);

        for (int i = 0; i < StDat.Collision.line_count; i++) {
            var lineDesc = StDat.MapLines[i];
            var lStart = StDat.Vertices[lineDesc.StartIdx] * stageScale;
            var lEnd = StDat.Vertices[lineDesc.EndIdx] * stageScale;

            // if StDat.Collision.joints.vtx_start or whatever contains this, ignore it so its drawn as moving

            var newColor = drawSchema switch {
                0 => MeleeDisplayUtils.MatTypeToColor[lineDesc.material_type],
                // there's more colltypes than i imagined previously..?
                1 => MeleeDisplayUtils.CollTypeToColor[lineDesc.coll_type],
                2 => MeleeDisplayUtils.InteractTypeToColor[lineDesc.interact_type],
                _ => throw new Exception("Bruh")
            };

            // var rotation = (lEnd - lStart).ToXNA().ToRotation();

            DrawLine(lStart, lEnd, newColor, lineZoom);

            var info1 = $"{lineDesc.StartIdx}";
            SpriteBatch.DrawString(Cascadia, info1,
                lStart,
                color: newColor,
                scale: new Vector2(0.04f, -0.04f),
                rotation: 0f, // rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info1)));

            var info2 = $"{lineDesc.EndIdx}";
            SpriteBatch.DrawString(Cascadia, info2,
                lEnd,
                color: newColor,
                scale: new Vector2(0.04f, -0.04f),
                rotation: 0f, // rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info2)));

            /*var info = lineDesc.ToString(); // + $"{(Match.Fighters[0].CollData.env_flags == (byte)lineDesc.coll_type)}";
            SpriteBatch.DrawString(Cascadia, info,
                MathUtils.GetMidpoint(lStart, lEnd), 
                color: newColor, 
                scale: new Vector2(0.04f, -0.04f),
                rotation: rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info)));*/
        }

        var lineLen = 5f;
        for (int i = 0; i < _fighterDeathLog.Count; i++) {
            var data = _fighterDeathLog[i];

            var botCross = new Vector2(lineLen);
            var topCross = new Vector2(lineLen, -lineLen);
            DrawLine(data.Position - botCross, data.Position + botCross, _portColors[data.Port], lineZoom);
            DrawLine(data.Position - topCross, data.Position + topCross, _portColors[data.Port], lineZoom);

            // draws velocity arrow
            var start = data.Position;
            var end = start + data.Velocity * 5f;

            DrawLine(start, end, _portColors[data.Port] * 0.5f, lineZoom);

            // draw caret
            var dir = Vector2.Normalize(end - start);
            var perp = new Vector2(-dir.Y, dir.X);

            float caretLength = 2f;
            float caretWidth = 1.5f;

            var left = end - dir * caretLength + perp * caretWidth;
            var right = end - dir * caretLength - perp * caretWidth;
            DrawLine(end, left, _portColors[data.Port] * 0.5f, lineZoom);
            DrawLine(end, right, _portColors[data.Port] * 0.5f, lineZoom);
        }
        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            DrawMeleePlayer(fd, _portColors[i], lineZoom);
        }

        SpriteBatch.End();
    }

    // draws using melee coordinates, Y = -Y
    public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f) {
        SpriteBatch.Draw(WhitePixel, start, null, color,
            (end - start).ToRotation(),
            new Vector2(0, 0.5f),
            new Vector2(Vector2.Distance(start, end), thickness),
            SpriteEffects.None, 0);
    }
    // eventually: draw ecb
    static string[] _infoArr;
    static readonly Vector3[] _prevVels = new Vector3[4];
    static readonly Vector3[] _prevKbs = new Vector3[4];
    static readonly bool[] _prevDead = new bool[4];
    public static bool DrawECBs = true;
    public static bool DrawHitboxes = true;
    public static bool DrawHurtboxes = true;
    public static bool DrawShields = true;
    public static bool DrawLedgeGrabBoxes = true;
    public static bool DrawUsefulPlayerData = true;


    public static void DrawMeleePlayer(FighterData fd, Color color, float thickness = 1f, bool drawExtras = true) {
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

                if (hb.capsule.state == HurtCapsuleState.Disabled) continue;
                if (hb.capsule.scale > 5) continue; // something has gone horribly wrong?

                // var jobj = Dolphinterop.Read<HSD_JObj>(hb.capsule.bone, -MeleeGlobals.ROM_SIZE);

                SpriteBatch.Draw(WhitePixel, hb.capsule.b_pos.ToXNA().Flatten(), null, Color.DeepSkyBlue, 0f, WhitePixel.Size() / 2, hb.capsule.scale, default, 0f);

                SpriteBatch.Draw(WhitePixel, hb.capsule.a_pos.ToXNA().Flatten(), null, Color.Orange, 0f, WhitePixel.Size() / 2, hb.capsule.scale, default, 0f);
            }
        }

        if (DrawHitboxes) {
            for (int i = 0; i < FighterData.HitCapsuleBuffer4.LENGTH; i++) {
                var hb = fd.Hitboxes[i];

                if (hb.state == HitCapsuleState.Disabled) continue;

                var cpos = hb.b_pos.ToXNA().Flatten();
                SpriteBatch.Draw(WhitePixel, cpos, null, Color.IndianRed, 0f, WhitePixel.Size() / 2, hb.scale, default, 0f);
                SpriteBatch.DrawString(Cascadia, hb.state.ToString(),
                    cpos,
                    color: Color.IndianRed,
                    scale: new Vector2(0.04f, -0.04f),
                    rotation: 0f);
            }
        }
        #endregion

        #region Shields
        if (DrawShields && fd.IsShielding) {
            const float magic_number = 2f;
            // lerp between initial size and 0.2f... or something?
            // this is not quite right but good enough
            var shieldSize = fd.Attr.initial_shield_size * (fd.ShieldHealth / 60) / (fd.Input.Triggers * magic_number); // / (fd.Input.Triggers * magic_number);
            // i'm not entirely sure of the sauce behind this yet
            //var shieldSizeAdjusted = fd.Attr.initial_shield_size / (fd.Input.Triggers * magic_number);
            //var shieldSize = MathHelper.Lerp(2f, shieldSizeAdjusted, fd.ShieldHealth / 60);
            // there's probably something in Fighter controlling this
            DrawCircleOutline(pos + ecb.Center, shieldSize, Color.SkyBlue, 32, thickness);
        }
        #endregion

        #region Ledgegrab Boxes

        if (DrawLedgeGrabBoxes) {
            // subtracting magic numbers for now
            float visualSeparationOtherwiseYouCantSeeAColor = 0.025f;
            // right box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X + fd.CollData.ledge_snap_x,
                Left = pos.X + visualSeparationOtherwiseYouCantSeeAColor,
                Bottom = pos.Y + fd.CollData.ledge_snap_y - fd.CollData.ledge_snap_height * 0.5f
            }, Color.Red, thickness, false);
            // left box
            DrawBoundingRect(new BoundingRect {
                Top = pos.Y + fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f,
                Right = pos.X - visualSeparationOtherwiseYouCantSeeAColor,
                Left = pos.X - fd.CollData.ledge_snap_x,
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

        var realCamBounds = StDat.GetRealCameraBounds();
        var realBlast = StDat.GetRealBlastZone();

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

        if (!DrawUsefulPlayerData) return;

        if (!drawExtras) return;

        #region Extra Details
        _infoArr = [
            $"kind: {fd.CharKind}",
            $"pos:  <{pos.X:F2}, {pos.Y:F2}>",
            $"anim: {fd.AnimState}",
            $"sh:   {fd.ShieldHealth}",
            $"%:    {fd.Percent}"
        ];

        var scale = 0.1f;
        var yOffset = new Vector2(0, fd.CollData.ledge_snap_y + fd.CollData.ledge_snap_height * 0.5f); // new Vector2(ecb.Top.X, ecb.Top.Y);
        for (int i = _infoArr.Length - 1; i >= 0; i--) {
            var info = _infoArr[i];
            SpriteBatch.DrawString(Cascadia, info,
                new Vector2(pos.X, pos.Y + i * 30 * scale) + yOffset, color, 
                scale: new Vector2(scale, -scale),
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info)));
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
        // flip Y because Y = Up in 2d space
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

        // i have to flip vertical text along the X axis
        // horizontal text along the Y axis...
        float zoneTextScale = 0.25f;
        SpriteBatch.DrawString(MeleeFont, topStr,
            (topLeft + topRight) / 2, color, scale: new Vector2(zoneTextScale, -zoneTextScale), 
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(topStr)));
        SpriteBatch.DrawString(MeleeFont, rightStr,
            (topRight + bottomRight) / 2, color, scale: new Vector2(-zoneTextScale, zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(rightStr)),
            rotation: MathHelper.PiOver2);
        SpriteBatch.DrawString(MeleeFont, botStr,
            (bottomRight + bottomLeft) / 2, color, scale: new Vector2(zoneTextScale, -zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.TopCenter, MeleeFont.MeasureString(botStr)));

        SpriteBatch.DrawString(MeleeFont, leftStr,
            (bottomLeft + topLeft) / 2, color, scale: new Vector2(-zoneTextScale, zoneTextScale),
            origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(leftStr)),
            rotation: -MathHelper.PiOver2);
    }

    // MATH:

    public void FitToBlastZone(StageData stage, float padding = 1.1f) {
        // 1. Calculate the center of the blast zone in world coordinates
        var blastZone = stage.GetRealBlastZone();
        float worldWidth = blastZone.Right - blastZone.Left;
        float worldHeight = blastZone.Top - blastZone.Bottom;

        // Set translation to the center of the zone
        _translation = new Vector3(
            (blastZone.Left + blastZone.Right) / 2f,
            (blastZone.Bottom + blastZone.Top) / 2f,
            0
        );

        // resolution independence
        float zoomX = GraphicsDevice.Viewport.Width / (worldWidth * padding);
        float zoomY = GraphicsDevice.Viewport.Height / (worldHeight * padding);

        targetZoom = Math.Min(zoomX, zoomY);
    }

    private static void DrawCircleOutline(Vector2 center, float radius, Color color, int segments, float thickness = 1f) {
        float angleStep = (float)(Math.PI * 2.0 / segments);
        Vector2 lastPoint = new(center.X + radius, center.Y);

        for (int i = 1; i <= segments; i++) {
            float angle = i * angleStep;
            Vector2 nextPoint = new(
                center.X + (float)Math.Cos(angle) * radius,
                center.Y + (float)Math.Sin(angle) * radius
            );

            // Replace this with your specific API (e.g., GX_Line, Gizmos.DrawLine, etc)
            DrawLine(lastPoint, nextPoint, color, thickness);
            lastPoint = nextPoint;
        }
    }
}


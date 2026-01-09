using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ExternalMeleeTool;
using System;
using FontStashSharp;
using System.IO;
using EMTDisplay.Utils;
using ExternalMeleeTool.Melee;
using System.Collections.Generic;

// could add some cool things like "average death position"
// --> this respects the blast zone so it never goes inside of it (tangent to rectangle)
namespace EMTDisplay;

// readonly?
// TODO: make EMT grab grab "last hit" data so it grabs what the fighter was killed by
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

    public EMTDisplay() {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Graphics.PreferredBackBufferHeight = 800;
        Graphics.PreferredBackBufferWidth = 1280;
        Window.AllowUserResizing = true;

        InactiveSleepTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here
        IsFixedTimeStep = false;
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

    protected override void Update(GameTime gameTime) {
        if (!Dolphinterop.IsConnected) {
            if (!Dolphinterop.Connect("GALE01", "GTME01")) {
                return;
            }
        }
        Match = Dolphinterop.GetMatchData();
        GlDat = Dolphinterop.GetGlobalData();
        OnDat = Dolphinterop.GetOnlineData(GlDat);
        StDat = Dolphinterop.GetStageData(GlDat);

        var ms = Mouse.GetState();

        try {
            if (!GlDat.IsIngame) {
                for (int i = 0; i < 4; i++)
                    _prevDead[i] = true; // cuz default animstate is DeadDown
                _fighterDeathLog.Clear();
            }

            if (GlDat.IsIngame && !_oldIngame) {
                _crashDeterrent = 60;
            }
            if (_crashDeterrent >= 0) {
                _crashDeterrent--;

                if (_crashDeterrent == 0) {
                    FitToBlastZone(StDat);
                }
                // essentially work as an update (after a second)
            }
            if (_crashDeterrent < 0 && GlDat.IsIngame) {
                MainUpdate(gameTime, ms);
            }
        } catch {
            // just catch errors...? maybe it will fix itself next frame
        }

        _oldMs = ms;
        _oldIngame = GlDat.IsIngame;

        if (IsActive)
            InputUtils.PollKBM();
    }

    public void MainUpdate(GameTime gameTime, MouseState ms) {
        if (InputUtils.KeyJustPressed(Keys.F)) {
            _writeToGameCam = !_writeToGameCam;
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
                60
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

            if (!_writeToGameCam) {
                Dolphinterop.SetCameraType(CameraKind.Normal);
            }
        }

        // only constantly writes if it's enabled, otherwise toggle off once
        if (_writeToGameCam) {
            Dolphinterop.SetCameraType(_writeToGameCam ? CameraKind.Develop : CameraKind.Normal);
        }

        // will never work i guess
        // _ftHover = -1;
        // var transMouse = Vector2.Transform(new Vector2(ms.X, ms.Y), CameraMatrix);
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
        if (GlDat.IsIngame)
            DrawScene();

        // draws regular info
        SpriteBatch.Begin();

        SpriteBatch.DrawString(MeleeFont,
            $"Zoom: {targetZoom:F2}\n" +
            $"StageScale: {StDat.GroundParams.StageScale}\n" +
            $"IsTeams: {Match.IsTeams}\n" +
            $"GameCamWrite: {_writeToGameCam}\n" +
            $"ftHover: {_ftHover}",
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

        for (int i = 0; i < StDat.LineCount; i++) {
            var lineDesc = StDat.MapLines[i];
            var lStart = StDat.Vertices[lineDesc.StartIdx] * stageScale;
            var lEnd = StDat.Vertices[lineDesc.EndIdx] * stageScale;
            var newColor = drawSchema switch {
                0 => MeleeDisplayUtils.MatTypeToColor[lineDesc.material_type],
                // there's more colltypes than i imagined previously..?
                1 => MeleeDisplayUtils.CollTypeToColor[lineDesc.coll_type],
                2 => MeleeDisplayUtils.InteractTypeToColor[lineDesc.interact_type],
                _ => throw new Exception("Bruh")
            };

            // var rotation = (lEnd - lStart).ToXNA().ToRotation();

            DrawLine(lStart, lEnd, newColor, lineZoom);

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
    Vector3[] _prevVels = new Vector3[4];
    Vector3[] _prevKbs = new Vector3[4];
    bool[] _prevDead = new bool[4];
    public static void DrawMeleePlayer(FighterData fd, Color color, float thickness = 1f, bool drawExtras = true) {
        var pos = new Vector2(fd.Position.X, fd.Position.Y);
        var ecb = fd.CollData.ecb;

        #region ECBs
        // because of update order these end up being the same at the end of the frame
        //DrawECB(pos, fd.CollData.desired_ecb, Color.Orange * 0.25f, thickness);
        DrawECB(pos, fd.CollData.prev_ecb, Color.Sienna, thickness);
        DrawECB(pos, ecb, color, thickness);
        #endregion
        // draw ledge grab boxes
        // var baseBoneTransform = fd.GetBoneTransform(FtPart.FtPart_WaistN).Translation;
        // right box

        #region Ledgegrab Boxes
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

        if (!drawExtras) return;

        #region Extra Details
        _infoArr = [
            $"{fd.CharKind}",
            $"<{pos.X:F2}, {pos.Y:F2}>",
            $"{fd.AnimState}",
            $"{fd.Knockback}",
            // $"{fd.CollData.FieldsToString()}"
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

        //if (fd.SlotKind == SlotKind.Human) {
        //    Slippinterop.WriteVec3(fd.Fighter + 0xB0, new System.Numerics.Vector3(0, 10, 0));
        //}
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
}

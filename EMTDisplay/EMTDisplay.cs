using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace EMTDisplay;

// readonly?
// TODO: make EMT grab grab "last hit" data so it grabs what the fighter was killed by
// include time of death
// could add some cool things like "average death position"
// --> this respects the blast zone so it never goes inside of it (tangent to rectangle)
public struct KillData {
    public int Port;
    public Vector2 Position, Velocity;
    public float Angle;
}
public class EMTDisplay : Game {
    public static GraphicsDeviceManager Graphics;
    public static SpriteBatch SpriteBatch;

    public static TimeSpan RenderTime;
    public static double RenderFPS;
    public static TimeSpan LogicTime;
    public static double LogicFPS;

    public static MatchData Match;
    public static SceneData ScDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    public Matrix CameraMatrix;

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

        base.Initialize();
    }

    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

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

        try {
            Match = Dolphinterop.GetMatchData();
            ScDat = Dolphinterop.GetGlobalData();
            OnDat = Dolphinterop.GetOnlineData(ScDat);
            StDat = Dolphinterop.GetStageData();
        }
        // just try and ignore
        catch (Exception e) {
            Console.WriteLine(e);
            Console.WriteLine(e.StackTrace);
        }

        // Console.WriteLine(Match.FieldsToString());

        UpdateCount++;
        UpdateCount60 %= 60;

        if (Match.Frame != lastFrameNum) {
            // Console.WriteLine("UpdateFixed: " + Match.Frame);
            FixedUpdate();
        }

        if (!float.IsFinite(targetZoom) || !float.IsFinite(zoom)) {
            zoom = 1;
            targetZoom = 1;
        }

        curIngame = ScDat.IsIngame || ScDat.IsSlippiReplay || ScDat.IsUnclePunch || ScDat.MinorScene == 5;

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

        _oldMs = ms;
        _oldIngame = curIngame;
        lastFrameNum = Match.Frame;

        if (IsActive)
            InputUtils.PollKBM();

        LogicTime = gameTime.ElapsedGameTime;
        LogicFPS = 1f / LogicTime.TotalSeconds;
    }

    public void MainUpdate(GameTime gameTime, MouseState ms) {
        if (InputUtils.KeyJustPressed(Keys.F)) {
            _writeToGameCam = !_writeToGameCam;

            Dolphinterop.SetCameraType(_writeToGameCam ? CameraKind.Develop : CameraKind.Normal);
        }

        // var camType = Dolphinterop.ReadU8(MeleeGlobals.CAM_TYPE);

        if (_writeToGameCam) {
            float baseDistance = -400f;
            // float zoomSpeed = 0.1f;

            // float zoomDepth = -baseDistance * MathF.Exp(-zoom * zoomSpeed);
            var sysVec = new System.Numerics.Vector3(_translation.X, _translation.Y, baseDistance/*zoomDepth*/);
            // arbitrary ahh
            var fovSet = 1f / zoom * 115 * GraphicsDevice.Viewport.AspectRatio;
            Dolphinterop.SetMeleeCamera(
                sysVec,
                sysVec + new System.Numerics.Vector3(0, 0, 20),
                // 40
                fovSet
            );
        }

        if (ms.ScrollWheelValue != _oldMs.ScrollWheelValue) {
            var diff = ms.ScrollWheelValue - _oldMs.ScrollWheelValue;
            targetZoom += diff / 120 * 0.2f;
            targetZoom = MathF.Max(targetZoom, 0.2f);
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

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
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
            catch(Exception e) {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
                SpriteBatch.End();
            }
        }

        // draws regular info
        SpriteBatch.Begin();

        if (curIngame && StDat.StageId != ExternalStageId.DUMMY)
        SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
            $"MeleeFrame: {Match.Frame}\n" +
            $"Logic: [FPS={LogicFPS:F2} Time={LogicTime.TotalMilliseconds:F2}ms]\n" +
            $"Render: [FPS={RenderFPS:F2} Time={RenderTime.TotalMilliseconds:F2}ms]\n" +
            $"Zoom: {targetZoom:F2}\n" +
            $"StageScale: {StDat.GroundParams.StageScale}\n" +
            $"IsTeams: {Match.IsTeams}\n" +
            $"Stage: {StDat.StageId}\n" +
            $"GameCamWrite: {_writeToGameCam}\n", // +
            //$"joints:\n{string.Join("\n", StDat.MapJoints.Select(x => x.FieldsToString()))}",
            Vector2.Zero, Color.White,
            scale: new Vector2(0.5f));

        var linesColor = Color.Gray;
        var linesLen = 8;
        MeleeDrawing.DrawLine(screenCenter - new Vector2(linesLen, 0), screenCenter + new Vector2(linesLen, 0), linesColor);
        MeleeDrawing.DrawLine(screenCenter - new Vector2(0, linesLen), screenCenter + new Vector2(0, linesLen), linesColor);

        // draw percents at bottom (for cool ahh melee style)
        var divs = GraphicsDevice.Viewport.Width / (Match.Fighters.Length + 1);
        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            var cKind = fd.CharKind.ToString();
            SpriteBatch.DrawString(MeleeDrawing.MeleeFont, cKind,
                    new Vector2(divs * (i + 1), GraphicsDevice.Viewport.Height - 100),
                    color: _portColors[i],
                    scale: new Vector2(1f),
                    rotation: 0f,
                    origin: MeleeDrawing.MeleeFont.MeasureString(cKind) / 2);

            var stocks = fd.Stocks.ToString();
            SpriteBatch.DrawString(MeleeDrawing.MeleeFont, stocks,
                    new Vector2(divs * (i + 1), GraphicsDevice.Viewport.Height - 15),
                    color: _portColors[i],
                    scale: new Vector2(0.75f),
                    rotation: 0f,
                    origin: MeleeDrawing.MeleeFont.MeasureString(stocks) / 2);

            var percent = fd.Percent.ToString() + "%";
            SpriteBatch.DrawString(MeleeDrawing.MeleeFont, percent,
                    new Vector2(divs * (i + 1), GraphicsDevice.Viewport.Height - 50),
                    color: _portColors[i],
                    scale: new Vector2(2f),
                    rotation: 0f,
                    origin: MeleeDrawing.MeleeFont.MeasureString(percent) / 2);
        }

        SpriteBatch.End();

        RenderTime = gameTime.ElapsedGameTime;
        RenderFPS = 1f / RenderTime.TotalSeconds;
    }

    public static int drawSchema;

    public static int PlayerFocus = -1;

    public static List<LineSegment> MapLineSegments = [];
    public void DrawScene() {
        var stageScale = StDat.GroundParams.StageScale;
        screenCenter = new(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);

        var hasPlayerFocus = PlayerFocus >= 0 && PlayerFocus < Match.Fighters.Length;
        if (hasPlayerFocus) {
            var plr = Match.Fighters[PlayerFocus];
            // var pos = new Vector2(plr.Position.X, plr.Position.Y) + plr.CollData.ecb.Center;
            var bone = plr.GetBone(FtPart.RShoulderN); // the actual head... weirdly enough
            var jobj = Dolphinterop.Read<JObj>(bone.jobj);

            _translation = new Vector3(jobj.mtx.Translation.X, jobj.mtx.Translation.Y, 0);
        }

        var transMatrix = Matrix.CreateTranslation(-_translation.X, -_translation.Y, 0);

        var lineThickness = 1f / zoom;
        CameraMatrix =
            transMatrix *
            // -1 ensures melee coordinates
            Matrix.CreateScale(1, -1, 1) *
            Matrix.CreateScale(zoom) *
            Matrix.CreateTranslation(new(screenCenter, 0));

        SpriteBatch.Begin(transformMatrix: CameraMatrix, rasterizerState: RasterizerState.CullNone);

        // the zones already account for stage scale parameter...
        MeleeDrawing.DrawBoundingRect(StDat.GetRealCameraBounds(), Color.CadetBlue, lineThickness, true);
        MeleeDrawing.DrawBoundingRect(StDat.GetRealBlastZone(), Color.DarkRed, lineThickness, true);

        MapLineSegments.Clear();
        // Icicle mountain lags the FUCK out of this
        for (int i = 0; i < StDat.Collision.line_count; i++) {
            var lineDesc = StDat.MapLines[i];
            var lStart = StDat.Vertices[lineDesc.StartIdx] * stageScale;
            var lEnd = StDat.Vertices[lineDesc.EndIdx] * stageScale;

            MapLineSegments.Add(new(lStart, lEnd));

            // if StDat.Collision.joints.vtx_start or whatever contains this, ignore it so its drawn as moving

            var newColor = drawSchema switch {
                0 => MeleeDisplayUtils.MatTypeToColor[lineDesc.material_type],
                // there's more colltypes than i imagined previously..?
                1 => MeleeDisplayUtils.CollKindToColor[lineDesc.coll_type],
                2 => MeleeDisplayUtils.CollPropertyToColor[lineDesc.coll_property],
                _ => throw new Exception("Bruh")
            };

            // var rotation = (lEnd - lStart).ToXNA().ToRotation();

            MeleeDrawing.DrawLine(lStart, lEnd, newColor, lineThickness);

            var vtxScale = 0.04f;

            var info1 = $"{lineDesc.StartIdx}";
            SpriteBatch.DrawString(MeleeDrawing.Cascadia, info1,
                lStart,
                color: newColor,
                scale: new Vector2(vtxScale, -vtxScale),
                rotation: 0f, // rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeDrawing.Cascadia.MeasureString(info1)));

            var info2 = $"{lineDesc.EndIdx}";
            SpriteBatch.DrawString(MeleeDrawing.Cascadia, info2,
                lEnd,
                color: newColor,
                scale: new Vector2(vtxScale, -vtxScale),
                rotation: 0f, // rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeDrawing.Cascadia.MeasureString(info2)));

            /*var info = lineDesc.ToString(); // + $"{(Match.Fighters[0].CollData.env_flags == (byte)lineDesc.coll_type)}";
            SpriteBatch.DrawString(Cascadia, info,
                MathUtils.GetMidpoint(lStart, lEnd), 
                color: newColor, 
                scale: new Vector2(0.04f, -0.04f),
                rotation: rotation,
                origin: RenderUtils.GetAnchor(Anchor.BottomCenter, MeleeFont.MeasureString(info)));*/
        }

        /*if (StDat.CollGroups != null) {
            for (int i = 0; i < StDat.CollGroups.Length; i++) {
                var cg = StDat.CollGroups[i];

                var br = new BoundingRect(cg.left_bound, cg.top_bound, cg.right_bound, cg.bottom_bound);
                MeleeDrawing.DrawBoundingRect(br, Color.Red, lineThickness);
            }
        }*/

        var lineLen = 5f;
        for (int i = 0; i < _fighterDeathLog.Count; i++) {
            var data = _fighterDeathLog[i];

            var botCross = new Vector2(lineLen);
            var topCross = new Vector2(lineLen, -lineLen);
            MeleeDrawing.DrawLine(data.Position - botCross, data.Position + botCross, _portColors[data.Port], lineThickness);
            MeleeDrawing.DrawLine(data.Position - topCross, data.Position + topCross, _portColors[data.Port], lineThickness);

            // draws velocity arrow
            var start = data.Position;
            var end = start + data.Velocity * 5f;

            MeleeDrawing.DrawLine(start, end, _portColors[data.Port] * 0.5f, lineThickness);

            // draw caret
            var dir = Vector2.Normalize(end - start);
            var perp = new Vector2(-dir.Y, dir.X);

            float caretLength = 2f;
            float caretWidth = 1.5f;

            var left = end - dir * caretLength + perp * caretWidth;
            var right = end - dir * caretLength - perp * caretWidth;
            MeleeDrawing.DrawLine(end, left, _portColors[data.Port] * 0.5f, lineThickness);
            MeleeDrawing.DrawLine(end, right, _portColors[data.Port] * 0.5f, lineThickness);
        }

        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            MeleeDrawing.DrawMeleePlayer(fd, StDat, _portColors[i], lineThickness);

            MeleeDrawing.DrawFighterPrediction(fd, lineThickness);
        }
        for (int i = 0; i < Match.Items.Count; i++) {
            var item = Match.Items[i];

            // if (item.ecb.Top > 50 || item.ecb.Right > 50) continue;
            // typically an invalid item..?
            // i think this is the best indicator of garbage values

            MeleeDrawing.DrawItem(item, Color.White, lineThickness);
        }

        SpriteBatch.End();
    }
    // eventually: draw ecb
    static readonly Vector3[] _prevVels = new Vector3[4];
    static readonly Vector3[] _prevKbs = new Vector3[4];
    static readonly bool[] _prevDead = new bool[4];

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


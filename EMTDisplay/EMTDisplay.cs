using EMTDisplay.CamExperiments;
using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EMTDisplay;

// readonly?
// TODO: make EMT grab "last hit" data so it grabs what the fighter was killed by
// include time of death
// could add some cool things like "average death position"
// --> this respects the blast zone so it never goes inside of it (tangent to rectangle)
public struct KillData {
    public int Port;
    public Vector2 Position, Velocity;
    public float Angle;
}
public struct HitData {
    public float Angle;
    public FtPart Bone;
    public string AtkSymbol;
}

public class EMTDisplay : Game {
    public static GraphicsDeviceManager Graphics;
    public static SpriteBatch SpriteBatch;

    public static TimeSpan RenderTime;
    public static double RenderFPS;
    public static TimeSpan LogicTime;
    public static double LogicFPS;

    public static float TotalTime;

    public static MatchData Match;
    public static SceneData ScDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;
    public static Camera MeleeCamera;
    public static CObj MeleeCamCobj;

    static WObj CamEyeWObj;
    static WObj CamIntWObj;

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

        InactiveSleepTime = TimeSpan.Zero;
    }

    protected override void Initialize() {
        // IsFixedTimeStep = false;
        // Graphics.SynchronizeWithVerticalRetrace = false;

        base.Initialize();
    }

    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    static short lastFrameNum;
    static bool curIngame;
    // whether or not to start the update cycle
    static bool _plCoGet;
    protected override void Update(GameTime gameTime) {
        UpdateCount++;
        UpdateCount60 %= 60;

        if (!Dolphinterop.IsConnected) {
            // NTSC, Training Mode, NTSJ
            if (!Dolphinterop.Connect("GALE01", "GTME01", "GALJ01")) {
                return;
            }
        }

        try {
            Match = MatchData.GetMatchData();
            ScDat = SceneData.GetSceneData();



            OnDat = SlippiOnlineData.GetOnlineData(ScDat);
            StDat = StageData.GetStageData();
            MeleeCamera = Camera.GetMeleeCamera();

            var cgobj = MeleeCamera.gobj.As<GObj>();
            MeleeCamCobj = cgobj.hsd_obj.As<CObj>();

            // world objects for eye and interest
            CamEyeWObj = MeleeCamCobj.eye.As<WObj>();
            CamIntWObj = MeleeCamCobj.interest.As<WObj>();

            if (!_plCoGet) {
                if (FighterData.TryGetPlCo()) {
                    _plCoGet = true;
                }
            }
            else {
                //FighterData.PlCo.sdi_dist = 10;
                // FighterData.PlCo.hitstun_mult = 2;
                // Dolphinterop.Write(Dolphinterop.ReadPtr(MeleePointers.PLCO_PTR), FighterData.PlCo);
                // Console.WriteLine(MeleeCamera.FieldsToString());
            }
        }
        // just try and ignore
        catch (Exception e) {
            Console.WriteLine(e);
            Console.WriteLine(e.StackTrace);
        }
        /*int[] boneManipList = [27]; //, 28];
        for (int i = 0; i < boneManipList.Length; i++) {
            var num = boneManipList[i];

            var b = fd.GetUnmappedBone(num);
            var bjobj = Dolphinterop.Read<JObj>(b.bone);
            // var bjobji = Dolphinterop.Read<JObj>(b.jobj_interpolate);
            // bjobj.flags &= ~JObjFlags.Hidden;
            // bjobj.flags |= JObjFlags.Hidden;

            float sin = (fd.Percent * 0.2f) + 1; // ((MathF.Sin(TotalTime) + 50) * 0.5f) + 0.2f;
            var randScale = new System.Numerics.Vector3(
                //rand.NextFloat(0.5f, 1.5f),
                //rand.NextFloat(0.5f, 1.5f),
                //rand.NextFloat(0.5f, 1.5f)
                1,
                bjobj.scale.Y,
                sin
            );
            //var randPos = new System.Numerics.Vector3(
            //    rand.NextFloat(-5f, 5f),
            //    rand.NextFloat(-5f, 5f),
            //    rand.NextFloat(-5f, 5f)
            //);
            bjobj.mtx.Rotation = new() {
                X = bjobj.mtx.Rotation.X * randScale.X,
                Y = bjobj.mtx.Rotation.Y * randScale.Y,
                Z = bjobj.mtx.Rotation.Z * randScale.Z,
            };
            // bjobj.mtx.Translation += randPos;

            bjobj.scale = randScale;
            Dolphinterop.WriteVec3(b.bone + 0x2C, bjobj.scale);
            Dolphinterop.WriteVec3(b.jobj_interpolate + 0x2C, bjobj.scale);
        }*/

        if (Match.Frame != lastFrameNum) {
            // Console.WriteLine("UpdateFixed: " + Match.Frame);
            FixedUpdate();
        }

        if (!float.IsFinite(targetZoom) || !float.IsFinite(zoom)) {
            zoom = 1;
            targetZoom = 1;
        }

        // i forget why there is a minorscene check.
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

                if (_crashDeterrent == 0 && PlayerFocus < 0) {
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
        } catch(Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine(e.StackTrace);
            // just catch errors...? maybe it will fix itself next frame
        }

        _oldMs = ms;
        _oldIngame = curIngame;
        lastFrameNum = Match.Frame;

        if (IsActive)
            InputUtils.PollKBM();

        MeleeEvents.PollEvents(Match, OnDat, ScDat);

        LogicTime = gameTime.ElapsedGameTime;
        LogicFPS = 1f / LogicTime.TotalSeconds;
        TotalTime += (float)LogicTime.TotalSeconds;
    }

    static float _startZoom;
    static float _targetZoom;
    static float _zoomElapsedSeconds;
    static float _zoomDurationSeconds;
    static bool _isZoomingAnim;
    static EasingFunction _camEasing = EasingFunction.InOutQuad;
    public static void SetZoomTarget(float newTarget, TimeSpan duration, EasingFunction easing = EasingFunction.InOutQuad) {
        _startZoom = zoom;
        _targetZoom = newTarget;
        _zoomDurationSeconds = (float)duration.TotalSeconds;
        _zoomElapsedSeconds = 0f;
        _isZoomingAnim = true;
        _camEasing = easing;
    }

    static Vector2 _startTranslation;
    static Vector2 _targetTranslation;
    static float _translationElapsedSeconds;
    static float _translationDurationSeconds;
    static bool _isTranslatingAnim;
    static EasingFunction _translationEasing = EasingFunction.InOutQuad;
    public static void SetTranslationTarget(Vector2 target, TimeSpan duration, EasingFunction easing = EasingFunction.InOutQuad) {
        _startTranslation = _translation; // Lock in the starting position
        _targetTranslation = target;
        _translationDurationSeconds = (float)duration.TotalSeconds;
        _translationElapsedSeconds = 0f;
        _isTranslatingAnim = true;
        _translationEasing = easing;
    }

    JsonSerializerOptions _indent = new JsonSerializerOptions() { 
        WriteIndented = true, 
        IncludeFields = true, 
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    
    struct CamKf {
        public Vector3 eye;
        public Vector3 interest;
        public Vector3 up;
        public float fov;

        public static bool Compare(CamKf a, CamKf b) => a.eye == b.eye && a.interest == b.interest && a.up == b.up && a.fov == b.fov;
    }
    List<CamKf> _kfs = [];
    bool _recording;
    bool _awaitingConf;
    public void MainUpdate(GameTime gameTime, MouseState ms) {
        if (InputUtils.KeyJustPressed(Keys.F)) {
            _writeToGameCam = !_writeToGameCam;

            Camera.SetCameraType(_writeToGameCam ? CameraType.DebugFree : CameraType.Standard);
        }


        // other "key bindings"

        if (InputUtils.KeyJustPressed(Keys.P)) {

            // if waiting for a nother confirmation, do nothing until it's confirmed
            if (!_awaitingConf) {
                _recording = !_recording;

                if (!_recording) {
                    _awaitingConf = true;
                    Console.Write("Would you like to trim identical keyframes starting from the beginning? (y/n) ");
                    var resp = Console.ReadLine();

                    if (resp.Equals("y", StringComparison.InvariantCultureIgnoreCase)) {
                        var initFrame = _kfs[0];
                        int trimEnd = -1; // where the list will stop being trimmed
                        for (int i = 0; i < _kfs.Count; i++) {
                            if (!CamKf.Compare(initFrame, _kfs[i])) {
                                trimEnd = i;
                                break;
                            }
                        }

                        _kfs = _kfs[trimEnd..];
                    }

                    var text = JsonSerializer.Serialize(_kfs, _indent);
                    var jsonName = "camera_kfs.json";

                    File.WriteAllText(jsonName, text);
                    Console.WriteLine($"Saved {_kfs.Count} keyframes to {jsonName}");
                    _kfs.Clear();
                    _awaitingConf = false;
                }
            }
        }

        if (_recording) {
            var kf = new CamKf {
                eye = CamEyeWObj.pos,
                interest = CamIntWObj.pos,
                up = MeleeCamCobj.up,
                fov = MeleeCamCobj.fov
            };
            _kfs.Add(kf);
        }
        // var camType = Dolphinterop.ReadU8(MeleePointers.CAM_TYPE);

        if (_writeToGameCam) {
            float baseDistance = -400f;
            // float zoomSpeed = 0.1f;

            // float zoomDepth = -baseDistance * MathF.Exp(-zoom * zoomSpeed);
            var sysVec = new System.Numerics.Vector3(_translation.X, _translation.Y, baseDistance/*zoomDepth*/);
            // arbitrary ahh
            var fovSet = 1f / zoom * 115 * GraphicsDevice.Viewport.AspectRatio;
            Camera.SetDevelopCam(
                sysVec,
                sysVec + new System.Numerics.Vector3(0, 0, 20),
                // 40
                fovSet
            );
        }

        // Fit(out _translation, out targetZoom, 2f, Match.ActiveFighters.Select(x => x.Position.ToXNA()).ToArray());

        if (ms.ScrollWheelValue != _oldMs.ScrollWheelValue) {
            var diff = ms.ScrollWheelValue - _oldMs.ScrollWheelValue;
            targetZoom += diff / 120 * 0.2f;
            targetZoom = MathF.Max(targetZoom, 0.2f);
        }
        if (_isZoomingAnim) {
            _zoomElapsedSeconds += gameTime.DeltaTime();

            float t = _zoomElapsedSeconds / _zoomDurationSeconds;

            if (t >= 1f) {
                t = 1f;
                _isZoomingAnim = false;
            }

            t = Easings.ComputeEase(_camEasing, t);
            targetZoom = zoom = MathHelper.Lerp(_startZoom, _targetZoom, t);
        }
        else 
            zoom = MathHelper.Lerp(zoom, targetZoom, 10f * gameTime.DeltaTime());

        if (_isTranslatingAnim) {
            _translationElapsedSeconds += gameTime.DeltaTime();
            float t = _translationElapsedSeconds / _translationDurationSeconds;

            if (t >= 1f) {
                t = 1f;
                _isTranslatingAnim = false;
            }

            t = Easings.ComputeEase(_translationEasing, t);
            _translation = Vector2.Lerp(_startTranslation, _targetTranslation, t);
        }
        else if (ms.LeftButton == ButtonState.Pressed && IsActive) {
            // Only allow manual panning if not currently locked in an animation
            _translation.X += (_oldMs.Position.X - ms.Position.X) / zoom;
            _translation.Y -= (_oldMs.Position.Y - ms.Position.Y) / zoom;
        }

        // only constantly writes if it's enabled, otherwise toggle off once
        if (_writeToGameCam) {
            Camera.SetCameraType(CameraType.DebugFree);
        }
    }
    public static void FixedUpdate() {
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

            // fighter pickup lol
            //Dolphinterop.WriteVec3(fd.FighterPtr + 0x8C,
            //    new Vector3(0, 2.5f, 0).ToNumerics());

            /*if (fd.ECB.Contains(transMouse.X, transMouse.Y)) {
                _ftHover = i; // change to i?
            }*/
        }
    }

    static bool _writeToGameCam;
    readonly Color[] _portColors = [Color.Red, Color.Blue, Color.Yellow, Color.Green];

    static Vector2 _translation;
    static MouseState _oldMs;

    static Vector2 screenCenter;

    static float zoom;
    internal static float targetZoom;
    static bool _oldIngame;
    static int _crashDeterrent = -1;

    protected unsafe override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Transparent);

        if (!Dolphinterop.IsConnected) {
            SpriteBatch.Begin();

            var sfm = $"Scanning for Melee";

            var mod = UpdateCount % 240;

            for (int i = 0; i < mod / 60; i++)
                sfm += '.';

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont, sfm,
                GraphicsDevice.Viewport.Bounds.Center.ToVector2(), Color.White,
                origin: MeleeDrawing.MeleeFont.MeasureString(sfm) / 2);

            SpriteBatch.End();
            return;
        }
        else if (!curIngame) {
            SpriteBatch.Begin();

            var sfm = $"Waiting for game";

            var mod = UpdateCount % 240;

            for (int i = 0; i < mod / 60; i++)
                sfm += '.';

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont, sfm,
                GraphicsDevice.Viewport.Bounds.Center.ToVector2(), Color.White,
                origin: MeleeDrawing.MeleeFont.MeasureString(sfm) / 2);

            SpriteBatch.End();
        }

        // todo: draw world origin?
        if (curIngame) {
            try {
                DrawScene();
            }
            // just try and ignore
            catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
                SpriteBatch.End();
            }
        }

        SpriteBatch.Begin();

        // draws a reticle at the center of the window
        var linesColor = Color.Gray;
        var linesLen = 8;
        MeleeDrawing.DrawLine2D(screenCenter - new Vector2(linesLen, 0), screenCenter + new Vector2(linesLen, 0), linesColor);
        MeleeDrawing.DrawLine2D(screenCenter - new Vector2(0, linesLen), screenCenter + new Vector2(0, linesLen), linesColor);

        // draw percents at bottom (for cool ahh melee style)

        var divs = GraphicsDevice.Viewport.Width / (Match.Fighters.Length + 1);
        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            if (OnDat.InOnlineMatch) {
                var onlinePlr = OnDat.PlayerData[i];
                var str = $"{onlinePlr.Name} ({onlinePlr.ConnectCode})\nRank = {onlinePlr.Rank}";
                SpriteBatch.DrawString(MeleeDrawing.MeleeFont, str,
                        new Vector2(divs * (i + 1), GraphicsDevice.Viewport.Height - 125),
                        color: _portColors[i],
                        scale: new Vector2(0.65f),
                        rotation: 0f,
                        origin: MeleeDrawing.MeleeFont.MeasureString(str) / 2);
            }

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

        // draws regular info
        if (curIngame && StDat.StageId != ExternalStageId.DUMMY) {
            // world objects of the eye and interest
            var eyewobj = MeleeCamCobj.eye.As<WObj>();
            var intwobj = MeleeCamCobj.interest.As<WObj>();

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
                $"MeleeFrame: {Match.Frame}\n" +
                $"Logic: [FPS={LogicFPS:F2} Time={LogicTime.TotalMilliseconds:F2}ms]\n" +
                $"Render: [FPS={RenderFPS:F2} Time={RenderTime.TotalMilliseconds:F2}ms]\n" +
                $"Zoom: {targetZoom:F2}\n" +
                $"StageScale: {StDat.GroundParams.StageScale}\n" +
                $"IsTeams: {Match.IsTeams}\n" +
                $"Stage: {StDat.StageId}\n" +
                $"GameCamWrite: {_writeToGameCam}\n" +
                $"Cam:\n" +
                $"  Pos={eyewobj.pos}\n" +
                $"  Foc={intwobj.pos}\n" +
                $"  Fov={MeleeCamCobj.fov}" +
                $"   Up={MeleeCamCobj.up}",
                Vector2.Zero, Color.White,
                scale: new Vector2(0.5f));

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
                $"Press P to toggle camera keyframing. Current={_recording}",

                new Vector2(10, Window.ClientBounds.Height - 20), Color.White,
                scale: new Vector2(0.5f));
        }

        if (CinematicCamera.IsEnabled)
            CinematicCamera.CineCamUpdate(Match, gameTime);

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
            var bone = plr.GetBone(FtPart.XRotN);
            var jobj = Dolphinterop.Read<JObj>(bone.jobj);

            _translation = new Vector2(jobj.mtx.Translation.X, jobj.mtx.Translation.Y);
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
        MeleeDrawing.DrawBoundingRect2D(StDat.GetRealCameraBounds(), Color.CadetBlue, lineThickness, true);
        MeleeDrawing.DrawBoundingRect2D(StDat.GetRealBlastZone(), Color.DarkRed, lineThickness, true);

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

            MeleeDrawing.DrawLine2D(lStart, lEnd, newColor, lineThickness);

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
                MeleeDrawing.DrawBoundingRect2D(br, Color.Red, lineThickness);
            }
        }*/

        var lineLen = 5f;
        for (int i = 0; i < _fighterDeathLog.Count; i++) {
            var data = _fighterDeathLog[i];

            var botCross = new Vector2(lineLen);
            var topCross = new Vector2(lineLen, -lineLen);
            MeleeDrawing.DrawLine2D(data.Position - botCross, data.Position + botCross, _portColors[data.Port], lineThickness);
            MeleeDrawing.DrawLine2D(data.Position - topCross, data.Position + topCross, _portColors[data.Port], lineThickness);

            // draws velocity arrow
            var start = data.Position;
            var end = start + data.Velocity * 5f;

            MeleeDrawing.DrawLine2D(start, end, _portColors[data.Port] * 0.5f, lineThickness);

            // draw caret
            var dir = Vector2.Normalize(end - start);
            var perp = new Vector2(-dir.Y, dir.X);

            float caretLength = 2f;
            float caretWidth = 1.5f;

            var left = end - dir * caretLength + perp * caretWidth;
            var right = end - dir * caretLength - perp * caretWidth;
            MeleeDrawing.DrawLine2D(end, left, _portColors[data.Port] * 0.5f, lineThickness);
            MeleeDrawing.DrawLine2D(end, right, _portColors[data.Port] * 0.5f, lineThickness);
        }

        for (int i = 0; i < Match.Fighters.Length; i++) {
            var fd = Match.Fighters[i];
            if (fd.SlotKind == SlotKind.None) continue;

            MeleeDrawing.DrawFighter2D(fd, StDat, _portColors[i], lineThickness);

            bool lockedOut = false; // juist temporary
            int lastLrPress = 0;
            MeleeDrawing.DrawFighterPrediction2D(fd, lastLrPress, lockedOut, lineThickness);
        }

        if (Match.Items != null) {
            for (int i = 0; i < Match.Items.Count; i++) {
                var item = Match.Items[i];

                // if (item.ecb.Top > 50 || item.ecb.Right > 50) continue;
                // typically an invalid item..?
                // i think this is the best indicator of garbage values

                MeleeDrawing.DrawItem2D(item, Color.White, lineThickness);
            }
        }

        SpriteBatch.End();
    }
    // eventually: draw ecb
    static readonly Vector3[] _prevVels = new Vector3[4];
    static readonly Vector3[] _prevKbs = new Vector3[4];
    static readonly bool[] _prevDead = new bool[4];

    // MATH:
    public void Fit(out Vector3 center, out float zoom, float padding = 1.1f, params Vector3[] interests) {
        center = Vector3.Zero;
        zoom = 1;
        if (interests == null || interests.Length == 0) {
            return; // Fallback if no interests are provided
        }

        // 1. Find the bounding box of all interests
        float minX = interests[0].X;
        float maxX = interests[0].X;
        float minY = interests[0].Y;
        float maxY = interests[0].Y;

        for (int i = 1; i < interests.Length; i++) {
            if (interests[i].X < minX) minX = interests[i].X;
            if (interests[i].X > maxX) maxX = interests[i].X;
            if (interests[i].Y < minY) minY = interests[i].Y;
            if (interests[i].Y > maxY) maxY = interests[i].Y;
        }

        // 2. Set the translation to the exact center of the bounding box
        center = new Vector3(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f,
            0
        );

        // 3. Calculate width and height of the bounding box
        float worldWidth = maxX - minX;
        float worldHeight = maxY - minY;

        // Prevent division by zero if there's only 1 point or overlapping points
        if (worldWidth <= 0.0001f) worldWidth = 50f; // Arbitrary default minimum width
        if (worldHeight <= 0.0001f) worldHeight = 50f;

        // resolution independence
        float zoomX = GraphicsDevice.Viewport.Width / (worldWidth * padding);
        float zoomY = GraphicsDevice.Viewport.Height / (worldHeight * padding);

        zoom = Math.Min(zoomX, zoomY);
    }
    public void FitToBlastZone(StageData stage, float padding = 1.1f) {
        var blastZone = stage.GetRealBlastZone();
        float worldWidth = blastZone.Right - blastZone.Left;
        float worldHeight = blastZone.Top - blastZone.Bottom;

        _translation = new Vector2(
            (blastZone.Left + blastZone.Right) / 2f,
            (blastZone.Bottom + blastZone.Top) / 2f
        );

        // resolution independence
        float zoomX = GraphicsDevice.Viewport.Width / (worldWidth * padding);
        float zoomY = GraphicsDevice.Viewport.Height / (worldHeight * padding);

        targetZoom = Math.Min(zoomX, zoomY);
    }
}


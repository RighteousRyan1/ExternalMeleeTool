using EMTDisplay.CamExperiments;
using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.SlpParser;
using ExternalMeleeTool.Utilities;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Slippi.NET;
using Slippi.NET.Stats;
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

        //string path = @"C:\Users\%UserProfile%\Documents\Slippi\2026-07\Game_20260725T160238.slp";
        //var file = SlpReader.Parse(path);
        //Console.WriteLine(file.rank);

        if (File.Exists(Program.DRAWPARAMS_JSON_FNAME))
            MeleeDrawing.Params = JsonSerializer.Deserialize<MeleeDrawing.DrawParams>(
                File.ReadAllText(Program.DRAWPARAMS_JSON_FNAME), 
                DrawParamsJsonContext.Default.DrawParams);

        Exiting += EMTDisplay_Exiting;

        base.Initialize();
    }

    private void EMTDisplay_Exiting(object sender, EventArgs e) {
        // save to file and move on
        File.WriteAllText(Program.DRAWPARAMS_JSON_FNAME, 
            JsonSerializer.Serialize(MeleeDrawing.Params, 
            DrawParamsJsonContext.Default.DrawParams));
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

        // switch between the 2D and 3D views whenever you like
        if (IsActive && InputUtils.KeyJustPressed(Keys.Tab)) {
            SceneView.Toggle();
            Console.WriteLine($"View: {(SceneView.Is3D ? "3D" : "2D")}");
        }

        if (IsActive && InputUtils.KeyJustPressed(Keys.C)) {
            SceneView.MatchGameCamera = !SceneView.MatchGameCamera;
            Console.WriteLine($"Match game camera: {SceneView.MatchGameCamera}");
        }

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
            // let the camera be cool and flexible in 3d!
            if (SceneView.Mode == ViewMode.View3D) {
                // while the viewer follows the game's camera there's nothing to write (it'd just freeze it)
                if (!SceneView.MatchGameCamera) {
                    var target = new Vector3(_translation.X, _translation.Y, _translationZ);
                    var eye = SceneView.GetOrbitEyeMelee(target, GraphicsDevice.Viewport.Height, zoom);

                    Camera.SetDevelopCam(
                        new System.Numerics.Vector3(eye.X, eye.Y, eye.Z * GameCamZSign),
                        new System.Numerics.Vector3(target.X, target.Y, target.Z * GameCamZSign),
                        MathHelper.ToDegrees(SceneView.FOV_Y) * GameFovScale3D
                    );
                }
            }
            else {
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
        else if (IsActive) {
            float dx = ms.Position.X - _oldMs.Position.X;
            float dy = ms.Position.Y - _oldMs.Position.Y;

            if (!SceneView.Is3D) {
                if (ms.LeftButton == ButtonState.Pressed) {
                    _translation.X -= dx / zoom;
                    _translation.Y += dy / zoom;
                }
            }
            else if (!SceneView.MatchGameCamera) {
                bool shift = Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift);
                bool pan = ms.RightButton == ButtonState.Pressed || (ms.LeftButton == ButtonState.Pressed && shift);
                bool orbit = ms.LeftButton == ButtonState.Pressed && !shift;

                if (orbit) {
                    // dragging right swings the camera left, dragging down raises it
                    SceneView.Orbit(-dx * 0.005f, dy * 0.005f);
                }
                else if (pan) {
                    var d = SceneView.ScreenDragToWorld(dx, dy, zoom);
                    _translation.X += d.X;
                    _translation.Y += d.Y;
                    _translationZ += d.Z;
                }
            }
        }

        if (SceneView.Is3D && InputUtils.KeyJustPressed(Keys.R)) {
            SceneView.ResetOrbit();
            _translationZ = 0;
        }

        // only constantly writes if it's enabled, otherwise toggle off once
        if (_writeToGameCam) {
            Camera.SetCameraType(CameraType.DebugFree);
        }
    }

    public static void CopyMeleeCamToViewer() {
        var e = CamEyeWObj.pos;
        var t = CamIntWObj.pos;
        var eye = new Vector3(e.X, e.Y, e.Z);
        var tgt = new Vector3(t.X, t.Y, t.Z);

        if (!SceneView.TryGetOrbitFromCamera(eye, tgt, Graphics.GraphicsDevice.Viewport.Height,
                out var yaw, out var pitch, out var z))
            throw new Exception("Melee's camera isn't in a usable state right now.");

        SceneView.Yaw = yaw;
        SceneView.Pitch = pitch;
        _translation = new Vector2(tgt.X, tgt.Y);
        _translationZ = tgt.Z;
        zoom = targetZoom = z;
        SceneView.MatchGameCamera = false;
        SceneView.SetMode(ViewMode.View3D);
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

    const float GameFovScale3D = 1f;

    public static float GameCamZSign = -1f;
    readonly Color[] _portColors = [Color.Red, Color.Blue, Color.Yellow, Color.Green];

    static Vector2 _translation;
    // depth of the camera target in melee space (only matters in the 3D view)
    static float _translationZ;
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
            }
        }

        SpriteBatch.Begin();

        // draws a reticle at the center of the window
        var linesColor = Color.Gray;
        var linesLen = 8;
        var reticleCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
        DrawScreenLine(reticleCenter - new Vector2(linesLen, 0), reticleCenter + new Vector2(linesLen, 0), linesColor);
        DrawScreenLine(reticleCenter - new Vector2(0, linesLen), reticleCenter + new Vector2(0, linesLen), linesColor);

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

            RandomUtils.GetMeleeRandomSeed();

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
                $"MeleeFrame: {Match.Frame}\n" +
                $"Logic: [FPS={LogicFPS:F2} Time={LogicTime.TotalMilliseconds:F2}ms]\n" +
                $"Render: [FPS={RenderFPS:F2} Time={RenderTime.TotalMilliseconds:F2}ms]\n" +
                $"View: {(SceneView.Is3D ? (SceneView.MatchGameCamera ? "3D (game camera)" : "3D") : "2D")} (Tab to switch)\n" +
                $"Zoom: {targetZoom:F2}\n" +
                $"StageScale: {StDat.GroundParams.StageScale}\n" +
                $"IsTeams: {Match.IsTeams}\n" +
                $"Stage: {StDat.StageId}\n" +
                $"GameCamWrite: {_writeToGameCam}\n" +
                $"\nRandom Seed: {RandomUtils.RandomSeed}\n" +
                $"Cam:\n" +
                $"  Pos={eyewobj.pos}\n" +
                $"  Foc={intwobj.pos}\n" +
                $"  Fov={MeleeCamCobj.fov}" +
                $"   Up={MeleeCamCobj.up}",
                Vector2.Zero, Color.White,
                scale: new Vector2(0.5f));

            SpriteBatch.DrawString(MeleeDrawing.MeleeFont,
                $"Press P to toggle camera keyframing. Current={_recording}" +
                (SceneView.Is3D ? (SceneView.MatchGameCamera ? "   |   3D: following game camera (C to release)" : "   |   3D: drag=orbit, right-drag/shift-drag=pan, scroll=zoom, R=reset, C=match game cam") : ""),

                new Vector2(10, Window.ClientBounds.Height - 20), Color.White,
                scale: new Vector2(0.5f));
        }

        if (CinematicCamera.IsEnabled)
            CinematicCamera.CineCamUpdate(Match, gameTime);

        SpriteBatch.End();

        RenderTime = gameTime.ElapsedGameTime;
        RenderFPS = 1f / RenderTime.TotalSeconds;
    }

    static void DrawScreenLine(Vector2 a, Vector2 b, Color color) {
        SpriteBatch.Draw(MeleeDrawing.WhitePixel, a, null, color,
            (b - a).ToRotation(), new Vector2(0, 0.5f),
            new Vector2(Vector2.Distance(a, b), 1f), SpriteEffects.None, 0);
    }
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
            _translationZ = jobj.mtx.Translation.Z;
        }

        // one camera drives both views (orthographic for 2D, perspective orbit for 3D)
        // feed Melee's own camera in so the 3D view can match it (see cam_match)
        try {
            var ge = CamEyeWObj.pos;
            var gi = CamIntWObj.pos;
            SceneView.SetGameCamera(new Vector3(ge.X, ge.Y, ge.Z), new Vector3(gi.X, gi.Y, gi.Z), MeleeCamCobj.fov);
        }
        catch {
            SceneView.SetGameCamera(Vector3.Zero, Vector3.Zero, 0f); // marks it invalid
        }

        SceneView.Update(GraphicsDevice.Viewport, _translation, _translationZ, zoom);

        LineBatch.Begin(GraphicsDevice);
        try {
            // the zones already account for stage scale parameter...
            MeleeDrawing.DrawBoundingRect(StDat.GetRealCameraBounds(), Color.CadetBlue, true, 0f, true);
            MeleeDrawing.DrawBoundingRect(StDat.GetRealBlastZone(), Color.DarkRed, true, 0f, true);

            MapLineSegments.Clear();
            // Icicle mountain lags the FUCK out of this
            for (int i = 0; i < StDat.Collision.line_count; i++) {
                // lines of joints the engine has switched off. skip
                bool lineActive = StDat.IsLineActive(i);
                if (!lineActive && !MeleeDrawing.Params.ShowInactiveLines) continue;

                var lineDesc = StDat.MapLines[i];
                var vStart = StDat.Vertices[lineDesc.StartIdx] * stageScale;
                var vEnd = StDat.Vertices[lineDesc.EndIdx] * stageScale;

                // inactive joints keep stale/garbage vertices
                if (!lineActive && (MathF.Abs(vStart.X) > 5000 || MathF.Abs(vStart.Y) > 5000
                                    || MathF.Abs(vEnd.X) > 5000 || MathF.Abs(vEnd.Y) > 5000)) continue;

                var lStart = new Vector2(vStart.X, vStart.Y);
                var lEnd = new Vector2(vEnd.X, vEnd.Y);

                if (lineActive) MapLineSegments.Add(new(lStart, lEnd));

                // if StDat.Collision.joints.vtx_start or whatever contains this, ignore it so its drawn as moving

                var newColor = MeleeDrawing.Params.DrawSchema switch {
                    0 => MeleeDisplayUtils.MatTypeToColor[lineDesc.material_type],
                    // there's more colltypes than i imagined previously..?
                    1 => MeleeDisplayUtils.CollKindToColor[lineDesc.coll_type],
                    2 => MeleeDisplayUtils.CollPropertyToColor[lineDesc.coll_property],
                    _ => throw new Exception("Bruh")
                };

                if (!lineActive) {
                    // draws disabled lines faintly
                    MeleeDrawing.DrawCollisionLine(lStart, lEnd, newColor * 0.2f);
                    continue;
                }

                MeleeDrawing.DrawCollisionLine(lStart, lEnd, newColor);

                const float vtxScale = 0.04f;
                MeleeDrawing.Text(MeleeDrawing.Cascadia, $"{lineDesc.StartIdx}", new Vector3(lStart, 0),
                    newColor, vtxScale, Anchor.BottomCenter);
                MeleeDrawing.Text(MeleeDrawing.Cascadia, $"{lineDesc.EndIdx}", new Vector3(lEnd, 0),
                    newColor, vtxScale, Anchor.BottomCenter);
            }

            /*if (StDat.CollGroups != null) {
                for (int i = 0; i < StDat.CollGroups.Length; i++) {
                    var cg = StDat.CollGroups[i];

                    var br = new BoundingRect(cg.left_bound, cg.top_bound, cg.right_bound, cg.bottom_bound);
                    MeleeDrawing.DrawBoundingRect(br, Color.Red);
                }
            }*/

            var lineLen = 5f;
            for (int i = 0; i < _fighterDeathLog.Count; i++) {
                var data = _fighterDeathLog[i];
                var col = _portColors[data.Port];

                // death markers live on the Z = 0 plane
                var botCross = new Vector2(lineLen);
                var topCross = new Vector2(lineLen, -lineLen);
                MeleeDrawing.DrawLine(new Vector3(data.Position - botCross, 0), new Vector3(data.Position + botCross, 0), col);
                MeleeDrawing.DrawLine(new Vector3(data.Position - topCross, 0), new Vector3(data.Position + topCross, 0), col);

                // draws velocity arrow
                var start = data.Position;
                var end = start + data.Velocity * 5f;

                MeleeDrawing.DrawLine(new Vector3(start, 0), new Vector3(end, 0), col * 0.5f);

                // draw caret
                var dir = Vector2.Normalize(end - start);
                var perp = new Vector2(-dir.Y, dir.X);

                float caretLength = 2f;
                float caretWidth = 1.5f;

                var left = end - dir * caretLength + perp * caretWidth;
                var right = end - dir * caretLength - perp * caretWidth;
                MeleeDrawing.DrawLine(new Vector3(end, 0), new Vector3(left, 0), col * 0.5f);
                MeleeDrawing.DrawLine(new Vector3(end, 0), new Vector3(right, 0), col * 0.5f);
            }

            for (int i = 0; i < Match.Fighters.Length; i++) {
                var fd = Match.Fighters[i];
                if (fd.SlotKind == SlotKind.None) continue;

                MeleeDrawing.DrawFighter(fd, StDat, _portColors[i]);

                bool lockedOut = false; // juist temporary
                int lastLrPress = 0;
                MeleeDrawing.DrawFighterPrediction(fd, lastLrPress, lockedOut);
            }

            if (Match.Items != null) {
                for (int i = 0; i < Match.Items.Count; i++) {
                    var item = Match.Items[i];

                    // if (item.ecb.Top > 50 || item.ecb.Right > 50) continue;
                    // typically an invalid item..?
                    // i think this is the best indicator of garbage values

                    MeleeDrawing.DrawItem(item, Color.White);
                }
            }
        }
        catch {
            // don't leave half-queued text around for the next frame
            MeleeDrawing.DiscardText();
            throw;
        }
        finally {
            LineBatch.End();
        }

        // labels are drawn last as screen-space billboards so they work in 3D too
        MeleeDrawing.FlushText();
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

        center = new Vector3(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f,
            0
        );

        float worldWidth = maxX - minX;
        float worldHeight = maxY - minY;

        if (worldWidth <= 0.0001f) worldWidth = 50f;
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
        _translationZ = 0f;

        // resolution independence
        float zoomX = GraphicsDevice.Viewport.Width / (worldWidth * padding);
        float zoomY = GraphicsDevice.Viewport.Height / (worldHeight * padding);

        targetZoom = Math.Min(zoomX, zoomY);
    }
}


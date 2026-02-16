using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Numerics;

namespace MeleeThirdPerson;
public class MeleeCamManip {
    public static float dt;
    public static float fps;

    public static float elapsedSeconds;

    public static bool limitFps = false;

    static ThirdPersonCamera TPCamera;

    public static DateTime latestTime;
    static DateTime oldLatestTime;
    static DateTime startTime;

    public static MatchData Match;
    public static SceneData ScDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    // implement when i figure out melee frame stuff
    public delegate void FixedUpdate(int frame);

    public static bool ForceToClientPort = true;
    public static bool HideMagBubbles;
    public static bool HardcoreMode;
    public static Version? version;
    static void Main() {
        version = typeof(MeleeCamManip).Assembly.GetName().Version!;
		Console.CursorVisible = false;

        Console.WindowWidth = 125;
        Console.WindowHeight = 35;

        WaitForMelee();

        Console.Clear();

        WriteStatics();

        // to prevent a time change check between the epoch of the universe
        oldLatestTime = DateTime.Now;
        TPCamera ??= new ThirdPersonCamera();
        // Console.WriteLine($"Connected! GALE01 found at 0x{Slippinterop.GALE01:X}");

        // starts infinite loop (until stopped)
        try {
            MainLoop();
        }
        catch (Exception ex) {
            Console.WriteLine("An error occurred:\n");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("\nPlease restart this window.");
        }

        Console.Clear();

        Main();
    }
    static void WaitForMelee() {
        // don't do anything with an invalid GALE01
        int rotIndex = 0;
        while (!Dolphinterop.Connect("GALE01", "GTME01", "GALJ01")) {
            Console.SetCursorPosition(0, 0);
            ShowWait(rotIndex);

            rotIndex++;
            if (rotIndex > 3) rotIndex = 0;

            Thread.Sleep(250);
        }
    }

    static readonly ConsoleKey NextFighterKey = ConsoleKey.N;
    static readonly ConsoleKey ToggleForcePortKey = ConsoleKey.K;
    static readonly ConsoleKey ChangeFocusKey = ConsoleKey.M;
    static readonly ConsoleKey toggleFpsLimit = ConsoleKey.L;
    static readonly ConsoleKey toggleFPMode = ConsoleKey.Z;
    static readonly ConsoleKey magToggle = ConsoleKey.H;
    static readonly ConsoleKey hcToggle = ConsoleKey.X;

    const float DEGS_PER_ADD = 5f;
    static int inputTimeout;
    static void HandleKeyPressEvents() {
        if (inputTimeout > 0) {
            inputTimeout--;
            return;
        }

        if (KeyUtils.WasKeyPressed(toggleFpsLimit)) {
            limitFps = !limitFps;
        }

        // if (myFighter.Input.Triggers < TRGR_THRESH) return;

        // cycle to next active fighter
        //if (MeleeUtils.GCInputPressed(myFighter, NextFighterPad)) {
        if (KeyUtils.WasKeyPressed(NextFighterKey)) {
            SlotMove(1);

            inputTimeout = 60;
        }
        // Old: KeyUtils.WasKeyPressed
        //else if (MeleeUtils.GCInputPressed(myFighter, ChangeFocusPad)) {
        else if (KeyUtils.WasKeyPressed(ChangeFocusKey)) {
            TPCamera.FocusType++;

            if (TPCamera.FocusType > CameraFollowKind.ClosestEnemy)
                TPCamera.FocusType = 0;

            inputTimeout = 60;
        }
        //else if (MeleeUtils.GCInputPressed(myFighter, ToggleForcePortPad)) {
        else if (KeyUtils.WasKeyPressed(ToggleForcePortKey)) {
            ForceToClientPort = !ForceToClientPort;
            inputTimeout = 60;
        }
        else if (KeyUtils.WasKeyPressed(toggleFPMode)) {
            FirstPersonManager.IsEnabled = !FirstPersonManager.IsEnabled;

            // restore camera up to normal.
            Camera.QuickManip((ref CObj cobj) => {
                cobj.flags |= CObjFlags.UseUp;
                cobj.up = Vector3.UnitY;
            });
            Console.Clear();
            WriteStatics();
        }
        else if (KeyUtils.WasKeyPressed(FirstPersonManager.fovUp)) {
            FirstPersonManager.FovDeg = MathF.Round(FirstPersonManager.FovDeg + DEGS_PER_ADD);
        }
        else if (KeyUtils.WasKeyPressed(FirstPersonManager.fovDown)) {
            FirstPersonManager.FovDeg = MathF.Round(FirstPersonManager.FovDeg - DEGS_PER_ADD);
        }
        else if (KeyUtils.WasKeyPressed(FirstPersonManager.motionSickKey)) {
            FirstPersonManager.MotionSickReduce = !FirstPersonManager.MotionSickReduce;

            // restore camera up to normal.
            Camera.QuickManip((ref CObj cobj) => {
                cobj.flags |= CObjFlags.UseUp;
                cobj.up = Vector3.UnitY;
            });
        }
        else if (KeyUtils.WasKeyPressed(FirstPersonManager.faceToggle)) {
            FirstPersonManager.HideFace = !FirstPersonManager.HideFace;

            if (FirstPersonManager.HideFace)
                FirstPersonManager.PlayerFaceHide(Match.Fighters[TPCamera.FocusPort]);
            else
                FirstPersonManager.PlayerDObjRestore(Match.Fighters[TPCamera.FocusPort]);
        }
        else if (KeyUtils.WasKeyPressed(magToggle)) {
            HideMagBubbles = !HideMagBubbles;
        }
        else if (KeyUtils.WasKeyPressed(hcToggle)) {
            HardcoreMode = !HardcoreMode;
        }
        FirstPersonManager.FovDeg = MathUtils.Clamp(FirstPersonManager.FovDeg, 30, 150);
    }
    static void WriteStatics() {
        Console.WriteLine($"MeleePerspectives v{version!.Truncate()} by RighteousRyan");
        Console.WriteLine();
        Console.WriteLine("Support the development of this and other projects!");
        Console.WriteLine("Patreon: https://patreon.com/c/RighteousRyan");
        Console.WriteLine("PayPal:  https://tinyurl.com/righteousryan");
        Console.WriteLine("YouTube: https://youtube.com/@RighteousRyan");
        Console.WriteLine("Twitch:  https://twitch.tv/righteousryan_");

        Console.WriteLine();
        WriteLineC("#-- Disclaimers --#", ConsoleColor.DarkYellow);
        WriteLineC("- If you experience melee crashing, please play on Ishiiruka (\"Faster Melee\").", ConsoleColor.Yellow);
        WriteLineC("- First Person Mode is currently in BETA. Bugs and camera issues may appear.", ConsoleColor.Yellow);
        WriteLineC("- First Person Mode works best with RECOLORED SKIN VARIANTS. Issues may appear with costumes/skins with varying apparel.", ConsoleColor.Yellow);
    }
    static void WriteUI() {
        var focusedFighter = Match.Fighters[TPCamera.FocusPort];

        int numFighters = 0;
        for (int i = 0; i < Match.Fighters.Length; i++) {
            if (Match.Fighters[i].SlotKind != SlotKind.None)
                numFighters++;
        }

        Console.SetCursorPosition(0, 13);
        Console.WriteLine($"FPS: {fps}                                    ");
        Console.WriteLine();
        Console.WriteLine("Controller/Key Binds:          ");

        if (FirstPersonManager.IsEnabled) {
            Console.WriteLine($"Toggle First Person:            Z      (Current={FirstPersonManager.IsEnabled})      ");
            Console.WriteLine($"Decrease/Increase FOV:          -/+    (Current={FirstPersonManager.FovDeg}°)");
            Console.WriteLine($"Toggle Motion Sickness Reducer: T      (Current={FirstPersonManager.MotionSickReduce})");
            WriteLineC("└─ Attempts to reduce motion sickness by not flipping the camera with the fighter's head (i.e: front/backflipping)", ConsoleColor.DarkGray);
            Console.WriteLine($"Toggle Face Parts:              Delete (Current={FirstPersonManager.HideFace})");
            WriteLineC("└─ If True, face parts will be hidden to allow for maximum visibility", ConsoleColor.DarkGray);
            Console.WriteLine($"Force Online Port:              K      (Current={ForceToClientPort})                 ");
            WriteLineC("└─ If True, while online, the focused fighter will be yours", ConsoleColor.DarkGray);
            Console.WriteLine($"Limit FPS (saves resources):    L      (Current={limitFps})          ");
            Console.WriteLine($"Focus Next Port:                N      (Current={TPCamera.FocusPort}, {Match.Fighters[TPCamera.FocusPort].CharKind})                ");
        }
        else {
            Console.WriteLine($"Change Focus Type:           M (Current={TPCamera.FocusType})          ");
            string focusDesc = TPCamera.FocusType switch {
                CameraFollowKind.PlayerDirection => "The Third Person camera looks in the direction of the focused fighter",
                CameraFollowKind.ClosestEnemy => "The Third Person camera looks in the direction of the closest enemy",
                _ => string.Empty
            };
            WriteLineC("└─ " + focusDesc, ConsoleColor.DarkGray);
            Console.WriteLine($"Toggle First Person:         Z (Current={FirstPersonManager.IsEnabled})");
            Console.WriteLine($"Force Online Port:           K (Current={ForceToClientPort})           ");
            WriteLineC("└─ If True, while online, the focused fighter will be yours", ConsoleColor.DarkGray);
            Console.WriteLine($"Limit FPS (saves resources): L (Current={limitFps})          ");
            Console.WriteLine($"Focus Next Port:             N (Current={TPCamera.FocusPort}, {Match.Fighters[TPCamera.FocusPort].CharKind})                ");
        }
        Console.WriteLine();
        WriteC(           "Hardcore Mode", ConsoleColor.DarkRed);
        Console.WriteLine($":          X (Current: {HardcoreMode})    ");
        WriteLineC("└─ If True, all of melee's in-game HUD is hidden", ConsoleColor.DarkGray);
        Console.WriteLine($"Hide Magnifier Bubbles: H (Current={HideMagBubbles})     ");
        WriteLineC("└─ If changing, a restart of Melee is required", ConsoleColor.Red);
        Console.WriteLine();
        // Console.WriteLine($"Global Data:         MajorScene={ScDat.MajorScene}, MinorScene={ScDat.MinorScene}               ");
        Console.WriteLine();
        Console.WriteLine($"# Players Active: {numFighters}         ");

        foreach (var ft in Match.ActiveFighters) {
            if (ft.SlotKind == SlotKind.None) continue;
            Console.WriteLine($"Player {ft.Port + 1}: {ft.FriendlyString()}                    ");
        }
        // Console.WriteLine($"tgr: {Match.Fighters[0].Input.Triggers}, {Convert.ToString(Match.Fighters[0].Input.ButtonsHeld, 2)}");
        Console.WriteLine("                                                  ");
        Console.WriteLine("                                                  ");
        Console.WriteLine("                                                  ");
    }

    static void WriteLineC(object? value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ResetColor();
    }
    static void WriteC(object? value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
    public static void MainLoop() {
        startTime = DateTime.Now;
        while (Dolphinterop.IsConnected) {
            try {
                latestTime = DateTime.Now;

                Match = MatchData.GetMatchData();
                ScDat = SceneData.GetSceneData();
                OnDat = SlippiOnlineData.GetOnlineData(ScDat);
                // zero point to this rn
                // this is causing NAOT to not work right
                // StDat = StageData.GetStageData();

                if (OnDat.InOnlineMatch)
                    if (ForceToClientPort)
                        if (OnDat.ClientPort != byte.MaxValue)
                            TPCamera.FocusPort = OnDat.ClientPort;

                var fd = Match.Fighters[TPCamera.FocusPort];

                // our camera manips won't work unless develop cam is enabled
                Camera.SetCameraType(CameraType.DebugFree);

                //var rotated = (Vector3.UnitX * 18).Rotate(Vector3.UnitY, (float)latestTime.TimeOfDay.TotalSeconds * 3 * MathF.PI * 2);
                //Camera.SetDevelopCam(new Vector3( fd.Position.X, fd.Position.Y + 10, 0) + rotated,  fd.Position + new Vector3(0, 10, 0), 90);

                // player data
                HandleKeyPressEvents();

                var dt = (float)(latestTime - oldLatestTime).TotalSeconds;
                if (float.IsFinite(dt)) {
                    fps = 1f / dt;
                    elapsedSeconds += dt;
                }

                // FunnyCinematicCamera();

                // hide all bubbles if wanted!
                // var bubbles = Dolphinterop.Read<OffscreenBubbleTable>(/*MeleeGlobals.OFFSCREEN_BUBBLE_TABLE*/ 0x804A1DE0);

                /*for (int i = 0; i < OffscreenBubbleTable.OffscreenBubbleDataBuffer6.LENGTH; i++) {
                    var bub = bubbles.bubbles[i];

                    if (bub.jobj == 0) continue;
                    if (i != 0) continue;

                    var jobj = Dolphinterop.Read<JObj>(bub.jobj);

                    var gobj = bub.gobj.As<GObj>();
                    bub.flags &= ~OffscreenBubbleFlags.IsOffscreen;
                    bub.flags |= OffscreenBubbleFlags.IgnoreOffscreen;
                    //jobj.mtx.Translation = new(500, 500, 0);
                    //Console.WriteLine($"Bub {i}: \n{jobj.mtx}");
                    Dolphinterop.Write(0x804A1DE0 + 0x14 + Marshal.SizeOf<OffscreenBubbleData>() * i, bub);
                    //Dolphinterop.Write(bub.jobj, jobj);
                }
                Console.WriteLine();*/

                if (FirstPersonManager.IsEnabled) {
                    FirstPersonManager.Update(fd, ScDat, Match, TPCamera.FocusPort);
                }
                else {
                    TPCamera.Update(dt);
                    TPCamera.Camera.ApplyToMelee();

                    // try to restore each fighter's head
                    foreach (var f in Match.Fighters) {
                        FirstPersonManager.PlayerDObjRestore(f);
                    }
                }

                // first person camera logic

                if (Match.Fighters[TPCamera.FocusPort].SlotKind == SlotKind.None) {
                    // find first human port
                    SlotMoveUntil(1, SlotKind.Human);
                }

                // every half-second
                if (elapsedSeconds % 0.2f < dt) {
                    WriteUI();
                }

                // every second
                if (elapsedSeconds % 1f < dt) {
                    // works, only if it runs before it's JIT-ed
                    // address = OffscreenBubbleThink, instruction 1
                    if (HideMagBubbles)
                        PpcAssembler.WritePpcInstruction(0x802fbbdc, "blr");
                    Dolphinterop.WriteU8(MeleeGlobals.MATCH_HUD_HIDDEN, (byte)(HardcoreMode ? 1 : 0));
                }

                oldLatestTime = latestTime;
            }
            catch(Exception e) {
                Console.WriteLine("UH OH!!!: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            // """"forces"""" fps to 63-64ish
            if (limitFps)
                Thread.Sleep(2);
        }
    }
    static void FunnyCinematicCamera() {
        if ((latestTime.Second != oldLatestTime.Second) && latestTime.Second % 4 == 0) {
            var stdat = StageData.GetStageData();

            var lines = stdat.MapLines;

            if (lines is null) return;

            List<(Vector2 start, Vector2 end)> segments = [];

            foreach (var lineDesc in lines) {
                if (lineDesc.coll_type != CollKind.Top) continue;
                segments.Add((stdat.Vertices[lineDesc.StartIdx], stdat.Vertices[lineDesc.EndIdx]));
            }

            var rand = new Random();
            var (start, end) = segments[rand.Next(segments.Count)];

            float randBetween(float min, float max) {
                var val = rand.NextSingle();
                var randf = val * (max - min) + min;

                return randf;
            }

            var randX = randBetween(start.X, end.X);
            var randY = randBetween(start.Y, end.Y) + 5f;


            var posAlongLine = new Vector2(randX, randY);

            var cam = new MeleeFreeCamera();

            float zRange = 100;
            float randZ = randBetween(-zRange, 0);

            cam.Eye = new Vector3(posAlongLine, randZ);

            var posAvg = Vector3.Zero;
            var ftcount = 0;
            for (int i = 0; i < Match.Fighters.Length; i++) {
                if (Match.Fighters[i].SlotKind != SlotKind.Human) continue;
                ftcount++;
                posAvg += Match.Fighters[i].Position;
            }

            posAvg /= ftcount;

            cam.Focus = posAvg;
            cam.Fov = randBetween(80, 100);

            Console.WriteLine($"{cam.Eye}, {cam.Focus}, {cam.Fov}");

            cam.ApplyToMelee();
        }
    }
    static void SlotMove(int amount) {
        int start = TPCamera.FocusPort;

        do {
            TPCamera.FocusPort += amount;
            if (TPCamera.FocusPort >= Match.Fighters.Length || TPCamera.FocusPort < 0)
                TPCamera.FocusPort = 0;

            if (TPCamera.FocusPort == start)
                break;

        } while (Match.Fighters[TPCamera.FocusPort].SlotKind == SlotKind.None);
    }
    static void SlotMoveUntil(int amount, SlotKind match) {
        int start = TPCamera.FocusPort;

        do {
            TPCamera.FocusPort += amount;
            if (TPCamera.FocusPort >= Match.Fighters.Length || TPCamera.FocusPort < 0)
                TPCamera.FocusPort = 0;

            if (TPCamera.FocusPort == start)
                break;

        } while (Match.Fighters[TPCamera.FocusPort].SlotKind != match);
    }
    // blah...
    static void ShowWait(int rotIndex) {
        char rotChar = rotIndex switch {
            0 => '|',
            1 => '/',
            2 => '-',
            3 => '\\',
            _ => '#'
        };
        Console.WriteLine($"Scanning for Melee ... {rotChar}    ");
        Console.WriteLine();
        Console.WriteLine($"If you have Melee open and this message is still showing, restart your game.");
        Console.WriteLine($"This program does not work with regular Dolphin. It must be Slippi Dolphin.");
    }
}

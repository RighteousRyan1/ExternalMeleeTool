using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
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

    public static MatchData Match;
    public static SceneData ScDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    // public static float GlobalSpeed = 1.0f; 

    // implement when i figure out melee frame stuff
    public delegate void FixedUpdate(int frame);

    public static bool ForceToClientPort = true;

    public static bool IsFirstPerson;
    public static Version version;
    static void Main() {
        version = typeof(MeleeCamManip).Assembly.GetName().Version!;
		Console.CursorVisible = false;

        WaitForMelee();

        Console.Clear();

        Console.WriteLine($"MeleeThirdPerson v{version.Truncate()} by RighteousRyan");
        Console.WriteLine();
        Console.WriteLine("Support the development of this and other projects!");
        Console.WriteLine("Patreon: https://patreon.com/c/RighteousRyan");
        Console.WriteLine("PayPal:  https://tinyurl.com/righteousryan");
        Console.WriteLine("YouTube: https://youtube.com/@RighteousRyan");
        Console.WriteLine("Twitch:  https://twitch.tv/righteousryan_");

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
        while (!Dolphinterop.Connect("GALE01", "GTME01")) {
            Console.SetCursorPosition(0, 0);
            ShowWait(rotIndex);

            rotIndex++;
            if (rotIndex > 3) rotIndex = 0;

            Thread.Sleep(250);
        }
    }

    // const float TRGR_THRESH = 0.5f;
    //static readonly HSDPadButton NextFighterPad = HSDPadButton.DPadRight;
    //static readonly HSDPadButton ToggleForcePortPad = HSDPadButton.DPadLeft;
    //tatic readonly HSDPadButton ChangeFocusPad = HSDPadButton.DPadDown;
    static readonly ConsoleKey NextFighterKey = ConsoleKey.N;
    static readonly ConsoleKey ToggleForcePortKey = ConsoleKey.K;
    static readonly ConsoleKey ChangeFocusKey = ConsoleKey.M;
    static readonly ConsoleKey toggleFpsLimit = ConsoleKey.L;

    static int inputTimeout;
    static void HandleKeyPressEvents() {
        if (inputTimeout > 0) {
            inputTimeout--;
            return;
        }

        // var myFighter = Match.Fighters[OnDat.ClientControllerPort];


        /*if (KeyUtils.WasKeyPressed(IncreaseGlobalSpeed)) {
            GlobalSpeed += 0.25f;
        }
        else if (KeyUtils.WasKeyPressed(ReduceGlobalSpeed)) {
            GlobalSpeed -= 0.25f;
        }*/

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
    }
    static void WriteUI() {
        var focusedFighter = Match.Fighters[TPCamera.FocusPort];

        int numFighters = 0;
        for (int i = 0; i < Match.Fighters.Length; i++) {
            if (Match.Fighters[i].SlotKind != SlotKind.None)
                numFighters++;
        }

        Console.SetCursorPosition(0, 8);
        Console.WriteLine($"FPS: {fps}                                    ");
        // Console.WriteLine($"Global Speed: {GlobalSpeed:F2}                ");
        Console.WriteLine();
        Console.WriteLine("Controller/Key Binds:          ");
        //Console.WriteLine($"Focus Next Port:   L/R + Right (Current={TPCamera.FocusPort}, {Match.Fighters[TPCamera.FocusPort].CharKind})                ");
        //Console.WriteLine($"Change Focus Type: L/R + Down  (Current={TPCamera.FocusType})         ");
        //Console.WriteLine($"Force Online Port: L/R + Left  (Current={ForceToClientPort})          ");
        Console.WriteLine($"Focus Next Port:   N  (Current={TPCamera.FocusPort}, {Match.Fighters[TPCamera.FocusPort].CharKind})                ");
        Console.WriteLine($"Change Focus Type: M  (Current={TPCamera.FocusType})         ");
        Console.WriteLine($"Force Online Port: K  (Current={ForceToClientPort})          ");
        Console.WriteLine($"Limit FPS (saves resources): L (Current={limitFps})          ");
        Console.WriteLine();
        Console.WriteLine($"Follow Data:         FocusType={TPCamera.FocusType}, FocusPort={TPCamera.FocusPort} ({focusedFighter.CharKind})             ");
        //Console.WriteLine($"Slippi Data:         IsOnline={OnDat.InOnlineMatch}, ClientPort={OnDat.ClientPort}, Frame={OnDat.Frame}      ");
        //Console.WriteLine($"Match Data:          IsTeams={Match.IsTeams}               ");
        //Console.WriteLine($"Stage Data:          StageId={StDat.StageId}               ");
        Console.WriteLine($"Global Data:         MajorScene={ScDat.MajorScene}, MinorScene={ScDat.MinorScene}               ");
        /*Console.WriteLine();
        Console.WriteLine($"Camera Position:     {TPCamera.Camera.Eye}                           ");
        Console.WriteLine($"Camera Focus:        {TPCamera.Camera.Focus}                         ");
        Console.WriteLine($"Camera FOV:          {TPCamera.Camera.Fov}                           ");*/
        Console.WriteLine();
        Console.WriteLine($"# Players Active: {numFighters}         ");

        for (int i = 0; i < Match.Fighters.Length; i++) {
            var ft = Match.Fighters[i];

            if (ft.SlotKind == SlotKind.None) continue;
            Console.WriteLine($"Player {i + 1}: {ft.FriendlyString()}                    ");
        }
        // Console.WriteLine($"tgr: {Match.Fighters[0].Input.Triggers}, {Convert.ToString(Match.Fighters[0].Input.ButtonsHeld, 2)}");
        Console.WriteLine("                                                  ");
        Console.WriteLine("                                                  ");
        Console.WriteLine("                                                  ");
    }

    public static void MainLoop() {
        while (Dolphinterop.IsConnected) {
            //Dolphinterop.WriteU8(MeleeConstants.MINOR_SCENE, 2);
            //var s = Dolphinterop.ReadU8(MeleeConstants.MINOR_SCENE);
            // Console.WriteLine(s);
            latestTime = DateTime.Now;

            Match = Dolphinterop.GetMatchData();
            ScDat = Dolphinterop.GetGlobalData();
            OnDat = Dolphinterop.GetOnlineData(ScDat);
            // zero point to this rn
            // this is causing NAOT to not work right
            StDat = Dolphinterop.GetStageData();

            if (OnDat.InOnlineMatch)
                if (ForceToClientPort)
                    if (OnDat.ClientPort != byte.MaxValue)
                        TPCamera.FocusPort = OnDat.ClientPort;

            // our camera manips won't work unless develop cam is enabled
            Dolphinterop.SetCameraType(CameraKind.Develop);

            // player data
            HandleKeyPressEvents();

            var dt = (float)(latestTime - oldLatestTime).TotalSeconds;
            if (float.IsFinite(dt)) {
                fps = 1f / dt;
                elapsedSeconds += dt;
            }

            //if (IsFirstPerson) {
            if (true) {
                MeleeFreeCamera cam = new();

                var fd = Match.Fighters[TPCamera.FocusPort];
                var hc = new FighterHurtCapsule();
                var part = FtPart.Invalid;
                // find head hitbox
                for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
                    var hurt = fd.Hurtboxes[i];

                    var partId = fd.GetPartFromJoint(hurt.capsule.bone_idx);
                    if (partId == FtPart.Invalid) {
                        hc = hurt;
                        part = partId;
                    }
                    if (partId == FtPart.WaistN) {
                        hc = hurt;
                        part = partId;
                        // dont break because we defer to WaistN only if there is no RShoulderN
                    }
                    // weirdly, RShoulderN is the head hitbox
                    if (partId == FtPart.RShoulderN) {
                        hc = hurt;
                        part = partId;
                        break;
                    }
                }

                var bone = fd.GetBone(part);
                var jobj = Dolphinterop.Read<JObj>(bone.jobj);
                // jobj.flags |= JObjFlags.Hidden;

                // Dolphinterop.Write(bone.jobj, jobj);

                Console.WriteLine(jobj.rotate);
                Console.WriteLine(jobj.mtx);
                Console.WriteLine(jobj.translate);
                Console.WriteLine();
                var pos = (hc.capsule.start + hc.capsule.end) / 2;
                cam.Eye = new Vector3(pos.X, pos.Y, -pos.Z);
                // cam.Focus = new Vector3(0, cam.Eye.Y, 0); // jobj.mtx.Translation;
                cam.Focus = pos + jobj.translate; //new Vector3(jobj.rotate.X, jobj.rotate.Y, jobj.rotate.Z);
                
                cam.Fov = 90;

                cam.SetCam();
            }
            else {
                TPCamera.Update(dt);
                TPCamera.Camera.SetCam();
            }

            // first person camera logic

            if (Match.Fighters[TPCamera.FocusPort].SlotKind == SlotKind.None) {
                // find first human port
                SlotMoveUntil(1, SlotKind.Human);
            }

            // every half-second
            if (elapsedSeconds % 0.2f < dt) {
                // WriteUI();
            }

            oldLatestTime = latestTime;

            // """"forces"""" fps to 63-64ish
            if (limitFps)
                Thread.Sleep(2);
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

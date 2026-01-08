using ExternalMeleeTool;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MeleeThirdPerson;
public class MeleeCamManip {
    static int updates;
    static int fps = 0;

    static ThirdPersonCamera TPCamera;

    public static DateTime latestTime;
    static DateTime oldLatestTime;

    public static MatchData Match;
    public static GlobalMeleeData GlDat;
    public static SlippiOnlineData OnDat;
    public static StageData StDat;

    public static bool ForceToClientPort = true;
    static void Main() {
        TPCamera ??= new ThirdPersonCamera();

        WaitForMelee();

        Console.Clear();
        // Console.WriteLine($"Connected! GALE01 found at 0x{Slippinterop.GALE01:X}");

        while (Slippinterop.IsConnected) {
            Console.CursorVisible = false;
            latestTime = DateTime.Now;

            Match = Slippinterop.GetMatchData();
            GlDat = Slippinterop.GetGlobalData();
            OnDat = Slippinterop.GetOnlineData(GlDat);
            StDat = Slippinterop.GetStageData(GlDat);

            if (ForceToClientPort)
                if (OnDat.InOnlineMatch)
                    if (OnDat.ClientPort != 255)
                        TPCamera.FocusPort = OnDat.ClientPort;

            // our camera manips won't work unless develop cam is enabled
            Slippinterop.SetCameraType(CameraKind.Develop); // 0x08 = develop cam

            // player data
            HandleKeyPressEvents();

            TPCamera.Update(/*(latestTime - oldLatestTime).Milliseconds*/);
            TPCamera.Camera.SetCam();

            var numFighters = Match.Fighters.Count(x => x.Position != Vector3.Zero);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"FPS: {fps}                                    ");
            Console.WriteLine();
            Console.WriteLine("Keybinds:");
            Console.WriteLine($"Focus Next Port:   {NextFighterPad} (Current={TPCamera.FocusPort}, {Match.Fighters[TPCamera.FocusPort].CharKind})                ");
            Console.WriteLine($"Change Focus Type: {ChangeFocusPad} (Current={TPCamera.FocusType})         ");
            Console.WriteLine($"Force Online Port: {ToggleForcePortPad} (Current={ForceToClientPort})          ");

            var focusedFighter = Match.Fighters[TPCamera.FocusPort];
            Console.WriteLine();
            Console.WriteLine($"Follow Data:         FocusType={TPCamera.FocusType}, FocusPort={TPCamera.FocusPort} ({focusedFighter.CharKind}, IsSub={focusedFighter.IsTransformed})             ");
            Console.WriteLine($"Slippi Data:         IsOnline={OnDat.InOnlineMatch}, ClientPort={OnDat.ClientPort}, Frame={OnDat.Frame}      ");
            Console.WriteLine($"Match Data:          IsTeams={Match.IsTeams}        ");
            Console.WriteLine($"Stage Data:          StageId={StDat.StageId}, CollVerts={StDat.VertexCount}       ");
            Console.WriteLine($"Global Data:         MajorScene={GlDat.MajorScene}, MinorScene={GlDat.MinorScene}        ");
            Console.WriteLine();
            Console.WriteLine($"Camera Position:     {TPCamera.Camera.Eye}                           ");
            Console.WriteLine($"Camera Focus:        {TPCamera.Camera.Focus}                         ");
            Console.WriteLine($"Camera FOV:          {TPCamera.Camera.Fov}                           ");
            Console.WriteLine();
            Console.WriteLine($"# Players Active: {numFighters}         ");

            for (int i = 0; i < Match.Fighters.Length; i++) {
                var ft = Match.Fighters[i];

                if (ft.SlotKind == SlotKind.None) continue;
                Console.WriteLine($"Player {i + 1}: {ft.FriendlyString()}                    ");
            }
            Console.WriteLine("                                                  ");
            Console.WriteLine("                                                  ");
            Console.WriteLine("                                                  ");

            updates++;
            if (latestTime.Second != oldLatestTime.Second) {
                fps = updates;
                updates = 0;
            }

            oldLatestTime = latestTime;
            // Thread.Sleep(TimeSpan.FromMilliseconds(17)); // thread sleeping is clearly not the way to go
        }

        Console.Clear();

        Main();
    }
    static void WaitForMelee() {
        // don't do anything with an invalid GALE01
        int rotIndex = 0;
        while (!Slippinterop.Connect()) {
            Console.SetCursorPosition(0, 0);
            ShowWait(rotIndex);

            rotIndex++;
            if (rotIndex > 3) rotIndex = 0;

            Thread.Sleep(250);
        }
    }
    static HSDPadButton NextFighterPad = HSDPadButton.DPadRight;
    static HSDPadButton ToggleForcePortPad = HSDPadButton.DPadLeft;
    static HSDPadButton ChangeFocusPad = HSDPadButton.DPadDown;

    static int inputTimeout;
    static void HandleKeyPressEvents() {
        if (inputTimeout > 0) {
            inputTimeout--;
            return;
        }

        var myFighter = Match.Fighters[OnDat.ClientControllerPort];

        static void slotMove(int amount) {
            int start = TPCamera.FocusPort;

            do {
                TPCamera.FocusPort += amount;
                if (TPCamera.FocusPort >= Match.Fighters.Length || TPCamera.FocusPort < 0)
                    TPCamera.FocusPort = 0;

                if (TPCamera.FocusPort == start)
                    break;

            } while (Match.Fighters[TPCamera.FocusPort].SlotKind == SlotKind.None);
        }

        // cycle to next active fighter
        if (MeleeUtils.GCInputPressed(myFighter, NextFighterPad)) {
            slotMove(1);

            inputTimeout = 60;
        }
        // Old: KeyUtils.WasKeyPressed
        else if (MeleeUtils.GCInputPressed(myFighter, ChangeFocusPad)) {
            TPCamera.FocusType++;

            if (TPCamera.FocusType > ThirdPersonFocusType.ClosestEnemy)
                TPCamera.FocusType = 0;

            inputTimeout = 60;
        }
        else if (MeleeUtils.GCInputPressed(myFighter, ToggleForcePortPad)) {
            ForceToClientPort = !ForceToClientPort;
            inputTimeout = 60;
        }
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
